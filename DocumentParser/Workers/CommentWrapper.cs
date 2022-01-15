using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Services.Documents.Settings;
using System;
using System.Collections.Generic;

namespace Services.Documents.Parser.Workers
{
    public struct CommentWrapper
    {
        public CommentWrapper(OpenXmlElement e, ISettings sett, DataExtractor extractor, WordProperties props)
        {
            Element = e as Comment;
            this.id = Element.Id;
            this.Auhtor = Element.Author;
            this.Date = Element.Date;
            this.Initials = Element.Initials;
            Paragraphs = new List<string>();
            foreach(var p in Element.ChildElements)
            {
                if(p.GetType() == typeof(Paragraph))
                {
                    var wrap = new ParagraphWrapper(p, sett, extractor, props);
                    string indent = "";
                    foreach(var r in wrap.RunWrapper.GetRuns())
                    {
                        indent += r.Text;
                    }
                    Paragraphs.Add(indent);
                }   
            }
        }
        public string id {get;}
        public Comment Element {get;}
        public string Auhtor {get;set;}
        public DateTime? Date {get;set;}
        public string Initials {get;set;}
        public List<string> Paragraphs {get;}
        public Services.Documents.Core.DocumentElements.Comment ToComment => new Services.Documents.Core.DocumentElements.Comment(Paragraphs, Auhtor, Initials, Date);
        
    }

}
