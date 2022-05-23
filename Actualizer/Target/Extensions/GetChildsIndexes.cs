using Actualizer.Structure;
using DocumentParser.DocumentElements;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Newtonsoft.Json.Linq;
using Utils;

namespace Actualizer.Target.Extensions;

public static class GetChildsIndexesExt
{
    private static List<int> indexes {get;set;}
    /// <summary>
    /// Получает все индексы дочерних документов
    /// </summary>
    /// <param name="parser">Парсер</param>
    /// <param name="index">Индекс элемента у которого будет производится поиск</param>
    /// <returns></returns>
    public static List<int> GetChildElementsIndexes(this Parser parser, int index)
    {
        indexes = new List<int>();
        //TODO нет поиска по итемам боди документа (поиск осущемтвляется только в хедерах)
        foreach(var h in parser.document.Body.Headers)
        {
            if(h.ElementIndex == index)
            {
                AddAllItems(h.Items);
                foreach(var ind in h.Indents)
                {
                    indexes.Add(ind.ElementIndex);
                }
            }
            if(h.Items != null)
            foreach(var itm in h.Items)
            {
                if(itm.ElementIndex == index)
                {
                    foreach(var ind in itm.Indents)
                    {
                        indexes.Add(ind.ElementIndex);
                    }
                    AddAllItems(itm.Items);
                } 
            }
        }
        indexes.Remove(index);
        return indexes;
    }
    private static void AddAllItems(List<Item> items)
    {
        if(items != null)
        foreach(var i in items)
        {
            foreach(var ind in i.Indents)
            {
                indexes.Add(ind.ElementIndex);
            }
            AddAllItems(i.Items);
        }
    }
    
}