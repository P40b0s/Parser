using System;
using Utils;
namespace Lexer;
[Serializable]
public class TokenException : Utils.CustomError
{
    public TokenException(){}
    public TokenException(string message) : base(message){}
    public TokenException(string message, ErrorType error) : base(message, error){}
}

