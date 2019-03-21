using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/* TODO
    gentle validation flag ie if invalid op or field is entered drop and move 
    benchmark on top of json deserialization 
    example integrations: dapper, dapper.crud/extensions(limit+ofset), sql mapper
    package(core,.sql) + build 
    try with DI framework, mvc/web-api + multi target
    pull out json deserializer and use own tree {left, v, right, isField, isOp...}
    js + typescript lib
    investigate json ops
    validate and right side is object and, or/nor is array
 */


namespace Rql.NET
{
    public class DbExpression
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Sort { get; set; }
        public string Filter { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public int GetPage()
        {
            if (Offset <= 0 || Offset < Limit) return 1;
            return (int)Math.Floor((double)Offset / (double)Limit) + 1;
        }
    }

    public class RqlExpression
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public List<string> Sort { get; set; }
        public Dictionary<string, object> Filter { get; set; }
        public int GetPage()
        {
            if (Offset <= 0 || Offset < Limit) return 1;
            return (int)Math.Floor((double)Offset / (double)Limit) + 1;
        }
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

    public class Parser<T> : RqlParser, IRqlParser<T>
    {
        public Parser(
            Func<string, string> opResolver = null,
            Func<IParameterTokenizer> tokenizerFactory = null
        )
        : base(
            new ClassSpecBuilder().Build(typeof(T)),
            opResolver,
            tokenizerFactory
            )
        {
        }
    }

    public interface IRqlParser<T> : IRqlParser
    {

    }

    public interface IRqlParser
    {
        (DbExpression, IEnumerable<IError>) Parse(string toParse);
    }

    public class RqlParser : IRqlParser
    {
        private readonly Func<IParameterTokenizer> _tokenizerFactory;
        private readonly ClassSpec _classSpec;
        private readonly Func<string, string> _opResolver;

        public RqlParser(ClassSpec classSpec, Func<string, string> opResolver = null, Func<IParameterTokenizer> tokenizerFactory = null)
        {
            if (tokenizerFactory == null)
            {
                tokenizerFactory = Defaults.DefaultTokenizerFactory;
            }
            if (opResolver == null)
            {
                opResolver = Defaults.DefaultOpMapper.GetDbOp;
            }
            _tokenizerFactory = tokenizerFactory;
            _classSpec = classSpec;
            _opResolver = opResolver;
        }


        public static (DbExpression, IEnumerable<IError>) Parse<T>(string toParse)
        {
            var parser = new Parser<T>();
            return parser.Parse(toParse);
        }

        public (DbExpression, IEnumerable<IError>) Parse(RqlExpression rqlExpression)
        {
            var raw = JsonConvert.SerializeObject(rqlExpression);
            // TODO: convert json to proper Dictionary<string,object> and remove hack
            return Parse(raw);
        }

        public (DbExpression, IEnumerable<IError>) Parse(string toParse)
        {
            // try
            // {
            var jsonObject = JsonConvert.DeserializeObject(toParse);

            var json = jsonObject as JContainer;

            var offset = json["Offset"]?.Value<Int32>() ?? json["offset"]?.Value<Int32>() ?? Defaults.Offset;
            var limit = json["Limit"]?.Value<Int32>() ?? json["limit"]?.Value<Int32>() ?? Defaults.Limit;
            var sort = json["Sort"] as JToken ?? json["sort"] as JToken;
            var (sortExp, errs) = parseSort(sort, _opResolver, _classSpec);

            var filterRaw = json["Filter"] as JToken ?? json["filter"] as JToken;
            var (filter, parameters, errors) = ParseTerms(this, filterRaw as JContainer, RqlOp.AND);
            if (errors != null) errs.AddRange(errors);
            return (
                new DbExpression
                {
                    Filter = filter,
                    Parameters = parameters,
                    Offset = offset,
                    Limit = limit,
                    Sort = string.Join(Defaults.SortSeperator, sortExp).Trim(),
                },
                errs.Any() ? errs : null
            );
            // }
            // catch (Exception e)
            // {
            //     throw e; // TODO: 
            // }
        }

