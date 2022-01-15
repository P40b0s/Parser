using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using System.Threading.Tasks;
using DocumentParser.DocumentElements;
using DocumentParser.TokensDefinitions;
using DocumentParser.Parsers.Requisites;
using System.Linq;
using DocumentParser.Parsers.Headers;
using DocumentParser.Parsers.Annex;
using DocumentParser.Parsers.Items;
using Utils.Extensions;
using System.IO;

namespace DocumentParser.Parsers
{
    //центральный модуль парсинга документа, один на каждый док, отсюда вызываются все остальные модули
    public class DocumentParser : ParserBase<DocumentToken>
    {
        ISettings settings {get;}
        public DocumentParser(string filePath)
        {
            this.filePath = filePath;
            settings = new Settings();
            word = new WordProcessing(settings);
        }
        public Document document {get;} = new Document();
        public WordProcessing word {get;}
        //RequisiteTokensModel tokensModel {get;set;}
        private string filePath {get;}
        
        public async ValueTask<bool> ParseDocument()
        {
            if(string.IsNullOrEmpty(filePath))
            {
                exceptions.Add(new ParserException("Не выбран файл для обработки"));
                return false;
            }
            var f = new FileInfo(filePath);
            document.FileName = f.Name;
            await word.LoadDocument(filePath);
            
            if(word.Errors.Count > 0)
            {
                foreach(var e in word.Errors)
                    exceptions.Add(new ParserException(e.Message));
                return false;
            }
            var tokens = lexer.Tokenize(word.FullText, new DocumentsTokensDefinition());
            //tokensModel = new RequisiteTokensModel(tokens);
            var requisites = new RequisitesParser(word, tokens.ToList(), document);
            requisites.UpdateCallback+= c => Status(c, true);
            requisites.ErrorCallback+= e => Error(e);
            requisites.Parse();
            exceptions.AddRange(requisites.exceptions);
            if(exceptions.Count > 0)
                return false;
            var changesParser = new ChangesParser(word);
            changesParser.UpdateCallback+= c => Status(c, true);
            changesParser.ErrorCallback+= e => Error(e);
            changesParser.Parse();
            exceptions.AddRange(changesParser.exceptions);
            var metaParser = new MetaParser(word);
            metaParser.UpdateCallback+= c => Status(c, true);
            metaParser.ErrorCallback+= e => Error(e);
            metaParser.Parse();
            exceptions.AddRange(metaParser.exceptions);
            var annexParser = new AnnexParser(word);
            annexParser.UpdateCallback+= c => Status(c, true);
            annexParser.ErrorCallback+= e => Error(e);
            annexParser.Parse();
            exceptions.AddRange(annexParser.exceptions);
            var headersParser = new HeadersParser(word, annexParser, requisites);
            headersParser.UpdateCallback+= c => Status(c, true);
            headersParser.ErrorCallback+= e => Error(e);
            headersParser.Parse();
            exceptions.AddRange(headersParser.exceptions);
            //
            //FIXME проблемы с выборкой итемов из примечаний и сносок
            //необходимо это делать после поиска заголовков, чтоб проставить конечные точки
            var footNodeParser = new FootNoteParser(word);
            footNodeParser.UpdateCallback+= c => Status(c, true);
            footNodeParser.ErrorCallback+= e => Error(e);
            footNodeParser.Parse();
            exceptions.AddRange(footNodeParser.exceptions);
            //Таблицу ищем после футнотов а футноты после хедеров и приложений...
            //замкнутый круг
            var tableParser = new TableParser(word);
            tableParser.Parse();
            headersParser.GetTables();
            headersParser.GetFootNotes();
            //находим все итемы подитемы итд...
            Status("Обработка списочных элементов...", true);
            var itemsParser = new ItemsParser(word);
            headersParser.BodyItems = itemsParser.Parse(headersParser.BodyRootElements);
            var count = headersParser.Headers.Count() + annexParser.Annexes.Count();
            var percentage = 0;
            foreach(var h in headersParser.Headers)
            {
                h.Header.Items = itemsParser.Parse(h.RootElements);
                percentage++;
                Percentage("Обработка списочных элементов...", count, percentage, true);
            }
            foreach(var a in annexParser.Annexes)
            {
                a.Annex.Items = itemsParser.Parse(a.RootElements);
                foreach(var h in a.Headers)
                {
                    //a/Annex.Headers уже является сслкой на h/Header
                    h.Header.Items = itemsParser.Parse(h.RootElements);
                }
                percentage++;
                Percentage("Обработка списочных элементов...", count, percentage, true);
            }
            //Перемещаем все неопознаные элементы из рутов хедеров в абзацы хедеров
            foreach(var h in headersParser.Headers)
            {
                foreach(var i in h.RootElements)
                {
                    //для элементов определенных как абзац
                    //Часть 31 статьи 1 Федерального закона от 26 декабря 2008 года № 294-ФЗ.....
                    //дополнить пунктом 24 следующего содержания:
                    //элемент имеет флаг IsChange и определен как абзац поэтому добавляем условие для абзаца
                    if(i.NodeType == NodeType.НеОпределено || i.NodeType == NodeType.Абзац || i.NodeType == NodeType.МетаАбзац || i.NodeType == NodeType.МетаИнформация)
                    {
                        if(h.Header.Indents == null)
                                h.Header.Indents = new List<Indent>();
                        
                        var ind = new Indent(i.ParagraphProperties,
                                            i.ElementIndex,
                                            i.WordElement.Text.GetHash(),
                                            i.WordElement.RunWrapper.GetCustRuns(),
                                            i.MetaInfo,
                                            i.HyperTextInfo,
                                            i.Comment,
                                            i.FootNoteInfo,
                                            null, //Почему не ищем таблицу?
                                            i.IsChange,
                                            0,
                                            NodeType.Абзац
                                            );
                        h.Header.Indents.Add(ind);
                    }
                    if(i.NodeType == NodeType.Таблица)
                    {
                       var l =  h.Header.Indents.LastOrDefault();
                       if(l != null)
                        l.Table = i.Table;
                    }
                }
                h.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
            }
            //Перемещаем все неопознаные элементы из рута документа абзацы документа
            foreach(var i in headersParser.BodyRootElements)
            {
                if(i.NodeType == NodeType.НеОпределено)
                {
                    var ind = new Indent(i.ParagraphProperties,
                                            i.ElementIndex,
                                            i.WordElement.Text.GetHash(),
                                            i.WordElement.RunWrapper.GetCustRuns(),
                                            i.MetaInfo,
                                            i.HyperTextInfo,
                                            i.Comment,
                                            i.FootNoteInfo,
                                            null,
                                            i.IsChange,
                                            0,
                                            NodeType.Абзац
                                            );
                    headersParser.BodyIndents.Add(ind);
                }
            }
            headersParser.BodyRootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
            //Перемещаем все неопознаные элементы из рутов приложений и хедеров приложений в абзацы приложений
            foreach(var a in annexParser.Annexes)
            {
                foreach(var i in a.RootElements)
                {
                    if(i.NodeType == NodeType.НеОпределено)
                    {
                        if(a.Annex.Indents == null)
                            a.Annex.Indents = new List<Indent>();
                        var ind = new Indent(i.ParagraphProperties,
                                            i.ElementIndex,
                                            i.WordElement.Text.GetHash(),
                                            i.WordElement.RunWrapper.GetCustRuns(),
                                            i.MetaInfo,
                                            i.HyperTextInfo,
                                            i.Comment,
                                            i.FootNoteInfo,
                                            null,
                                            i.IsChange,
                                            0,
                                            NodeType.Абзац
                                            );
                        a.Annex.Indents.Add(ind);
                    }
                }
                a.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
                foreach(var h in a.Headers)
                {
                    foreach(var i in h.RootElements)
                    {
                        if(i.NodeType == NodeType.НеОпределено)
                        {
                            if(h.Header.Indents == null)
                                    h.Header.Indents = new List<Indent>();
                            var ind = new Indent(i.ParagraphProperties,
                                                i.ElementIndex,
                                                i.WordElement.Text.GetHash(),
                                                i.WordElement.RunWrapper.GetCustRuns(),
                                                i.MetaInfo,
                                                i.HyperTextInfo,
                                                i.Comment,
                                                i.FootNoteInfo,
                                                null,
                                                i.IsChange,
                                                0,
                                                NodeType.Абзац
                                                );
                            h.Header.Indents.Add(ind);
                        }
                    }
                    h.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
                }
            }
            //Рассовываем все приложения согласно иерархии
            GetAnnexesHierarchy(annexParser);
            // List<AnnexParserModel> forRemove = new List<AnnexParserModel>();
            // var index = -1;
            // for (int i = 0; i < annexParser.Annexes.Count; i++)
            // {
            //     //FIXME тут надо изменить !!!
            //     //var items = annexParser.Annexes
            //     if(index >=0)
            //     {
            //         if(annexParser.Annexes[index].Hierarchy < annexParser.Annexes[i].Hierarchy)
            //         {
            //             if(annexParser.Annexes[index].Annex.Annexes == null)
            //                 annexParser.Annexes[index].Annex.Annexes = new List<Core.DocumentElements.Annex>();
            //             annexParser.Annexes[index].Annex.Annexes.Add(annexParser.Annexes[i].Annex);
            //             forRemove.Add(annexParser.Annexes[i]);
            //             i++;
            //             continue;
            //         }
            //     }
            //     index = i-1;
            // }
            // annexParser.Annexes.RemoveAll(r=>forRemove.Contains(r));
            
            Status("Формирование документа...", true);
            document.Body = new DocumentBody(headersParser.Headers.Select(s=>s.Header).ToList(),
                                            headersParser.BodyIndents,
                                            headersParser.BodyItems,
                                            annexParser.Annexes.Select(s=>s.Annex).ToList());
            if(word.DocumentImages.Count > 0)
            {
                document.Images = word.DocumentImages;
                document.ImagesLength = word.ImagesLenth;
            }
            var dd = word.GetElement(0);
         
                
           
            //Находим все хедеры в приложениях, все что не вошло в хедеры добавляем в рутовые элементы
            

        

           
            //var items = word.GetElementsExcept(annexParser.Annexes.SelectMany(s=>s.RootItems), headersParser.Headers.SelectMany(s=>s.Items));
            //TEST
            //word.SaveDocument("/home/phobos/Документы/354_hl.docx");
            
            return true;
        }

