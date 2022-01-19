using System;
namespace Lexer;
public enum TokenErrorType
{
    /// <summary>
    /// Токен не найден
    /// </summary>
    NotFound,
    /// <summary>
    /// Найден не ожидаемый токен а другой
    /// </summary>
    WrongFound,
    /// <summary>
    /// Запрос находится за пределами массива
    /// </summary>
    Range,
    Info
}

[Serializable]
public class TokenException : Exception
{
    public TokenErrorType ErrorType {get;}
    public DateTime Date {get;}
    public TokenException(string message, TokenErrorType errorType = TokenErrorType.NotFound)
        : base(message)
    {
        ErrorType = errorType;
        Date = DateTime.Now;
    }
}

