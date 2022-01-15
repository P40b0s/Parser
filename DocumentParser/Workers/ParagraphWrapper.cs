using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.DocumentElements;
using Services.Documents.Parser.Regexes;
using Services.Documents.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentParser.Workers
{
    public struct ParagraphWrapper
    {
        WordProperties props {get;}
        DataExtractor extractor {get;}
        public ParagraphWrapper(OpenXmlElement e, ISettings sett, DataExtractor extractor, WordProperties props)
        {
            Element = e;
            this.props = props;
            this.extractor = extractor;
            RunWrapper = new RunWrapper(e, sett, props, extractor);
            //var commnetRangeStart = Element.ChildElements.OfType<CommentRangeStart>().FirstOrDefault()?.Id;
            //var commnetRangeEnd = Element.ChildElements.OfType<CommentRangeEnd>().FirstOrDefault();
            //Поставим первый попавшийся коммент из ранов
            CommentId = RunWrapper.Comments.FirstOrDefault();
        }
        public ParagraphWrapper(OpenXmlElement e, ISettings sett,  DataExtractor extractor, WordProperties props, List<Image> runImages = null)
        {
            Element = e;
            this.props = props;
            this.extractor = extractor;
            RunWrapper = new RunWrapper(e, sett, props, extractor, runImages);
            //var commnetRangeStart = Element.ChildElements.OfType<CommentRangeStart>().FirstOrDefault()?.Id;
            //var commnetRangeEnd = Element.ChildElements.OfType<CommentRangeEnd>().FirstOrDefault();
            //Поставим первый попавшийся коммент из ранов
            CommentId = RunWrapper.Comments.FirstOrDefault();
        }
        public RunWrapper RunWrapper {get;}
        public bool IsParagraph => Element.GetType() == typeof(Paragraph);
        public bool IsTable => Element.GetType() == typeof(Table);
        public bool IsBold => props.IsBold(Element);
        public OpenXmlElement Element {get;}
        /// <summary>
        /// Используется для работы с токенами, для извлечения структуры текста необходимо тиспользовать RunWrapper
        /// </summary>
        /// <value></value>
        public string Text
        {
            get
            {
                var txt = "";
                foreach(var r in Element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>())
                {
                    foreach(var e in r.ChildElements)
                    {
                        if(e.GetType() == typeof(Text))
                            txt += (e as Text).Text;
                        if(e.GetType() == typeof(Break))
                            txt+=Templates.BRChar;
                    }
                }
                return txt;
                //String.Join("", Element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>().Select(s=>s.InnerText));
                //'\u00ad'
            }
        }
        public DocumentFormat.OpenXml.Math.OfficeMath Formula => Element.Descendants<DocumentFormat.OpenXml.Math.OfficeMath>().FirstOrDefault();
        public bool ContainsFormula => RunWrapper.HaveFormula;
        public bool ContainsImage => RunWrapper.HaveImage;
        public bool ContainsText => !String.IsNullOrEmpty(Text);
        public bool IsEmpty => String.IsNullOrWhiteSpace(Text) && !ContainsFormula && !ContainsImage;
        public int Length => ContainsText ? Text.Length : 0;
        public string CommentId {get;}
        
    }

}
