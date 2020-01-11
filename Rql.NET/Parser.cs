using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rql.NET
{
    public class DbExpression
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Sort { get; set; }
        public string Filter { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class RqlExpression
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public List<string> Sort { get; set; }
        public Dictionary<string, object> Filter { get; set; }
    }

    internal class ParseState
    {
        internal readonly List<IError> Errors = new List<IError>();
        internal readonly Dictionary<string, object> FilterParameters = new Dictionary<string, object>();
        internal readonly IParameterTokenizer ParameterTokenizer;
        internal readonly StringBuilder Query = new StringBuilder();

        internal ParseState(IParameterTokenizer parameterTokenizer = null)
        {
            ParameterTokenizer = parameterTokenizer;
        }
    }

    public class RqlParser<T> : RqlParser, IRqlParser<T>
    {
        public RqlParser(
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
        (DbExpression, IEnumerable<IError>) Parse(RqlExpression rqlExpression);

    }

    public class RqlParser : IRqlParser
    {
        private readonly ClassSpec _classSpec;
        private readonly Func<string, string> _opResolver;
        private readonly Func<IParameterTokenizer> _tokenizerFactory;

        public RqlParser(ClassSpec classSpec, Func<string, string> opResolver = null,
            Func<IParameterTokenizer> tokenizerFactory = null)
        {
            if (tokenizerFactory == null) tokenizerFactory = Defaults.DefaultTokenizerFactory;
            if (opResolver == null) opResolver = Defaults.DefaultOpMapper.GetDbOp;
            _tokenizerFactory = tokenizerFactory;
            _classSpec = classSpec;
            _opResolver = opResolver;
        }

        public (DbExpression, IEnumerable<IError>) Parse(string toParse)
        {
            // try
            // {
            var jsonObject = JsonConvert.DeserializeObject(toParse);

            var json = jsonObject as JContainer;
            // TODO: handle errors better
            var offset = json["Offset"]?.Value<int>() ?? json["offset"]?.Value<int>() ?? Defaults.Offset;
            var limit = json["Limit"]?.Value<int>() ?? json["limit"]?.Value<int>() ?? Defaults.Limit;
            var sort = json["Sort"] ?? json["sort"];

            var (sortExp, errs) = sort == null ? ("", new List<IError>()) : ParseSort(sort, _opResolver, _classSpec);

            var filterRaw = json["Filter"] ?? json["filter"];
            var filterContainer = filterRaw as JContainer;

            if (filterContainer == null) return (null, new[] { new Error("missing required field 'filter'") });
            var (filter, parameters, errors) = ParseTerms(this, filterContainer, RqlOp.AND);

            if (errors != null) errs.AddRange(errors);
            return (
                new DbExpression
                {
                    Filter = filter,
                    Parameters = parameters,
                    Offset = offset,
                    Limit = limit,
                    Sort = sortExp,
                },
                errs.Any() ? errs : null
            );
            // }
            // catch (Exception e)
            // {
            //     throw e; // TODO:
            // }
        }


        public static (DbExpression, IEnumerable<IError>) Parse<T>(string toParse)
        {
            var parser = new RqlParser<T>();
            return parser.Parse(toParse);
        }

        public static (DbExpression, IEnumerable<IError>) Parse<T>(RqlExpression rqlExpression)
        {
            var parser = new RqlParser<T>();
            return parser.Parse(rqlExpression);
        }
        public (DbExpression, IEnumerable<IError>) Parse(RqlExpression rqlExpression)
        {
            var raw = JsonConvert.SerializeObject(rqlExpression);
            // TODO: convert json to proper Dictionary<string,object> and remove hack
            return Parse(raw);
        }

        private static (string, List<IError>) ParseSort(JToken sort, Func<string, string> opResolver,
            ClassSpec classSpec)
        {
            var sortArry = sort?.ToList();
            var sortOut = new List<string>();
            var errs = new List<IError>();
            if (sortArry == null) return ("", errs);

            foreach (var s in sortArry)
            {
                // get sort direction
                var sortDir = RqlOp.ASC;
                var sortStr = s.Value<string>();
                if (sortStr.StartsWith(RqlOp.ASC) || sortStr.StartsWith(RqlOp.DESC))
                {
                    sortDir = sortStr[0].ToString();
                    sortStr = sortStr.Substring(1);
                }

                // verifies field is sortable
                var sqlSort = opResolver(sortDir);
                var val = s.Value<string>();
                var fieldSpec = classSpec.Fields.ContainsKey(val) ? classSpec.Fields[val] : null;
                if (fieldSpec == null || !fieldSpec.IsSortable)
                {
                    errs.Add(new Error("not allowed to sort []"));
                    continue;
                }

                // adds sql sort statement
                sortOut.Add($"{fieldSpec.ColumnName} {sqlSort} ");
            }

            return (string.Join(Defaults.SortSeparator, sortOut).Trim(), errs);
        }

        private static (string, Dictionary<string, object> FilterParameters, IEnumerable<IError>) ParseTerms(
            RqlParser parser,
            JContainer container,
            string parentToken,
            ParseState state = null
        )
        {
            if (container == null)
            {
            }
            // invalid right side or json serialize

            if (state == null) state = new ParseState(parser._tokenizerFactory());

            var idx = 0;
            foreach (var raw in container.Children())
            {
                idx++;
                var token = raw as JProperty ?? raw.First() as JProperty;
                var leftSide = token?.Name;

                if (idx > 1 && idx < container.Count + 1 && RqlOp.IsOp(parentToken))
                {
                    // This adds "and","or" sql tokens to the query if they are needed
                    if (parentToken == RqlOp.OR || parentToken == RqlOp.AND)
                        state.Query.Append($"{parser._opResolver(parentToken)} ");
                    else if (parentToken == RqlOp.NOR) state.Query.Append($"{parser._opResolver(RqlOp.OR)} ");
                    else if (parentToken == RqlOp.NOT) state.Query.Append($"{parser._opResolver(RqlOp.AND)} ");
                }
                else if (idx > 1 && idx < container.Count + 1)
                {
                    // TODO: merge with upper if
                    // TODO: fix so that [] are ORed and {} are objects
                    // TODO: validate $or:[] and $and:{}
                    // container.Type == Object vs container.Type == JArray
                    state.Query.Append($"{parser._opResolver(RqlOp.AND)} ");
                }

                // get a fieldspec if the left json token matchs a known field name
                var field = parser._classSpec.Fields.ContainsKey(leftSide) ? parser._classSpec.Fields[leftSide] : null;
                var nextTerm = token.Value as JContainer;


                if (field != null && nextTerm != null)
                {
                    // left is known field and nextTerm is a json container ie {} or []
                    // recurse, recurse
                    ParseTerms(parser, nextTerm, leftSide, state);
                }
                else if (
                    RqlOp.IsOp(leftSide)
                    && leftSide == RqlOp.OR
                    || leftSide == RqlOp.AND
                    || leftSide == RqlOp.NOT
                    || leftSide == RqlOp.NOR
                )
                {
                    // TODO: nor bug? need a not for nor somewhere?
                    // Left side is an operation so before we go on lets add our brackets
                    if (container.Count > 0) state.Query.Append("( ");
                    ParseTerms(parser, nextTerm, leftSide, state);
                    if (container.Count > 0) state.Query.Append(") ");
                }
                else if (RqlOp.IsOp(leftSide) || field != null)
                {
                    // recursion root
                    // right side is a primitive value so we are at the bottom
                    // this means the field token is left or the parent
                    var parentField = parser._classSpec.Fields.ContainsKey(parentToken)
                        ? parser._classSpec.Fields[parentToken]
                        : null;
                    var op = RqlOp.IsOp(leftSide) ? leftSide : RqlOp.EQ;
                    ResolveNode(parser, field ?? parentField, op, token, state);
                }
                else
                {
                    // recursion root - error
                    state.Errors.Add(new Error($"invalid field or op: '{leftSide}', parent: '{parentToken}'"));
                }
            }

            return (
                state.Query.ToString().Trim(),
                state.FilterParameters,
                state.Errors
            );
        }

        /// <summary>
        /// This function adds the final statement ie " some_col_name >= 5 "
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="fieldSpec"></param>
        /// <param name="rqlOp"></param>
        /// <param name="val"></param>
        /// <param name="state"></param>
        private static void ResolveNode(
            RqlParser parser,
            FieldSpec fieldSpec,
            string rqlOp,
            JProperty val,
            ParseState state
        )
        {

            var sqlOp = parser._opResolver(rqlOp);
            if (sqlOp == null)
            {
                state.Errors.Add(new Error($"'{rqlOp}' is not supported by data provider"));
                return;
            }

            // validate this field supports this operation
            if (!fieldSpec.Ops.Contains(rqlOp))
            {
                state.Errors.Add(new Error($"'{fieldSpec.Name}' does not support op: '{rqlOp}'"));
                return;
            }

            // unwrap primitive
            var preVal = PrepPrim(val, rqlOp == RqlOp.IN || rqlOp == RqlOp.NIN);

            // gets the sql query parameter name, and converted value
            var (key, processedVal, err) = GetParameter(fieldSpec, preVal, state.ParameterTokenizer,
                parser._classSpec.Converter, parser._classSpec.Validator);
            if (err != null)
            {
                state.Errors.Add(err);
                return;
            }

            // adds sql query paramters to param dictionary
            state.FilterParameters.Add(key, processedVal);
            // adds sql conditional to query
            state.Query.Append($"{fieldSpec.ColumnName} {sqlOp} {key} ");
        }

        /// <summary>
        /// This is just a massager for unwrapping primitive values (including arrays of primitives) from a JProperty aka a json token.
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="expectArray"></param>
        /// <returns></returns>
        private static object PrepPrim(JProperty jToken, bool expectArray = false)
        {
            if (expectArray)
            {
                var jArray = jToken.Value as JArray;
                var res = jArray?.Select(x => x as JValue).Select(x => x?.Value).ToList();
                return res;
            }

            var jProp = jToken.Value as JProperty;
            var jValue = (jProp?.Value ?? jToken?.Value) as JValue;

            return jValue?.Value;
        }

        /// <summary>
        /// This converts then validates the token primitive if a converter, or validator are provided for a given field.
        /// </summary>
        /// <returns></returns>
        private static (string, object, IError) GetParameter(
            FieldSpec field,
            object val,
            IParameterTokenizer tokenizer,
            Func<string, Type, object, (object, IError)> defaultConverter = null,
            Func<string, Type, object, IError> defaultValidator = null
        )
        {
            if (val == null) return (null, null, new Error("could not parse right side as a valid primitive"));
            var converter = field.Converter ?? defaultConverter;

            // convert raw primitive if converter is provided for this field
            if (converter != null)
            {
                var (v, err) = converter(field.Name, field.PropType, val);
                if (err != null) return (null, null, err);
                val = v;
            }

            // validates the field value
            var validator = field.Validator ?? defaultValidator;
            if (validator != null)
            {
                var err = validator(field.Name, field.PropType, val);
                if (err != null) return (null, null, err);
            }

            // gets the parameter token name ie "someJsonField" -> "@someJsonField"
            var parameterName = tokenizer.GetToken(field.Name, field.PropType);

            return (parameterName, val, null);
        }
    }
}