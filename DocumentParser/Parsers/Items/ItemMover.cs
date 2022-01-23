using System.Collections.Generic;
using DocumentParser.DocumentElements;
using System.Linq;
using DocumentParser.Parsers.Headers;
using DocumentParser.Parsers.Annex;
namespace DocumentParser.Parsers.Items;
public partial class ItemsParser
{
    //AnnexParser annexParser {get;}
    //HeadersParser headersParser{get;}
    /// <summary>
    /// Находим и рассовываем все итемы в приложениях, заголовках и в теле самого документа
    /// </summary>
    /// <param name="headersParser">Парсер заголовков</param>
    /// <param name="annexParser">Парсер приложений</param>
    // public ItemsParser(HeadersParser headersParser, AnnexParser annexParser)
    // {
    //     this.headersParser = headersParser;
    //     this.annexParser = annexParser;
    // }
    /// <summary>
    /// Находим и рассовываем все итемы в приложениях, заголовках и в теле самого документа
    /// </summary>
    /// <param name="headersParser">Парсер заголовков</param>
    /// <param name="annexParser">Парсер приложений</param>
    // public ItemsParser(HeadersParser headersParser)
    // {
    //     this.headersParser = headersParser;
    // }
    /// <summary>
    /// Находим и рассовываем все итемы в приложениях, заголовках и в теле самого документа
    /// </summary>
    /// <param name="headersParser">Парсер заголовков</param>
    /// <param name="annexParser">Парсер приложений</param>
    // public ItemsParser(AnnexParser annexParser)
    // {
    //     this.annexParser = annexParser;
    // }

    private void headersMove(List<HeaderParserModel> headers, System.Predicate<NodeType> predicate)
    {
        //Перемещаем все неопознаные элементы из рутов хедеров в абзацы хедеров
        //быстрая версия
        for(int h = 0 - 1; h < headers.Count; h++)
        {
            for(int i = headers[h].RootElements.Count - 1; i >=0 ; i--)
            {
                //для элементов определенных как абзац
                //Часть 31 статьи 1 Федерального закона от 26 декабря 2008 года № 294-ФЗ.....
                //дополнить пунктом 24 следующего содержания:
                //элемент имеет флаг IsChange и определен как абзац поэтому добавляем условие для абзаца
                if(predicate(headers[h].RootElements[i].NodeType))
                {
                    if(headers[h].Header.Indents == null)
                            headers[h].Header.Indents = new List<Indent>();
                    headers[h].Header.Indents.Insert(0, headers[h].RootElements[i].ToIndent());
                }
                if(headers[h].RootElements[i].NodeType == NodeType.Таблица)
                {
                    var l =  headers[h].Header.Indents.LastOrDefault();
                    if(l != null)
                        l.Table = headers[h].RootElements[i].Table;
                }
                headers[h].RootElements.RemoveAt(i);
            }
        }
    }

    private void elementsMove(List<Elements.ElementStructure> from, List<Indent> to, System.Predicate<NodeType> predicate)
    {
         for(int i = from.Count - 1; i >=0; i--)
        {
            if(predicate(from[i].NodeType))
            {
                to.Insert(0, from[i].ToIndent());
                //Удаляем элемент из боди документа, так кон относился к заголовку и уже перемещен туда
                from.RemoveAt(i);
            }
        }
    }

