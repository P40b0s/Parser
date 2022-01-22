using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using DocumentParser.DocumentElements;
using SettingsWorker;
using Utils;

namespace DocumentParser.Workers
{    
    public struct FormulaResult
    {
        public FormulaResult(string latexFormat, string mathMlFormat)
        {
            LatexFormat = latexFormat;
            MathMlFormat = mathMlFormat;
        }
        public string LatexFormat { get; set; }
        public string MathMlFormat { get; set; }
    }
    
    public class DataExtractor
    {
        public List<Exception> Errors { get; set; } = new List<Exception>();
        const string MATHML = @"OMML2MML.XSL";
        const string LATEX = @"mmltex.xsl";
        private string DocumentsDir { get; }
        private string FilesRootDirectory { get; }
        private string RootDirectory { get; }
        private MainDocumentPart part {get;}
        ISettings settings {get;}
        int thumbSizeX {get;}
        int thumbSizeY {get;}
        
        public DataExtractor(MainDocumentPart prt, ISettings _settings)
        {
            part = prt;
            settings = _settings;
            DocumentsDir = settings.Paths.DocumentsDirectory;
            FilesRootDirectory = settings.Paths.RootDirectory;
            RootDirectory = settings.Paths.RootDirectory;
            //ставим true иначе XslCompiledTransform не подгружает зависимые файлы схем
            AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);
            thumbSizeX = 256;
            thumbSizeY = 256;
        }
        public Result<DocumentParser.DocumentElements.Image, ParserException> GetImage(Drawing drawing)
        {
            try
            {
                if (drawing.Inline != null)
                {
                    var imageFirst = drawing.Inline.Graphic.GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();
                    var blip = imageFirst.BlipFill.Blip.Embed?.Value;
                    if(blip == null)
                    {
                        var nopic = imageFirst.ChildElements.OfType<DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties>().FirstOrDefault();
                        if(nopic != null)
                        {
                            var nopicprops = nopic.ChildElements.OfType<DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties>().FirstOrDefault();
                            if(nopicprops != null)
                                return new Result<Image, ParserException>(new ParserException($"Изображение {nopicprops.Name.Value} по адресу {nopicprops.Description.Value} не найдено"));
                            else
                                return new Result<Image, ParserException>(new ParserException($"Изображение не найдено {imageFirst.BlipFill.Blip.OuterXml}"));
                        }   
                        else
                            return new Result<Image, ParserException>(new ParserException($"Изображение не найдено {imageFirst.BlipFill.Blip.OuterXml}"));
                    }
                    var extent = drawing.Inline.GetFirstChild<Extent>();
                    long x = 0;
                    long y = 0;
                    
                    if (extent != null)
                    {

                        x = DataConverter.EmuToPixels((double)extent.Cx);
                        y = DataConverter.EmuToPixels((double)extent.Cy);
                    }
                    //string folder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    ImagePart img = (ImagePart)part.GetPartById(blip);
                    //string imageFileName = string.Empty;
                    //the image is stored in a zip file code behind, so it must be extracted
                    // byte[] buffer = new byte[16 * 1024];
                    using (Stream s = img.GetStream())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[s.Length];
                        int read;
                        while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        var th = GetReducedImage(thumbSizeX, thumbSizeY, ms);
                        return new Result<Image, ParserException>(new Image(Guid.Empty, ms.ToArray(), x, y, new Thumb(th, thumbSizeX, thumbSizeY)));
                    }
                }
                if(drawing.Anchor != null)
                {
                    var graph = drawing.Anchor.Elements<DocumentFormat.OpenXml.Drawing.Graphic>().FirstOrDefault();
                    if(graph!= null)
                    {
                        var imageFirst = graph.GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();
                        var blip = imageFirst.BlipFill.Blip.Embed.Value;
                        var extent = drawing.Anchor.GetFirstChild<Extent>();
                        long x = 0;
                        long y = 0;
                        
                        if (extent != null)
                        {

                            x = DataConverter.EmuToPixels((double)extent.Cx);
                            y = DataConverter.EmuToPixels((double)extent.Cy);
                        }
                        //string folder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                        ImagePart img = (ImagePart)part.GetPartById(blip);
                        //string imageFileName = string.Empty;
                        //the image is stored in a zip file code behind, so it must be extracted
                        // byte[] buffer = new byte[16 * 1024];
                        using (Stream s = img.GetStream())
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] buffer = new byte[s.Length];
                            int read;
                            while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                            var th = GetReducedImage(thumbSizeX, thumbSizeY, ms);
                            return new Result<Image, ParserException>(new Image(Guid.Empty, ms.ToArray(), x, y, new Thumb(th, thumbSizeX, thumbSizeY)));
                        }
                    }
                }
                throw new Exception("Какая то ошибка связанная с извлечением изображения");
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
                return new Result<Image, ParserException>(new ParserException(ex.Message));
            }
        }
        public Result<DocumentParser.DocumentElements.Image, ParserException> GetImage(Picture pic)
        {
            try
            {
                CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";
                var hPattern = new Regex(@"(height:)(?<val>(\d{1,}[.]\d{1,})|(\d{1,}))+", RegexOptions.Compiled);
                var wPattern = new Regex(@"(width:)(?<val>(\d{1,}[.]\d{1,})|(\d{1,}))+", RegexOptions.Compiled);
                var sh = pic.OfType<DocumentFormat.OpenXml.Vml.Shape>().FirstOrDefault();
                var st = sh.Style.Value;
                double w = 0, h = 0;
                var wMatch = wPattern.Match(st);
                if (wMatch.Success)
                    w = double.Parse(wMatch.Groups["val"].Value, NumberStyles.Any, ci);
                var hMatch = hPattern.Match(st);
                if (hMatch.Success)
                    h = double.Parse(hMatch.Groups["val"].Value, NumberStyles.Any, ci);
                var imagedata = sh.OfType<ImageData>().FirstOrDefault();
                var r = imagedata.RelationshipId.Value;
                //var r = imagedata.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                var images = part.ImageParts;
                var img = part.GetPartById(r);
                long x = DataConverter.PtToPixels(w);
                long y = DataConverter.PtToPixels(h);
                using (Stream s = img.GetStream())
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[s.Length];
                    int read;
                    while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    var th = GetReducedImage(thumbSizeX, thumbSizeY, ms);
                    return new Result<Image, ParserException>(new Image(Guid.Empty, ms.ToArray(), x, y, new Thumb(th, thumbSizeX, thumbSizeY)));
                }
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
                return new Result<Image, ParserException>(new ParserException(ex.Message));
            }
        }

        public byte[] GetReducedImage(int width, int height, Stream resourceImage)
        {
            var image = System.Drawing.Image.FromStream(resourceImage);
            var thumb = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
            using (var ms = new MemoryStream())
            {
                thumb.Save(ms,thumb.RawFormat);
                return  ms.ToArray();
            }
        }

        // public bool SaveImage(DocumentParser.DocumentElements.Image img, Guid docId, Guid runId)
        // {
        //     try
        //     {
        //         var pic = JsonConvert.SerializeObject(img);
        //         using (FileStream fs = new FileStream(System.IO.Path.Combine(FilesRootDirectory, DocumentsDir, docId.ToString(), runId.ToString() + ".img"), FileMode.Create))
        //         using (StreamWriter sw = new StreamWriter(fs))
        //         {
        //             sw.Write(pic);
        //         }
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Errors.Add(ex);
        //         return false;
        //     }
           
        // }
       
        public FormulaResult ExtractMathOffice(string folmulaOuterXml)
        {
            string officeML = string.Empty;
            string output = String.Empty;
            string mathMlXlt = @$"{RootDirectory}/cfg/formulas/{MATHML}";
            string latexXlt = @$"{RootDirectory}/cfg/formulas/{LATEX}";
            try
            {
                XslCompiledTransform xslTransform = new XslCompiledTransform();

                // The OMML2MML.xsl file is located under 
                // %ProgramFiles%\Microsoft Office\Office15\

                xslTransform.Load(mathMlXlt);

                using (TextReader tr = new StringReader(folmulaOuterXml))
                {
                    // Load the xml of your main document part.
                    using (XmlReader reader = XmlReader.Create(tr))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            XmlWriterSettings settings = xslTransform.OutputSettings.Clone();

                            // Configure xml writer to omit xml declaration.
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            settings.OmitXmlDeclaration = false;

                            XmlWriter xw = XmlWriter.Create(ms, settings);

                            // Transform our OfficeMathML to MathML.
                            xslTransform.Transform(reader, xw);
                            ms.Seek(0, SeekOrigin.Begin);

                            using (StreamReader sr = new StreamReader(ms, Encoding.UTF8))
                            {
                                officeML = sr.ReadToEnd();
                                // Console.Out.WriteLine(officeML);
                            }
                        }
                    }
                }
                
                XslCompiledTransform latexTrans = new XslCompiledTransform();
                latexTrans.Load(latexXlt); //this is the stylesheet
                                           // Configure xml writer to omit xml declaration.
                                           //latexTrans.Transform("myMMLfile.mml", "outputTeXfile.tex");
               
                using (TextReader tr = new StringReader(officeML))
                {
                    // Load the xml of your main document part.
                    using (XmlReader reader = XmlReader.Create(tr))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            XmlWriter xw = XmlWriter.Create(ms, latexTrans.OutputSettings);

                            // Transform our OfficeMathML to MathML.
                            latexTrans.Transform(reader, xw);
                            ms.Seek(0, SeekOrigin.Begin);

                            using (StreamReader sr = new StreamReader(ms, Encoding.UTF8))
                            {
                                output = sr.ReadToEnd();
                                // Console.Out.WriteLine(officeML);
                            }
                        }
                    }
                }
                var MLbase64 = System.Text.Encoding.UTF8.GetBytes(officeML);
                var Latexbase64 = System.Text.Encoding.UTF8.GetBytes(output);
                return new FormulaResult(Convert.ToBase64String(Latexbase64), Convert.ToBase64String(MLbase64));
            }
            catch (Exception ex)
            {
                Errors.Add(ex);
                return new FormulaResult("ОШИБКА ПРЕОБРАЗОВАНИЯ ФОРМУЛЫ", "ОШИБКА ПРЕОБРАЗОВАНИЯ ФОРМУЛЫ");
            }
            
               
        }

    }
}
