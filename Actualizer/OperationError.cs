using System.Collections.Generic;
using Actualizer.Source;
using Lexer;
using SettingsWorker.Actualizer;

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
    public OperationError(string error, string text, List<Token<ActualizerTokenType>> tokens, DocumentRequisites requisites)
    {
        Error = error;
        OriginalText = text;
        Tokens = tokens;
        Requisites = requisites;
        Path = null;
    }
    public DocumentRequisites Requisites {init; get;}
    public string OriginalText {init; get;}
    public string Error {init; get;}
    public string Path {init; get;}
    public List<Token<ActualizerTokenType>> Tokens {init; get;}
}