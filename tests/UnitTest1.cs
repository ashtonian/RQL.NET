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
    public class TestClass2
    {
        public int X_Int { get; set; }
        public string X_String { get; set; }
        public bool X_Bool { get; set; }
        public DateTime X_DateTime { get; set; }
    }

    public class TestClass
    {
        public long T_Long { get; set; }
        public int T_Int { get; set; }
        public string T_String { get; set; }
        public bool T_Bool { get; set; }
        public DateTime T_DateTime { get; set; }
        // public TestClass2 T_SubClass { get; set; }
    }

    public class UnitTest1
    {
        [Fact]
        public void ClassSpecBuilder()
        {

            var specBuilder = new ClassSpecBuilder();
            var classSpec = specBuilder.Build(typeof(TestClass));
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
            // var spec = new ClassSpec(fields);
            // var p = new Parser(spec);

            // var (str, parms, error) = p.ParseQuery(raw);
            // Console.WriteLine(str, parms, error);

            // "WHERE city  =  TLV  AND (( city  =  TLV ) OR ( city  =  NYC )) AND  account  LIKE  =   % github %  "
        }

    }
}

public interface IOpMapper
{
    (string, bool) GetDbOp(string rqlOp);
    HashSet<String> GetSupportedOps(Type type);
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

    public HashSet<string> GetSupportedOps(Type type)
    {
        throw new NotImplementedException();
    }

    (string, bool) IOpMapper.GetDbOp(string rqlOp)
    {
        if (rqlOp == null || rqlOp.Trim().Length < 0) return (null, false);

        string result;
        var didFind = _ops.TryGetValue(rqlOp, out result);
        return (result, didFind);
    }
}



public class ClassSpecBuilder
{
    private readonly Func<string, string> columnNamer;
    private readonly Func<string, string> fieldNamer;
    private readonly IOpMapper opMapper;

    public ClassSpec Build(Type t)
    {
        var _fields = new Dictionary<string, FieldSpec>() { };

        var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var classAttributes = t.GetCustomAttributes(true);
        var classFilter = classAttributes?.OfType<Filterable>()?.First();
        var classSortable = classAttributes?.OfType<Filterable>()?.First();

        // if is class use name resolver and concat fields to class TODO:

        // class attribute validator 
        // class attribute converter

        foreach (var p in properties)
        {
            var attributes = p.GetCustomAttributes(true);

            var ignore = attributes?.OfType<Ignore>()?.First();
            var ignoreSort = attributes?.OfType<Ignore.Sort>()?.First();
            var ignoreFilter = attributes?.OfType<Ignore.Filter>()?.First();

            var opsBlacklist = attributes?.OfType<Ops.Disallowed>()?.First();
            var columnName = attributes?.OfType<ColumnName>()?.First();
            var fieldName = attributes?.OfType<FieldName>()?.First();

            var _field = new FieldSpec
            {
                Ops = opMapper.GetSupportedOps(p.PropertyType),
                PropType = p.PropertyType,
                ColumnName = columnName?._name ?? columnNamer(p.Name),
                Name = fieldName?._name ?? fieldNamer(p.Name),
                Converter = null,               // ?? attribute that looks for IConverter on field, then class
                Validator = null,               // ?? attribute that looks for IValidator on field, then class
            };

        }

        return new ClassSpec()
        {
            Fields = _fields,
            Converter = null, // ?? staticRef default converter
            Validator = null, // ?? staticRef
        };
    }
}

public class ClassSpec
{
    public Dictionary<string, FieldSpec> Fields { get; set; }
    public Func<string, Type, object, IError> Validator { get; set; }
    public Func<string, Type, object, (object, IError)> Converter { get; set; }
}

public class FieldSpec
{
    public string Name { get; set; }
    public string ColumnName { get; set; }
    public HashSet<string> Ops { get; set; }
    public Type PropType { get; set; }
    public Func<string, Type, object, IError> Validator { get; set; }
    public Func<string, Type, object, (object, IError)> Converter { get; set; }
    // public ClassSpec _classSpec; // TODO: use for flat object a sub queries 
}


// TODO: cache reflection 
// TODO: gentle validation flag ie if invalid op or field is entered drop and move 
// TODO: benchmark on top of json deserialization 
// TODO: example integrations: dapper, dapper.crud/extensions(limit+ofset), sql mapper
// TODO: package(core,.sql) + build 
// TODO: try with DI framework, mvc/web-api + multi target
// TODO: pull out json deserializer and use own tree {left, v, right, isField, isOp}
// TODO: js + typescript lib
// TODO: investigate json ops
// TODO: validate not + and right side is object and, or/nor is array

public class ParseResult
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public string SortExpression { get; set; }
    public string FilterExpression { get; set; }
    public Dictionary<string, object> FilterParameters { get; set; }
}

