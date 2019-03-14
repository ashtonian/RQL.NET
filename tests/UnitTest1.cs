using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var raw = @"{
                        ""city"": ""TLV"",
                        ""$or"": [
                                { ""city"": ""TLV"" },
                                { ""city"": ""NYC"" } 
                			],
                        ""account"": { ""$like"": "" % github % "" }
                		}
                  ";

            var fields = new List<Field> {
                new Field{
                    Name= "city",
                },
                new Field{
                    Name = "account",
                },
            };
            var spec = new FieldSpec(fields);
            var p = new Parser(spec);

            var (str, parms, error) = p.ParseQuery(raw);
            Console.WriteLine(str, parms, error);

            // "WHERE city  =  TLV  AND (( city  =  TLV ) OR ( city  =  NYC )) AND  account  LIKE  =   % github %  "
        }

    }
}

// TODO:
/*  Attributes
    Ignore
    Ignore.Sort
    Ignore.Filter
    CustomTypeConverter
    CustomValidator
    Custom/Limit operations, only want allow EQ not GTE for example for a given field..  
*/
[System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct)
]
public class Test : System.Attribute
{
    private string[] _ops;

    public Test(params string[] ops)
    {
        _ops = ops;
    }
}

public class Test2
{
    // TODO: attempt to hack enums [Test(new[] { RqlOp.AND })]
    public string Mem { get; set; }
}
public class SqlOp
{

    private readonly string value;
    public static readonly SqlOp EQ = new SqlOp("=");

    public static readonly SqlOp NEQ = new SqlOp("!=");

    public static readonly SqlOp LT = new SqlOp("<");

    public static readonly SqlOp GT = new SqlOp(">");

    public static readonly SqlOp LTE = new SqlOp("<=");

    public static readonly SqlOp GTE = new SqlOp(">=");

    public static readonly SqlOp LIKE = new SqlOp("LIKE");

    public static readonly SqlOp OR = new SqlOp("OR");

    public static readonly SqlOp AND = new SqlOp("AND");

    public static readonly SqlOp NIN = new SqlOp("NOT IN");

    public static readonly SqlOp IN = new SqlOp("IN");

    // public static readonly SqlOp NOT = new SqlOp("$not");

    private SqlOp(String value)
    {
        this.value = value;
    }

    private static readonly Dictionary<string, SqlOp> _SqlOps = new Dictionary<string, SqlOp>
    {
        {EQ.value, EQ},
        {NEQ.value, NEQ},
        {LT.value, LT},
        {GT.value, GT},
        {LTE.value, LTE},
        {GTE.value, GTE},
        {LIKE.value, LIKE},
        {OR.value, OR},
        {AND.value, AND},
        {NIN.value, NIN},
        {IN.value, IN},
    };

    private static readonly Dictionary<RqlOp, SqlOp> _rqlOps = new Dictionary<RqlOp, SqlOp>
    {
        {RqlOp.EQ, EQ},
        {RqlOp.NEQ, NEQ},
        {RqlOp.LT, LT},
        {RqlOp.GT, GT},
        {RqlOp.LTE, LTE},
        {RqlOp.GTE, GTE},
        {RqlOp.LIKE, LIKE},
        {RqlOp.OR, OR},
        {RqlOp.AND, AND},
        {RqlOp.NIN, NIN},
        {RqlOp.IN, IN},
    };

    public static (SqlOp, bool) TryParse(RqlOp val)
    {
        if (val == null)
        {
            return (null, false);
        }
        SqlOp SqlOp;
        var didGet = _rqlOps.TryGetValue(val, out SqlOp);

        return (SqlOp, didGet);

    }

    public static (SqlOp, bool) TryParse(string val)
    {
        if (val == null || val.Trim().Length < 0)
        {
            return (null, false);
        }
        val = val.ToLower();
        SqlOp SqlOp;
        var didGet = _SqlOps.TryGetValue(val, out SqlOp);

        return (SqlOp, didGet);

    }

    public override String ToString()
    {
        return value;
    }
}


public class RqlOp
{
    private readonly string value;
    public static readonly RqlOp EQ = new RqlOp("$eq");

    public static readonly RqlOp NEQ = new RqlOp("$neq");

    public static readonly RqlOp LT = new RqlOp("$lt");

    public static readonly RqlOp GT = new RqlOp("$gt");

    public static readonly RqlOp LTE = new RqlOp("$lte");

    public static readonly RqlOp GTE = new RqlOp("$gte");

    public static readonly RqlOp LIKE = new RqlOp("$like");

    public static readonly RqlOp OR = new RqlOp("$or");

    public static readonly RqlOp AND = new RqlOp("$and");

    public static readonly RqlOp NIN = new RqlOp("$nin");

    public static readonly RqlOp IN = new RqlOp("$in");

    // public static readonly RqlOp NOT = new RqlOp("$not");
    public static implicit operator string(RqlOp x)
    {
        return x.ToString();
    }

    private RqlOp(String value)
    {
        this.value = value;
    }

    private static readonly Dictionary<string, RqlOp> _rqlOps = new Dictionary<string, RqlOp>
    {
        {EQ.value, EQ},
        {NEQ.value, NEQ},
        {LT.value, LT},
        {GT.value, GT},
        {LTE.value, LTE},
        {GTE.value, GTE},
        {LIKE.value, LIKE},
        {OR.value, OR},
        {AND.value, AND},
        {NIN.value, NIN},
        {IN.value, IN},
    };

    public static (RqlOp, bool) TryParse(string val)
    {
        if (val == null || val.Trim().Length < 0)
        {
            return (null, false);
        }
        val = val.ToLower();
        RqlOp rqlOp;
        var didGet = _rqlOps.TryGetValue(val, out rqlOp);

        return (rqlOp, didGet);

    }

