using System;
using System.Collections.Generic;

public interface IParameterTokenizer
{
    string GetToken(string fieldName, Type propType);
}


// produces @1, @2
public class IndexTokenizer : IParameterTokenizer
{
    private string _tokenPrefix;
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
    private string _tokenPrefix;
    private Dictionary<string, int> _propCount = new Dictionary<string, int>();

    public NamedTokenizer(string tokenPrefix = "@")
    {
        _tokenPrefix = tokenPrefix;
    }

    public string GetToken(string fieldName, Type propType)
    {
        _propCount[fieldName]++;
        if (_propCount[fieldName] > 1) return $"{_tokenPrefix}{fieldName}{_propCount[fieldName]}";
        return $"{_tokenPrefix}{fieldName}";
    }
}
