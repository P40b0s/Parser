using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public enum ErrorType
    {
        Fatal,
        Warning,
        Info
    }
    [Serializable]
    public class ParserException : Exception
    {
        public ErrorType ErrorType {get;set;}
        public DateTime Date {get;}
        public ParserException(string message, ErrorType errorType = ErrorType.Fatal)
            : base(message)
        {
            ErrorType = errorType;
            Date = DateTime.Now;
        }
        public ParserException(string message, TokenException te, ErrorType errorType = ErrorType.Fatal)
            : base(message + ". " + te.CustomMessage)
        {
            ErrorType = errorType;
            Date = DateTime.Now;
        }
       
    }
}