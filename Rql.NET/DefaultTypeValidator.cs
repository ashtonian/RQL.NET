using System;
using System.Collections.Generic;

namespace Rql.NET
{
    /// <summary>
    /// This is validates that the json type is compatible and convertable to the corresponding c# type.
    /// </summary>
    public static class DefaultTypeValidator
    {
        private static readonly Dictionary<Type, HashSet<Type>> TypeMap = new Dictionary<Type, HashSet<Type>>
        {
            {
                typeof(bool),
                new HashSet<Type>
                {
                    typeof(bool)
                }
            },
            {
                typeof(long),
                new HashSet<Type>
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(ushort),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(double),
                new HashSet<Type>
                {
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(DateTime),
                new HashSet<Type>
                {
                    typeof(DateTime),
                    typeof(string),
                    typeof(long)
                }
            },
            {
                typeof(string),
                new HashSet<Type>
                {
                    typeof(string),
                    typeof(char[]),
                    typeof(IEnumerable<char>)
                }
            }
        };

        public static IError Validate(string propName, Type propType, object value)
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