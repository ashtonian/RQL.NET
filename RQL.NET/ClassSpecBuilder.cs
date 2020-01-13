using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RQL.NET
{
    /// <summary>
    /// FieldSpec contains all the field (class property) meta data required to parse information for that field from json.
    /// </summary>
    public class FieldSpec
    {
        /// <summary>
        /// json field name.
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public HashSet<string> Ops { get; set; }
        public Type PropType { get; set; }
        public bool IsSortable { get; set; }
        public Func<string, Type, object, IError> Validator { get; set; }
        public Func<string, Type, object, (object, IError)> Converter { get; set; }
    }

    /// <summary>
    /// Contains all the required metadata to parse an object.
    /// </summary>
    public class ClassSpec
    {
        public Dictionary<string, FieldSpec> Fields { get; set; }
        public Func<string, Type, object, IError> Validator { get; set; }
        public Func<string, Type, object, (object, IError)> Converter { get; set; }
    }


    public interface ClassSpecCache
    {
        ClassSpec Get(Type t);
        void Set(Type t, ClassSpec spec);
    }

    public class InMemoryClassSpecCache : ClassSpecCache
    {
        private static ConcurrentDictionary<Type, ClassSpec> TypeCache = new ConcurrentDictionary<Type, ClassSpec>();

        public ClassSpec Get(Type t)
        {
            return TypeCache.ContainsKey(t) ? TypeCache[t] : null;
        }

        public void Set(Type t, ClassSpec spec)
        {
            var result = TypeCache.AddOrUpdate(t, spec, (key, spec2) => { return spec2; });
        }
    }

    /// <summary>
    /// Uses reflection (once, then cached) to generate required parse metadata.
    /// </summary>
    public class ClassSpecBuilder
    {
        private readonly Func<string, string> _columnNamer;
        private readonly Func<string, Type, object, (object, IError)> _converter;
        private readonly Func<string, string> _fieldNamer;
        private readonly IOpMapper _opMapper;
        private readonly Func<string, Type, object, IError> _validator;

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

        /// <summary>
        /// Generates and caches all required metadata to parse an object.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public ClassSpec Build(Type t)
        {
            var spec = Defaults.SpecCache.Get(t);
            if (spec != null) return spec;

            var fields = new Dictionary<string, FieldSpec>();

            var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var classAttributes = t.GetCustomAttributes(true).ToList();

            // TODO: implement class props
            // var classFilter = classAttributes?.OfType<Filterable>()?.FirstOrDefault();
            // var classSortable = classAttributes?.OfType<Filterable>()?.FirstOrDefault();

            // TODO: is class use name resolver and concat fields to class
            // v1 flat table nested class
            // v2 assume table is joined, or do a sub query, or both?

            // loop through class properties and generate field spec
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
                if (opsBlacklist != null)
                    ops.ToList().RemoveAll(opsBlacklist.Ops.Contains); // TODO: don't think this modifies the ops ref
                var columnName = attributes?.OfType<ColumnName>()?.FirstOrDefault();
                var fieldName = attributes?.OfType<FieldName>()?.FirstOrDefault();

                var field = new FieldSpec
                {
                    IsSortable = ignoreSort == null,
                    Ops = ops,
                    PropType = p.PropertyType,
                    ColumnName = columnName?.Name ??
                                 (_columnNamer != null ? _columnNamer(p.Name) : Defaults.DefaultColumnNamer(p.Name)),

                    Name = fieldName?.Name ??
                           (fieldName != null ? _fieldNamer(p.Name) : Defaults.DefaultFieldNamer(p.Name)),

                    Converter = p.PropertyType == typeof(DateTime)
                        ? Defaults.DefaultConverter
                        : null, // ?? attribute that looks for IConverter on field, then class

                    Validator = null // ?? attribute that looks for IValidator on field, then class
                };
                fields.Add(field.Name, field);
            }

            spec = new ClassSpec
            {
                Fields = fields,
                Converter = _converter,
                Validator = _validator ?? Defaults.DefaultValidator
            };

            Defaults.SpecCache.Set(t, spec);
            return spec;
        }
    }
}