using System.Collections.Generic;
using DocumentParser.Elements;

namespace DocumentParser.Parsers.Items
{
    public class ItemParserModel
    {
        public ItemParserModel Parent {get;set;}
        public ElementStructure LastElement {get;set;}
        /// <summary>
        /// Иерархия приложения в приложениях (чтоб было понятно что это приложение к приложению)
        /// </summary>
        /// <value> 0 - корень 1 - приложение к 0 приложению</value>
        public int StartIndex {get;set;}
        public int EndIndex {get;set;}
        public DocumentElements.Item Item {get;} = new DocumentElements.Item();
        public List<ElementStructure> Items {get;} = new List<ElementStructure>();
    }
}