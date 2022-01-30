using System.Collections.Generic;
using Lexer;
using SettingsWorker.Requisite;

namespace DocumentParser.Parsers.Requisites
{
    public class RequisiteTokensModel
    {
        public Token<RequisiteTokenType> typeToken {get;set;}
        public List<Token<RequisiteTokenType>> organsTokens {get;set;} = new List<Token<RequisiteTokenType>>();
        public Elements.ElementStructure nameElement {get;set;}
        public List<ExecutorRequisiteToken> personToken {get;set;} = new List<ExecutorRequisiteToken>();
        public Token<RequisiteTokenType> signDateToken {get;set;}
        public Token<RequisiteTokenType> numberToken {get;set;}
        public bool NotHaveName {get;set;} = false;
    }

    public class ExecutorRequisiteToken
    {
        public Token<RequisiteTokenType> executorToken {get;set;}
        public Token<RequisiteTokenType> postToken {get;set;}
    }
}