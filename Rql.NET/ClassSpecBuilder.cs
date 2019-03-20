using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rql.NET
{
    public class FieldSpec
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public HashSet<string> Ops { get; set; }
        public Type PropType { get; set; }
        public bool IsSortable { get; set; }
        public Func<string, Type, object, IError> Validator { get; set; }
        public Func<string, Type, object, (object, IError)> Converter { get; set; }
    }

    public class ClassSpec
    {
        public Dictionary<string, FieldSpec> Fields { get; set; }
        public Func<string, Type, object, IError> Validator { get; set; }
        public Func<string, Type, object, (object, IError)> Converter { get; set; }
    }

    public class ClassSpecBuilder
    {
        private readonly Func<string, string> _columnNamer;
        private readonly Func<string, string> _fieldNamer;
        private readonly IOpMapper _opMapper;
        private readonly Func<string, Type, object, IError> _validator;
        private readonly Func<string, Type, object, (object, IError)> _converter;
        public ClassSpecBuilder(
            Func<string, string> columnNamer = null,
            Func<string, string> fieldNamer = null,
            IOpMapper opMapper = null,
            Func<string, Type, object, IError> validator = null,
            Func<string, Type, object, (object, IError)> converter = null
        )
        {
            _columnNamer = columnNamer;
            _fieldNamer = fieldNamer;
            _opMapper = opMapper;
            _validator = validator;
            _converter = converter;
        }
        public ClassSpec Build(Type t)
        {
            var spec = Defaults.CacheResolver(t);
            if (spec != null) return spec;

            var _fields = new Dictionary<string, FieldSpec>() { };

            var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var classAttributes = t.GetCustomAttributes(true).ToList();
            var classFilter = classAttributes?.OfType<Filterable>()?.FirstOrDefault();
            var classSortable = classAttributes?.OfType<Filterable>()?.FirstOrDefault();

            // TODO: is class use name resolver and concat fields to class 
            foreach (var p in properties)
            {
                var attributes = p.GetCustomAttributes(true);

                var ignore = attributes?.OfType<Ignore>()?.FirstOrDefault();
                var ignoreSort = attributes?.OfType<Ignore.Sort>()?.FirstOrDefault();
                var ignoreFilter = attributes?.OfType<Ignore.Filter>()?.FirstOrDefault();
                if (ignore != null || ignoreFilter != null) continue;

                var opMapper = _opMapper ?? Defaults.DefaultOpMapper;
                var ops = opMapper.GetSupportedOps(p.PropertyType);
                var opsBlacklist = attributes?.OfType<Ops.Disallowed>()?.FirstOrDefault();
                if (opsBlacklist != null) ops.ToList().RemoveAll(opsBlacklist._ops.Contains); // TODO: don't think this modifies the ops ref
                var columnName = attributes?.OfType<ColumnName>()?.FirstOrDefault();
                var fieldName = attributes?.OfType<FieldName>()?.FirstOrDefault();

                var _field = new FieldSpec
                {
                    IsSortable = (ignoreSort == null),
                    Ops = ops,
                    PropType = p.PropertyType,
                    ColumnName = columnName?._name ?? (_columnNamer != null ? _columnNamer(p.Name) : Defaults.DefaultColumnNamer(p.Name)),
                    Name = fieldName?._name ?? (fieldName != null ? _fieldNamer(p.Name) : Defaults.DefaultFieldNamer(p.Name)),
                    Converter = p.PropertyType == typeof(DateTime) ? Defaults.DefaultConverter : null,               // ?? attribute that looks for IConverter on field, then class
                    Validator = null,               // ?? attribute that looks for IValidator on field, then class
                };
                _fields.Add(_field.Name, _field);
            }

            return new ClassSpec()
            {
                Fields = _fields,
                Converter = _converter,
                Validator = _validator ?? Defaults.DefaultValidator,
            };
        }
    }
}