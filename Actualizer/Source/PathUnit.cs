using Lexer;
namespace Actualizer.Source;

public struct PathUnit
{
    public StructureType Type {get;set;}
    public string Number {get;set;}
    public string AnnexType {get;set;}
    public string AnnexName {get;set;}
    public Token<Actualizer.ActualizerToken> Token {get;set;}
}
