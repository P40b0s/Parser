using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentParser.DocumentElements
{
    public class DocumentTable : StructureNodeBase
    {
        public DocumentTable(int elementIndex, TableProperties properties, List<TRow> rows)
        {
            ElementIndex = elementIndex;
            nodeType = NodeType.Таблица;
            Properties = properties;
            Rows= rows;
        }
        public TableProperties Properties { get; set; } = new TableProperties();
        public List<TRow> Rows { get; set; } = new List<TRow>();
    }
    
   

}