internal class ParseState
{
    public ParseState(IParameterTokenizer parameterTokenizer = null)
    {
        _parameterTokenizer = parameterTokenizer;
    }
    public readonly StringBuilder _query = new StringBuilder();
    public readonly Dictionary<string, object> _filterParameters = new Dictionary<string, object>();
    public readonly List<IError> _errors = new List<IError>();
    public readonly IParameterTokenizer _parameterTokenizer;
}

public class Parser<T> : Parser
{
    // pass static class calls in constructor
    public Parser() : base(null, null, null) { }
}
public class Parser
{
    private readonly Func<IParameterTokenizer> _tokenizerFactory;
    private readonly ClassSpec _classSpec;
    private readonly Func<string, string> _opResolver;

    public Parser(ClassSpec classSpec, Func<string, string> opResolver, Func<IParameterTokenizer> tokenizerFactory)
    {
        _tokenizerFactory = tokenizerFactory;
        _classSpec = classSpec;
        _opResolver = opResolver;
    }

    public (ParseResult, IEnumerable<IError>) Parse(string toParse)
    {
        try
        {
            var jsonObject = JsonConvert.DeserializeObject(toParse);

            return ParseTerms(this, jsonObject as JContainer, RqlOp.AND); // TODO: or if array
        }
        catch (Exception e)
        {
            throw e; // TODO: 
        }
    }

    private static (ParseResult, IEnumerable<IError>) ParseTerms(
        Parser parser,
        JContainer container,
        string parentToken,
        ParseState state = null
    )
    {
        if (container == null) { } // invalid rigt side or json serialiize
        if (state == null) { state = new ParseState(parser._tokenizerFactory()); };


        for (var idx = 0; idx < container.Count; idx++)
        {
            var raw = container[idx];
            var token = raw as JProperty;
            var leftSide = token.Name;

            if (idx > 0 && idx < container.Count - 1 && RqlOp.IsOp(parentToken))
            {
                if (parentToken == RqlOp.OR || parentToken == RqlOp.AND) state._query.Append($" {parser._opResolver(parentToken)} ");
                if (parentToken == RqlOp.NOT) state._query.Append($" {parser._opResolver(RqlOp.AND)} ");
                if (parentToken == RqlOp.NOR) state._query.Append($" {parser._opResolver(RqlOp.OR)} ");
            }

            var field = parser._classSpec.Fields[leftSide];
            var nextTerm = token.Value as JContainer;

            // Parse Field value is recursive 
            if (field != null && nextTerm != null)
            {
                ParseTerms(parser, nextTerm, leftSide);
            }
            // Parse recursive op
            else if (
                RqlOp.IsOp(leftSide)
                && leftSide == RqlOp.OR
                || leftSide == RqlOp.AND
                || leftSide == RqlOp.NOT
                || leftSide == RqlOp.NOR
            )
            {
                if (container.Count > 0) state._query.Append($" {leftSide} (");
                ParseTerms(parser, nextTerm, leftSide);
                if (container.Count > 0) state._query.Append($")");
            }
            // Right side is primitive and and field is left side or parent 
            else if (RqlOp.IsOp(leftSide) || field != null)
            {
                var parentField = parser._classSpec.Fields[parentToken];
                var jProp = token.Value as JProperty;
                resolveNode(parser, field ?? parentField, leftSide, jProp, state);
            }
            else
            {
                state._errors.Add(new Error($"invalid field or op {leftSide}, parent:{parentToken}"));
            }
        }

        return (
            new ParseResult
            {
                FilterExpression = state._query.ToString(),
                FilterParameters = state._filterParameters,
            },
            state._errors
        );
    }

    private static void resolveNode(
        Parser parser,
        FieldSpec fieldSpec,
        string rqlOp,
        JProperty val,
        ParseState state
    )
    {
        var sqlOp = parser._opResolver(rqlOp);
        if (sqlOp == null) { state._errors.Add(new Error($"{rqlOp} is not supported.")); return; }

        if (!fieldSpec.Ops.Contains(rqlOp)) { state._errors.Add(new Error($"{fieldSpec.Name} does not support {rqlOp}.")); return; }

        var preVal = prepPrim(val, rqlOp == RqlOp.IN || rqlOp == RqlOp.NIN);
        var (key, processedVal, err) = getParameter(fieldSpec, preVal, state._parameterTokenizer, parser._classSpec.Converter, parser._classSpec.Validator);
        if (err != null) { state._errors.Add(err); return; }

        state._filterParameters.Add(key, processedVal);
        state._query.Append($" {fieldSpec.ColumnName} {sqlOp} {key} ");
    }

    private static object prepPrim(JProperty jProp, bool isArray = false)
    {
        // take jcontainer 
        // type to jvalue or jprop 
        if (isArray) throw new NotImplementedException();

        var jValue = jProp.Value as JValue;


        return jValue;
    }


    private static (string, object, IError) getParameter(
        FieldSpec field,
        object val,
        IParameterTokenizer _tokenizer,
        Func<string, Type, object, (object, IError)> _defaultConverter = null,
        Func<string, Type, object, IError> _defaultValidator = null
    )
    {
        if (val == null) return (null, null, new Error("could not parse right side as valid primitive"));
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
}
