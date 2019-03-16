using System;
using System.Collections;
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
            var spec = new ClassSpec(typeof(TestClass));
            var spec2 = new ClassSpec<TestClass>();

            // var p = new Parser(spec);

            // var (str, parms, error) = p.ParseQuery(raw);
            // Console.WriteLine(str, parms, error);

            // "WHERE city  =  TLV  AND (( city  =  TLV ) OR ( city  =  NYC )) AND  account  LIKE  =   % github %  "
        }
        [Fact]
        public void Test1()
        {
            var raw = @"{ 
                        ""city5"":3,
                        ""city1"": 2.0, 
                        ""city2"": false, 
                        ""city3"": ""true"", 
                        ""city4"": ""1.0"", 
                        ""city"": ""TLV"", 
                        ""$or"": [
                                { ""city"": ""TLV"" },
                                { ""city"": ""NYC"" } 
                			],
                        ""account"": { ""$like"": "" % github % "" }
                		}
                  ";

            var fields = new List<FieldSpec> {
                new FieldSpec{
                    Name= "city",
                },
                    new FieldSpec{
                    Name= "city1",
                },
                    new FieldSpec{
                    Name= "city2",
                },
                    new FieldSpec{
                    Name= "city3",
                },
                    new FieldSpec{
                    Name= "city4",
                },
                new FieldSpec{
                    Name= "city5",
                },
                new FieldSpec{
                    Name = "account",
                },
            };
            var spec = new ClassSpec(fields);
            var p = new Parser(spec);

            var (str, parms, error) = p.ParseQuery(raw);
            Console.WriteLine(str, parms, error);

            // "WHERE city  =  TLV  AND (( city  =  TLV ) OR ( city  =  NYC )) AND  account  LIKE  =   % github %  "
        }

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

public class ClassSpecBuilder
{
    public ClassSpec Build(Type t)
    {
        var _fields = new Dictionary<string, FieldSpec>() { };

        var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        // if (property.GetCustomAttributes(true)
        // .Any(attr => attr.GetType().Name == typeof(IgnoreSelectAttribute).Name || attr.GetType().Name == typeof(NotMappedAttribute).Name)) ;
        // continue;

        //  _fields = fields.ToDictionary(f => f.Name, f => f);
        return new ClassSpec();
    }
}
public class ClassSpecResolver
{

}

public class ClassSpec
{
    public Dictionary<string, FieldSpec> Fields { get; set; }
    public Func<string, Type, object, IError> Validator { get; set; }
    public Func<string, Type, object, (object, IError)> Converter { get; set; }
    public Func<string, string> ColumnNamer { get; set; }
}

public class FieldSpec
{
    public string Name { get; set; }
    public string ColumnName { get; set; }
    public IEnumerable<string> Ops { get; set; }
    public Type PropType { get; set; }
    public Func<string, Type, object, IError> Validator { get; set; }
    public Func<string, Type, object, (object, IError)> Converter { get; set; }
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
// TODO: deal with tokenizer state and parse() should be static. 
// TODO: feature - class level default validator, converter 
// TODO: class spec should contain all relevant information required to parse a given class the generation of such class should be extracted, I think this is the cache point, can pass in for one off. 


public class ParseResult
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public string SortExpression { get; set; }
    public string FilterExpression { get; set; }
    public Dictionary<string, object> FilterParameters { get; set; }
    public IEnumerable<IErrors> Errors { get; set; }
}


public class ResultState
{
    public Dictionary<string, object> FilterParameters { get; set; }
    public List<IErrors> Errors { get; set; }
    public StringBuilder Query { get; set; }

}

public class Parser
{
    public Func<IParameterTokenizer> TokenizerFactory { get; set; }
    public ClassSpec ClassSpec { get; set; }
    public Func<string, (string, bool)> OpResolver { get; set; }

    private static ParseResult ParseTerms(
        Parser parser,
        JContainer container,
        string parentToken = null,
        IParameterTokenizer tokenizer = null,
        ResultState res = null
    )
    {
        if (res == null) { res = new ResultState() { }; }
        if (tokenizer == null) { parser.TokenizerFactory(); }

        var fieldCount = 0;
        foreach (var raw in container.Children())
        {
            var token = raw as JProperty;
            // TODO:  if (fieldCount > 0 && fieldCount < termContainer.Count) query.Append($" {SqlOp.AND} ");
            fieldCount++;

            var field = parser.ClassSpec.Fields[token.Name.Trim()];
            var nextTerm = token.Value as JContainer;

            // Parse Field 
            if (field != null)
            {
                // Right side of field is term, recursive call 
                if (nextTerm != null)
                {
                    ParseTerms(parser, nextTerm, token.Name.Trim(), tokenizer, res);
                }
                // Right side is primitive and this is a return statement
                else
                {
                    var columnName = field.ColumnName ?? parser.ClassSpec.ColumnNamer(field.Name);
                    res.Query.Append($" {columnName} ");

                    var jValue = token.Value as JValue;
                    if (jValue == null)
                    {
                        res.Errors.Add(new Error("Unable to cast right side as primitive"));
                        continue;
                    }

                    var (key, val, err) = getParameter(field, jValue.Value, tokenizer, parser.ClassSpec.Converter, parser.ClassSpec.Validator);
                    if (err != null)
                    {
                        res.Errors.Add(err);
                        continue;
                    }
                    res.FilterParameters.Add(key, val);

                    res.Query.Append($" {parser.OpResolver(RqlOp.EQ)} ");
                    res.Query.Append($" {key} ");
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
                    var paramToken = "";

                    // query.Append($" {SqlOp.EQ} "); // TODO: don't always add 
                    query.Append($" {paramToken} ");
                }
                continue;
            }

            return (null, new Error("not valid rql operation or model field name, invalid property"));
        }


        return (query, null);
    }




    private static (string, object, IError) getParameter(
        FieldSpec field,
        object val,
        IParameterTokenizer _tokenizer,
        Func<string, Type, object, (object, IError)> _defaultConverter = null,
        Func<string, Type, object, IError> _defaultValidator = null
    )
    {
        var converter = field.Converter ?? _defaultConverter;

        if (converter != null)
        {
            var (v, err) = converter(field.Name, field.PropType, val);
            if (err != null) return (null, null, err);
            val = v;
        }

        var validator = field.Validator ?? _defaultValidator;
        if (validator != null)
        {
            var err = validator(field.Name, field.PropType, val);
            if (err != null) return (null, null, err);
        }

        var parameterName = _tokenizer.GetToken(field.Name, field.PropType);

        return (parameterName, val, null);
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
