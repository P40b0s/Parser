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
        var struc = SourceOperations.GetTokensSequence(tokens);
        s.ChangePartName = SourceOperations.GetPathArray(struc, parser, s, element);
        //if(s.StructureOperation == OperationType.Represent)
        //{
        element.AddChangedNodes(s);
        s.TargetDocumentRequisites = SourceOperations.GetTargetDocumentRequisites(op.status, tokens, element, parser);
        //}
        element.IsParsed = true;
        return s.OptionFromValueOrDefault();
    }
}
