using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql.NET
{
    public class DefaultTypeValidator
    {
        private static readonly Dictionary<Type, HashSet<Type>> TypeMap = new Dictionary<Type, HashSet<Type>> {
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
                typeof(int) ,
                typeof(long) ,
                typeof(ushort) ,
                typeof(ulong) ,
                typeof(float),
                typeof(Double),
                typeof(Decimal),
            }
        },
        {
            typeof(Double),
            new HashSet<Type>
            {
                typeof(float),
                typeof(double),
                typeof(decimal),
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
            switch (value)
            {
                case null:
                    break; // TODO: 
                case IEnumerable<object> objList:
                {
                    foreach (var v in objList)
                    {
                        var objType2 = v.GetType();
                        var expectedTypes2 = TypeMap[objType2];
                        var isValid2 = expectedTypes2.Contains(propType);
                        if (!isValid2) return new Error($"Invalid type Expected:{expectedTypes2}, found: {objType2}");
                    }
                    return null;
                }
            }

            var objType = value.GetType();
            var expectedTypes = TypeMap[objType];
            var isValid = expectedTypes.Contains(propType);
            return !isValid ? new Error($"Invalid type Expected:{expectedTypes}, found: {objType}") : null;
        }
    }
}