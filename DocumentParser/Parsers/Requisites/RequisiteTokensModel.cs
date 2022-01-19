using System.Collections.Generic;
using System.Linq;
using Lexer;
using DocumentParser.TokensDefinitions;

namespace DocumentParser.Parsers.Requisites
{
    public class RequisiteTokensModel
    {
        public Token<DocumentToken> typeToken {get;set;}
        public List<Token<DocumentToken>> organsTokens {get;set;} = new List<Token<DocumentToken>>();
        public Token<DocumentToken> nameToken {get;set;}
        public List<ExecutorRequisiteToken> personToken {get;set;} = new List<ExecutorRequisiteToken>();
        public Token<DocumentToken> signDateToken {get;set;}
        public Token<DocumentToken> numberToken {get;set;}
    }

    public class ExecutorRequisiteToken
    {
        public Token<DocumentToken> executorToken {get;set;}
        public Token<DocumentToken> postToken {get;set;}
    }
}