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
public class TokenException : CustomException<TokenErrorType>
{
    public TokenException(string message, TokenErrorType error = TokenErrorType.NotFound) : base(message, error){}
}

