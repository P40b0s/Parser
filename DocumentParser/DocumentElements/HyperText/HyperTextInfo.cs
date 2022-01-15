
using System;
using System.Collections.Generic;

namespace DocumentParser.DocumentElements.HyperText
{
    public struct HyperTextInfo
    {
        public HyperTextInfo(Guid id)
        {
            linkObject = new LinkObject(id);
            hyperLinks = null;
        }
        public HyperTextInfo(Guid id, int linkStartIndex ,int linkLength, string target)
        {
            hyperLinks = new List<HyperLink>();
            hyperLinks.Add(new HyperLink(id, linkStartIndex, linkLength, target));
            linkObject = null;
        }
        public bool isLinkObject => linkObject != null;
        public bool isHyperLink => hyperLinks != null;
        public bool hasValue => isLinkObject || isHyperLink;
        public LinkObject linkObject {get;}
        public List<HyperLink> hyperLinks {get;}
    }
}
