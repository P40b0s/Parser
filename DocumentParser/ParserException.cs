using Utils;
namespace DocumentParser;

public enum ErrorType
{
    Fatal,
    Warning,
    Info
}

public class ParserException : CustomError<ErrorType>
{
    public ParserException(){}
    public ParserException(string message) : base(message){}
    public ParserException(string message, ErrorType error) : base(message, error){}
}

public class ElementQueryException : CustomError<ErrorType>
{
    public ElementQueryException(){}
    public ElementQueryException(string message) : base(message){}
    public ElementQueryException(string message, ErrorType error) : base(message, error){}
}



