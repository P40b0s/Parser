using Lexer;
using SettingsWorker.Actualizer;

namespace Actualizer.Structure;

public struct PathUnit
{
    public StructureType Type {get;set;}
    public string Number {get;set;}
    public string AnnexType {get;set;}
    public string AnnexName {get;set;}
    public Token<ActualizerTokenType> Token {get;set;}
}
