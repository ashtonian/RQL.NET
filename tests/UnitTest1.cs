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
                        ""$or"": [
                                { ""city"": ""TLV"" },
                                { ""city"": ""NYC"" } 
                			],
                        ""account"": { ""$like"": "" % github % "" }
                		}
                  ";

            var p = new Parser();

            var (str, parms, error) = p.ParseQuery(raw);
            Console.WriteLine(str, parms, error);
        }

    }
}

/* Attributes
    Ignore.* 
    Ignore.Sort
    Ignore.Filter
    CustomTypeConverter
    CustomValidator
    Custom/Limit operations, only want EQ not GTE for example..  
*/

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

}
public class Error : IError
{

}
public class Parser
{
    private readonly FieldSpec _fieldspec;
    // TODO: pull params to dict 
    // TODO : dapper + sql raw examples 
    public (StringBuilder, IError) ParseTerms(JContainer termContainer, StringBuilder query = null)
    {
        if (termContainer == null) return (null, new Error()); //TODO: null container 
        if (query == null) query = new StringBuilder("WHERE");

        var propCount = 0;
        foreach (var token in termContainer.Children())
        {
            var prop = token as JProperty;
            if (propCount > 0 && propCount < termContainer.Count) query.Append(SqlOp.AND);
            propCount++;

            var (op, isOp) = RqlOp.TryParse(prop.Name);
            var (sqlOp, isSqlOp) = SqlOp.TryParse(op);  // TODO: sql instruction abstraction 
            if (isOp && !isSqlOp) return (null, new Error()); //TODO: unsupported sql op 

            // TODO: clean up if tree and continues 
            if (isOp && op == RqlOp.OR || op == RqlOp.AND)
            {
                // TODO: can handling of () + and/or appending be done at root via flag? 
                var jArr = prop?.Values()?.ToArray();
                for (var idx = 0; idx < jArr.Length; idx++)
                {
                    if (idx > 0 && idx != jArr.Length) query.Append(sqlOp);
                    if (jArr.Length > 0) query.Append("(");

                    var childTerm = jArr[idx] as JContainer;
                    ParseTerms(childTerm, query); // TODO: pass field spec for validation

                    if (jArr.Length > 0) query.Append(")");
                }
                continue;
            }
            if (isOp)
            {
                // right is value 
                query.Append(sqlOp);
                // TODO: hard type from field spec + error out 
                var val = prop.Value as JValue;
                query.Append(val.ToString());

            }
            // TODO: var (field, isField) = _fieldspec.GetField(prop.Path);
            var isField = true; // TODO: generate spec
            if (!isOp && !isField) return (null, new Error()); // not field or op bad prop 
            if (isField)
            {
                query.Append(prop.Name);
                var val = prop.Value as JValue;
                // TODO: hard type from field spec + error out 
                if (val != null)
                {
                    query.Append(SqlOp.EQ);
                    query.Append(val.ToString());
                    continue;
                }

                var container = prop.Value as JContainer;
                if (container != null)
                {
                    ParseTerms(container, query); // TODO: pass field spec for validation
                    continue;
                }
                // invalid field? 
            }
        }

        return (query, null);
    }
    public (string, Dictionary<string, object>, IError) ParseQuery(string query)
    {
        try
        {
            var jsonObject = JsonConvert.DeserializeObject(query) as JContainer;

            var (queryBuilder, err) = ParseTerms(jsonObject);
            return (queryBuilder.ToString(), null, err);
        }
        catch (Exception e)
        {
            return (null, null, new Error()); // CAUGHT exception likely from json
        }

        /*
        JToken             - abstract base class     
        JContainer      - abstract base class of JTokens that can contain other JTokens
            JArray      - represents a JSON array (contains an ordered list of JTokens)
            JObject     - represents a JSON object (contains a collection of JProperties)
            JProperty   - represents a JSON property (a name/JToken pair inside a JObject)
        JValue          - represents a primitive JSON value (string, number, boolean, null)
        */
    }
}