    private void parseHeaders(HeadersParser headersParser)
    {
        headersParser.BodyItems = Parse(headersParser.BodyRootElements);
        var count = headersParser.Headers.Count();
        var percentage = 0;
        foreach(var h in headersParser.Headers)
        {
            h.Header.Items = Parse(h.RootElements);
            percentage++;
            UpdateStatus("Обработка списочных элементов заголовков...", count, percentage);
        }
        //Перемещаем все неопознаные элементы из рутов хедеров в абзацы хедеров
        //быстрая версия
        headersMove(headersParser.Headers, p=>  
                p == NodeType.НеОпределено 
                || p == NodeType.Абзац 
                || p == NodeType.МетаАбзац 
                || p == NodeType.МетаИнформация);
        // for(int h = 0 - 1; h < headersParser.Headers.Count; h++)
        // {
        //     for(int i = headersParser.Headers[h].RootElements.Count - 1; i >=0 ; i--)
        //     {
        //         //для элементов определенных как абзац
        //         //Часть 31 статьи 1 Федерального закона от 26 декабря 2008 года № 294-ФЗ.....
        //         //дополнить пунктом 24 следующего содержания:
        //         //элемент имеет флаг IsChange и определен как абзац поэтому добавляем условие для абзаца
        //         if(headersParser.Headers[h].RootElements[i].NodeType == NodeType.НеОпределено 
        //         || headersParser.Headers[h].RootElements[i].NodeType == NodeType.Абзац 
        //         || headersParser.Headers[h].RootElements[i].NodeType == NodeType.МетаАбзац 
        //         || headersParser.Headers[h].RootElements[i].NodeType == NodeType.МетаИнформация)
        //         {
        //             if(headersParser.Headers[h].Header.Indents == null)
        //                     headersParser.Headers[h].Header.Indents = new List<Indent>();
        //             headersParser.Headers[h].Header.Indents.Insert(0, headersParser.Headers[h].RootElements[i].ToIndent());
        //         }
        //         if(headersParser.Headers[h].RootElements[i].NodeType == NodeType.Таблица)
        //         {
        //             var l =  headersParser.Headers[h].Header.Indents.LastOrDefault();
        //             if(l != null)
        //                 l.Table = headersParser.Headers[h].RootElements[i].Table;
        //         }
        //         headersParser.Headers[h].RootElements.RemoveAt(i);
        //     }
        // }
        //Старая версия
        // foreach(var h in headersParser.Headers)
        // {
        //     foreach(var i in h.RootElements)
        //     {
        //         //для элементов определенных как абзац
        //         //Часть 31 статьи 1 Федерального закона от 26 декабря 2008 года № 294-ФЗ.....
        //         //дополнить пунктом 24 следующего содержания:
        //         //элемент имеет флаг IsChange и определен как абзац поэтому добавляем условие для абзаца
        //         if(i.NodeType == NodeType.НеОпределено || i.NodeType == NodeType.Абзац || i.NodeType == NodeType.МетаАбзац || i.NodeType == NodeType.МетаИнформация)
        //         {
        //             if(h.Header.Indents == null)
        //                     h.Header.Indents = new List<Indent>();
        //             h.Header.Indents.Add(i.ToIndent());
        //         }
        //         if(i.NodeType == NodeType.Таблица)
        //         {
        //             var l =  h.Header.Indents.LastOrDefault();
        //             if(l != null)
        //             l.Table = i.Table;
        //         }
                
        //     }
        //     h.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
        
        //Перемещаем все неопознаные элементы из рута документа В абзацы документа
        elementsMove(headersParser.BodyRootElements, headersParser.BodyIndents, p=> p == NodeType.НеОпределено);
        // for(int i = headersParser.BodyRootElements.Count - 1; i >=0; i--)
        // {
        //     if(headersParser.BodyRootElements[i].NodeType == NodeType.НеОпределено)
        //     {
        //         headersParser.BodyIndents.Insert(0, headersParser.BodyRootElements[i].ToIndent());
        //         //Удаляем элемент из боли документа, так кон относился к заголовку и уже перемещен туда
        //         headersParser.BodyRootElements.RemoveAt(i);
        //     }
                
        // }
        //пока закомменчу старую версию
        // foreach(var i in headersParser.BodyRootElements)
        // {
        //     if(i.NodeType == NodeType.НеОпределено)
        //         headersParser.BodyIndents.Add(i.ToIndent());
        // }
        // headersParser.BodyRootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
    }

