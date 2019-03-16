using System;

[AttributeUsage(AttributeTargets.Class)]
public class Filterable : System.Attribute
{
    internal string[] _props;
    public Filterable(params string[] props) { _props = props; }
}

[AttributeUsage(AttributeTargets.Class)]
public class Sortable : System.Attribute
{
    internal string[] _props;
    public Sortable(params string[] props) { _props = props; }
}


[AttributeUsage(AttributeTargets.Property)]
public class Ignore : System.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Sort : System.Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class Filter : System.Attribute { }
}
public class Ops
{

    [AttributeUsage(AttributeTargets.Property)]
    public class Disallowed : System.Attribute
    {
        internal string[] _ops;
        public Disallowed(params string[] ops) { _ops = ops; }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ColumnName : System.Attribute
{
    internal string _name;
    public ColumnName(string name) { _name = name; }
}

[AttributeUsage(AttributeTargets.Property)]
public class FieldName : System.Attribute
{
    internal string _name;
    public FieldName(string name) { _name = name; }
}


// TODO:
/*  Attributes
    CustomTypeConverter
    CustomValidator
    FieldName? 
*/
