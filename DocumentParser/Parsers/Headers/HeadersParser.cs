using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using System.Linq;
using DocumentParser.Parsers.Annex;
using DocumentParser.Parsers.Requisites;
using DocumentParser.DocumentElements;
using DocumentParser.DocumentElements.FootNotes;
using DocumentParser.Elements;
using SettingsWorker;
using SettingsWorker.Headers;

namespace DocumentParser.Parsers.Headers
{

    public class Root
    {
        public List<ElementStructure> BodyRootElements {get;} = new List<ElementStructure>();
        public List<Item> Items {get;} = new List<Item>();
        public List<Indent> Indents {get;} = new List<Indent>();
    }
    /// <summary>
    /// Находим все хедеры, остальное добавляем в BodyRootElements
    /// </summary>
    public class HeadersParser : LexerBase<HeaderTokenType>
    {
        public List<HeaderParserModel> Headers {get;} = new List<HeaderParserModel>();

        /// <summary>
        /// Корневые итемы в теле самого документа
        /// </summary>
        public List<ElementStructure> BodyRootElements {get;} = new List<ElementStructure>();
        public List<Indent> BodyIndents {get;set;} = new List<Indent>();
        public List<Item> BodyItems {get;set;} = new List<Item>();
        public List<FootNoteInfo> BodyFootNotes {get;set;} = new List<FootNoteInfo>();
        public List<ElementStructure> BodyNoteElements {get;} = new List<ElementStructure>();
        MetaParser metaParser {get; set;}
        TableParser tableParser {get;set;}
        AnnexParser annexParser {get;set;}
        ElementStructure bodyStartAfterThisElemet {get;}
        /// <summary>
        /// Для корректтного парсинкга хедеров нам нужен последний абзац заголовка, это мы узнаем только после рпоиска реквизитов
        /// и информация от парсера мета информации чтоб эту информацию сразу добавить в заголовки
        /// если мета парсер не передавать то мета информация не будет добавлятся в заголовки
        /// </summary>
        /// <param name="extractor"></param>
        /// <param name="lastHeadElement"></param>
        /// <param name="meta"></param>
        public HeadersParser(WordProcessing extractor, ElementStructure lastHeadElement)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
            bodyStartAfterThisElemet = lastHeadElement;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        public HeadersParser WithMetaNodes(MetaParser meta)
        {
            this.metaParser = meta;
            return this;
        }
        public HeadersParser WithAnnexes(AnnexParser aParser)
        {
            this.annexParser = aParser;
            return this;
        }
        public HeadersParser GetTables(TableParser table)
        {
            this.tableParser = table;
            GetTables();
            return this;
        }
        public HeadersParser Parse()
        {
            UpdateStatus("Поиск заголовков");
            var percentage = 0;
            Tokenize(extractor.FullText, new HeaderTokensDefinition(settings.TokensDefinitions.HeadersTokenDefinitions.TokenDefinitionSettings));
            var count = tokens.Count();
            foreach(var token in tokens)
            {
                if(token.TokenType == HeaderTokenType.Заголовок)
                    parseHeaders(token);
                percentage++;
                UpdateStatus("Поиск заголовков...", count, percentage);
            }
            foreach(var h in Headers)
            {
                var items = h.LastElement.TakeTo(t=>t.NodeType == NodeType.Заголовок);
                var last = items.LastOrDefault();
                if(last != null)
                    h.EndIndex = last.ElementIndex;
                h.RootElements.AddRange(items);
            }
            
            //getAnnexHeaders(annexParser);
             //Берем все итемы в теле документа которые не вошли в хедеры
            BodyRootElements.AddRange(bodyStartAfterThisElemet.TakeTo(t=>t.NodeType == NodeType.Заголовок));
            if(annexParser != null)
                extractAnnexHeaders(Headers);
            return this;
        }
        /// <summary>
        /// Добавляем таблицу к хедерам и к приложениям но только после поиска таблиц!
        /// Метка что нода является таблицей уже есть а сама таблица еще не парсилась!
        /// </summary>
        public void GetTables()
        {
            //Добавляем к хедеру таблицу если она там есть
            for(int h = 0; h < Headers.Count; h++)
            //foreach (var h in Headers)
            {
                var firstItem = Headers[h].RootElements.FirstOrDefault();
                if(firstItem != null)
                {
                    if(firstItem.NodeType == NodeType.Таблица)
                    {
                        Headers[h].Header.Table = firstItem.Table;
                        Headers[h].RootElements.Remove(firstItem);
                    } 
                    else
                    {
                        var table = firstItem.FindForward(t=>t.NodeType == NodeType.Таблица, 1);
                        if(table.IsOk)
                        {
                            Headers[h].Header.Table = table.Value.Table;
                            Headers[h].RootElements.Remove(table.Value);
                        }
                    }
                }
            }
            //foreach (var h in Headers)
            // {
            //     var firstItem = h.RootElements.FirstOrDefault();
            //     if(firstItem != null)
            //     {
            //         if(firstItem.NodeType == NodeType.Таблица)
            //         {
            //             h.Header.Table = firstItem.Table;
            //             h.RootElements.Remove(firstItem);
            //         } 
            //         else
            //         {
            //             var table = firstItem.FindForward(t=>t.NodeType == NodeType.Таблица, 1);
            //             if(table.IsOk)
            //             {
            //                 h.Header.Table = table.Value.Table;
            //                 h.RootElements.Remove(table.Value);
            //             }
            //         }
            //     }
            // }

            //Добавляем к приложению таблицу если она там есть
           
        }
        /// <summary>
        /// Извлкечение футнотов возможно только после их поиска поэтому требует футнотпарсер
        /// но по сути он не используется
        /// </summary>
        /// <param name="foot"></param>
        public HeadersParser GetFootNotes(FootNoteParser foot)
        {
            //Добавляем к хедеру футноты
            foreach (var h in Headers)
            {
                var foots = h.RootElements.Where(w=>w.NodeType == NodeType.Сноска);
                var footsPars = h.RootElements.Where(w=>w.NodeType == NodeType.АбзацСноски);
                foreach(var f in foots)
                {
                    if(h.Header.FootNotes == null)
                        h.Header.FootNotes = new List<FootNoteInfo>();
                    h.Header.FootNotes.Add(f.FootNoteInfo);
                }
                h.RootElements.RemoveAll(r=>foots.Contains(r) || footsPars.Contains(r));
            }
            
            //Добавляем к боди документа футноты
            var foots0 = BodyRootElements.Where(w=>w.NodeType == NodeType.Сноска);
            var footsPars0 = BodyRootElements.Where(w=>w.NodeType == NodeType.АбзацСноски);
            foreach(var f in foots0)
            {
                BodyFootNotes.Add(f.FootNoteInfo);
            }
            BodyRootElements.RemoveAll(r=>foots0.Contains(r) || footsPars0.Contains(r));
            return this;
        }