    private void parseAnnexes(AnnexParser annexParser)
    {
        var count = annexParser.Annexes.Count();
        var percentage = 0;
        foreach(var a in annexParser.Annexes)
        {
            a.Annex.Items = Parse(a.RootElements);
            foreach(var h in a.Headers)
            {
                //a/Annex.Headers уже является сслкой на h/Header
                h.Header.Items = Parse(h.RootElements);
            }
            percentage++;
            UpdateStatus("Обработка списочных элементов приложений...", count, percentage);
        }
        //Перемещаем все неопознаные элементы из рутов приложений и хедеров приложений в абзацы приложений
        for(int a = 0; a < annexParser.Annexes.Count; a++)
        {
            elementsMove(annexParser.Annexes[a].RootElements, annexParser.Annexes[a].Annex.Indents, p=> p == NodeType.НеОпределено);
            // for(int i = annexParser.Annexes[a].RootElements.Count - 1; i >=0 ; i--)
            // {
            //     //для элементов определенных как абзац
            //     //Часть 31 статьи 1 Федерального закона от 26 декабря 2008 года № 294-ФЗ.....
            //     //дополнить пунктом 24 следующего содержания:
            //     //элемент имеет флаг IsChange и определен как абзац поэтому добавляем условие для абзаца
            //     if(annexParser.Annexes[a].RootElements[i].NodeType == NodeType.НеОпределено)
            //     {
            //        if(annexParser.Annexes[a].Annex.Indents == null)
            //             annexParser.Annexes[a].Annex.Indents = new List<Indent>();
            //         annexParser.Annexes[a].Annex.Indents.Insert(0, annexParser.Annexes[a].RootElements[i].ToIndent());
            //         annexParser.Annexes[a].RootElements.RemoveAt(i);
            //     }
            // }
            for(int h = 0; h < annexParser.Annexes[a].Headers.Count; h++)
            {
                headersMove(annexParser.Annexes[a].Headers, p=> p == NodeType.НеОпределено);
                // for(var i = annexParser.Annexes[a].Headers[h].RootElements.Count - 1; i >=0; i--)
                // {
                //     if(i.NodeType == NodeType.НеОпределено)
                //     {
                //         if(h.Header.Indents == null)
                //                 h.Header.Indents = new List<Indent>();
                //         var ind = new Indent(i.ParagraphProperties,
                //                             i.ElementIndex,
                //                             i.WordElement.Text.GetHash(),
                //                             i.WordElement.RunWrapper.GetCustRuns(),
                //                             i.MetaInfo,
                //                             i.HyperTextInfo,
                //                             i.Comment,
                //                             i.FootNoteInfo,
                //                             null,
                //                             i.IsChange,
                //                             0,
                //                             NodeType.Абзац
                //                             );
                //         h.Header.Indents.Add(ind);
                //     }
                // }
                // h.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
            }
        }
        //СТАРАЯ ВЕРСИЯ
        // foreach(var a in annexParser.Annexes)
        // {
        //     foreach(var i in a.RootElements)
        //     {
        //         if(i.NodeType == NodeType.НеОпределено)
        //         {
        //             if(a.Annex.Indents == null)
        //                 a.Annex.Indents = new List<Indent>();
        //             var ind = new Indent(i.ParagraphProperties,
        //                                 i.ElementIndex,
        //                                 i.WordElement.Text.GetHash(),
        //                                 i.WordElement.RunWrapper.GetCustRuns(),
        //                                 i.MetaInfo,
        //                                 i.HyperTextInfo,
        //                                 i.Comment,
        //                                 i.FootNoteInfo,
        //                                 null,
        //                                 i.IsChange,
        //                                 0,
        //                                 NodeType.Абзац
        //                                 );
        //             a.Annex.Indents.Add(ind);
        //         }
        //     }
        //     a.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);


            // foreach(var h in a.Headers)
            // {
            //     foreach(var i in h.RootElements)
            //     {
            //         if(i.NodeType == NodeType.НеОпределено)
            //         {
            //             if(h.Header.Indents == null)
            //                     h.Header.Indents = new List<Indent>();
            //             var ind = new Indent(i.ParagraphProperties,
            //                                 i.ElementIndex,
            //                                 i.WordElement.Text.GetHash(),
            //                                 i.WordElement.RunWrapper.GetCustRuns(),
            //                                 i.MetaInfo,
            //                                 i.HyperTextInfo,
            //                                 i.Comment,
            //                                 i.FootNoteInfo,
            //                                 null,
            //                                 i.IsChange,
            //                                 0,
            //                                 NodeType.Абзац
            //                                 );
            //             h.Header.Indents.Add(ind);
            //         }
            //     }
            //     h.RootElements.RemoveAll(r=>r.NodeType == NodeType.НеОпределено);
            // }
    }
    
    public void Parse(HeadersParser hParser)
    {
        if(hParser != null)
            parseHeaders(hParser);
        else
            AddError("Для извлечения нумерованных элементов HeaderParser не должен иметь значение null");
    }
    public void Parse(AnnexParser aParser)
    {
        if(aParser != null)
        {
            parseAnnexes(aParser);
            aParser.MoveAnnexByHierarchy();
        }
        else
            AddError("Для извлечения нумерованных элементов AnnexParser не должен иметь значение null");
    }
    public void Parse(HeadersParser hParser, AnnexParser aParser)
    {
        Parse(hParser);
        Parse(aParser);
    }

             /// <summary>
        /// ВЫФносим сюда для рекурсивного добавления элементов
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        private List<int> indexes {get;set;}
        //TODO непонятно что это но оно не работало
        // public List<int> GetChildElementsIndexes(int index)
        // {
        //     indexes = new List<int>();
        //     //TODO нет поиска по итемам боди документа (поиск осущемтвляется только в хедерах)
        //     foreach(var h in document.Body.Headers)
        //     {
        //         if(h.ElementIndex == index)
        //         {
        //             AddAllItems(h.Items);
        //             foreach(var ind in h.Indents)
        //             {
        //                 indexes.Add(ind.ElementIndex);
        //             }
        //         }
        //         if(h.Items != null)
        //         foreach(var itm in h.Items)
        //         {
        //             if(itm.ElementIndex == index)
        //             {
        //                 foreach(var ind in itm.Indents)
        //                 {
        //                     indexes.Add(ind.ElementIndex);
        //                 }
        //                 AddAllItems(itm.Items);
        //             } 
        //         }
        //     }
        //     indexes.Remove(index);
        //     return indexes;
        // }

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
