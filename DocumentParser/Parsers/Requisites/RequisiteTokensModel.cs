using System.Collections.Generic;
using Lexer;
using SettingsWorker.Requisites;

namespace DocumentParser.Parsers.Requisites
{
    public class RequisiteTokensModel
    {
        public Token<RequisitesTokenType> typeToken {get;set;}
        public List<Token<RequisitesTokenType>> organsTokens {get;set;} = new List<Token<RequisitesTokenType>>();
        public Token<RequisitesTokenType> nameToken {get;set;}
        public List<ExecutorRequisiteToken> personToken {get;set;} = new List<ExecutorRequisiteToken>();
        public Token<RequisitesTokenType> signDateToken {get;set;}
        public Token<RequisitesTokenType> numberToken {get;set;}
        public bool NotHaveName {get;set;} = false;
    }

    public class ExecutorRequisiteToken
    {
        public Token<RequisitesTokenType> executorToken {get;set;}
        public Token<RequisitesTokenType> postToken {get;set;}
    }
}