using System.Collections.Generic;
using DocumentParser.Elements;

namespace DocumentParser.Parsers.Headers
{
    public class HeaderParserModel
    {
        public HeaderParserModel Parent {get;set;}
        public ElementStructure LastElement {get;set;}
        /// <summary>
        /// Иерархия приложения в приложениях (чтоб было понятно что это приложение к приложению)
        /// </summary>
        /// <value> 0 - корень 1 - приложение к 0 приложению</value>
        public int StartIndex {get;set;}
        public int EndIndex {get;set;}
        public DocumentElements.Header Header {get;} = new DocumentElements.Header();
        /// <summary>
        /// все элементы находящиеся в корне хедера
        /// </summary>
        /// <typeparam name="ElementStructure"></typeparam>
        /// <returns></returns>
        public List<ElementStructure> RootElements {get;} = new List<ElementStructure>();
    }
}