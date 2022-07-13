using Actualizer.Structure;
using DocumentParser.Parsers;
using Newtonsoft.Json.Linq;

namespace Actualizer.Target.Extensions;
public static class  AtomicWordsOperationsEx
{
    public static async ValueTask<bool> AtomicWordsOperations(this Operation op, Parser parser, JObject JDoc, StructureNode node, SourceDocumentParserResult source)
    {
        
        foreach(var n in node.Nodes)
        {
            foreach(var change in n.WordsOperations)
            switch(change.StructureOperation)
            {
                default:
                {
                    op.status.AddError("Ошибка актуализации", $"Не определен метод для операции актуализации: {Enum.GetName(typeof(OperationType), n.StructureOperation)}");
                    return false;
                }
                case OperationType.ApplyAfterWords:
                {
                    var element = JDoc.GetTargetElement(n, parser);
                    if(element.IsError)
                    {
                        op.status.AddError("Ошибка", element.Error().Message);
                        return false;
                    }
                    var start = element.Value().WordElement.Text.IndexOf(change.SourceText);
                    var tt = "";
                    break;
                }
                case OperationType.ReplaceWords:
                {
                    op.ReplaceWord(parser, JDoc, n, source);
                    break;
                }
            }
        }
        return true;
    }
}