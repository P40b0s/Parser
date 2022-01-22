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
public class TokenException : Utils.CustomError<TokenErrorType>
{
    public TokenException(){}
    public TokenException(string message) : base(message){}
    public TokenException(string message, TokenErrorType error) : base(message, error){}
}

