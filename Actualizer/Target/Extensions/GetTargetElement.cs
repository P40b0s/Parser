using DocumentParser.Elements;
using DocumentParser.Parsers;
using Newtonsoft.Json.Linq;
using Utils;

namespace Actualizer.Structure;

public static class GetTargetElementExt
{
    /// <summary>
    /// Получение элемента с которым будут проводиться операции актуализации
    /// </summary>
    /// <param name="paths">пути для формирования запроса</param>
    /// <param name="getAll">получение всех элементов того же родителя</param>
    /// <returns></returns>
    public static Result<ElementStructure> GetTargetElement(this JObject JDoc, StructureNode node, Parser parser)
    {
        //путь запроса
        var path = node.Path.GetJsonPathItem();
        //запрос к документу в представлении JSON
        var token = JDoc.SelectToken(path.Path);
        
        //Получаем индекс найденого элемента и берем его из парсера
        var startChangeElementIndex = token.Value<int>("ElementIndex");
        var element = parser.word.GetElement(startChangeElementIndex);
        if(element.IsError)
            return Result<ElementStructure>.Err("Элемент по адресу " + path + "не найден");
        return element;
    }
}