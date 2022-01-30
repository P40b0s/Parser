using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Services.Documents.Parser;
using Test.Regexes.Actualizer.Source;

namespace Actualizer;
public static class ParagraphsExtension
{
    public static void CopyParagraphStyle(this OpenXmlElement par, Paragraph target, SourceDocumentParserResult source, Services.Documents.Parser.Parsers.DocumentParser parser)
    {
        var pPr = par.Elements<DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties>().FirstOrDefault();
        if(pPr?.ParagraphStyleId != null)
        {
            var style = (par as Paragraph).ParagraphProperties.ParagraphStyleId;
            if(style != null)
            {
                var st = source.Parser.word.Properties.GetStyle(style.Val);
                var style0 = st.CloneNode(true);
                var exists = parser.word.StylePart.StylesPart.Styles.OfType<Style>().FirstOrDefault(f=>f.StyleId == target.ParagraphProperties.ParagraphStyleId.Val);
                if(exists!= null)
                    parser.word.StylePart.StylesPart.Styles.RemoveChild(exists);
                parser.word.StylePart.StylesPart.Styles.Append(style0);
            }              
        }
    }
    public static void CopyAllRunsStyles(this Paragraph par, SourceDocumentParserResult source, Services.Documents.Parser.Parsers.DocumentParser parser)
    {
        foreach(var rr in par.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>())
        {
            var rprops= rr.Elements<DocumentFormat.OpenXml.Wordprocessing.RunProperties>().FirstOrDefault();
            if(rprops != null && rprops.RunStyle != null)
            {
                var styleId = rprops.RunStyle.Val;
                var runStyle = source.Parser.word.Properties.GetStyle(styleId);
                var runStyleClone = runStyle.CloneNode(true);
                var exists2 = parser.word.StylePart.StylesPart.Styles.OfType<Style>().FirstOrDefault(f=>f.StyleId == styleId);
                if(exists2!= null)
                        parser.word.StylePart.StylesPart.Styles.RemoveChild(exists2);
                parser.word.StylePart.StylesPart.Styles.Append(runStyleClone);
                //styleRunProperties1.Append(rprops.ChildElements);
                //style0.Append(styleRunProperties1);
            }
        }
    }

    public static void CopyAllImages(this Paragraph par, SourceDocumentParserResult source, Services.Documents.Parser.Parsers.DocumentParser parser)
    {
        par.Descendants<DocumentFormat.OpenXml.Drawing.Blip>()
        .ToList().ForEach(blip =>
        {
            var p = source.Parser.word.Document.MainDocumentPart.GetPartById(blip.Embed) as ImagePart;
            var newPart = parser.word.Document.MainDocumentPart.AddPart(p);
            newPart.FeedData(p.GetStream());
            var newRel =  parser.word.Document.MainDocumentPart.GetIdOfPart(newPart);
            blip.Embed = newRel;
        });
        par.Descendants<DocumentFormat.OpenXml.Vml.ImageData>()
        .ToList().ForEach(imageData =>
        {
            var p = source.Parser.word.Document.MainDocumentPart.GetPartById(imageData.RelationshipId) as ImagePart;
            var newPart = parser.word.Document.MainDocumentPart.AddPart(p);
            newPart.FeedData(p.GetStream());
            var newRel =  parser.word.Document.MainDocumentPart.GetIdOfPart(newPart);
            imageData.RelationshipId = newRel;
        });
    }
}

public static class RunsExtension
{
    public static DocumentFormat.OpenXml.Wordprocessing.Run GetRun(this ElementStructure element, int inIndex)
    {
        var runs = element.GetRuns();
        int index = 0;
        foreach(var r in runs)
        {
            foreach(var t in r.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
            {
                for(int i = index; i < t.Text.Length; i++)
                {
                    if(i == inIndex - index)
                        return r;
                }
            }
            index = r.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Sum(s=>s.Text.Length);
        }
        return null;
    }
    /// <summary>
    /// Разделение рана
    /// </summary>
    /// <param name="run">Ран который необходимо разрезать</param>
    /// <param name="startIndex">индекс в котором необходимо разделить ран</param>
    /// <param name="newRun">Ран который нужно вставить между разделенным раном (он булет посередине)</param>
    /// <returns></returns>
    public static List<DocumentFormat.OpenXml.OpenXmlElement> Split(this DocumentFormat.OpenXml.Wordprocessing.Run run, int startIndex, DocumentFormat.OpenXml.OpenXmlElement newRun = null)
    {
        var runs = new List<DocumentFormat.OpenXml.OpenXmlElement>();
        foreach(var t in run.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
        {
            for(int i = 0; i < t.Text.Length; i++)
            {
                if(i == startIndex)
                {
                    var run1 = run.CloneNode(true);
                    run1.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().First().Text = t.Text.Substring(0, startIndex);
                    var run2 = run.CloneNode(true);
                    run2.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().First().Text = t.Text.Substring(startIndex, t.Text.Length);
                    runs.Add(run1);
                    if(newRun != null)
                        runs.Add(newRun);
                    runs.Add(run2);
                }
            }
        }
        return runs;
    }

    public static DocumentFormat.OpenXml.Wordprocessing.Run DeleteText(this ElementStructure element, int startIndex, int endIndex)
    {
        var runs = element.GetRuns();
        DocumentFormat.OpenXml.Wordprocessing.Run runWithText = null;
        int delCount = 0;
        foreach(var r in runs)
        {
            //удаляем символы пока не дойдем до нужного количества
            foreach(var t in r.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
            {
                for(int i = 0; i < t.Text.Length; i++)
                {
                    if(i == startIndex)
                        runWithText = r;
                    if(i >= startIndex && i < endIndex)
                    {
                        t.Text = t.Text.Remove(i, 1);
                        delCount--;
                        i--;
                    }
                    else
                    if(delCount  < 0 && (delCount + (endIndex - startIndex)) > 0)
                    {
                        t.Text = t.Text.Remove(i, 1);
                        delCount--;
                        i--;
                    }
                }
            }
        }
        return runWithText;
    }
}