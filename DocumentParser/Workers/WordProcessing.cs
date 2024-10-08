﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.DocumentElements;
using DocumentParser.DocumentElements.MetaInformation;
using DocumentParser.Elements;
using Lexer;
using SettingsWorker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace DocumentParser.Workers
{
    public class WordProcessing : Parsers.ParserBase, IDisposable
    {
        List<ElementStructure> ElementsList { get; } = new List<ElementStructure>();
        public List<CommentWrapper> Comments {get;}= new List<CommentWrapper>();
        /// <summary>
        /// Выносим имаджи сюда, потому что если присутствуют тяжелые рисунки и надо загрузить весь док, то это может длиться очень долго
        /// поэтому запрашиваем загрузку рисунка только когда необходимо, а на фронт сливаем облегченный вариант
        /// </summary>
        /// <typeparam name="Image"></typeparam>
        /// <returns></returns>
        public List<Image> DocumentImages {get;set;} = new List<Image>();
        public int ImagesLenth => DocumentImages.Where(w=>w.Data != null).Sum(s=>s.Data.Length);
        public List<ElementStructure> GetElementsList => ElementsList;
        public DataExtractor DataExtractor {get;set;}
        public WordProperties Properties { get; set;}
        public WordprocessingDocument Document {get;set;}
        public Styles StylePart { get; set; }
        Body Body {get;set;}
        public string FullText {get;set;}
        private string _docxPath {get; set;}
        /// <summary>
        /// Дефолтные настройки преопределяются после парсера реквизитов,
        /// Там мы можем предопределить настройки для нужных пар Орган/вид документа
        /// </summary>
        /// <value></value>
        public ISettings Settings {get;}
        public WordProcessing(ISettings _settings)
        {
            Settings = _settings;
        }
        public WordProcessing()
        {
        
        }
        private FileStream file {get;set;}
        public async ValueTask LoadDocument(string documentPath)
        {
            try
            {
                _docxPath = documentPath;
                for (int i=1; i <= 10; ++i) 
                {
                    try 
                    {
                        file = File.Open(_docxPath, FileMode.Open);
                        break; 
                    }
                    catch (IOException e) when (i <= 10) 
                    {
                        Thread.Sleep(1000);
                    }
                }
                if(file == null)
                {
                    AddError($"Ошибка чтения файла {documentPath} файл открыт в другой программе");
                    return;
                } 
                Document = WordprocessingDocument.Open(file, true);
                StylePart = Document.MainDocumentPart.StyleDefinitionsPart.Styles;
                Body = Document.MainDocumentPart.Document.Body;
                DataExtractor = new DataExtractor(Document.MainDocumentPart, Settings);
                Properties = new WordProperties(StylePart, Settings);
                SearchComments();
                await ProcessDocument(Document);

            }
            catch (Exception ex)
            {
                AddError(ex);
            }
        }
        public List<ElementStructure> GetParagrapsByComment(string comment)
        {
            var com = Comments.FirstOrDefault(f=>f.ToComment.Values.Contains(comment));
            if(com.id != null)
                return ElementsList.Where(w=>w.Comments.Contains(com.id)).ToList();
            else return new List<ElementStructure>();
        }
         

        private void SearchComments()
        {
            //Добавляем коментарии документа
            if(Document.MainDocumentPart.WordprocessingCommentsPart != null)
            {
                var com = Document.MainDocumentPart.WordprocessingCommentsPart.Comments.Elements<DocumentFormat.OpenXml.Wordprocessing.Comment>();
                foreach(var c in com)
                {
                    Comments.Add(new CommentWrapper(c, Settings, DataExtractor, Properties));
                }
            }
        }
        public IEnumerable<ElementStructure> NotParsedElements => ElementsList.Where(w=>!w.IsParsed);
        bool hl = false;
        public void SetChange(IEnumerable<ElementStructure> pars)
        {
            foreach(var p in pars)
            {
                    if(hl)
                    {
                        foreach(var r in p.WordElement.Element.Descendants<Run>())
                        {
                            if(r.RunProperties == null)
                                r.RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties(){Highlight = new DocumentFormat.OpenXml.Wordprocessing.Highlight { Val = HighlightColorValues.Yellow }};
                            else
                                r.RunProperties.Highlight = new DocumentFormat.OpenXml.Wordprocessing.Highlight { Val = HighlightColorValues.Yellow };
                        }
                    }
                    if(p.NodeType != NodeType.Таблица)
                        SetElementNode(p, NodeType.Абзац);
                    p.IsChange = true;
            }
        }
        public void SaveDocument(string path, bool close = false)
        {
            var sd = Document.SaveAs(path);
            if(close)
                sd.Close();
        }
     

        public NodeType SetElementNode(OpenXmlElement p, NodeType nodeType)
        {
                var e = ElementsList.FirstOrDefault(f => f.WordElement.Element.Equals(p));
                if (e != null)
                    e.NodeType = nodeType;
            return nodeType;
        }
        public ElementStructure SetMetaNode(ElementStructure el, NodeType nodeType, MetaAction action, string info, bool full)
        {
            el.NodeType = nodeType;
            el.MetaInfo = new MetaInfo(action, info, full);
            return el;
        }
        public ElementStructure GetElementNode(OpenXmlElement el) => ElementsList.FirstOrDefault(f=>f.WordElement.Element == el);
        
        List<CommentRange> runsWithComments = new List<CommentRange>();
        /// <summary>
        /// Установка индексов элемена в бодике
        /// </summary>
        /// <param name="content"></param>
        /// <param name="wDoc"></param>
        async ValueTask ProcessDocument(WordprocessingDocument doc)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var elements = Body.Elements<OpenXmlElement>();
                    int count = 0;
                    int all = elements.Count();
                    int paragraphStartIndex = 0;
                    int currentElement = 0;
                    int arrayIndex = 0;
                    foreach(var par in elements)
                    {
                        foreach (var el in par)
                        {
                            if(el.GetType() == typeof(CommentRangeStart))
                            {
                                var id = ((CommentRangeStart)el).Id;
                                var end = Body.Descendants<CommentReference>().FirstOrDefault(f=>f.Id == id);
                                if(end == null)
                                {
                                    AddError($"Не могу найти окончание комментария {id}");
                                    break;
                                }
                                //Находим все раны в параграфе после начала коммента
                                var parsFrom = el.ElementsAfter().OfType<Run>().ToList();
                                //находим все раны в прараграфах которые идут после нашего параграфа
                                parsFrom.AddRange(par.ElementsAfter().SelectMany(s=>s.Descendants<Run>()));
                                var currentRunsWithCommentRange = new List<Run>();
                                //если в параграфе встречаем наш стоп ран (ран в котором находится CommentReference) то останавливаем цикл
                                foreach(var pp in parsFrom)
                                {
                                    if(pp == end.Parent)
                                        break;
                                    currentRunsWithCommentRange.Add(pp);
                                }
                                //Если есть вложенные комменты, то при каждой итерации заменяыем комменты на вложенные
                                for(int r = currentRunsWithCommentRange.Count -1; r >= 0; r--)
                                {
                                    for(int i = 0; i < runsWithComments.Count; i++)
                                    {
                                        if(runsWithComments[i].Run == currentRunsWithCommentRange[r])
                                        {
                                            runsWithComments[i] = new CommentRange(){CommentId = ((CommentRangeStart)el).Id, Run = currentRunsWithCommentRange[r]};
                                            currentRunsWithCommentRange.RemoveAt(r);
                                        }
                                    }        
                                }
                                //добавляем все раны что не были обнаружены в списке вложенных комментов
                                runsWithComments.AddRange(currentRunsWithCommentRange.Select(s=> new CommentRange(){CommentId = id, Run = s}));
                            }    
                        }
                        var p = new ParagraphWrapper(par, Settings, DataExtractor, Properties, runsWithComments, Comments, DocumentImages);
                        if(p.IsParagraph)
                        {
                            if(!p.IsEmpty)
                            {

                                ElementsList.Add(new ElementStructure(ElementsList, arrayIndex) 
                                {  
                                    ElementIndex = count,
                                    WordElement = p,
                                    StartIndex = paragraphStartIndex,
                                    Length = p.Length,
                                    ParagraphProperties = Properties.ExtractParagraphProperties(par)
                                });
                                arrayIndex++;
                                count++;
                            }
                            FullText += p.Text + "\n";
                            paragraphStartIndex += p.Length+1;
                        }
                        if (p.IsTable)
                        {
                            ElementsList.Add(new ElementStructure(ElementsList, arrayIndex) 
                            { 
                                ElementIndex = count,
                                WordElement = p,
                                StartIndex = paragraphStartIndex,
                                Length = p.Length,
                                NodeType = NodeType.Таблица
                            });
                            arrayIndex++;
                            count++;
                            var parsInTable = p.Element.Descendants<Paragraph>();
                            foreach(var tpar in parsInTable)
                            {
                                var twrap = new ParagraphWrapper(tpar, Settings, DataExtractor, Properties, runsWithComments, Comments, DocumentImages);
                                if(!p.IsEmpty)
                                {
                                    ElementsList.Add(new ElementStructure(ElementsList, arrayIndex) 
                                    {
                                        ElementIndex = -1,
                                        WordElement = twrap,
                                        StartIndex = paragraphStartIndex,
                                        Length = twrap.Length,
                                        NodeType = NodeType.АбзацТаблицы,
                                        ParagraphProperties = Properties.ExtractParagraphProperties(tpar)
                                    });
                                    arrayIndex++;

                                }
                                FullText += twrap.Text + "\n";
                                paragraphStartIndex += twrap.Length+1;
                            }
                        }
                        currentElement++;
                        UpdateStatus("Предобработка текста", elements.Count(), currentElement);
                    }
                    //ProcessComments();
                    AddError(DataExtractor);
                    AddError(Properties);
                }
                catch (Exception ex)
                {
                    AddError(ex);
                }
            });
        }

        public void SetElementNode(ITextIndex txt, NodeType nodeType)
        {
            var e = GetElements(txt);
            e.ForEach(f=>f.NodeType = nodeType);
        }
        public void SetElementNode(ElementStructure el, NodeType nodeType) => el.NodeType = nodeType;
       
        public List<ElementStructure> GetElements(ITextIndex txt)
        {
            List<ElementStructure> p = new List<ElementStructure>();
            try
            {
                var elements = ElementsList.LastOrDefault(w=> w.StartIndex <= txt.StartIndex);
                if(elements != null)
                {
                    var lenght = elements.Length;
                    var index = elements.CurrentIndex;
                    int mod = 1;
                    p.Add(elements);
                    while(txt.Length > lenght && ElementsList.Count > (index + mod))
                    {
                        var next = ElementsList[(index + mod)];
                        if(next != null)
                        {
                            lenght = next.Length;
                            p.Add(next);
                            mod++;
                        }
                    }
                }
                return p;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Result<ElementStructure> GetElement(ITextIndex txt)
        {
            var element = ElementsList.LastOrDefault(w=> w.StartIndex <= txt.StartIndex);
            if(element == null)
                return Result<ElementStructure>.Err($"Элемент с индексом {txt.StartIndex} не найден!"); 
            return Result<ElementStructure>.Ok(element);
        }
        // public ElementStructure GetElement(ITextIndex txt) 
        // => ElementsList.LastOrDefault(w=> w.StartIndex <= txt.StartIndex);  
     
        public IEnumerable<ElementStructure> GetElements(NodeType node) => ElementsList.Where(w=>w.NodeType == node);
        
        public List<ElementStructure> GetElements(int from, int to)
        {
            var elements = ElementsList.Where(w=> w.ElementIndex >= from && w.ElementIndex <= to);
                return elements.ToList();
        }
        public Result<ElementStructure> GetElement(int index)
        {
            var element = ElementsList.FirstOrDefault(f=>f.ElementIndex == index);
            if(element == null)
                return Result<ElementStructure>.Err($"Элемент с порядковым номером абзаца в структуре документа № {index} не найден");
            return Result<ElementStructure>.Ok(element);
        }
        
        List<int> getRange(int startIndex, int length)
        {
            //var range0 = startIndex..length;
            List<int> range = new List<int>();
            for(int i = startIndex; i <= (length + startIndex); i++)
            {
                range.Add(i);
            }
            return range;
        }

        public void DeleteText(ITextIndex txt)
        {
            {
                var pars = GetElements(txt);
                var currentCharCount = 0;
                var range = getRange(txt.StartIndex, txt.Length);
                if(pars.Count > 0)
                {
                    var startDelIndex = txt.StartIndex - pars[0].StartIndex;
                    foreach (var par in pars)
                    {
                        foreach (var r in par.GetRuns())
                        {
                            foreach (var t in r.Elements<Text>())
                            {
                                var currentText = "";
                                foreach(var ch in t.Text)
                                {
                                    if(!range.Contains(currentCharCount))
                                        currentText += ch;
                                    currentCharCount++;
                                }
                                t.Text = currentText;
                            }
                        }
                    }
                }
            }
        }
        public void DeleteText(ElementStructure el,  ITextIndex txt)
        {
            {
                var currentCharCount = 0;
                var range = getRange(txt.StartIndex, txt.Length);
                var startDelIndex = txt.StartIndex - el.StartIndex;
                foreach (var r in el.GetRunElements())
                {
                    var currentText = "";
                    foreach(var ch in r.Text)
                    {
                        if(!range.Contains(currentCharCount))
                            currentText+=ch;
                        currentCharCount++;
                    }
                    r.Text = currentText;
                }
            }
        }
        
        /// <summary>
        /// Получение параграфов по начальному и конечному индексу вхождения
        /// </summary>
        /// <param name="startIndex">начальный индекс</param>
        /// <param name="endIndex">конечный индекс</param>
        /// <returns></returns>
        // public List<Paragraph> GetParagraphsByIndex(int startIndex, int endIndex) =>
        //     GetParagraphsByTextIndex(startIndex, endIndex - startIndex);
        
        public string GetUnicodeString(ITextIndex txt)
        {
            string text = null;
            var paragraphs = GetElements(txt);
            if(paragraphs.Count > 0)
                text = GetUnicodeString(paragraphs, txt);
            return text;
        }
        /// <summary>
        /// получение части параграфа если индекс вхождения расчитывается с нуля
        /// поиск для конкретного параграфа и все, не для текста в целом.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        // public string GetLocalUnicodeString(ElementStructure par, ITextIndex txt)
        // {
        //     var str = GetUnicodeString(par);
        //     var ret = str.Substring(txt.StartIndex, txt.Length);
        //     return ret;
        // }

        string GetUnicodeString(List<ElementStructure> paragraphs, ITextIndex txt)
        {
            var text = "";
            var matchChars = new List<char>();
            try
            {
                if (paragraphs.Count > 0)
                {
                    var paragraphStartGlobalIndex = paragraphs[0].StartIndex;
                    var startIndex = (txt.StartIndex - paragraphStartGlobalIndex);
                    if(startIndex < 0)
                        throw new Exception($"Ошибка при поиске части вхождения в параграфах. Индекс вхождения {txt.StartIndex} меньше индекса первого параграфа {paragraphStartGlobalIndex}");
                    foreach (var n in paragraphs)
                    {
                        text += GetUnicodeString(n);
                    }
                    if(startIndex == 0 && text.Length == txt.Length)
                        return text;
                    for(int i = 0; i < text.Count(); i++)
                    {
                        if(i >= startIndex && matchChars.Count < txt.Length)
                            matchChars.Add(text[i]);
                    }
                    text = new string(matchChars.ToArray());
                    return text;
                }
                else return "";
            }
            catch (Exception ex)
            {
                AddError(ex);
                return "";
            }
        }

        public string GetUnicodeString(ElementStructure paragraph, ITextIndex txt)
        {
            try
            {
                var str = GetUnicodeString(paragraph);
                var ret = str.Substring(txt.StartIndex, txt.Length);
                return ret;
            }
            catch (Exception ex)
            {
                AddError(ex);
                return "";
            }
        }

        /// <summary>
        /// Конвертирование параграфов в строку с заменой стиля seperscript на символ юникода
        /// </summary>
        /// <param name="paragraphs"></param>
        /// <returns></returns>
        public string GetUnicodeString(ElementStructure paragraph)
        {
            var txt = "";
            try
            {
                var par = paragraph.WordElement.Element;
                //Было  OType но заменил на Descendants так как если в параграфе встречается гиперлинк то
                //то у него ран лежит внутри, и OfType такое не берет
                var runs = par.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>();
                foreach (var r in runs)
                {
                   var runProperties = Properties.ExtractRunProperties(r);
                   foreach(var el in r.ChildElements)
                   {
                        if(el.GetType() == typeof(Text))
                            txt += DataConverter.ConvertText((el as Text).Text, runProperties).Item1;
                        if(el.GetType() == typeof(Break))
                            txt += SettingsWorker.Regexes.Templates.BRChar;
                   }
                 }
                return txt;
            }
            catch (Exception ex)
            {
                AddError(ex);
                return "";
            }
        }
        
    public void Dispose()
    {
        Document.Close();
        Document.Dispose();
        file.Close();
        file.Dispose();
    }
    }
}