        private static (List<string>, List<IError>) parseSort(JToken sort, Func<string, string> _opResolver, ClassSpec _classSpec)
        {
            var sortArry = sort?.ToList();
            var sortOut = new List<string>() { };
            var errs = new List<IError>() { };
            if (sortArry == null) return (sortOut, errs);

            foreach (var s in sortArry)
            {
                var sortStr = s.Value<string>();
                var sortDir = "+";
                if (sortStr.StartsWith("+") || sortStr.StartsWith("-"))
                {
                    sortDir = sortStr[0].ToString();
                    sortStr = sortStr.Substring(1);
                }
                var sqlSort = _opResolver(sortDir);
                var val = s.Value<string>();
                var fieldSpec = _classSpec.Fields.ContainsKey(val) ? _classSpec.Fields[val] : null;
                if (fieldSpec == null || !fieldSpec.IsSortable)
                {
                    errs.Add(new Error("not allowed to sort []"));
                    continue;
                }
                sortOut.Add($"{fieldSpec.ColumnName} {sqlSort} ");
            }
            return (sortOut, errs);
        }

        private static (string, Dictionary<string, object> FilterParameters, IEnumerable<IError>) ParseTerms(
            RqlParser parser,
            JContainer container,
            string parentToken,
            ParseState state = null
        )
        {
            if (container == null) { } // invalid rigt side or json serialiize
            if (state == null) { state = new ParseState(parser._tokenizerFactory()); };
            // { ""t_String"": { ""$like"":"" % testing % "", ""$neq"" : ""test""} },                                

            var idx = -1;
            foreach (var raw in container.Children())
            {
                idx++;
                var token = raw as JProperty ?? raw.First() as JProperty;
                var leftSide = token?.Name;

                if (idx > 0 && idx < container.Count && RqlOp.IsOp(parentToken))
                {
                    if (parentToken == RqlOp.OR || parentToken == RqlOp.AND) state._query.Append($"{parser._opResolver(parentToken)} ");
                    else if (parentToken == RqlOp.NOT) state._query.Append($"{parser._opResolver(RqlOp.AND)} ");
                    else if (parentToken == RqlOp.NOR) state._query.Append($"{parser._opResolver(RqlOp.OR)} ");
                }
                else if (idx > 0 && idx < container.Count)
                {
                    // TODO: merge with upper if 
                    // TODO: fix so that [] are ORed and {} are objects 
                    // TODO: validate $or:[] and $and:{}
                    // container.Type == Object vs container.Type == JArray 
                    state._query.Append($"{parser._opResolver(RqlOp.AND)} ");
                }

                var field = parser._classSpec.Fields.ContainsKey(leftSide) ? parser._classSpec.Fields[leftSide] : null;
                var nextTerm = token.Value as JContainer;

                // Parse Field value is recursive 
                if (field != null && nextTerm != null)
                {
                    ParseTerms(parser, nextTerm, leftSide, state);
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
                    if (container.Count > 0) state._query.Append($"( ");
                    // state._query.Append($"{leftSide} (");
                    ParseTerms(parser, nextTerm, leftSide, state);
                    if (container.Count > 0) state._query.Append($") ");
                }
                // Right side is primitive and and field is left side or parent 
                else if (RqlOp.IsOp(leftSide) || field != null)
                {
                    var parentField = parser._classSpec.Fields.ContainsKey(parentToken) ? parser._classSpec.Fields[parentToken] : null;
                    var op = RqlOp.IsOp(leftSide) ? leftSide : RqlOp.EQ;
                    resolveNode(parser, field ?? parentField, op, token, state);
                }
                else
                {
                    state._errors.Add(new Error($"invalid field or op {leftSide}, parent:{parentToken}"));
                }
            }

            return (
                state._query.ToString().Trim(),
                state._filterParameters,
                state._errors
            );
        }

        private static void resolveNode(
            RqlParser parser,
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
            state._query.Append($"{fieldSpec.ColumnName} {sqlOp} {key} ");
        }

        private static object prepPrim(JProperty jToken, bool expectArray = false)
        {
            if (expectArray)
            {
                var jArray = jToken.Value as JArray;
                var res = jArray.Select(x => x as JValue).Select(x => x.Value).ToList();
                return res;
            }

            var jProp = jToken.Value as JProperty;
            var jValue = (jProp?.Value ?? jToken?.Value) as JValue;

            return jValue.Value;
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

}