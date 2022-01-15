using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentParser.DocumentElements
{
    public class Item : StructureNodeBase
    {
        public Item(int elementIndex, NodeType node, int hierarchy, string number, string postfix)
        {
            ElementIndex = elementIndex;
            nodeType = node;
            Number = number;
            Postfix = postfix;
        }
         public Item(int elementIndex, NodeType node, int hierarchy, string number, string postfix, List<Indent> indents, List<Item> items)
        {
            ElementIndex = elementIndex;
            nodeType = node;
            Number = number;
            Postfix = postfix;
            Indents = indents;
            Items = items;
        }
        public Item() { }
        public bool IsExists(int index) => Indents.FirstOrDefault(f=>f.ElementIndex == index) != null || this.ElementIndex == index;
        public string Number { get; set; }
        public string Postfix { get; set; }
        public List<Indent> Indents {get;set;} = new List<Indent>();
        public List<Item> Items {get;set;}
    }
}
