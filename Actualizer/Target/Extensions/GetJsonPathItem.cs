namespace Actualizer.Structure;

public static class JsonPathExt
{
    /// <summary>
    /// Конвертирование модели в запрос JsonPath
    /// </summary>
    /// <param name="paths">пути для формирования запроса</param>
    /// <param name="getAll">получение всех элементов того же родителя</param>
    /// <returns></returns>
    public static JsonPathItem GetJsonPathItem(this List<PathUnit> paths, bool getAll = false)
    {
        
        var tmpPath = "$";
        JsonItemType t = JsonItemType.None;
        for(int i = 0; i < paths.Count(); i++)
        {
            if(paths[i].Type == StructureType.Annex)
            {
                tmpPath +=$".Annexes[?(@.SearchName == '{paths[i].AnnexName}')]";
                t = JsonItemType.Annex;
            }

            if(paths[i].Type == StructureType.Header)
            {
                if(getAll && i == paths.Count -1)
                {
                    tmpPath +=$".Headers[*]";
                    t = JsonItemType.Headers;
                }
                else
                {
                    tmpPath +=$".Headers[?(@.Number == '{paths[i].Number}')]";
                    t = JsonItemType.Header;
                } 
            }
                
            if(paths[i].Type == StructureType.Item)
            {
                if(getAll && i == paths.Count -1)
                {
                    tmpPath +=$".Items[*]";
                    t = JsonItemType.Items;
                }
                else
                {
                    tmpPath +=$".Items[?(@.Number == '{paths[i].Number}')]";
                    t = JsonItemType.Item;
                }
            }
                
            if(paths[i].Type == StructureType.Indent)
            {
                if(getAll && i == paths.Count -1)
                {
                    tmpPath +=$".Indents[*]";
                    t = JsonItemType.Indents;
                }
                else
                {
                    int number = -1;
                    int.TryParse(paths[i].Number, out number);
                    tmpPath +=$".Indents[{number -1}]";
                    t = JsonItemType.Indent;
                }
            }
        }
        return new JsonPathItem(tmpPath, t);
    }
}