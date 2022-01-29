using System.Collections.Generic;
using DocumentParser.DocumentElements;
using System.Linq;
using DocumentParser.Parsers.Headers;
using DocumentParser.Parsers.Annex;
namespace DocumentParser.Parsers.Items;

public partial class ItemsParser
{
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
                //Надо ли ставить флаг только для хедеров?
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
        headersMove(headersParser.Headers, p=>  
                p == NodeType.НеОпределено 
                || p == NodeType.Абзац 
                || p == NodeType.МетаАбзац 
                || p == NodeType.МетаИнформация);
        //Перемещаем все неопознаные элементы из рута документа В абзацы документа
        elementsMove(headersParser.BodyRootElements, headersParser.BodyIndents, p=> p == NodeType.НеОпределено);
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
            for(int h = 0; h < annexParser.Annexes[a].Headers.Count; h++)
            {
                headersMove(annexParser.Annexes[a].Headers, p=> p == NodeType.НеОпределено);
            }
        }
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
            aParser.SortAnnexByHierarchy();
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
