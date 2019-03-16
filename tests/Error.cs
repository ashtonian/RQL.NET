

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

public interface IError
{
    string GetMessage();
}
