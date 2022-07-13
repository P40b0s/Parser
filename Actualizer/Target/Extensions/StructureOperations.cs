using Actualizer.Structure;
using DocumentParser.Parsers;
using Newtonsoft.Json.Linq;

namespace Actualizer.Target.Extensions;
public static class  StructureOperationsEx
{
    public static async ValueTask<bool> StructureOperations(this Operation op, Parser parser, JObject JDoc, StructureNode node, SourceDocumentParserResult source, Func<ValueTask> reload)
    {
        foreach(var n in node.Nodes)
        {
            switch(n.StructureOperation)
            {
                default:
                {
                    op.status.AddError("Ошибка актуализации", $"Не определен метод для операции актуализации: {Enum.GetName(typeof(OperationType), n.StructureOperation)}");
                    return false;
                }
                case OperationType.AddNewElement:
                {
                    var beforeElement = JDoc.GetElementBeforeInsert(parser, n);
                    if(beforeElement.IsError)
                    {
                        op.status.AddError("Ошибка", beforeElement.Error().Message);
                        return false;
                    }
                    await n.AddNewElement(beforeElement.Value(), parser, source, reload);
                    break;
                }
                case OperationType.Represent:
                {
                    await op.Represent(parser, JDoc, n, source, reload);
                    break;
                }
            }
        }
        return true;
    }
}