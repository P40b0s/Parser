using DocumentParser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentParser.DocumentElements
{
    public struct DocumentBody
    {
        public DocumentBody(List<Header> headers,
                            List<Indent> indents,
                            List<Item> items,
                            List<Annex> annexes)
        {
           Headers = headers;
           Indents = indents;
           Items = items;
           Annexes = annexes;
        }
        
        /// <summary>
        ///Заголовок (Раздел, глава, статья)
        /// </summary>
        public List<Header> Headers {get;}
        public List<Indent> Indents {get;}
        public List<Item> Items {get;}
        public List<Annex> Annexes {get;}
    }

}
