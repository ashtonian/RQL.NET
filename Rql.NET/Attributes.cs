using System;


// TODO:
/*  Attributes
    CustomTypeConverter
    CustomValidator
    DefaultSort
*/
namespace Rql.NET
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Filterable : Attribute
    {
        internal readonly string[] Props;

        public Filterable(params string[] props)
        {
            Props = props;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Sortable : Attribute
    {
        internal readonly string[] Props;

        public Sortable(params string[] props)
        {
            Props = props;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class Ignore : Attribute
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class Sort : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class Filter : Attribute
        {
        }
    }

    public class Ops
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class Disallowed : Attribute
        {
            internal readonly string[] Ops;

            public Disallowed(params string[] ops)
            {
                Ops = ops;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnName : Attribute
    {
        internal readonly string Name;

        public ColumnName(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldName : Attribute
    {
        internal readonly string Name;

        public FieldName(string name)
        {
            Name = name;
        }
    }
}