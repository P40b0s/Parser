using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentParser.DocumentElements.HyperText;
using DocumentParser.DocumentElements.MetaInformation;
using DocumentParser.Interfaces;

namespace DocumentParser.DocumentElements
{
    /// <summary>
    /// Примечания могут иметь и абзацы и пункты
    /// ищем их после поиска заголовков
    /// А еще у нас есть технические примечания, особые примечания и структура у них у всех как у заголовков!
    /// И даже есть разделы!!!!
    /// указ президента от 19 февраля 2021 года....
    /// </summary>
    public class Note : StructureNodeBase, IComment, IMetaInformation, IHyperTextInfo
    {
        public Note(int elementIndex)
        {
            ElementIndex = elementIndex;
            nodeType = NodeType.Примечание;
        }
        
        public Note() { }
        public string Name {get;set;}
        public Comment Comment {get;set;}
        public HyperTextInfo HyperTextInfo {get;set;}
        public MetaInfo Meta {get;set;}
        public List<Header> Headers {get;set;} = new List<Header>();
        public List<Indent> Indents {get;set;} = new List<Indent>();
        public List<Item> Items {get;set;} = new List<Item>();

    }

   

   
    
}
