using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Services.Documents.Core;
using Services.Documents.Parser.Regexes;
using System.Collections.Generic;
using System.Linq;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using RunProperties = Services.Documents.Core.DocumentElements.RunProperties;
using Comment = Services.Documents.Core.DocumentElements.Comment;
using Services.Documents.Core.DocumentElements;
using Services.Documents.Settings;

namespace Services.Documents.Parser.Workers
{
    
    public class RunElement : Services.Documents.Core.DocumentElements.Run
    {
        public RunElement(bool commentStart,
         bool commentEnd,
         string commentId,
         Comment comment,
         string text,
         RunProperties properties,
         Image image,
         //int imagePosition,
         //int formulaPosition,
         string formulaLatexFormat,
         string formulaMathMlFormat,
         ISettings sett)
        {
            CommentStart = commentStart;
            CommentEnd = commentEnd;
            CommentId = commentId;
            Comment = comment;
            Text = text;
            Properties = properties;
            Image = image;
            FormulaLatexFormat = formulaLatexFormat;
            FormulaMathMlFormat = formulaMathMlFormat;
            settings = sett;
        }
        public bool CommentStart {get;set;}
        public bool CommentEnd {get;set;}
        public string CommentId {get; set;}
        
        public bool HaveRun => run != null;
        public bool HaveComment => Comment != null;
        public bool CanAdd => HaveFormula || HaveImage || (HaveRun && Text != "") || HaveComment;
        public RunElement Clone()
        {
            return new RunElement(CommentStart, CommentEnd, CommentId, Comment, Text, Properties, Image, FormulaLatexFormat, FormulaMathMlFormat, settings);
        }
       // WordProperties properties{get;}
        //DataExtractor extractor {get;}
        Run run {get;}
        List<RunElement> currentArray {get;}
        ISettings settings {get;}
        public RunElement(OpenXmlElement el, ISettings sett,  WordProperties props, DataExtractor extractor, List<RunElement> arr, List<Image> runImages = null)
        {
            currentArray = arr;
            settings = sett;
            if(el.GetType() == typeof(DocumentFormat.OpenXml.Math.OfficeMath))
            {
                var f = extractor.ExtractMathOffice((el as DocumentFormat.OpenXml.Math.OfficeMath).OuterXml);
                FormulaLatexFormat = f.LatexFormat;
                FormulaMathMlFormat = f.MathMlFormat;
            } 
            if(el.GetType() == typeof(CommentRangeStart))
            {
                CommentId = (el as CommentRangeStart).Id;
            }
            if(el.GetType() == typeof(CommentRangeEnd))
            {
                CommentId = (el as CommentRangeEnd).Id;
            }
            if(el.GetType() == typeof(Run))
            {
                var bef = el.ElementsBefore().LastOrDefault();
                if(bef!= null && bef.GetType() == typeof(CommentRangeStart))
                    CommentStart = true;
                var aft = el.ElementsAfter().FirstOrDefault();
                if(aft!= null && aft.GetType() == typeof(CommentRangeEnd))
                    CommentEnd = true;
                run = el as Run;
                Properties = props.ExtractRunProperties(run);
                foreach(var r in run)
                {
                    if(r.GetType() == typeof(Text))
                    {
                        var converted = DataConverter.ConvertText((r as Text).Text, Properties);
                        if(converted.Item2)
                            Properties.VerticalAligment = null;
                        Text += converted.Item1;
                    } 
                    if(r.GetType() == typeof(Break))
                        Text += Templates.BRChar;
                    if(r.GetType() == typeof(CommentReference))
                    {
                        CommentId = (r as CommentReference).Id;
                    }
                    //Те изображения которые занимают меньше 240 кб храним прямо в теле, остальные храним в отдельной базе
                    //Для изображений больше 240 кб храним тумбы размером 256х256 для предпросмотра
                    if(r.GetType() == typeof(Picture))
                    {
                        var img = new Image(Image = extractor.GetImage((r as Picture)), Id);
                        if(img.Length <= sett.Current.Settings.MaxBodyImageSize)
                            Image = new Image(img, Id);
                        else if(runImages != null)
                        {
                            runImages.Add(img);
                            Image = new Image(Id, img.Length, img.Thumbland);
                        }
                    } 
                    if(r.GetType() == typeof(Drawing))
                    {
                        var img = new Image(Image = extractor.GetImage((r as Drawing)), Id);
                        if(img.Length <= sett.Current.Settings.MaxBodyImageSize)
                            Image = new Image(img, Id);
                        else if(runImages != null)
                        {
                            runImages.Add(img);
                            Image = new Image(Id, img.Length, img.Thumbland);
                        }
                    }
                }
            }

        }
        
    }

