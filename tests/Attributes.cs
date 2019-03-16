using System;

[AttributeUsage(AttributeTargets.Class)]
public class Filterable : System.Attribute
{
    private string[] _props;
    public Filterable(params string[] props) { _props = props; }
}

[AttributeUsage(AttributeTargets.Class)]
public class Sortable : System.Attribute
{
    private string[] _props;
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
    public class Allowed : System.Attribute
    {
        private string[] _ops;
        public Allowed(params string[] ops) { _ops = ops; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Disallowed : System.Attribute
    {
        private string[] _ops;
        public Disallowed(params string[] ops) { _ops = ops; }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ColumnName : System.Attribute
{
    private string _name;
    public ColumnName(string name) { _name = name; }
}

[AttributeUsage(AttributeTargets.Property)]
public class FieldName : System.Attribute
{
    private string _name;
    public FieldName(string name) { _name = name; }
}


// TODO:
/*  Attributes
    CustomTypeConverter
    CustomValidator
    FieldName? 
*/
