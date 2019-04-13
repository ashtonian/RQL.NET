using System;
using System.Collections.Generic;

namespace Rql.NET
{
    public static class Defaults
    {
        // TODO: pass through parsed request sort list<string> and eventually structured rql query
        // TODO: add default sort fields id, key if detected and not ignored
        public static string SortSeparator = ",";
        public static string Prefix = "$";
        public static int Offset = 0;
        public static int Limit = 10000;
        public static Func<IParameterTokenizer> DefaultTokenizerFactory = () => new NamedTokenizer();
        public static Func<string, Type, object, IError> DefaultValidator = DefaultTypeValidator.Validate;
        public static Func<string, string> DefaultColumnNamer = x => x;
        public static Func<string, string> DefaultFieldNamer = x => char.ToLowerInvariant(x[0]) + x.Substring(1);
        public static IOpMapper DefaultOpMapper = new SqlMapper();
        public static Dictionary<Type, ClassSpec> TypeCache = new Dictionary<Type, ClassSpec>();

        public static Func<Type, ClassSpec>
            CacheResolver = type => TypeCache.ContainsKey(type) ? TypeCache[type] : null;

        public static Func<string, Type, object, (object, IError)> DefaultConverter =
            (fieldName, type, raw) =>
            {
                if (type != typeof(DateTime) || type == raw.GetType()) return (raw, null);
                switch (raw)
                {
                    case long longVal:
                        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(longVal);
                        return (dateTimeOffset.DateTime, null);
                    case string strVal:
                        var success = DateTime.TryParse(strVal, out var result);
                        if (!success) return (null, new Error("unable to convert datetime"));
                        return (result, null);
                }

                return (raw, null);
            };
    }
}