using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using DocumentParser.DocumentElements;
using System.Linq;
using DocumentParser.Parsers.Headers;
using DocumentParser.TokensDefinitions;
using Utils.Extensions;

namespace DocumentParser.Parsers.Annex
{
    //центральный модуль парсинга документа, один на каждый док, отсюда вызываются все остальные модули
    public class AnnexParser : ParserBase<AnnexToken>
    {
        public List<AnnexParserModel> Annexes {get;} = new List<AnnexParserModel>();
        public AnnexParser(WordProcessing extractor)
        {
            this.extractor = extractor;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        //Пример шапки приложения
        //УТВЕРЖДЕНЫ(токен)
        //постановлением Правительства
        //Российской Федерации
        //от 6 мая 2011 г.(токен) № 354(токен)

        //пример приложения приложения
        //         ПРИЛОЖЕНИЕ(токен) № 1(токен)
        // к Правилам(токен правилам реестрам составам итд) предоставления
        // коммунальных услуг собственникам
        // и пользователям помещений
        // в многоквартирных домах и жилых домов
        //  
        //  
        // ТРЕБОВАНИЯ
        // к качеству коммунальных услуг
        public bool Parse()
        {
            Status("Поиск приложений");
            var percentage = 0;
            tokens = lexer.Tokenize(extractor.FullText, new AnnexTokensDefinition()).ToList();
            var count = tokens.Count();
            foreach(var token in tokens)
            {
               
                if(token.TokenType == AnnexToken.Утверждено)
                {
                    parseApproved(token);
                }
                if(token.TokenType == AnnexToken.Приложение)
                {
                    parseAnnex(token);
                }
                percentage++;
                Percentage("Поиск приложений...", count, percentage);
            }
            foreach(var a in Annexes)
            {
                var items = a.LastElement.TakeWhile();
                var last = items.LastOrDefault();
                if(last != null)
                    a.EndIndex = last.ElementIndex;
                a.RootElements.AddRange(items);
            }
            return true;
        }
        // /// <summary>
        // /// Перемещаем все итемы из рута в хедеры если они в хедерах
        // /// удаляем все хедеры которые были задействованы в приложениях
        // /// теперь в HeadersParser лежат только хедеры боди документа
        // /// а здесь лежат только хедеры приложений
        // /// </summary>
        // /// <param name="headers"></param>
        // public void GetHeaders(List<HeaderParserModel> headers)
        // {
        //     var headersForRemove = new List<HeaderParserModel>();
        //     foreach(var a in Annexes)
        //     {
                
        //         foreach(var h in headers)
        //         {
        //             if(a.StartIndex <= h.StartIndex && a.EndIndex >= h.EndIndex)
        //             {
        //                 a.Headers.Add(h);
        //                 headersForRemove.Add(h);
        //                 a.RootItems.RemoveAll(r=>h.Items.Contains(r) || h.Header.ElementIndex == r.ElementIndex);
        //             }
        //         }
        //     }
        //     headers.RemoveAll(r=> headersForRemove.Contains(r));
        //     foreach(var a in Annexes)
        //     {
        //         var firstRootItem = a.RootItems.FirstOrDefault();
        //         if(firstRootItem != null)
        //         {
        //             if(firstRootItem.NodeType == Core.NodeType.Таблица)
        //                 a.Annex.Table = firstRootItem.Table;
        //             else
        //             {
        //                 var table = firstRootItem.FindForward(t=>t.NodeType == Core.NodeType.Таблица, 1);
        //                 a.Annex.Table = table?.Table;
        //             }
        //         }

        //     }
        // }

        private bool parseApproved(Token<AnnexToken> token)
        {
            try
            {
                var approvedDataToken = token.Next(AnnexToken.Дата);
                if(approvedDataToken == null)
                    return false;
                var approvedNumberToken = approvedDataToken.Next(AnnexToken.Номер);
                if(approvedNumberToken == null)
                    return false;
                var newAnnex = new AnnexParserModel();
                //так как это блок "УТВЕРЖДЕНО" то здесь будет инфа об утвердившем органе
                newAnnex.Annex.ApprovedPrefix = new AnnexApprovedPrefix();
                newAnnex.Annex.ApprovedPrefix.Prefix = token.Value;
                //должно по идее все быть в пределах 1 параграфа
                var approvedElement = extractor.GetElements(token).FirstOrDefault();
                extractor.SetElementNode(approvedElement, NodeType.Приложение);
                newAnnex.Annex.ApprovedPrefix.Number = extractor.GetUnicodeString(approvedNumberToken.CustomGroups[0]);
                var date = Utils.Extensions.DateTimeExtensions.GetDate(approvedDataToken.CustomGroups[0].Value, approvedDataToken.CustomGroups[1].Value, approvedDataToken.CustomGroups[2].Value);
                if(!date.HasValue)
                    throw new ParserException($"Неправильный формат даты в приложении № {Annexes.Count + 1}", ErrorType.Fatal);
                newAnnex.Annex.ApprovedPrefix.Date = date.Value;
                newAnnex.Annex.ApprovedPrefix.Organ = extractor.GetUnicodeString(approvedElement, new TextIndex(token.Length, (approvedDataToken.StartIndex - token.Length - approvedElement.StartIndex ))).Trim();
                //возможно надо взять в расчет indentation(left) у этих абзацев он примерно 5000
                //прверяем следующий может такое быть что это типа лоп информации
                // у него такой же отступ как у первго
                var nextPar = approvedElement.Next();
                //найден дополнительный параграф с какими то комментами
                if(nextPar != null && nextPar.ParagraphProperties.Ind?.Left > 300)
                {
                    newAnnex.Annex.ApprovedPrefix.ExtendedInfo = extractor.GetUnicodeString(nextPar);
                    extractor.SetElementNode(nextPar, NodeType.Приложение);
                }
                var nameToken = approvedNumberToken.FindForward(AnnexToken.ТипПриложения, 3);
                searchName(nameToken, newAnnex);
                Annexes.Add(newAnnex);
                Count++;
                return true;
            }
            catch(ParserException pe)
            {
                exceptions.Add(pe);
                return false;
            }     
        }

        private bool parseAnnex(Token<AnnexToken> token)
        {
            try
            {
                var annex = new AnnexParserModel();
                annex.Annex.AnnexPrefix = new AnnexPrefix();
                var numberToken = token.Next(AnnexToken.Номер);
                Token<AnnexToken> annexParentToken = null;
                if(numberToken != null)
                {
                    annex.Annex.AnnexPrefix.Number = extractor.GetUnicodeString(numberToken.CustomGroups[0]);
                    annexParentToken = numberToken.Next(AnnexToken.ПриложениеКДокументу);
                    if(annexParentToken == null)
                        annexParentToken = numberToken.Next(AnnexToken.ПриложениеКПриложению);
                }
                else
                {
                    annexParentToken = token.Next(AnnexToken.ПриложениеКДокументу);
                    if(annexParentToken == null)
                        annexParentToken = token.Next(AnnexToken.ПриложениеКПриложению);
                } 
                if(annexParentToken == null)
                {
                    throw new ParserException($"Не могу определить к чему данное приложение {token.Value} {numberToken?.Value} {token.Value} {annex.Annex.AnnexPrefix.AnnexTo}", ErrorType.Fatal);
                }
                if(annexParentToken.TokenType == AnnexToken.ПриложениеКПриложению)
                {
                    var prefixElement = extractor.GetElements(annexParentToken).FirstOrDefault();
                    annex.Annex.AnnexPrefix.AnnexTo = extractor.GetUnicodeString(prefixElement, new TextIndex(annexParentToken.EndIndex - prefixElement.StartIndex, prefixElement.WordElement.Length - (annexParentToken.EndIndex - prefixElement.StartIndex))).Trim();
                    var parentName = annex.Annex.AnnexPrefix.AnnexTo.ToSearchString();
                    var parent = Annexes.FirstOrDefault(f=>f.SearchName == parentName);
                    if(parent == null)
                            throw new ParserException($"Не могу определить предка {token.Value} {numberToken?.Value} {annexParentToken.Value} {annex.Annex.AnnexPrefix.AnnexTo}", ErrorType.Fatal);
                    annex.Parent = parent;
                    annex.Hierarchy = 1;
                    extractor.SetElementNode(prefixElement, NodeType.Приложение);
                }
                else
                {
                    //Здесь больше ничего не делаем и так понятно что приложение к документу который мы разбираем в данный момент
                    //сюда его реквизиты пробрасывать незачем
                    var prefixElement = extractor.GetElements(annexParentToken).FirstOrDefault();
                    annex.Annex.AnnexPrefix.AnnexTo = extractor.GetUnicodeString(prefixElement, new TextIndex(annexParentToken.Length, prefixElement.WordElement.Length - annexParentToken.Length)).Trim();
                    annex.Hierarchy = 0;
                    extractor.SetElementNode(prefixElement, NodeType.Приложение);
                }
                extractor.SetElementNode(token, NodeType.Приложение);
                var nameToken = annexParentToken.FindForward(AnnexToken.ТипПриложения, 3);
                searchName(nameToken, annex);
                Annexes.Add(annex);
                Count++;
                return true;
            }
            catch(ParserException pe)
            {
                exceptions.Add(pe);
                return false;
            }     
        }

        private bool searchName(Token<AnnexToken> nameToken, AnnexParserModel annex)
        {
            if(nameToken == null)
            {
                exceptions.Add(new ParserException($"Не удалось определить наименование приложения № {Annexes.Count + 1}", ErrorType.Fatal));
                return false;
            }
            var nameElement = extractor.GetElements(nameToken).FirstOrDefault();
            if(!extractor.Properties.IsBold(nameElement.WordElement.Element))
                exceptions.Add(new ParserException($"Наименование приложения № {Annexes.Count + 1} не выделено жирным шрифтом", ErrorType.Warning));
            annex.Annex.AnnexType = extractor.GetUnicodeString(nameToken);
            annex.Annex.Name = extractor.GetUnicodeString(nameElement,  new TextIndex(nameToken.Length, nameElement.WordElement.Length - nameToken.Length));
            annex.SearchName = annex.Annex.Name.ToSearchString();
            annex.Annex.SearchName = annex.SearchName;
            extractor.SetElementNode(nameElement, NodeType.Приложение);
            annex.LastElement = nameElement;
            annex.StartIndex = nameElement.ElementIndex + 1;
            var meta = nameElement.Next();
            if(meta != null && meta.MetaInfo.FullIsMeta)
            {
                annex.Annex.Meta = meta.MetaInfo;
                annex.LastElement = meta;
                annex.StartIndex = meta.ElementIndex + 1;
            }
           
            return true;
        }

        
    }
}