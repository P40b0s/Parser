using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentParser.DocumentElements
{
    public abstract class StructureNodeBase
    {
        public StructureNodeBase()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; }
       // public Guid DocumentId { get; set; }
        public int ElementIndex { get; set; }
        public NodeType nodeType { get; set; }
        
    }
}
