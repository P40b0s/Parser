using System.Collections.Generic;
using Actualizer.Source;
using Actualizer.Structure;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;

namespace Actualizer;
public struct OperationError
{
    public OperationError(string error, string text, string path, Option<DocumentRequisites> requisites)
    {
        Error = error;
        OriginalText = text;
        Path = path;
        Requisites = requisites;
        Tokens = null;
    }
    public OperationError(string error, string text, List<Token<ActualizerTokenType>> tokens, Option<DocumentRequisites> requisites)
    {
        Error = error;
        OriginalText = text;
        Tokens = tokens;
        Requisites = requisites;
        Path = null;
    }
    public Option<DocumentRequisites> Requisites {init; get;}
    public string OriginalText {init; get;}
    public string Error {init; get;}
    public string Path {init; get;}
    public List<Token<ActualizerTokenType>> Tokens {init; get;}
}