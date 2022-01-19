using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentParser.DocumentElements
{
    public class PublicationInfo
    {
        public string PublicationNumber {get;set;}
        public DateTime PublicationDate {get;set;}
        public List<string> PublicationPoints {get;set;}
    }
}
