using Utils;
namespace DocumentParser;

public class ParserException : CustomError
{
    public ParserException(){}
    public ParserException(string message) : base(message){}
    public ParserException(string message, ErrorType error) : base(message, error){}
}

public class ElementQueryException : CustomError
{
    public ElementQueryException(){}
    public ElementQueryException(string message) : base(message){}
    public ElementQueryException(string message, ErrorType error) : base(message, error){}
}



