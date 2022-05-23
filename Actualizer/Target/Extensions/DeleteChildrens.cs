using Actualizer.Structure;
using DocumentParser.DocumentElements;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Newtonsoft.Json.Linq;
using Utils;

namespace Actualizer.Target.Extensions;

public static class DeleteChildrensExt
{
    /// <summary>
    /// Удаляем всех потомков указанного элемента (если пункт например имеет несколько абзацев или подпункты то все они удалятся)
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="startChangeElementIndex">Индекс элемента, потомки которого будут удалены</param>
    public static void ClearChildrens(this Parser parser, int startChangeElementIndex)
    {
        var childElementsIndexes = parser.GetChildElementsIndexes(startChangeElementIndex);
        foreach(var child in childElementsIndexes)
        {
            var childElement = parser.word.GetElement(child);
            childElement.Value().WordElement.Element.Remove();
        }
    }
}