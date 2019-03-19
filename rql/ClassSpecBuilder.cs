using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    private static Func<string, Type, object, IError> defaultValidiator = new DefaultTypeValidator().Validate; // Todo config static fields? 
    private static Func<string, string> defaultColumnNamer = new Func<string, string>(x => x);
    private static Func<string, string> defaultFieldNamer = new Func<string, string>(x => Char.ToLowerInvariant(x[0]) + x.Substring(1));
    private static IOpMapper defaultOpMapper = new SqlMapper();
    private static Dictionary<Type, ClassSpec> typeCache = new Dictionary<Type, ClassSpec>();
    private readonly Func<string, string> columnNamer;
    private readonly Func<string, string> fieldNamer;
    private readonly IOpMapper opMapper;
    private readonly Func<string, Type, object, IError> Validator;
    private readonly Func<string, Type, object, (object, IError)> Converter;
    public ClassSpecBuilder() { }
    public ClassSpec Build(Type t)
    {
        if (typeCache.ContainsKey(t)) return typeCache[t];

        var _fields = new Dictionary<string, FieldSpec>() { };

        var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var classAttributes = t.GetCustomAttributes(true).ToList();
        var classFilter = classAttributes?.OfType<Filterable>()?.FirstOrDefault();
        var classSortable = classAttributes?.OfType<Filterable>()?.FirstOrDefault();

        // if is class use name resolver and concat fields to class TODO:

        foreach (var p in properties)
        {
            var attributes = p.GetCustomAttributes(true);

            var ignore = attributes?.OfType<Ignore>()?.FirstOrDefault();
            var ignoreSort = attributes?.OfType<Ignore.Sort>()?.FirstOrDefault();
            var ignoreFilter = attributes?.OfType<Ignore.Filter>()?.FirstOrDefault();
            if (ignore != null || ignoreFilter != null) continue;

            var opMapper = this.opMapper ?? defaultOpMapper;
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
                ColumnName = columnName?._name ?? (columnNamer != null ? columnNamer(p.Name) : defaultColumnNamer(p.Name)),
                Name = fieldName?._name ?? (fieldName != null ? fieldNamer(p.Name) : defaultFieldNamer(p.Name)),
                Converter = null,               // ?? attribute that looks for IConverter on field, then class
                Validator = null,               // ?? attribute that looks for IValidator on field, then class
            };
            _fields.Add(_field.Name, _field);
        }

        return new ClassSpec()
        {
            Fields = _fields,
            Converter = Converter,
            Validator = Validator ?? defaultValidiator,
        };
    }
}
