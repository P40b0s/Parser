using Actualizer.Source.Operations;
using Actualizer.Structure;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Extensions;

public static class ProcessingWordsOperationsEx
{
    /// <summary>
    /// Замена слов
    /// </summary>
    /// <param name="op"></param>
    /// <param name="parser"></param>
    /// <param name="tokens"></param>
    /// <param name="element"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    public static Option<StructureNode> ProcessingWordsOperations(this Operation op, Parser parser, List<Token<ActualizerTokenType>> tokens, ElementStructure element, int correction)
    {
        var wordsOperations = op.GetWordOperationType(tokens);
        if(wordsOperations.Count() == 0)
            return Option.None<StructureNode>();
        var wordNode = SourceOperations.GetTokensSequence(tokens);
        var newNode = new StructureNode(element, OperationType.WordsOperations);
        newNode.ChangePartName = SourceOperations.GetPathArray(wordNode, parser, newNode, element, correction);
        foreach(var w in wordsOperations)
        {
            var res = op.WordsOperations(w, newNode, tokens, element, parser, correction);
            if(res.IsError)
            {
                op.status.AddErrors(res.Error().statuses);
                return Option.None<StructureNode>();
            }
        }
        element.IsParsed = true;
        return newNode.OptionFromValueOrDefault();
    }
}
