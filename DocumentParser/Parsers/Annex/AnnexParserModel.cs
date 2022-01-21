using System.Collections.Generic;
using DocumentParser.Elements;
using DocumentParser.Interfaces;
using DocumentParser.Parsers.Headers;
using DocumentParser.Workers;

namespace DocumentParser.Parsers.Annex
{
    public class AnnexParserModel
    {
        public AnnexParserModel Parent {get;set;}
        public ElementStructure LastElement {get;set;}
        /// <summary>
        /// Наименование приложения для поиска
        /// </summary>
        /// <value></value>
        public string SearchName{get;set;}
        public int StartIndex {get;set;}
        public int EndIndex {get;set;}
        /// <summary>
        /// Иерархия приложения в приложениях (чтоб было понятно что это приложение к приложению)
        /// </summary>
        /// <value> 0 - корень 1 - приложение к 0 приложению</value>
        public int Hierarchy {get;set;}
        public bool HierarchyChecked = false;
        public DocumentElements.Annex Annex {get;} = new DocumentElements.Annex();
        public List<ElementStructure> RootElements {get;} = new List<ElementStructure>();
        public List<HeaderParserModel> Headers {get;} = new List<HeaderParserModel>();
    }
}