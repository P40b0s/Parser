using System;
namespace Lexer;

    public struct TokenResult<T>
    {
        public TokenResult(Token<T> result, TokenException exception = null)
        {
            Token = result;
            Error = exception;
            Date = null;
        }
        public TokenResult(Token<T> result, DateTime? date)
        {
            Token = result;
            Date = date;
            Error = null;
        }
        public Token<T> Token {get;}
        public TokenException Error {get;}
        public bool IsOk => Token != null;
        public bool IsError => Error != null;
        public DateTime? Date {get;}
    }
