using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentParser.DocumentElements.FootNotes;
using DocumentParser.DocumentElements.HyperText;
using DocumentParser.DocumentElements.MetaInformation;
using DocumentParser.Interfaces;

namespace DocumentParser.DocumentElements
{
    public class Indent : StructureNodeBase, IMetaInformation, IIsDefinition, IIsChange, IHyperTextInfo, IFootNote
    {
        public Indent(ParagraphProperties properties,
                     int elementIndex,
                     string hash,
                     List<Run> runs,
                     MetaInfo meta,
                     HyperTextInfo hyperTextInfo,
                     FootNoteInfo footNote,
                     DocumentTable table,
                     bool isChange,
                     int cuttingLenght = 0,
                     NodeType node = NodeType.Абзац)
        {
            ElementIndex = elementIndex;
            Properties = properties;
            IndentHash = hash;
            nodeType = node;
            Runs = runs;
            Meta = meta;
            HyperTextInfo = hyperTextInfo;
            Table = table;
            IsChange = isChange;
            FootNote = footNote;
            if(HyperTextInfo.hasValue)
            {
                foreach(var l in HyperTextInfo.hyperLinks)
                {
                    l.LinkStartIndex = l.LinkStartIndex - cuttingLenght;
                }
            }
        }
        public Indent() { }
        public List<Run> Runs {get;set;}
        public string Text()
        {
            string t = "";
            Runs.ForEach(f=>t += f.Text);
            return t;
        }
        public ParagraphProperties Properties { get; set; }
        public MetaInfo Meta {get;set;}
        public HyperTextInfo HyperTextInfo {get;set;}
        public FootNoteInfo FootNote {get;set;}
        public bool IsDefinition {get;set;}
        public bool IsChange {get;set;}
        public string IndentHash { get; set; }
        public DocumentTable Table {get;set;}
    }
}
