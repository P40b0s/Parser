using System;

namespace Actualizer.Structure;

public struct JsonPathItem
{
    public JsonPathItem(string path, JsonItemType type)
    {
        Path = path;
        Type = type;
    }
    /// <summary>
    /// Текущай путь JsonPath
    /// </summary>
    /// <value></value>
    public string Path {get;}
    /// <summary>
    /// Тип обекта в структуре
    /// </summary>
    /// <value></value>
    public JsonItemType Type {get;}
    
}

    public enum JsonItemType
{
    None,
    Annex,
    Header,
    Headers,
    Item,
    Items,
    Indent,
    Indents
}