    public override String ToString()
    {
        return value;
    }
}

public class FieldSpec
{
    public FieldSpec(IEnumerable<Field> fields)
    {
        _fields = fields.ToDictionary(f => f.Name, f => f);
    }
    private Dictionary<string, Field> _fields = new Dictionary<string, Field>();
    public (Field, bool) GetField(string name)
    {
        if (name == null || name.Trim().Length <= 0)
        {
            return (null, false);
        }

        name = name.ToLower();
        Field field;
        var didGet = _fields.TryGetValue(name, out field);

        return (field, didGet);
    }
}
public class Field
{
    public string Name { get; set; }

}

public interface IError
{
    string GetMessage();
}
public class Error : IError
{
    private string _msg;
    public Error(string msg)
    {
        _msg = msg;
    }
    public string GetMessage()
    {
        return _msg;
    }
}
// TODO: hard typed errors/exceptions
// TODO: pull params to dict 
// TODO: dapper + sql raw examples 
// TODO: pull out op mapper func from rql to sql to allow mongo or other.
// TODO: column mapping 
// TODO: limit offset sort 
// TODO: feature throw vs return err
// TODO: not and nor 
// TODO: gentle validation flag ie if invalid op or field is entered drop and move 
// TODO: flat nested object
// TODO: benchmark on top of json deserialization 
// TODO: example integrations: dapper, dapper.crud/extensions(limit+ofset), sql mapper
// TODO: package(core,.sql) + build 
// TODO: try with DI framework, mvc/web-api + multi target
// TODO: allow token prefix config 
// TODO: tokenizer ? vs @param1
// TODO: pull out where and return {limit,offset,sort,expression, args}
// TODO: eventually try and pull out json deserializer
// TODO: refactor error messages to provide parent token details
// TODO: eventually add nested query support
// TODO: js + typescript lib
// TODO: convert enum classes to const
// TODO: investigate json ops
public class Parser
{
    public Parser(FieldSpec fieldspec)
    {
        _fieldspec = fieldspec;
    }
    private readonly FieldSpec _fieldspec;
    public (StringBuilder, IError) ParseTerms(JContainer termContainer, StringBuilder query = null, Field parentField = null)
    {
        if (termContainer == null) return (null, new Error("null container"));
        if (query == null) query = new StringBuilder("WHERE");

        var propCount = 0;
        foreach (var token in termContainer.Children())
        {
            var prop = token as JProperty;
            if (propCount > 0 && propCount < termContainer.Count) query.Append($" {SqlOp.AND} ");
            propCount++;

            var (field, isField) = _fieldspec.GetField(prop.Name);
            var nextTerm = prop.Value as JContainer;

            // Parse Field 
            if (isField)
            {
                // Right side of field is term, recursive call 
                if (nextTerm != null)
                {
                    ParseTerms(nextTerm, query, field);
                }
                // Right side is primitive and this is a root return 
                else
                {
                    query.Append($" {prop.Name} ");
                    var val = prop.Value as JValue;
                    if (val == null) return (null, new Error("cannot cast to primitive, invalid value"));

                    // TODO: use field spec to validate, format/convert value
                    // TODO: add value to parameters map and add ? or @ token 
                    query.Append($" {SqlOp.EQ} ");
                    query.Append($" {val.ToString()} ");
                }

                continue;
            }

            // Parse rql op
            var (op, isOp) = RqlOp.TryParse(prop.Name);
            if (isOp)
            {
                var (sqlOp, isSqlOp) = SqlOp.TryParse(op);
                if (!isSqlOp) return (null, new Error("rql operation not supported in sql"));

                // Right side of op is collection of terms recursive call
                if (op == RqlOp.OR || op == RqlOp.AND)
                {
                    query.Append("(");
                    if (nextTerm == null) return (null, new Error("expected collection of terms on right"));
                    var jArr = prop?.Values()?.ToArray();
                    for (var idx = 0; idx < jArr.Length; idx++)
                    {
                        if (idx > 0 && idx != jArr.Length) query.Append($" {sqlOp} ");
                        if (jArr.Length > 0) query.Append("(");

                        var childTerm = jArr[idx] as JContainer;
                        ParseTerms(childTerm, query);

                        if (jArr.Length > 0) query.Append(")");
                    }
                    query.Append(")");
                }
                // Right side is primitive collection and this is root return 
                else if (op == RqlOp.IN || op == RqlOp.NIN)
                {
                    throw new NotImplementedException("IN + NIN not implemented");
                }
                // Right side is primitive and this is root return 
                else
                {
                    if (parentField == null) return (null, new Error("missing expected field spec"));
                    query.Append($" {parentField.Name} ");
                    query.Append($" {sqlOp} ");
                    var val = prop.Value as JValue;
                    if (val == null) return (null, new Error("cannot cast to primitive, invalid value"));

                    // TODO: use field spec to validate, format/convert value
                    // TODO: add value to parameters map and add ? or @ token 
                    query.Append($" {SqlOp.EQ} ");
                    query.Append($" {val.ToString()} ");
                }
                continue;
            }

            return (null, new Error("not valid rql operation or model field name, invalid property"));
        }


        return (query, null);
    }

    public (string, Dictionary<string, object>, IError) ParseQuery(string query)
    {
        try
        {
            var jsonObject = JsonConvert.DeserializeObject(query);

            var (queryBuilder, err) = ParseTerms(jsonObject as JContainer);
            return (queryBuilder.ToString(), null, err);
        }
        catch (Exception e)
        {
            return (null, null, new Error(e.ToString())); // CAUGHT exception likely from json
        }
    }
}
