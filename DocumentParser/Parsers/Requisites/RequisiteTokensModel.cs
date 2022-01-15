using System.Collections.Generic;
using System.Linq;
using Services.Documents.Lexer.Tokens;
using Services.Documents.Parser.TokensDefinitions;

namespace Services.Documents.Parser.Parsers.Requisites
{
    public class RequisiteTokensModel
    {
        public Token<DocumentToken> typeToken {get;set;}
        public Token<DocumentToken> organToken {get;set;}
        public Token<DocumentToken> nameToken {get;set;}
        public Token<DocumentToken> executorToken {get;set;}
        public Token<DocumentToken> postToken {get;set;}
        public Token<DocumentToken> signDateToken {get;set;}
        public Token<DocumentToken> numberToken {get;set;}
    }
}