using System;
namespace Lexer
{
    public struct TokenResult<T>
    {
        public TokenResult(Token<T> result, TokenException exception = null)
        {
            Token = result;
            Error = exception;
        }
        public Token<T> Token {get;}
        public TokenException Error {get;}
        public bool IsOk => Token != null;
    }


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
        public string CustomMessage {get;}
        public TokenException(string message, TokenErrorType errorType = TokenErrorType.NotFound)
            : base(message)
        {
            ErrorType = errorType;
            Date = DateTime.Now;
            CustomMessage = message;
        }
       
    }
}