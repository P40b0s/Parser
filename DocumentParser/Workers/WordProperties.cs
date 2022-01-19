using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Services.Documents.Core;
using DocumentParser.DocumentElements;
using Services.Documents.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Borders = DocumentParser.DocumentElements.Borders;
using ParagraphProperties = DocumentParser.DocumentElements.ParagraphProperties;
using RunProperties = DocumentParser.DocumentElements.RunProperties;
using TableAlignment = DocumentParser.DocumentElements.TableAlignment;
using TablePosition = DocumentParser.DocumentElements.TablePosition;

namespace DocumentParser.Workers
{
    public struct RunPropertiesStruct
    {
        public RunPropertiesStruct(DocumentFormat.OpenXml.Wordprocessing.Run _run, RunProperties _props)
        {
            run = _run;
            props = _props;
        }
        public DocumentFormat.OpenXml.Wordprocessing.Run run { get; }
        public RunProperties props { get; }
        bool Equals(RunPropertiesStruct other)
        {
            if (this.run.Equals(other.run))
                return true;
            else
                return false;
        }
        public override bool Equals(object obj)
        {
            return Equals((RunPropertiesStruct)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (run.GetHashCode());
                return hash;
            }
        }
    }
    // public interface IWordPropertiesService
    // {
    //     Core.TableProperties ExtractTableProperties(Table table, Styles s);
    //     TRowProperties ExtractRowProperties(TableRow row, Styles s);
    //     TCellProperties ExtractCellProperties(TableCell cell, Styles s);
    //     ParagraphProperties ExtractParagraphProperties(Paragraph pr, Styles styles);
    //     RunProperties ExtractRunProperties(DocumentFormat.OpenXml.Wordprocessing.Run run, Styles styles);
    //     RunProperties ExtractRunProperties(StyleRunProperties rsp, Styles styles);
    //     bool IsBold(Paragraph p, Styles s);
    // }
    public class WordProperties
    {
        public List<Exception> Errors = new List<Exception>();
        private Styles styles {get;}
        ISettings settings {get;}
        public WordProperties(Styles stylePart, ISettings _settings)
        {
            styles = stylePart;
            settings = _settings;
        }

        #region Методы для которых требуются свойства параграфа или рана
        public bool IsBold(OpenXmlElement p)
        {
            var par = ExtractParagraphProperties(p);
            var runs = p.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>().Select(r => ExtractRunProperties(r)).All(a => a.IsBold);
            return runs || (par?.RProperties?.IsBold ?? false);
        }

        public Style GetStyle(string styleid)
        {
            Style outStyle = null;
            var attribute = new OpenXmlAttribute("w", "styleId", @"http://schemas.openxmlformats.org/wordprocessingml/2006/main", styleid);
            foreach (var s in styles)
            {
                var attr = s.GetAttributes().FirstOrDefault(f => f.Equals(attribute));
                if (attr.Value != null)
                    outStyle = (Style)s;
            }
            return outStyle;
        }

        #endregion