    public struct RunWrapper
    {
        public RunWrapper(OpenXmlElement p, ISettings sett, WordProperties props, DataExtractor extractor, List<Image> runImages = null)
        {
            if(p.GetType() == typeof(Paragraph))
            {
                
                Runs = new List<RunElement>();
                var tempRuns = new List<RunElement>();
                foreach(var el in p)
                {
                    var wrap = new RunElement(el, sett, props, extractor, tempRuns, runImages);
                    if(wrap.CanAdd)
                    {
                        tempRuns.Add(wrap);
                    }
                }
                for(int r = 0; r < tempRuns.Count; r++)
                {
                    var currentRun = tempRuns[r];
                    var equals = tempRuns.Skip(r).TakeWhile(t =>
                                t.Properties != null &&
                                t.Properties.Equals(currentRun.Properties)
                                && !t.HaveImage
                                && !t.HaveFormula).Skip(1)
                                .ToList();
                    if(equals.Count > 1)
                    foreach(var e in equals)
                    {
                        currentRun.Text += e.Text;
                        if(currentRun.CommentId == null)
                            currentRun.CommentId = e.CommentId;
                        r++;
                    }
                    Runs.Add(currentRun);
                }
            }
            else
            {
                Runs = new List<RunElement>();
            }
            CutRuns = new List<RunElement>();
        }
        List<RunElement> Runs {get;}
        List<RunElement> CutRuns {get;}
        public void SetComment(Comment comment)
        {
            // foreach(var r in Runs)
            // {
            //     r.Comment = comment;
            // }
            var items = Runs.Where(w=>w.CommentStart || w.CommentEnd);
            if(items.Count() == 0)
                items = Runs;
            var startCommentItem = items.FirstOrDefault();
            var endCommentItem = items.LastOrDefault();
            if(startCommentItem != null && endCommentItem != null)
            {                                                                     // +1 потому что берем по колчичеству а не по индексам
                var between =  Runs.Skip(Runs.IndexOf(startCommentItem)).Take((Runs.IndexOf(endCommentItem) - Runs.IndexOf(startCommentItem)) + 1);
                foreach(var b in between)
                {
                    b.CommentStart = true;
                    b.Comment = comment;
                }
            }
            //Удаляем все элементы с пустым текстом и индексом комментов
            Runs.RemoveAll(r=>r.Text == "" && r.CommentId != null);
        }
        public Comment Comment => Runs.FirstOrDefault(f=>f.Comment != null)?.Comment;
        public List<string> Comments => Runs.Where(w=>w.HaveComment).Select(s=>s.CommentId).ToList();
        public bool HaveFormula => Runs.Any(c=>c.HaveFormula);
        public bool HaveImage => Runs.Any(c=>c.HaveImage);
        public bool CommentStart => Runs.Any(a=>a.CommentStart);
        public List<RunElement> GetRuns() => Runs;
        public List<Services.Documents.Core.DocumentElements.Run> GetCustRuns() => Runs.Cast<Services.Documents.Core.DocumentElements.Run>().ToList();
        public List<RunElement> GetRuns(int indentStartIndex)
        {
            var currentCount = 0;
            foreach(var r in Runs)
            {
                string runText = "";
                foreach(var c in r.Text)
                {
                    if(currentCount >= indentStartIndex)
                    {
                        runText += c;
                    }
                    currentCount++;
                }
                var cl = r.Clone();
                cl.Text = runText;
                CutRuns.Add(cl);
            }
            return CutRuns;
        }
        public List<Services.Documents.Core.DocumentElements.Run> GetCustRuns(int indentStartIndex)
        {
            return GetRuns(indentStartIndex).Cast<Services.Documents.Core.DocumentElements.Run>().ToList();
        }
    }

}
