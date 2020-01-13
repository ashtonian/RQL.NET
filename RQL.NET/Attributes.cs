using System;


// TODO:
/*  Attributes
    CustomTypeConverter
    CustomValidator
    DefaultSort
    Filterable
    Sortable
*/
namespace RQL.NET
{
    // [AttributeUsage(AttributeTargets.Class)]
    // public class Filterable : Attribute
    // {
    //     internal readonly string[] Props;

    //     public Filterable(params string[] props)
    //     {
    //         Props = props;
    //     }
    // }

    // [AttributeUsage(AttributeTargets.Class)]
    // public class Sortable : Attribute
    // {
    //     internal readonly string[] Props;

    //     public Sortable(params string[] props)
    //     {
    //         Props = props;
    //     }
    // }


    /// <summary>
    /// Ignores the class property entirely.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Ignore : Attribute
    {
        /// <summary>
        /// Prevents the target property from being sortable.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class Sort : Attribute
        {
        }

        /// <summary>
        /// Prevents the target property from being filterable.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class Filter : Attribute
        {
        }
    }

    public class Ops
    {
        /// <summary>
        /// Disallows specified operations for a given property.
        /// </summary>
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


    /// <summary>
    /// ColumnName overrides the target column name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnName : Attribute
    {
        internal readonly string Name;

        public ColumnName(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// FieldName overrides the target json field name.
    /// </summary>
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