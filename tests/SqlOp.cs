using System;
using System.Collections.Generic;

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