        #region table!
        public Core.DocumentElements.TableProperties ExtractTableProperties(OpenXmlElement table)
        {
            var returnProperties = new Core.DocumentElements.TableProperties();
            try
            {
                DocumentFormat.OpenXml.Wordprocessing.TableProperties tProps = table.OfType<DocumentFormat.OpenXml.Wordprocessing.TableProperties>().SingleOrDefault();
                if (tProps != null)
                {
                    var st = tProps.Elements<TableStyle>().FirstOrDefault();
                    var tStyle = (st != null && GetStyle(st.Val) != null) ? GetStyle(st.Val).StyleTableProperties : null;

                    var borders = tProps.TableBorders ?? tStyle?.TableBorders;
                    var jc = tProps.TableJustification ?? tStyle?.TableJustification;
                    var w = tProps.TableWidth;
                    var ind = tProps.TableIndentation ?? tStyle?.TableIndentation;
                    var pos = tProps.TablePositionProperties;
                    if (borders != null)
                    {
                        returnProperties.TableBorders = new Borders();
                        if (borders.TopBorder != null && borders.TopBorder.Val != (BorderValues.None | BorderValues.Nil))
                            returnProperties.TableBorders.Top = Core.DocumentElements.BorderType.single;
                        if (borders.BottomBorder != null && borders.BottomBorder.Val != (BorderValues.None | BorderValues.Nil))
                            returnProperties.TableBorders.Bottom = Core.DocumentElements.BorderType.single;
                        //Для 2003 left для 2007+ start
                        if (borders.StartBorder != null && borders.StartBorder.Val != (BorderValues.None | BorderValues.Nil))
                            returnProperties.TableBorders.Left = Core.DocumentElements.BorderType.single;
                        if (borders.LeftBorder != null && borders.LeftBorder.Val != (BorderValues.None | BorderValues.Nil))
                            returnProperties.TableBorders.Left = Core.DocumentElements.BorderType.single;
                        if (borders.EndBorder != null && borders.EndBorder.Val != (BorderValues.None | BorderValues.Nil))
                            returnProperties.TableBorders.Right = Core.DocumentElements.BorderType.single;
                        if (borders.RightBorder != null && borders.RightBorder.Val != (BorderValues.None | BorderValues.Nil))
                            returnProperties.TableBorders.Right = Core.DocumentElements.BorderType.single;
                    }
                    if (jc != null)
                    {
                        if (jc.Val == TableRowAlignmentValues.Center)
                            returnProperties.Alignment = TableAlignment.center;
                        if (jc.Val == TableRowAlignmentValues.Left)
                            returnProperties.Alignment = TableAlignment.left;
                        if (jc.Val == TableRowAlignmentValues.Right)
                            returnProperties.Alignment = TableAlignment.right;
                    }
                    if (w != null)
                    {
                        if (w.Type == TableWidthUnitValues.Dxa)
                            returnProperties.TableWidth = DataConverter.DxaToPixels(w.Width.Value);
                        if (w.Type == TableWidthUnitValues.Pct)
                            returnProperties.TableWidth = DataConverter.PtToPixels(w.Width.Value);

                    }
                    if (ind != null)
                    {
                        if (ind.Type == TableWidthUnitValues.Dxa)
                            returnProperties.TableIndent = DataConverter.DxaToPixels(ind.Width.Value);
                        if (ind.Type == TableWidthUnitValues.Pct)
                            returnProperties.TableIndent = DataConverter.PtToPixels(ind.Width.Value);
                    }
                    if (pos != null)
                    {
                        returnProperties.Position = new TablePosition();
                        if (pos.LeftFromText.HasValue)
                            returnProperties.Position.LeftFromText = pos.LeftFromText.Value;
                        if (pos.RightFromText.HasValue)
                            returnProperties.Position.RightFromText = pos.RightFromText.Value;
                        if (pos.TopFromText.HasValue)
                            returnProperties.Position.TopFromText = pos.TopFromText.Value;
                        if (pos.BottomFromText.HasValue)
                            returnProperties.Position.BottomFromText = pos.BottomFromText.Value;
                    }
                }
                return returnProperties;
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
                return null;
            }
        }
        public Core.DocumentElements.TRowProperties ExtractRowProperties(TableRow row)
        {
            var returnProperties = new Core.DocumentElements.TRowProperties();
            try
            {
                var rProps = row.Elements<TableRowProperties>().SingleOrDefault();
                if (rProps != null)
                {
                    var hg = rProps.Elements<TableRowHeight>().FirstOrDefault();
                    var jc = rProps.Elements<TableJustification>().FirstOrDefault();
                    if (hg != null)
                    {
                        //if (hg.HeightType == HeightRuleValues.Exact)
                        returnProperties.Height = DataConverter.DxaToPixels(hg.Val.Value);
                    }
                    if (jc != null)
                    {
                        if (jc.Val == TableRowAlignmentValues.Center)
                            returnProperties.Alignment = TableAlignment.center;
                        if (jc.Val == TableRowAlignmentValues.Left)
                            returnProperties.Alignment = TableAlignment.left;
                        if (jc.Val == TableRowAlignmentValues.Right)
                            returnProperties.Alignment = TableAlignment.right;
                    }
                }
                return returnProperties;
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
                return null;
            }
        }
        public Core.DocumentElements.TCellProperties ExtractCellProperties(TableCell cell)
        {
            var returnProperties = new Core.DocumentElements.TCellProperties();
            try
            {
                var cProps = cell.Elements<TableCellProperties>().SingleOrDefault();
                if (cProps != null)
                {
                    var w = cProps.TableCellWidth;
                    var borders = cProps.TableCellBorders;

                    var gridSpan = cProps.Elements<GridSpan>().FirstOrDefault();
                    var vMerge = cProps.Elements<VerticalMerge>().FirstOrDefault();
                    var vAlign = cProps.Elements<TableCellVerticalAlignment>().FirstOrDefault();
                    if (gridSpan != null)
                    {
                        returnProperties.GridSpan = gridSpan.Val.Value;
                    }
                    returnProperties.RowSpan = CalcRowspan(cell);
                    if (vMerge != null)
                    {
                        if (vMerge.Val != null && vMerge.Val == "restart")
                            returnProperties.IsFirstRowSpan = true;
                        if (vMerge.Val == null)
                            returnProperties.IsBodyRowSpan = true;
                    }
                    if (vAlign != null && vAlign.Val != null)
                    {
                        var align = vAlign.Val.Value;
                        returnProperties.VAlign = (Core.DocumentElements.TableVerticalAlignmentValues)Enum.Parse(typeof(Core.DocumentElements.TableVerticalAlignmentValues), Enum.GetName(typeof(DocumentFormat.OpenXml.Wordprocessing.TableVerticalAlignmentValues), align));
                    }
                    if (borders != null)
                    {
                        returnProperties.CellBorders = new Core.DocumentElements.Borders();
                        if (borders.TopBorder != null)
                            returnProperties.CellBorders.Top = GetBorderType(borders.TopBorder.Val);
                        if (borders.BottomBorder != null)
                            returnProperties.CellBorders.Bottom = GetBorderType(borders.BottomBorder.Val);
                        //Для 2003 left для 2007+ start
                        if (borders.StartBorder != null)
                            returnProperties.CellBorders.Left = GetBorderType(borders.StartBorder.Val);
                        if (borders.LeftBorder != null)
                            returnProperties.CellBorders.Left = GetBorderType(borders.LeftBorder.Val);
                        if (borders.EndBorder != null)
                            returnProperties.CellBorders.Right = GetBorderType(borders.EndBorder.Val);
                        if (borders.RightBorder != null)
                            returnProperties.CellBorders.Right = GetBorderType(borders.RightBorder.Val);
                    }
                    if (w != null)
                    {
                        if (w.Type == TableWidthUnitValues.Dxa)
                            returnProperties.CellWidth = DataConverter.DxaToPixels(w.Width.Value);
                        if (w.Type == TableWidthUnitValues.Pct)
                            returnProperties.CellWidth = DataConverter.PtToPixels(w.Width.Value);

                    }
                }
                return returnProperties;
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
                return null;
            }
        }

