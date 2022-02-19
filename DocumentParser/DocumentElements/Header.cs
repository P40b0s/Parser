using System.Collections.Generic;
using DocumentParser.DocumentElements.FootNotes;
using DocumentParser.DocumentElements.HyperText;
using DocumentParser.DocumentElements.MetaInformation;
using DocumentParser.Interfaces;

namespace DocumentParser.DocumentElements
{
    //TODO внутри хедеров не могут быть другие хедеры, все хедеры одноуровневые, идут друг за другом
    //сделано для того чтобы можно было получить элемент сразу со статьи апример не за рагивая разделы и части
    //а вот внутри хедеров могут быть абзацы итемы и таблицы
    /// <summary>
    /// Подзаголовки - Статьи разделы главы
    /// </summary>
    public class Header : StructureNodeBase, IComment, IMetaInformation, IHyperTextInfo
    {
        public Header(int elementIndex, NodeType node, string name, string number)
        {
            ElementIndex = elementIndex;
            nodeType = node;
            Name = name;
            Number = number;
        }
        public Header() { }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Postfix {get;set;}
        public string Type {get;set;}
        public string CommentId {get;set;}
        public HyperTextInfo HyperTextInfo {get;set;}
        public MetaInfo Meta {get;set;}
        public DocumentTable Table {get;set;}
        public List<Indent> Indents {get;set;} = new List<Indent>();
        public List<Item> Items {get;set;} = new List<Item>();
        public List<FootNoteInfo> FootNotes {get;set;} = new List<FootNoteInfo>();
        public List<Note> Notes {get;set;} = new List<Note>();

    }
}
