using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using System.Linq;
using System;
using DocumentParser.DocumentElements;
using DocumentParser.DocumentElements.FootNotes;
using Utils.Extensions;
using DocumentParser.Elements;
using SettingsWorker.FootNote;
using SettingsWorker;
using DocumentParser.Parsers.Headers;
using DocumentParser.Parsers.Annex;

namespace DocumentParser.Parsers
{
    public class FootNoteWithItems
    {
        public FootNoteWithItems(ElementStructure first,
                                string stringNumber,
                                int number)
        {
            First = first;
            Foot = new FootNoteInfo(Guid.NewGuid(), stringNumber, number);
        }
        public ElementStructure First {get;set;}
        public FootNoteInfo Foot {get;set;}
    }
    public class FootNoteParser : LexerBase<FootNoteTokenType>
    {
        public FootNoteParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}

        public bool Parse()
        {
            UpdateStatus("Поиск сносок...");
            Tokenize(extractor.FullText, new FootNoteTokensDefinition(settings.TokensDefinitions.FootNoteTokenDefinitions.TokenDefinitionSettings));
            var count = tokens.Count();
            foreach(var token in tokens)
            {
                if(token.TokenType == FootNoteTokenType.Подчеркивания)
                {
                    var pod = extractor.GetElements(token).FirstOrDefault();
                    pod.NodeType = NodeType.Подчеркивание;
                }
                if(token.TokenType == FootNoteTokenType.Примечание)
                {
                    var pod = extractor.GetElements(token).FirstOrDefault();
                    pod.NodeType = NodeType.Примечание;
                }
            }
            var footsCommentParagraphs = extractor.GetParagrapsByComment("сноски");
            if(footsCommentParagraphs.Count > 0)
                return ParseByComments(footsCommentParagraphs);
            else return ParseByTokens();
        }

    public void Parse(HeadersParser hParser)
    {
        if(hParser != null)
            getHeadersFootNotes(hParser);
        else
            AddError("Для извлечения сносок HeaderParser не должен иметь значение null");
    }
    public void Parse(AnnexParser aParser)
    {
        if(aParser != null)
        {
            getAnnexFootNotes(aParser);
        }
        else
            AddError("Для извлечения сносок AnnexParser не должен иметь значение null");
    }
    public void Parse(HeadersParser hParser, AnnexParser aParser)
    {
        Parse(hParser);
        Parse(aParser);
    }

    /// <summary>
        /// Извлкечение футнотов возможно только после их поиска поэтому требует футнотпарсер
        /// но по сути он не используется
        /// </summary>
        /// <param name="foot"></param>
        void getAnnexFootNotes(AnnexParser aParser)
        {
            //Добавляем к приложению футноты
            foreach(var a in aParser.Annexes)
            {
                var foots = a.RootElements.Where(w=>w.NodeType == NodeType.Сноска);
                var footsPars = a.RootElements.Where(w=>w.NodeType == NodeType.АбзацСноски);
                foreach(var f in foots)
                {
                    if(a.Annex.FootNotes == null)
                        a.Annex.FootNotes = new List<FootNoteInfo>();
                    a.Annex.FootNotes.Add(f.FootNoteInfo);
                }
                a.RootElements.RemoveAll(r=>foots.Contains(r) || footsPars.Contains(r));
            }
        }

          /// <summary>
        /// Извлкечение футнотов возможно только после их поиска поэтому требует футнотпарсер
        /// но по сути он не используется
        /// </summary>
        /// <param name="foot"></param>
        public void getHeadersFootNotes(HeadersParser hParser)
        {
            //Добавляем к хедеру футноты
            foreach (var h in hParser.Headers)
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
            var foots0 = hParser.BodyRootElements.Where(w=>w.NodeType == NodeType.Сноска);
            var footsPars0 = hParser.BodyRootElements.Where(w=>w.NodeType == NodeType.АбзацСноски);
            foreach(var f in foots0)
            {
                hParser.BodyFootNotes.Add(f.FootNoteInfo);
            }
            hParser.BodyRootElements.RemoveAll(r=>foots0.Contains(r) || footsPars0.Contains(r));
        }


       
        public bool ParseByTokens()
        {
            var percentage = 0;
            var count = tokens.Count();
            foreach(var token in tokens)
            {
                var foot = searchFootNotes(token);
                if(foot.Item1 != null)
                {
                    getLinks(foot);
                }
                percentage++;
                UpdateStatus("Поиск сносок...", count, percentage);
            }
            return true;
        }

