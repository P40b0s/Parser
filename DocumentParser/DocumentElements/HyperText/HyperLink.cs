
using System;

namespace DocumentParser.DocumentElements.HyperText
{
    public class HyperLink : LinkObject
    {
        public HyperLink(Guid id, int linkStartIndex ,int linkLength, string target)
         : base(id)
        {
            LinkStartIndex = linkStartIndex;
            LinkLength = linkLength;
            Target = target;
        }
        public int LinkStartIndex {get; set;}
        public int LinkLength {get;}
        public string Target {get; set;}
    }
}
