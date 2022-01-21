using System;
namespace Lexer;

[Serializable]
public abstract class CustomException<T> : Exception
{
    public T ErrorType {get;}
    public DateTime Date {get;}
    public CustomException(string message, T errorType = default(T))
        : base(message)
    {
        ErrorType = errorType;
        Date = DateTime.Now;
    }
}