        public bool ParseByComments(List<ElementStructure> footsElements)
        {
            var percentage = 0;
            var count = footsElements.Count();
            var currentTokens = new List<Token<FootNoteTokenType>>();
            foreach(var i in footsElements)
            {
                foreach(var t in tokens)
                {
                    if((i.StartIndex <= t.StartIndex && (i.StartIndex + i.Length) >= t.EndIndex))
                        currentTokens.Add(t);
                }
            }
            foreach(var token in currentTokens)
            {
                var foot = searchFootNotes(token);
                if(foot.Item1 != null)
                {
                    getLinks(foot);
                }
                percentage++;
                UpdateStatus("Поиск сносок...", count, percentage);
            }
            return true;
        }
        private void getLinks((List<FootNoteWithItems> foots, Token<FootNoteTokenType> line) list)
        {
            var allLinks = list.line.TakeWhileBackward(f=>f.TokenType == FootNoteTokenType.СсылкаНаСноску, s=>s.TokenType == FootNoteTokenType.Сноска);
            foreach(var foo in list.foots)
            {
                var links = allLinks.Where(w=>parse(w) == foo.Foot.Number);
                if(links.Count() == 0)
                {
                    notFoundException(foo.First);
                    continue;
                }
                foo.First.FootNoteInfo = foo.Foot;
                foreach(var l in links)
                {

                    var linkPar = extractor.GetElements(l).FirstOrDefault();
                    if(linkPar.FootNoteInfo != null && linkPar.FootNoteInfo.isFootNoteLink)
                    {
                        linkPar.FootNoteInfo.footNoteLinks.Add(new FootNoteLink(foo.Foot.Id,
                                                    l.CustomGroups[0].Value,
                                                    parse(l),
                                                    foo.Foot.Indents,
                                                    l.CustomGroups[0].StartIndex - linkPar.StartIndex,
                                                    l.CustomGroups[0].Length));
                        foo.First.FootNoteInfo.LinksCount++;
                    }
                    else
                    {
                        linkPar.FootNoteInfo = new FootNoteInfo(foo.Foot.Id,
                                                    l.CustomGroups[0].Value,
                                                    parse(l),
                                                    foo.Foot.Indents,
                                                    l.CustomGroups[0].StartIndex - linkPar.StartIndex,
                                                    l.CustomGroups[0].Length);
                        foo.First.FootNoteInfo.LinksCount++;
                    }
                }
            }
                    //TODO надо както вычленить абзацы сносок походу только коментариями
                    //они могут быть либо одним абзацем либо нумерованным списком без вложенности
        }
        private (List<FootNoteWithItems>, Token<FootNoteTokenType> line) searchFootNotes(Token<FootNoteTokenType> token)
        {
            var list = new List<FootNoteWithItems>();
            if(token.TokenType == FootNoteTokenType.Сноска)
            {
                var broken = new List<Token<FootNoteTokenType>>();
                var line = token.Before(FootNoteTokenType.Подчеркивания);
                if(!line.IsOk)
                    return (null, null);
                
                var footnotes = line.Value.FindForwardMany(f=>f.TokenType == FootNoteTokenType.Сноска);
                var prevNum = 0;
                for(int i = 0; i< footnotes.Count; i++)
                {
                    var num = parse(footnotes[i]);
                    if(num < 0)
                        broken.Add(footnotes[i]);
                    if(i == 0)
                    {
                        prevNum = num;
                        continue;
                    }
                        
                    if(num == prevNum +1)
                    {
                        prevNum = num;
                        continue;
                    }
                    else broken.Add(footnotes[i]); 
                }
                footnotes.RemoveAll(b=>broken.Contains(b));
                foreach(var f in footnotes)
                {
                    var pod = extractor.GetElements(f).FirstOrDefault();
                    pod.NodeType = NodeType.Сноска;
                }

                foreach(var f in footnotes)
                {
                    var par = extractor.GetElements(f).FirstOrDefault();
                    var pars = par.TakeTo(t=>t.NodeType == NodeType.Сноска || t.NodeType == NodeType.Примечание);
                    var item = new FootNoteWithItems(par, extractor.GetUnicodeString(f.CustomGroups[0]), parse(f));
                    var fInd = new Indent(par.ParagraphProperties,
                                         par.ElementIndex,
                                         par.WordElement.Text.GetHash(),
                                         par.WordElement.RunWrapper.GetCustRuns(f.CustomGroups[0].Length),
                                         par.MetaInfo,
                                         par.HyperTextInfo,
                                         item.Foot,
                                         null,
                                         false,
                                         f.CustomGroups[0].Length,
                                         NodeType.АбзацСноски
                                         );
                    item.Foot.Indents = new List<Indent>();
                    item.Foot.Indents.Add(fInd);
                    foreach(var p in pars)
                    {
                        item.Foot.Indents.Add(new Indent(p.ParagraphProperties,
                                         p.ElementIndex,
                                         p.WordElement.Text.GetHash(),
                                         p.WordElement.RunWrapper.GetCustRuns(),
                                         p.MetaInfo,
                                         p.HyperTextInfo,
                                         null,
                                         null,
                                         false,
                                         0,
                                         NodeType.АбзацСноски
                                         ));
                        extractor.SetElementNode(p, NodeType.АбзацСноски);
                    }
                    list.Add(item);

                }
                return (list, line.Value);
            }
            return (null, null);
        }
        private int parse(Token<FootNoteTokenType> token)
        {
            var result = -1;
            if(token.CustomGroups[0].Value.Contains("*"))
                result = token.CustomGroups[0].Value.Length;
            else
            {
                int.TryParse(token.CustomGroups[0].Value, out result);
            }
            if(result == -1)
                AddError($"Не могу преобразовать значение \"{token.CustomGroups[0].Value}\" в числовой формат");
            return result;
        }

        private void notFoundException(Token<FootNoteTokenType> token)
        {
            var par = extractor.GetElements(token).FirstOrDefault();
            AddError($"Не найдена ссылка на: {par.WordElement.Text}");
        }
        private void notFoundException(ElementStructure par)
        {
            AddError($"Не найдена ссылка на: {par.WordElement.Text}");
        }

        
    }
}