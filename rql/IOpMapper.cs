using System;
using System.Collections.Generic;

public interface IOpMapper
{
    string GetDbOp(string rqlOp);
    HashSet<String> GetSupportedOps(Type type);

    // TODO: sort specific function
}

public class SqlMapper : IOpMapper
{
    private readonly Dictionary<string, string> _ops = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {RqlOp.EQ,SqlOp.EQ},
        {RqlOp.NEQ,SqlOp.NEQ},
        {RqlOp.LT,SqlOp.LT},
        {RqlOp.GT,SqlOp.GT},
        {RqlOp.LTE,SqlOp.LTE},
        {RqlOp.GTE,SqlOp.GTE},
        {RqlOp.LIKE,SqlOp.LIKE},
        {RqlOp.OR,SqlOp.OR},
        {RqlOp.AND,SqlOp.AND},
        {RqlOp.NIN,SqlOp.NIN},
        {RqlOp.IN,SqlOp.IN},
        {RqlOp.ASC,SqlOp.ASC},
        {RqlOp.DESC,SqlOp.DESC},

    };

    // Must return RQL supported ops 
    public HashSet<string> GetSupportedOps(Type type)
    {
        var numerics = new HashSet<Type>() {
            typeof(sbyte),
            typeof(byte),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(UInt16),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
        };
        var strings = new HashSet<Type>() {
            typeof(string),
            typeof(char[]),
            typeof(IEnumerable<char>),
        };
        var bools = new HashSet<Type>() { typeof(bool) };
        var dateTime = new HashSet<Type> {
            typeof(DateTime)
        };

        if (numerics.Contains(type)) return new HashSet<string> {
            RqlOp.EQ,
            RqlOp.NEQ,
            RqlOp.LT,
            RqlOp.GT,
            RqlOp.LTE,
            RqlOp.GTE,
            RqlOp.OR,
            RqlOp.AND,
            RqlOp.IN,
            RqlOp.NIN,
        };
        if (strings.Contains(type)) return new HashSet<string> {
            RqlOp.EQ,
            RqlOp.NEQ,
            RqlOp.LIKE,
            RqlOp.OR,
            RqlOp.AND,
            RqlOp.IN,
            RqlOp.NIN,
        };
        if (bools.Contains(type)) return new HashSet<string> {
            RqlOp.EQ,
            RqlOp.NEQ,
            RqlOp.OR,
            RqlOp.AND,
        };
        if (dateTime.Contains(type)) return new HashSet<string> {
            RqlOp.EQ,
            RqlOp.NEQ,
            RqlOp.LT,
            RqlOp.GT,
            RqlOp.LTE,
            RqlOp.GTE,
            RqlOp.OR,
            RqlOp.AND,
            RqlOp.IN,
            RqlOp.NIN,
         };
        return null;
    }

    public string GetDbOp(string rqlOp)
    {
        if (rqlOp == null || rqlOp.Trim().Length < 0) return null;

        return _ops[rqlOp];
    }
}

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
    private static readonly HashSet<string> _SqlOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        {EQ},
        {NEQ},
        {LT},
        {GT},
        {LTE},
        {GTE},
        {LIKE},
        {OR},
        {AND},
        {NIN},
        {IN},
    };

    public static bool IsOp(string val)
    {
        if (val == null || val.Trim().Length < 0) return false;

        return _SqlOps.Contains(val);
    }
}


// TODO: make $ prefix configurable
public static class RqlOp
{
    public const string EQ = "$eq";
    public const string NEQ = "$neq";
    public const string LT = "$lt";
    public const string GT = "$gt";
    public const string LTE = "$lte";
    public const string GTE = "$gte";
    public const string LIKE = "$like";
    public const string OR = "$or";
    public const string NOR = "$nor"; // TODO:
    public const string NOT = "$not"; // TODO:
    public const string AND = "$and";
    public const string IN = "$in"; // TODO:
    public const string NIN = "$nin"; // TODO:
    public const string ASC = "+";
    public const string DESC = "-";
    private static readonly HashSet<string> _rqlOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        {EQ},
        {NEQ},
        {LT},
        {GT},
        {LTE},
        {GTE},
        {LIKE},
        {OR},
        {AND},
        {NIN},
        {IN},
        {NOT},
        {NOR}
    };

    public static bool IsOp(string val)
    {
        if (val == null || val.Trim().Length < 0) return false;

        return _rqlOps.Contains(val);
    }
}