        private void GetAnnexesHierarchy(AnnexParser annexParser)
        {
             //Рассовываем все приложения согласно иерархии
            List<AnnexParserModel> forRemove = new List<AnnexParserModel>();
            List<AnnexParserModel> except = new List<AnnexParserModel>();
            annexParser.Annexes.Reverse();
            for (int i = 0; i < annexParser.Annexes.Count; i++)
            {
                var first = annexParser.Annexes.Except(except).FirstOrDefault(f=>f.Hierarchy < annexParser.Annexes[i].Hierarchy);
                if(first != null)
                {
                    if(annexParser.Annexes.IndexOf(first) < i)
                    {
                        except.Add(first);
                        i--;
                        continue;
                    }
                    if(first.Annex.Annexes == null)
                            first.Annex.Annexes = new List<DocumentElements.Annex>();
                    first.Annex.Annexes.Insert(0, annexParser.Annexes[i].Annex);
                    forRemove.Add(annexParser.Annexes[i]);
                }

            }
            annexParser.Annexes.RemoveAll(r=>forRemove.Contains(r));
            annexParser.Annexes.Reverse();
        }
        /// <summary>
        /// ВЫФносим сюда для рекурсивного добавления элементов
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        private List<int> indexes {get;set;}
        public List<int> GetChildElementsIndexes(int index)
        {
            indexes = new List<int>();
            //TODO нет поиска по итемам боди документа (поиск осущемтвляется только в хедерах)
            foreach(var h in document.Body.Headers)
            {
                if(h.ElementIndex == index)
                {
                    AddAllItems(h.Items);
                    foreach(var ind in h.Indents)
                    {
                        indexes.Add(ind.ElementIndex);
                    }
                }
                if(h.Items != null)
                foreach(var itm in h.Items)
                {
                    if(itm.ElementIndex == index)
                    {
                        foreach(var ind in itm.Indents)
                        {
                            indexes.Add(ind.ElementIndex);
                        }
                        AddAllItems(itm.Items);
                    } 
                }
            }
            indexes.Remove(index);
            return indexes;
        }

        private void AddAllItems(List<Item> items)
        {
            if(items != null)
            foreach(var i in items)
            {
                foreach(var ind in i.Indents)
                {
                    indexes.Add(ind.ElementIndex);
                }
                AddAllItems(i.Items);
            }
        }
    }
}