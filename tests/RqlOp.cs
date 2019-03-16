using System;
using System.Collections.Generic;

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