        #region Calc table cell VMerge
        int CalcColIndex(TableCell cell)
        {
            var c0 = cell.Elements<TableCellProperties>().FirstOrDefault()?.Elements<GridSpan>().FirstOrDefault();
            return cell.Elements<TableCellProperties>().FirstOrDefault()?.Elements<GridSpan>().FirstOrDefault() == null ? 1 :
                    cell.Elements<TableCellProperties>().FirstOrDefault()?.Elements<GridSpan>().FirstOrDefault().Val;
        }
        int CalcRowspan(OpenXmlElement cell)
        {
            int rowspan = 1, colNum = 0;
            var currentRow = cell.Parent;
            //Сколько столбцов до этой ячейки
            foreach (var tc in cell.ElementsBefore().OfType<TableCell>())
            {
                colNum += CalcColIndex(tc);
            }
            bool endOfSpan = false;
            foreach (var row in currentRow.ElementsAfter().OfType<TableRow>())
            {
                int currentColNum = 0;
                foreach (var tc in row.Elements<TableCell>())
                {
                    //if (tc.LocalName != "w:tc")
                    //    continue;
                    if (currentColNum == colNum)
                    {
                        if (tc.GetFirstChild<TableCellProperties>().GetFirstChild<VerticalMerge>() != null)
                        {
                            if (tc.GetFirstChild<TableCellProperties>().GetFirstChild<VerticalMerge>().Val != "restart")
                            {
                                rowspan++;
                                break;
                            }
                        }
                        endOfSpan = true;
                    }
                    currentColNum += CalcColIndex(tc);
                }
                if (endOfSpan)
                    break;
            }
            return rowspan;
        }

