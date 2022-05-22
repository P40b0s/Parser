using Actualizer.Source.Operations;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Extensions;

public static class ReplaceWordsEx
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
    public static Option<StructureNode> ReplaceWords(this Operation op, Parser parser, List<Token<ActualizerTokenType>> tokens, ElementStructure element, OperationType operationType )
    {
        var str = Structure.GetTokensSequence(tokens);
        var newNode = new StructureNode(element, operationType);
        newNode.ChangePartName = Structure.GetPathArray(str, parser, newNode, element);
        if(!op.wordOperations.Recognize(operationType, newNode, tokens, element, parser))
        {
            op.status.AddErrors(op.wordOperations.status.statuses);
            return Option.None<StructureNode>();
        }
        element.IsParsed = true;
        return newNode.OptionFromValueOrDefault();
    }
}
