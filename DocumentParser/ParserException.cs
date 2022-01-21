using Lexer;
namespace DocumentParser;

public enum ErrorType
{
    Fatal,
    Warning,
    Info
}

public class ParserException : CustomException<ErrorType>
{
    public ParserException(string message, ErrorType error = ErrorType.Fatal) : base(message, error){}
}

public class ElementQueryException : CustomException<ErrorType>
{
    public ElementQueryException(string message, ErrorType error = ErrorType.Fatal) : base(message, error){}
}



