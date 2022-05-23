using Actualizer.Source.Operations;
using Actualizer.Structure;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Extensions;

public static class TargetDocumentRequisitesParagraphEx
{
    /// <summary>
    /// В параграфе с реквизитами изменяемого документа есть и само изменение, обычно оно заключается в замене дополнении или удалени каких то слов
    /// </summary>
    public static Option<StructureNode> TargetDocumentRequisitesParagraph(this Operation op, Parser parser, List<Token<ActualizerTokenType>> tokens, ElementStructure element, OperationType operationType )
    {
        var s = new StructureNode(element, operationType);
        var req = SourceOperations.GetTargetDocumentRequisites(op.status, tokens, element, parser);
        if(req.IsNone)
        {
            op.status.AddError("Ошибка парсинга реквизитов изменяющего документа", parser.word.FullText);
            return Option.None<StructureNode>();
        }
        s.TargetDocumentRequisites = req;
        var struc = SourceOperations.GetTokensSequence(tokens);
        s.ChangePartName = SourceOperations.GetPathArray(struc, parser, s, element);
        var operation = op.WordsOperations(s.StructureOperation, s, tokens, element, parser);
        if(operation.IsError)
        {
            op.status.AddErrors(operation.Error().statuses);
            return Option.None<StructureNode>();
        }
        element.IsParsed = true;
        return s.OptionFromValueOrDefault();
    }

   
}