        #endregion

        private Core.DocumentElements.BorderType GetBorderType(BorderValues val) =>

            val switch
            {
                BorderValues.None => Core.DocumentElements.BorderType.none,
                BorderValues.Nil => Core.DocumentElements.BorderType.nill,
                BorderValues.Single => Core.DocumentElements.BorderType.single,
                _ => Core.DocumentElements.BorderType.fromParent
            };
        #endregion


        #region Извлечение свойств параграфов и ранов

        public ParagraphProperties ExtractParagraphProperties(OpenXmlElement pr)
        {
            ParagraphProperties returnProps = null;
            var currentProps = (pr as Paragraph).ParagraphProperties;
            if (currentProps != null)
            {
                returnProps = new ParagraphProperties();
                try
                {
                    var st = currentProps.Elements<ParagraphStyleId>().FirstOrDefault();
                    var style = GetStyle(st?.Val);
                    var parStyle = style?.StyleParagraphProperties;
                    var runStyle = style?.StyleRunProperties;
                    var jc = currentProps.Justification ?? parStyle?.Justification;
                    returnProps.Alignment = GetTextAlignment(jc);
                    var ind = currentProps.Indentation ?? parStyle?.Indentation;
                    if (ind != null)
                    {
                        returnProps.Ind = new Core.DocumentElements.Indentation()
                        {
                            FirstLine = DataConverter.DxaToPixels(ind.FirstLine?.Value ?? "0"),
                            Left = DataConverter.DxaToPixels((ind.Left?.Value ?? ind.Start?.Value) ?? "0"),
                            Right = DataConverter.DxaToPixels((ind.Right?.Value ?? ind.End?.Value) ?? "0")
                        };
                    }
                    var sp = currentProps.SpacingBetweenLines ?? parStyle?.SpacingBetweenLines;
                    if (sp != null)
                    {
                        returnProps.Spacing = new ParagraphSpacing()
                        {
                            After = DataConverter.TwipsToPixels(sp.After ?? "0"),
                            Line = DataConverter.TwipsToPixels(sp.Line ?? "0"),
                            LineRule = sp.LineRule
                        };
                    }
                    //var rp = currentProps.ParagraphMarkRunProperties;
                    if (runStyle != null)
                    {
                        var rProps = ExtractRunProperties(runStyle);
                        returnProps.RProperties = rProps;
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add(ex);
                    return null;
                }
            }
            return returnProps;
        }

        TextAlignmentEnum GetTextAlignment(DocumentFormat.OpenXml.Wordprocessing.Justification jc)
        {
            var a = TextAlignmentEnum.justify;
            if (jc != null)
            {
                switch (jc.Val.InnerText)
                {
                    default:
                    case "justify":
                        {
                            return TextAlignmentEnum.justify;
                        }
                    case "center":
                        {
                            return TextAlignmentEnum.center;
                        }
                    case "right":
                        {
                            return TextAlignmentEnum.right;

                        }
                    case "left":
                        {
                            return TextAlignmentEnum.left;
                        }

                }
            }
            return a;
        }

        RunFontsType GetFontsType(RunFonts fonts)
        {
            var f = new RunFontsType();
            if (fonts != null)
                f = new RunFontsType()
                {
                    Ascii = fonts.Ascii,
                    ComplexScript = fonts.ComplexScript,
                    EastAsia = fonts.EastAsia,
                    HighAnsi = fonts.HighAnsi
                };
            return f;
        }


        public RunProperties ExtractRunProperties(StyleRunProperties rsp)
        {

            return ExtractRP(null, rsp);
        }
        public RunProperties ExtractRunProperties(DocumentFormat.OpenXml.Wordprocessing.Run run)
        {

            return ExtractRP(run);
        }

        private RunProperties ExtractRP(DocumentFormat.OpenXml.Wordprocessing.Run run, StyleRunProperties rsp = null)
        {
            RunProperties returnProps = null;
            var properties = run?.RunProperties;
            if (properties != null || rsp != null)
            {
                returnProps = new RunProperties();
                try
                {
                    var st = run?.RunProperties.Elements<RunStyle>().FirstOrDefault();
                    var runStyle = rsp ?? (GetStyle(st?.Val) != null ? GetStyle(st?.Val).StyleRunProperties : null);
                    var verAlgn = properties?.VerticalTextAlignment?.Val.Value ?? runStyle?.VerticalTextAlignment?.Val.Value;
                    if (verAlgn != null)
                    {
                        if (verAlgn == VerticalPositionValues.Superscript)
                            returnProps.VerticalAligment = "superscript";
                        if (verAlgn == VerticalPositionValues.Subscript)
                            returnProps.VerticalAligment = "subscript";

                    }
                    //returnProps.IsBold = properties?.Bold != null || runStyle?.Bold?.Val?.Value == true || runStyle?.Bold != null;
                    returnProps.IsBold = IsTrue(properties?.Bold, runStyle?.Bold);
                    returnProps.RunFonts = GetFontsType(properties?.RunFonts ?? runStyle?.RunFonts);
                    var sps = properties?.Spacing?.Val ?? runStyle?.Spacing?.Val;
                    if (sps != null)
                    {
                        returnProps.Spacing = new RunSpacing();
                        returnProps.Spacing.Val = DataConverter.DxaToPixels(sps.Value);
                    }
                    //returnProps.Caps = properties?.Caps != null || runStyle?.Caps != null;
                    returnProps.Caps = IsTrue(properties?.Caps, runStyle?.Caps);
                    //returnProps.IsItalic = properties?.Italic != null || runStyle?.Italic != null;
                    returnProps.IsItalic = IsTrue(properties?.Italic, runStyle?.Italic);
                    //returnProps.SmallCaps = properties?.SmallCaps != null || runStyle?.SmallCaps != null;
                    returnProps.SmallCaps = IsTrue(properties?.SmallCaps, runStyle?.SmallCaps);
                    returnProps.Color = properties?.Color?.Val ?? runStyle?.Color?.Val;
                    int defFontSize = 27;
                    var fs = properties?.FontSize?.Val ?? runStyle?.FontSize?.Val;
                    if (fs != null)
                        defFontSize = int.Parse(fs);
                    returnProps.FontSize = defFontSize;
                }
                catch (Exception ex)
                {
                    Errors.Add(ex);
                    return null;
                }
            }
            return returnProps;
        }

        private bool IsTrue<T, S>(T props, S style) where T:  OnOffType
                                                    where S:  OnOffType
        {
            bool p = false;
            if (props != null)
            {
                if (props.Val != null)
                    p = props.Val;
                else p = true;
            }
            bool s = false;
            if (style != null)
            {
                if (style.Val != null)
                    s = style.Val;
                else s = true;
            }
            return p || s;
        }

        #endregion
    }
}
