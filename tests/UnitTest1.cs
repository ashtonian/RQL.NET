using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestFieldSpec()
        {


            var spec = new FieldSpec(typeof(TestClass));
            var spec2 = new FieldSpec<TestClass>();

            // var p = new Parser(spec);

            // var (str, parms, error) = p.ParseQuery(raw);
            // Console.WriteLine(str, parms, error);

            // "WHERE city  =  TLV  AND (( city  =  TLV ) OR ( city  =  NYC )) AND  account  LIKE  =   % github %  "
        }
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
            // var spec = new FieldSpec(fields);
            // var p = new Parser(spec);

            // var (str, parms, error) = p.ParseQuery(raw);
            // Console.WriteLine(str, parms, error);

            // "WHERE city  =  TLV  AND (( city  =  TLV ) OR ( city  =  NYC )) AND  account  LIKE  =   % github %  "
        }

    }
}

// TODO:
/*  Attributes
    CustomTypeConverter
    CustomValidator
*/

[AttributeUsage(AttributeTargets.Class)]
public class Filterable : System.Attribute
{
    private string[] _props;
    public Filterable(params string[] props) { _props = props; }
}

[AttributeUsage(AttributeTargets.Class)]
public class Sortable : System.Attribute
{
    private string[] _props;
    public Sortable(params string[] props) { _props = props; }
}

[AttributeUsage(AttributeTargets.Property)]
public class Ignore : System.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Sort : System.Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class Filter : System.Attribute { }
}
public class Ops
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Allowed : System.Attribute
    {
        private string[] _ops;
        public Allowed(params string[] ops) { _ops = ops; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Disallowed : System.Attribute
    {
        private string[] _ops;
        public Disallowed(params string[] ops) { _ops = ops; }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ColumnName : System.Attribute
{
    private string _name;
    public ColumnName(string name) { _name = name; }
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

public interface IOpMapper
{
    (string, bool) GetDbOp(string rqlOp);
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
    };
    (string, bool) IOpMapper.GetDbOp(string rqlOp)
    {
        if (rqlOp == null || rqlOp.Trim().Length < 0) return (null, false);

        string result;
        var didFind = _ops.TryGetValue(rqlOp, out result);
        return (result, didFind);
    }
}

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
    public const string AND = "$and";
    public const string NIN = "$nin";
    public const string IN = "$in";
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
    };

    public static bool IsOp(string val)
    {
        if (val == null || val.Trim().Length < 0) return false;

        return _rqlOps.Contains(val);
    }
}

public class TestClass2
{
    public int X_Int { get; set; }
    public string X_String { get; set; }
    public bool X_Bool { get; set; }
    public DateTime X_DateTime { get; set; }
}

public class TestClass
{
    public int T_Int { get; set; }
    public string T_String { get; set; }
    public bool T_Bool { get; set; }
    public DateTime T_DateTime { get; set; }
    public TestClass2 T_SubClass { get; set; }
}

public class FieldSpec<T> : FieldSpec
{
    public FieldSpec() : base(typeof(T)) { }
}
public class FieldSpec
{
    private FieldSpec() { }
    public FieldSpec(Type t)
    {
        var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        // if (property.GetCustomAttributes(true)
        // .Any(attr => attr.GetType().Name == typeof(IgnoreSelectAttribute).Name || attr.GetType().Name == typeof(NotMappedAttribute).Name)) ;
        // continue;

        //  _fields = fields.ToDictionary(f => f.Name, f => f);
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
    // Expected type
    // custom converter
    // Validator ? 
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

// TODO: cache reflection 
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
// TODO: dynamic spec so that it doesn't have to be global for the same struct, sometimes might not want that.
// TODO: c# 7.0 is expressions https://www.danielcrabtree.com/blog/152/c-sharp-7-is-operator-patterns-you-wont-need-as-as-often
public class Parser
{
    private readonly IOpMapper _opMapper;
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
            var isOp = RqlOp.IsOp(prop.Name);
            if (isOp)
            {
                var op = prop.Name;
                var (sqlOp, isSqlOp) = _opMapper.GetDbOp(op);
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
