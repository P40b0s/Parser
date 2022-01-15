using System;
using System.Collections.Generic;

namespace DocumentParser.DocumentElements.FootNotes
{
   public class FootNoteInfo : FootNote
    {
        public FootNoteInfo() {}
        public FootNoteInfo(Guid id, string stringNumber, int number)
         : base(id, stringNumber, number)
        {
            footNoteLinks = null;
            //elements =new List<ElementStructure>();
        }
        public FootNoteInfo(Guid id,  string stringNumber, int number, List<DocumentParser.DocumentElements.Indent> information, int linkStartIndex ,int linkLength)
         : base(id, stringNumber, number)
        {
            footNoteLinks = new List<FootNoteLink>();
            footNoteLinks.Add(new FootNoteLink(id, stringNumber, number, information, linkStartIndex, linkLength));
            //elements = null;
        }
        public bool isFootNoteLink => footNoteLinks != null;
        //public List<ElementStructure> elements {get;}
        public List<FootNoteLink> footNoteLinks {get;}
        /// <summary>
        /// Количество ссылок на данную сноску
        /// </summary>
        /// <value></value>
        public int LinksCount {get;set;}
        public List<DocumentParser.DocumentElements.Item> Items {get;set;}
        public List<DocumentParser.DocumentElements.Indent> Indents {get;set;}

    }
}
