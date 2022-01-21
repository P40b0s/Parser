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
    public class AnnexParser : LexerBase<AnnexToken>
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
        // к Правилам(токен, правилам, реестрам, составам итд) предоставления
        // коммунальных услуг собственникам
        // и пользователям помещений
        // в многоквартирных домах и жилых домов
        //  
        //  
        // ТРЕБОВАНИЯ
        // к качеству коммунальных услуг
        public bool Parse()
        {
            UpdateStatus("Поиск приложений");
            var percentage = 0;
            Tokenize(extractor.FullText, new AnnexTokensDefinition());
            //tokens = lexer.Tokenize(extractor.FullText, new AnnexTokensDefinition()).ToList();
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
                UpdateStatus("Поиск приложений...", count, percentage);
            }
            foreach(var a in Annexes)
            {
                var items = a.LastElement.TakeWhile();
                var last = items.LastOrDefault();
                if(last != null)
                    a.EndIndex = last.ElementIndex;
                a.RootElements.AddRange(items);
            }
            return !HasFatalError;
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
           
            var approvedDataToken = token.Next(AnnexToken.Дата);
            if(approvedDataToken.IsError)
                return AddError("Дата утверждения приложения не найдена");
            var approvedNumberToken = approvedDataToken.Value.Next(AnnexToken.Номер);
            if(approvedNumberToken.IsError)
                return AddError("Номер утверждения приложения не найден");
            var newAnnex = new AnnexParserModel();
            //так как это блок "УТВЕРЖДЕНО" то здесь будет инфа об утвердившем органе
            newAnnex.Annex.ApprovedPrefix = new AnnexApprovedPrefix();
            newAnnex.Annex.ApprovedPrefix.Prefix = token.Value;
            //должно по идее все быть в пределах 1 параграфа
            var approvedElement = extractor.GetElements(token).FirstOrDefault();
            extractor.SetElementNode(approvedElement, NodeType.Приложение);
            newAnnex.Annex.ApprovedPrefix.Number = extractor.GetUnicodeString(approvedNumberToken.Value.CustomGroups[0]);
            var date = approvedDataToken.Value.GetDate();
            if(!date.IsError)
                return AddError($"В приложении № {Annexes.Count + 1} " + date.Error.Message);
            newAnnex.Annex.ApprovedPrefix.Date = date.Value.Value;
            newAnnex.Annex.ApprovedPrefix.Organ = extractor.GetUnicodeString(approvedElement, new TextIndex(token.Length, (approvedDataToken.Value.StartIndex - token.Length - approvedElement.StartIndex))).Trim();
            //возможно надо взять в расчет indentation(left) у этих абзацев он примерно 5000
            //прверяем следующий может такое быть что это типа лоп информации
            // у него такой же отступ как у первго
            var nextPar = approvedElement.Next();
            //найден дополнительный параграф с какими то комментами
            if(nextPar.IsOk && nextPar.Element.ParagraphProperties.Ind?.Left > 300)
            {
                newAnnex.Annex.ApprovedPrefix.ExtendedInfo = extractor.GetUnicodeString(nextPar.Element);
                extractor.SetElementNode(nextPar.Element, NodeType.Приложение);
            }
            var nameToken = approvedNumberToken.Value.FindForward(AnnexToken.ТипПриложения, 3);
            if(!searchName(nameToken, newAnnex))
                return false;
            Annexes.Add(newAnnex);
            Count++;
            return true;
        }

        private bool parseAnnex(Token<AnnexToken> token)
        {
            var annex = new AnnexParserModel();
            annex.Annex.AnnexPrefix = new AnnexPrefix();
            var numberToken = token.Next(AnnexToken.Номер);
            var annexParentToken = getAnnexParent(token, annex);
            if(annexParentToken.IsError)
            {
                var n = numberToken.IsOk ? numberToken.Value.Value : numberToken.Error.Message;
                return AddError($"Не могу определить поле \"Утверждено...\" {token.Value} {n} {annex.Annex.AnnexPrefix.AnnexTo}",annexParentToken.Error);
            }
                
            if(annexParentToken.Value.TokenType == AnnexToken.ПриложениеКПриложению)
            {
                var prefixElement = extractor.GetElements(annexParentToken.Value).FirstOrDefault();
                annex.Annex.AnnexPrefix.AnnexTo = extractor.GetUnicodeString(prefixElement, new TextIndex(annexParentToken.Value.EndIndex - prefixElement.StartIndex, prefixElement.WordElement.Length - (annexParentToken.Value.EndIndex - prefixElement.StartIndex))).Trim();
                var parentName = annex.Annex.AnnexPrefix.AnnexTo.ToSearchString();
                var parent = Annexes.FirstOrDefault(f=>f.SearchName == parentName);
                if(parent == null)
                {
                    var n = numberToken.IsOk ? numberToken.Value.Value : numberToken.Error.Message;
                    return AddError($"Не могу определить предка (Не найдено наименование приложения {annex.Annex.AnnexPrefix.AnnexTo}) {token.Value} {n} {annexParentToken.Value.Value} {annex.Annex.AnnexPrefix.AnnexTo}");
                }
                annex.Parent = parent;
                annex.Hierarchy = 1;
                extractor.SetElementNode(prefixElement, NodeType.Приложение);
            }
            else
            {
                //Здесь больше ничего не делаем и так понятно что приложение к документу который мы разбираем в данный момент
                //сюда его реквизиты пробрасывать незачем
                var prefixElement = extractor.GetElements(annexParentToken.Value).FirstOrDefault();
                annex.Annex.AnnexPrefix.AnnexTo = extractor.GetUnicodeString(prefixElement, new TextIndex(annexParentToken.Value.Length, prefixElement.WordElement.Length - annexParentToken.Value.Length)).Trim();
                annex.Hierarchy = 0;
                extractor.SetElementNode(prefixElement, NodeType.Приложение);
            }
            extractor.SetElementNode(token, NodeType.Приложение);
            var nameToken = annexParentToken.Value.FindForward(AnnexToken.ТипПриложения, 3);
            if(!searchName(nameToken, annex))
                return false;
            Annexes.Add(annex);
            Count++;
            return true;
        }

        private Result<Token<AnnexToken>, TokenException> getAnnexParent(Token<AnnexToken> token, AnnexParserModel annex)
        {
            var numberToken = token.Next(AnnexToken.Номер);
            if(numberToken.IsOk)
            {
                annex.Annex.AnnexPrefix.Number = extractor.GetUnicodeString(numberToken.Value.CustomGroups[0]);
                var annexParentToken = numberToken.Value.Next(AnnexToken.ПриложениеКДокументу);
                if(annexParentToken.IsError)
                    annexParentToken = numberToken.Value.Next(AnnexToken.ПриложениеКПриложению);
                return annexParentToken;
            }
            else
            {
                var annexParentToken = token.Next(AnnexToken.ПриложениеКДокументу);
                if(annexParentToken.IsError)
                    annexParentToken = token.Next(AnnexToken.ПриложениеКПриложению);
                return annexParentToken;
            } 
        }

        private bool searchName(Result<Token<AnnexToken>, TokenException> nameToken, AnnexParserModel annex)
        {
            if(nameToken.IsError)
                return AddError($"Не удалось определить наименование приложения № {Annexes.Count + 1} ", nameToken.Error);
            
            var nameElement = extractor.GetElements(nameToken.Value).FirstOrDefault();
            if(!extractor.Properties.IsBold(nameElement.WordElement.Element))
                AddError($"Наименование приложения № {Annexes.Count + 1} не выделено жирным шрифтом", ErrorType.Warning);
            annex.Annex.AnnexType = extractor.GetUnicodeString(nameToken.Value);
            annex.Annex.Name = extractor.GetUnicodeString(nameElement,  new TextIndex(nameToken.Value.Length, nameElement.WordElement.Length - nameToken.Value.Length));
            annex.SearchName = annex.Annex.Name.ToSearchString();
            annex.Annex.SearchName = annex.SearchName;
            extractor.SetElementNode(nameElement, NodeType.Приложение);
            annex.LastElement = nameElement;
            annex.StartIndex = nameElement.ElementIndex + 1;
            //Поиск мета информации
            var meta = nameElement.Next();
            if(meta.IsOk && meta.Element.MetaInfo.FullIsMeta)
            {
                annex.Annex.Meta = meta.Element.MetaInfo;
                annex.LastElement = meta.Element;
                annex.StartIndex = meta.Element.ElementIndex + 1;
            }
            return true;
        }

        
    }
}