        //  /// <summary>
        // /// Перемещаем все итемы из рута в хедеры если они в хедерах
        // /// удаляем все хедеры которые были задействованы в приложениях
        // /// теперь в HeadersParser лежат только хедеры боди документа
        // /// а здесь лежат только хедеры приложений
        // /// </summary>
        // /// <param name="headers"></param>
        // void getAnnexHeaders(AnnexParser ann)
        // {
        //     var headersForRemove = new List<HeaderParserModel>();
        //     foreach(var a in ann.Annexes)
        //     {
                
        //         foreach(var h in Headers)
        //         {
        //             if(a.StartIndex <= h.StartIndex && a.EndIndex >= h.EndIndex)
        //             {
        //                 a.Headers.Add(h);
        //                 if(a.Annex.Headers == null)
        //                     a.Annex.Headers = new List<Header>();
        //                 a.Annex.Headers.Add(h.Header);
        //                 headersForRemove.Add(h);
        //                 a.RootElements.RemoveAll(r=>h.RootElements.Contains(r) || h.Header.ElementIndex == r.ElementIndex);
        //             }
        //         }
        //     }
        //     Headers.RemoveAll(r=> headersForRemove.Contains(r));            
        // }

        private bool parseHeaders(Token<HeaderTokenType> token)
        {
            var headerPar = extractor.GetElements(token).FirstOrDefault();
            if(headerPar.NodeType == NodeType.АбзацТаблицы)
                return false;
            if(headerPar.IsChange)
                return false;
            var header = new HeaderParserModel();
            extractor.SetElementNode(token, NodeType.Заголовок);
            header.Header.Type = token.CustomGroups[0].Value;
            var number = extractor.GetUnicodeString(token.CustomGroups[1]);
            if(number == null)
                return AddError($"Не могу определить номер заголовка {token.Value}");
            header.Header.Number = number;
            header.Header.Postfix = token.CustomGroups[2].Value;
            //Хрена в приложениях не выделено жирным шрифтом!
            //if(!extractor.Properties.IsBold(headerPar.WordElement.Element))
            //{
                ////Наименование статьи скорее всего слито с первым абзацем тут будем реализовывать эту логику
                ////Скорее всего возьмем первый абзац и добафим к абзацам
                ////только для начала его надо распарсить.....
            //    throw new Exception("Способ обработки таких статей еще не определен");
            //}
            var name = extractor.GetUnicodeString(headerPar, new TextIndex(token.Length, headerPar.Length - token.Length));
            header.Header.Name = name;
            header.Header.ElementIndex = headerPar.ElementIndex;
           
            header.LastElement = headerPar;
            header.StartIndex = headerPar.ElementIndex +1;
            var meta = headerPar.Next();
            if(metaParser != null && meta.IsOk && meta.IsOk && meta.Value.MetaInfo.FullIsMeta)
            {
                header.Header.Meta = meta.Value.MetaInfo;
                header.LastElement = meta.Value;
                header.StartIndex = meta.Value.ElementIndex +1;
            }
            Headers.Add(header);
            Count++;
            return true;
        }

        /// <summary>
        /// Забираем все хедеры которые относятся к приложения из парсера хедеров
        /// добавляем их в хедеры приложений а из парсера хедеров удаляем
        /// </summary>
        /// <param name="headers">массив хедеров из парсера хедеров</param>
        void extractAnnexHeaders(List<HeaderParserModel> headers)
        {
            for(int a = 0; a < annexParser.Annexes.Count; a++)
            {
                for(int h = headers.Count -1; h >= 0; h--)
                {
                    if(annexParser.Annexes[a].StartIndex <= headers[h].StartIndex && annexParser.Annexes[a].EndIndex >= headers[h].EndIndex)
                    {
                        annexParser.Annexes[a].Headers.Insert(0, headers[h]);
                        if(annexParser.Annexes[a].Annex.Headers == null)
                            annexParser.Annexes[a].Annex.Headers = new List<Header>();
                        annexParser.Annexes[a].Annex.Headers.Insert(0, headers[h].Header);
                        for(int r = annexParser.Annexes[a].RootElements.Count -1; r >=0; r--)
                        {
                            if(headers[h].RootElements.Contains(annexParser.Annexes[a].RootElements[r])
                                || headers[h].Header.ElementIndex == annexParser.Annexes[a].RootElements[r].ElementIndex)
                                {
                                    annexParser.Annexes[a].RootElements.RemoveAt(r);  
                                }
                        }
                        headers.RemoveAt(h);
                    }
                }
            }
            //Headers.RemoveAll(r=> headersForRemove.Contains(r));            
        }
    }
}