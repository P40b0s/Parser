using Actualizer.Source.Operations;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Extensions;

public static class NewEditionEx
{
    /// <summary>
    /// Следующие 1+ параграфов будут в новой редакции
    /// в первом параграфе вохможны реквизиты изменяемого документа
    /// </summary>
    public static Option<StructureNode> NewEdition(this Operation op, Parser parser, List<Token<ActualizerTokenType>> tokens, ElementStructure element, OperationType operationType )
    {
        var s = new StructureNode(element, operationType);
        var struc = Structure.GetTokensSequence(tokens);
        s.ChangePartName = Structure.GetPathArray(struc, parser, s, element);
        //if(s.StructureOperation == OperationType.Represent)
        //{
        Structure.AddChangedNodes(element, s);
        s.TargetDocumentRequisites = Structure.GetTargetDocumentRequisites(op.status, tokens, element, parser);
        //}
        element.IsParsed = true;
        return s.OptionFromValueOrDefault();
    }

    
}
