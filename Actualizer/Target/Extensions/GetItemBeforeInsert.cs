using Actualizer.Structure;
using DocumentParser.DocumentElements;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Newtonsoft.Json.Linq;
using Utils;

namespace Actualizer.Target.Extensions;

public static class GetItemBeforeInsertExt
{
    /// <summary>
    /// Возвращаем элемент ПОСЛЕ которого необходимо добавить перечень или элемент
    /// </summary>
    /// <param name="node"></param>
    /// <param name="targetDocumentJObject"></param>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static Result<ElementStructure> GetElementBeforeInsert(this JObject JDoc, Parser parser, StructureNode node)
    {
        //путь запроса
        var path = node.Path.GetJsonPathItem(true);
        var sorter = new NumbersSorter();
        if(path.Type == JsonItemType.Indents)
        {
            var items = JDoc.SelectTokens(path.Path);
            List<DocumentParser.DocumentElements.Indent> l = items.Select(s=>s.ToObject<DocumentParser.DocumentElements.Indent>()).ToList();
            var num = sorter.GetItemNumberBefore(l.Select(s=>l.IndexOf(s).ToString()), node.Path.Last());
            int index = 0;
            if(!int.TryParse(num, out index))
            {
                return Result<ElementStructure>.Err($"Некорректный номер абзаца: {num}, Текст изменения: {node.Element.WordElement.Text}, Путь: {path.Path}");
            }
            var itemBefore = l[index];
            var item = parser.word.GetElement(itemBefore.ElementIndex);
            return item;
            //не играет роли то что я отдам, серавно я буду добавлять вордовские элементы после этого элемента!!
            //в смысле что такие же итерации повторяем для хедеров, отлично, сократили количество условий
        }
        if(path.Type == JsonItemType.Headers)
        {
            var items = JDoc.SelectTokens(path.Path);
            List<DocumentParser.DocumentElements.Header> l = items.Select(s=>s.ToObject<DocumentParser.DocumentElements.Header>()).ToList();
            var num = sorter.GetItemNumberBefore(l.Select(s=>s.Number), node.Path.Last());
            var itemBefore = l.First(f=>f.Number == num);
            var item = parser.word.GetElement(itemBefore.ElementIndex);
            var childs = parser.GetChildElementsIndexes(item.Value().ElementIndex);
            if(childs.Count > 0)
            {
                var lastElement = parser.word.GetElement(childs.Last());
                return lastElement;
            }
            else
                return item;
        }
        if(path.Type == JsonItemType.Items)
        {
            var items = JDoc.SelectTokens(path.Path);
            List<DocumentParser.DocumentElements.Item> l = items.Select(s=>s.ToObject<DocumentParser.DocumentElements.Item>()).ToList();
            var num = sorter.GetItemNumberBefore(l.Select(s=>s.Number), node.Path.Last());
            var itemBefore = l.First(f=>f.Number == num);
            var item = parser.word.GetElement(itemBefore.ElementIndex);
            var childs = parser.GetChildElementsIndexes(item.Value().ElementIndex);
            if(childs.Count > 0)
            {
                var lastElement = parser.word.GetElement(childs.Last());
                return lastElement;
            }
            else
                return item;
        
        }
        return Result<ElementStructure>.Err($"Элемент после которого должна идти вставка не найден. Текст изменения: {node.Element.WordElement.Text}, Путь: {path.Path}");
        // var token = targetDocumentJObject.SelectToken(path.Path);
        
        // //Получаем индекс найденого элемента и берем его из парсера
        // var startChangeElementIndex = token.Value<int>("ElementIndex");
        // var element = parser.word.GetElement(startChangeElementIndex);
        // if(element == null)
        //     throw new  Exception("Элемент по адресу " + path + "не найден");
        // return element;
    }
    
}