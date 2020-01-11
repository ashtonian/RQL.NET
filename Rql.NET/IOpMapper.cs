using System;
using System.Collections.Generic;

namespace Rql.NET
{
    /// <summary>
    /// Responsible for mapping rql operations to the corresponding backend (sql) operation.
    /// </summary>
    public interface IOpMapper
    {
        string GetDbOp(string rqlOp);

        HashSet<string> GetSupportedOps(Type type);
        // TODO: sort ops only function
    }

    public class SqlMapper : IOpMapper
    {
        private readonly Dictionary<string, string> _ops =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {RqlOp.EQ, SqlOp.EQ},
                {RqlOp.NEQ, SqlOp.NEQ},
                {RqlOp.LT, SqlOp.LT},
                {RqlOp.GT, SqlOp.GT},
                {RqlOp.LTE, SqlOp.LTE},
                {RqlOp.GTE, SqlOp.GTE},
                {RqlOp.LIKE, SqlOp.LIKE},
                {RqlOp.OR, SqlOp.OR},
                {RqlOp.AND, SqlOp.AND},
                {RqlOp.NIN, SqlOp.NIN},
                {RqlOp.IN, SqlOp.IN},
                {RqlOp.ASC, SqlOp.ASC},
                {RqlOp.DESC, SqlOp.DESC}
            };

        // Must return RQL supported ops
        public HashSet<string> GetSupportedOps(Type type)
        {
            var numerics = new HashSet<Type>
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
            };
            var strings = new HashSet<Type>
            {
                typeof(string),
                typeof(char[]),
                typeof(IEnumerable<char>)
            };
            var bools = new HashSet<Type> { typeof(bool) };
            var dateTime = new HashSet<Type>
            {
                typeof(DateTime)
            };

            if (numerics.Contains(type))
                return new HashSet<string>
                {
                    RqlOp.EQ,
                    RqlOp.NEQ,
                    RqlOp.LT,
                    RqlOp.GT,
                    RqlOp.LTE,
                    RqlOp.GTE,
                    RqlOp.OR,
                    RqlOp.AND,
                    RqlOp.IN,
                    RqlOp.NIN
                };
            if (strings.Contains(type))
                return new HashSet<string>
                {
                    RqlOp.EQ,
                    RqlOp.NEQ,
                    RqlOp.LIKE,
                    RqlOp.OR,
                    RqlOp.AND,
                    RqlOp.IN,
                    RqlOp.NIN
                };
            if (bools.Contains(type))
                return new HashSet<string>
                {
                    RqlOp.EQ,
                    RqlOp.NEQ,
                    RqlOp.OR,
                    RqlOp.AND
                };
            if (dateTime.Contains(type))
                return new HashSet<string>
                {
                    RqlOp.EQ,
                    RqlOp.NEQ,
                    RqlOp.LT,
                    RqlOp.GT,
                    RqlOp.LTE,
                    RqlOp.GTE,
                    RqlOp.OR,
                    RqlOp.AND,
                    RqlOp.IN,
                    RqlOp.NIN
                };
            return null;
        }

        public string GetDbOp(string rqlOp)
        {
            if (rqlOp == null || rqlOp.Trim().Length < 0) return null;

            return _ops[rqlOp];
        }
    }

    /// <summary>
    /// Contains all of the Sql operation string tokens.
    /// </summary>
    public static class SqlOp
    {
        public const string EQ = "=";
        public const string NEQ = "!=";
        public const string LT = "<";
        public const string GT = ">";
        public const string LTE = "<=";
        public const string GTE = ">=";
        public const string LIKE = "LIKE";
        public const string OR = "OR";
        public const string AND = "AND";
        public const string NIN = "NOT IN";
        public const string IN = "IN";
        public const string ASC = "ASC";
        public const string DESC = "DESC";

        private static readonly HashSet<string> SqlOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            EQ,
            NEQ,
            LT,
            GT,
            LTE,
            GTE,
            LIKE,
            OR,
            AND,
            NIN,
            IN
        };

        public static bool IsOp(string val)
        {
            if (val == null || val.Trim().Length < 0) return false;

            return SqlOps.Contains(val);
        }
    }

    /// <summary>
    /// Contains all of the rql operation string tokens.
    /// </summary>
    public static class RqlOp
    {
        public const string ASC = "+";
        public const string DESC = "-";
        public static readonly string EQ = $"{Defaults.Prefix}eq";
        public static readonly string NEQ = $"{Defaults.Prefix}neq";
        public static readonly string LT = $"{Defaults.Prefix}lt";
        public static readonly string GT = $"{Defaults.Prefix}gt";
        public static readonly string LTE = $"{Defaults.Prefix}lte";
        public static readonly string GTE = $"{Defaults.Prefix}gte";
        public static readonly string LIKE = $"{Defaults.Prefix}like";
        public static readonly string OR = $"{Defaults.Prefix}or";

        public static readonly string NOR = $"{Defaults.Prefix}nor";
        public static readonly string NOT = $"{Defaults.Prefix}not";
        public static readonly string AND = $"{Defaults.Prefix}and";
        public static readonly string IN = $"{Defaults.Prefix}in";
        public static readonly string NIN = $"{Defaults.Prefix}nin";

        private static readonly HashSet<string> RqlOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            EQ,
            NEQ,
            LT,
            GT,
            LTE,
            GTE,
            LIKE,
            OR,
            AND,
            NIN,
            IN,
            NOT,
            NOR
        };

        public static bool IsOp(string val)
        {
            return val != null && RqlOps.Contains(val);
        }
    }
}