using System;
using System.Collections.Generic;

namespace Rql.NET
{

    /// <summary>
    /// Responsible for creating backend query field tokens. Implementation must potentially account for duplicate field names being used within the same backend query.
    /// </summary>
    public interface IParameterTokenizer
    {
        string GetToken(string fieldName, Type propType);
    }


    /// <summary>
    /// Simple index iterator tokenizer. Given "fieldName, fieldName, anotherFieldName" it will return "@1, @2, @3"
    /// </summary>
    public class IndexTokenizer : IParameterTokenizer
    {
        private readonly string _tokenPrefix;
        private int _tokenCount;

        public IndexTokenizer(string tokenPrefix = "@")
        {
            _tokenPrefix = tokenPrefix;
        }

        public string GetToken(string fieldName, Type propType)
        {
            return $"{_tokenPrefix}{++_tokenCount}";
        }
    }

    /// <summary>
    /// Returns named tokens matching field name. Given "fieldName, fieldName, anotherFieldName" it will return "@fieldName1, @fieldName2, @anotherFieldName"
    /// </summary>
    public class NamedTokenizer : IParameterTokenizer
    {
        private readonly Dictionary<string, int> _propCount = new Dictionary<string, int>();
        private readonly string _tokenPrefix;

        public NamedTokenizer(string tokenPrefix = "@")
        {
            _tokenPrefix = tokenPrefix;
        }

        public string GetToken(string fieldName, Type propType)
        {
            _propCount.TryGetValue(fieldName, out var currentCount);
            _propCount[fieldName] = currentCount + 1;

            return _propCount[fieldName] > 1
                ? $"{_tokenPrefix}{fieldName}{_propCount[fieldName]}"
                : $"{_tokenPrefix}{fieldName}";
        }
    }
}