using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Linq;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using RunProperties = DocumentParser.DocumentElements.RunProperties;
using Comment = DocumentParser.DocumentElements.Comment;
using DocumentParser.DocumentElements;
using SettingsWorker;
using SettingsWorker.Regexes;

namespace DocumentParser.Workers
{
    
    public class RunElement : DocumentParser.DocumentElements.Run
    {
        public RunElement(
         string commentId,
         string text,
         RunProperties properties,
         Image image,
         //int imagePosition,
         //int formulaPosition,
         string formulaLatexFormat,
         string formulaMathMlFormat,
         ISettings sett)
        {
            //CommentStart = commentStart;
            //CommentEnd = commentEnd;
            CommentId = commentId;
            Text = text;
            Properties = properties;
            Image = image;
            FormulaLatexFormat = formulaLatexFormat;
            FormulaMathMlFormat = formulaMathMlFormat;
            settings = sett;
        }
        //public bool CommentStart {get;set;}
        //public bool CommentEnd {get;set;}
        public string CommentId {get; set;}
        public bool HaveRun => run != null;
        public bool HaveComment => CommentId != null;
        public bool CanAdd => HaveFormula || HaveImage || (HaveRun && Text != "") || HaveComment;
        public RunElement Clone()
        {
            return new RunElement(CommentId, Text, Properties, Image, FormulaLatexFormat, FormulaMathMlFormat, settings);
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
            // if(el.GetType() == typeof(CommentRangeStart))
            // {
            //     CommentId = (el as CommentRangeStart).Id;
            // }
            // if(el.GetType() == typeof(CommentRangeEnd))
            // {
            //     CommentId = (el as CommentRangeEnd).Id;
            // }
            if(el.GetType() == typeof(Run))
            {
                //var bef = el.ElementsBefore().LastOrDefault();
                //if(bef!= null && bef.GetType() == typeof(CommentRangeStart))
                //    CommentStart = true;
                //var aft = el.ElementsAfter().FirstOrDefault();
                //if(aft!= null && aft.GetType() == typeof(CommentRangeEnd))
                //    CommentEnd = true;
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
                    //if(r.GetType() == typeof(CommentReference))
                    //{
                    //    CommentId = (r as CommentReference).Id;
                    //}
                    //TODO убираем это, все храним в одном джсоне
                    //Те изображения которые занимают меньше 240 кб храним прямо в теле, остальные храним в отдельной базе
                    //Для изображений больше 240 кб храним тумбы размером 256х256 для предпросмотра
                    if(r.GetType() == typeof(Picture))
                    {
                        var i = extractor.GetImage((r as Picture));
                        if(i.IsOk)
                            Image = new Image(i.Value, Id);
                        else
                            System.Console.WriteLine(i.Error);
                    } 
                    if(r.GetType() == typeof(Drawing))
                    {
                        var i = extractor.GetImage((r as Drawing));
                        if(i.IsOk)
                            Image = new Image(i.Value, Id);
                        else
                            System.Console.WriteLine(i.Error);
                    }
                }
            }

        }
        
    }

    public struct RunWrapper
    {
        public RunWrapper(OpenXmlElement p, ISettings sett, WordProperties props, DataExtractor extractor, List<CommentRange> commentRange, List<CommentWrapper> comments, List<Image> runImages = null)
        {
            if(p.GetType() == typeof(Paragraph))
            {
                Runs = new List<RunElement>();
                var tempRuns = new List<RunElement>();
                foreach(var el in p)
                {
                    // if(el.GetType() == typeof(CommentRangeStart))
                    // {
                    //     localCommentRange = true;
                    //     commentId = ((CommentRangeStart)el).Id;
                    // }
                    // if(el.GetType() == typeof(CommentRangeEnd))
                    // { 
                    //     localCommentRange = false;
                    //     commentId = "";
                    // }
                    var wrap = new RunElement(el, sett, props, extractor, tempRuns, runImages);
                    var runInComment = commentRange.FirstOrDefault(f=>f.Run == el);
                    if(runInComment.CommentId != null)
                    {
                        wrap.CommentId = runInComment.CommentId;
                        //wrap.Comment = comments.FirstOrDefault(f=>f.id == runInComment.CommentId).ToComment;
                    }
                       
                    //if(commentRange.HaveCommentRange)
                    // {
                    //    wrap.CommentId = commentRange.CommentId;
                    // }
                    // if(!commentRange.HaveCommentRange && localCommentRange)
                    // {
                    //     wrap.CommentId = commentId;
                    //     wrap.Comment = comments.FirstOrDefault(f=>f.id == commentId).ToComment;
                    // }
                    
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
                                && !t.HaveFormula
                                && string.IsNullOrEmpty(t.CommentId)).Skip(1)
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
        // public void SetComment(Comment comment)
        // {
        //     // foreach(var r in Runs)
        //     // {
        //     //     r.Comment = comment;
        //     // }
        //     var items = Runs.Where(w=>!string.IsNullOrEmpty(w.CommentId));
        //     if(items.Count() == 0)
        //         items = Runs;
        //     var startCommentItem = items.FirstOrDefault();
        //     var endCommentItem = items.LastOrDefault();
        //     if(startCommentItem != null && endCommentItem != null)
        //     {                                                                     // +1 потому что берем по колчичеству а не по индексам
        //         var between =  Runs.Skip(Runs.IndexOf(startCommentItem)).Take((Runs.IndexOf(endCommentItem) - Runs.IndexOf(startCommentItem)) + 1);
        //         foreach(var b in between)
        //         {
        //             b.CommentStart = true;
        //             b.Comment = comment;
        //         }
        //     }
        //     //Удаляем все элементы с пустым текстом и индексом комментов
        //     Runs.RemoveAll(r=>r.Text == "" && r.CommentId != null);
        // }
        //public List<Comment> Comments => Runs.Where(f=>f.Comment != null).Select(s=>s.Comment).ToList();
        public List<string> Comments => Runs.Where(w=>w.HaveComment).Select(s=>s.CommentId).ToList();
        public bool HaveFormula => Runs.Any(c=>c.HaveFormula);
        public bool HaveImage => Runs.Any(c=>c.HaveImage);
        public List<RunElement> GetRuns() => Runs;
        public List<DocumentParser.DocumentElements.Run> GetCustRuns() => Runs.Cast<DocumentParser.DocumentElements.Run>().ToList();
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
        public List<DocumentParser.DocumentElements.Run> GetCustRuns(int indentStartIndex)
        {
            return GetRuns(indentStartIndex).Cast<DocumentParser.DocumentElements.Run>().ToList();
        }
    }

}
