using System.Collections.Generic;
using Services.Documents.Lexer.Tokens;
using Test.Regexes.Actualizer.Source;

namespace Actualizer;
public struct OperationError
{
    public OperationError(string error, string text, string path, DocumentRequisites requisites)
    {
        Error = error;
        OriginalText = text;
        Path = path;
        Requisites = requisites;
        Tokens = null;
    }
    public OperationError(string error, string text, List<Token<ActualizerToken>> tokens, DocumentRequisites requisites)
    {
        Error = error;
        OriginalText = text;
        Tokens = tokens;
        Requisites = requisites;
        Path = null;
    }
    public DocumentRequisites Requisites {get;}
    public string OriginalText {get;}
    public string Error {get;}
    public string Path {get;}
    public List<Token<ActualizerToken>> Tokens {get;}
}