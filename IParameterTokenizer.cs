using System;
using System.Collections.Generic;

namespace Rql.NET
{
    public interface IParameterTokenizer
    {
        string GetToken(string fieldName, Type propType);
    }


    // produces @1, @2
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

    // produces @City and @City2
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