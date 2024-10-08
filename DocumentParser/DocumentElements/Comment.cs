﻿using System;
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
    public class Comment 
    {
        public Comment(string id, List<string> values, string subject, string initials, DateTime? date)
        {
            Id = id;
            Values = values;
            Subject = subject;
            Initials = initials;
            Date = date;
        }
        public string Id {get;}
        public List<string> Values {get;}
        public string Subject {get;}
        public string Initials {get;}
        public DateTime? Date{get;}
        public override string ToString()
        {
            return string.Join("\n", Values);
        }
    }
}
