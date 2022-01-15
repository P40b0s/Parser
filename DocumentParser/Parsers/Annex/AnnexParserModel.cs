using System.Collections.Generic;
using Services.Documents.Core.Interfaces;
using Services.Documents.Parser.Parsers.Headers;
using Services.Documents.Parser.Workers;

namespace Services.Documents.Parser.Parsers.Annex
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
        public Services.Documents.Core.DocumentElements.Annex Annex {get;} = new Services.Documents.Core.DocumentElements.Annex();
        public List<ElementStructure> RootElements {get;} = new List<ElementStructure>();
        // public List<FootNoteInfo> FootNoteElements {get;} = new List<FootNoteInfo>();
        // public List<ElementStructure> NoteElements {get;} = new List<ElementStructure>();
        public List<HeaderParserModel> Headers {get;} = new List<HeaderParserModel>();
    }
}