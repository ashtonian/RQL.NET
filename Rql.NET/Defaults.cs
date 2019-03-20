using System;
using System.Collections.Generic;


namespace Rql.NET
{
    public static class Defaults
    {
        public static Func<IParameterTokenizer> DefaultTokenizerFactory = new Func<IParameterTokenizer>(() => { return new NamedTokenizer(); });
        public static Func<string, Type, object, IError> DefaultValidator = new DefaultTypeValidator().Validate;
        public static Func<string, string> DefaultColumnNamer = new Func<string, string>(x => x);
        public static Func<string, string> DefaultFieldNamer = new Func<string, string>(x => Char.ToLowerInvariant(x[0]) + x.Substring(1));
        public static IOpMapper DefaultOpMapper = new SqlMapper();
        public static Dictionary<Type, ClassSpec> TypeCache = new Dictionary<Type, ClassSpec>();
        public static Func<Type, ClassSpec> CacheResolver = new Func<Type, ClassSpec>((type) =>
        {
            if (TypeCache.ContainsKey(type)) return TypeCache[type];
            return null;
        });

        public static Func<string, Type, object, (object, IError)> DefaultConverter =
        new Func<string, Type, object, (object, IError)>(
            (FieldName, type, raw) =>
            {
                if (type == typeof(DateTime) && type != raw.GetType())
                {
                    switch (raw)
                    {
                        case long longVal:
                            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(longVal);
                            return (dateTimeOffset.DateTime, null);
                        case string strVal:
                            DateTime result;
                            var success = DateTime.TryParse(strVal, out result);
                            if (!success) return (null, new Error("unable to convert datetime"));
                            return (result, null);
                    }
                }
                return (raw, null);
            }
        );
    }
}