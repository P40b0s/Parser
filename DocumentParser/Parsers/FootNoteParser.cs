using System.Collections.Generic;
using Services.Documents.Lexer;
using Services.Documents.Parser.Workers;
using Services.Documents.Parser.TokensDefinitions;
using Services.Documents.Lexer.Tokens;
using System.Linq;
using System;
using Core;
using Services.Documents.Core.Interfaces;
using Services.Documents.Core.DocumentElements;
using Core.Extensions;
using Services.Documents.Core.DocumentElements.FootNotes;

namespace Services.Documents.Parser.Parsers
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
    public class FootNoteParser : ParserBase<FootNoteToken>
    {
        public FootNoteParser(WordProcessing extractor)
        {
            this.extractor = extractor;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}

        public bool Parse()
        {
            Status("Поиск сносок...");
            tokens = lexer.Tokenize(extractor.FullText, new FootNoteTokensDefinition()).ToList();
            var count = tokens.Count();
            foreach(var token in tokens)
            {
                if(token.TokenType == FootNoteToken.Подчеркивания)
                {
                    var pod = extractor.GetElements(token).FirstOrDefault();
                    pod.NodeType = Core.NodeType.Подчеркивание;
                }
                if(token.TokenType == FootNoteToken.Примечание)
                {
                    var pod = extractor.GetElements(token).FirstOrDefault();
                    pod.NodeType = Core.NodeType.Примечание;
                }
            }
            var footsCommentParagraphs = extractor.GetParagrapsByComment("сноски");
            if(footsCommentParagraphs.Count > 0)
                return ParseByComments(footsCommentParagraphs);
            else return ParseByTokens();
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
                Percentage("Поиск сносок...", count, percentage);
            }
            return true;
        }

        public bool ParseByComments(List<ElementStructure> footsElements)
        {
            var percentage = 0;
            var count = footsElements.Count();
            var currentTokens = new List<Token<FootNoteToken>>();
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
                Percentage("Поиск сносок...", count, percentage);
            }
            return true;
        }
        private void getLinks((List<FootNoteWithItems> foots, Token<FootNoteToken> line) list)
        {
            var allLinks = list.line.FindBackward(f=>f.TokenType == FootNoteToken.СсылкаНаСноску, s=>s.TokenType == FootNoteToken.Сноска);
            foreach(var foo in list.foots)
            {
                var links = allLinks.Where(w=>parse(w) == foo.Foot.Number);
                if(links.Count() == 0)
                {
                    addException(foo.First);
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
        private (List<FootNoteWithItems>, Token<FootNoteToken> line) searchFootNotes(Token<FootNoteToken> token)
        {
            var list = new List<FootNoteWithItems>();
            if(token.TokenType == FootNoteToken.Сноска)
            {
                var broken = new List<Token<FootNoteToken>>();
                var line = token.Before(FootNoteToken.Подчеркивания);
                if(line == null)
                    return (null, null);
                
                var footnotes = line.TakeForward(FootNoteToken.Сноска);
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
                    pod.NodeType = Core.NodeType.Сноска;
                }

                foreach(var f in footnotes)
                {
                    var par = extractor.GetElements(f).FirstOrDefault();
                    var pars = par.TakeTo(t=>t.NodeType == Core.NodeType.Сноска || t.NodeType == Core.NodeType.Примечание);
                    var item = new FootNoteWithItems(par, extractor.GetUnicodeString(f.CustomGroups[0]), parse(f));
                    var fInd = new Indent(par.ParagraphProperties,
                                         par.ElementIndex,
                                         par.WordElement.Text.GetHash(),
                                         par.WordElement.RunWrapper.GetCustRuns(f.CustomGroups[0].Length),
                                         par.MetaInfo,
                                         par.HyperTextInfo,
                                         par.Comment,
                                         item.Foot,
                                         null,
                                         false,
                                         f.CustomGroups[0].Length,
                                         Core.NodeType.АбзацСноски
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
                                         p.Comment,
                                         null,
                                         null,
                                         false,
                                         0,
                                         Core.NodeType.АбзацСноски
                                         ));
                        extractor.SetElementNode(p, Core.NodeType.АбзацСноски);
                    }
                    list.Add(item);

                }
                return (list, line);
            }
            return (null, null);
        }
        private int parse(Token<FootNoteToken> token)
        {
            var result = -1;
            if(token.CustomGroups[0].Value.Contains("*"))
                result = token.CustomGroups[0].Value.Length;
            else
            {
                int.TryParse(token.CustomGroups[0].Value, out result);
            }
            if(result == -1)
                exceptions.Add(new ParserException($"Не могу преобразовать значение \"{token.CustomGroups[0].Value}\" в числовой формат"));
            return result;
        }

        private void addException(Token<FootNoteToken> token)
        {
            var par = extractor.GetElements(token).FirstOrDefault();
            exceptions.Add(new ParserException($"Не найдена ссылка на: {par.WordElement.Text}"));
        }
        private void addException(ElementStructure par)
        {
            exceptions.Add(new ParserException($"Не найдена ссылка на: {par.WordElement.Text}"));
        }

        
    }
}