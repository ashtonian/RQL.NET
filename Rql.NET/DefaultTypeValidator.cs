using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql.NET
{
    public class DefaultTypeValidator
    {
        private static Dictionary<Type, HashSet<Type>> _typeMap = new Dictionary<Type, HashSet<Type>> {
        {
            typeof(bool),
            new HashSet<Type>
            {
                typeof(bool)
            }
        },
        {
            typeof(Int64),
            new HashSet<Type>
            {
                typeof(sbyte),
                typeof(byte),
                typeof(Int16),
                typeof(Int32) ,
                typeof(Int64) ,
                typeof(UInt16) ,
                typeof(UInt64) ,
                typeof(Single),
                typeof(Double),
                typeof(Decimal),
            }
        },
        {
            typeof(Double),
            new HashSet<Type>
            {
                typeof(Single),
                typeof(Double),
                typeof(Decimal),
            }
        },
        {
            typeof(DateTime),
            new HashSet<Type>
            {
                typeof(DateTime),
                typeof(string),
                typeof(long),

            }
        },
        {
            typeof(string),
            new HashSet<Type>
            {
                typeof(string),
                typeof(char[]),
                typeof(IEnumerable<char>),
            }
        }
    };

        public IError Validate(string propName, Type propType, object value)
        {
            if (value == null) { } // TODO: 
            var objList = value as IEnumerable<Object>;
            if (objList != null)
            {
                foreach (var v in objList)
                {
                    var objType2 = v.GetType();
                    var expectedTypes2 = _typeMap[objType2];
                    var isValid2 = expectedTypes2.Contains(propType);
                    if (!isValid2) return new Error($"Invalid type Expected:{expectedTypes2.ToString()}, found: {objType2}");
                }
                return null;
            }

            var objType = value.GetType();
            var expectedTypes = _typeMap[objType];
            var isValid = expectedTypes.Contains(propType);
            if (!isValid) return new Error($"Invalid type Expected:{expectedTypes.ToString()}, found: {objType}");

            return null;
        }
    }
}