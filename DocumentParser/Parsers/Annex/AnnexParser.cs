using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using DocumentParser.DocumentElements;
using System.Linq;
using Utils.Extensions;
using SettingsWorker;
using SettingsWorker.Annexes;
using Utils;
using DocumentParser.Parsers.Headers;
using DocumentParser.DocumentElements.FootNotes;

namespace DocumentParser.Parsers.Annex
{
    //центральный модуль парсинга документа, один на каждый док, отсюда вызываются все остальные модули
    public class AnnexParser : LexerBase<AnnexTokenType>
    {
        public List<AnnexParserModel> Annexes {get;} = new List<AnnexParserModel>();
        bool withMeta {get;set;}
        bool withChanges {get;set;}
        bool withTables {get;set;}
        public AnnexParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
        }
        public int Count {get;set;}
        public int AnnexesCount {get;set;}
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
            if (!withMeta)
                AddError("Парсер запущен без параметров мата-информации, мета данные не будут добавлены.", ErrorType.Warning);
            if (!withChanges)
                AddError("Парсер запущен без параметра поиска изменений. Возможно ошибочное добавление приложений из параграфов с внесением изменения.", ErrorType.Warning);
            if (!withTables)
                AddError("Парсер запущен без параметров привязки таблиц. Таблицы не будут добавлены к заголовкам приложений", ErrorType.Warning);
            UpdateStatus("Поиск приложений");
            var percentage = 0;
            Tokenize(extractor.FullText, new AnnexTokensDefinition(settings.TokensDefinitions.AnnexTokenDefinitions.TokenDefinitionSettings));
            var count = tokens.Count();
            foreach(var token in tokens)
            {
               
                if(token.TokenType == AnnexTokenType.Утверждено)
                {
                    parseApproved(token);
                }
                if(token.TokenType == AnnexTokenType.Приложение)
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
            if(withTables)
                getTables();
            return !HasFatalError;
        }
                /// <summary>
        /// Использовать информацию полученнцю от MetaParser и привязать ее к заголовкам
        /// </summary>
        /// <returns></returns>
        public AnnexParser WithMeta()
        {
            withMeta = true;
            return this;
        }
        /// <summary>
        /// Использовать информацию полученнцю от ChangesParser для корректного распознавания заголовков
        /// </summary>
        /// <returns></returns>
        public AnnexParser WithChanges()
        {
            withChanges = true;
            return this;
        }
        /// <summary>
        /// Использовать информацию полученнцю от TableParser для привязки таблиц к заголовкам
        /// </summary>
        /// <returns></returns>
        public AnnexParser WithTables()
        {
            withTables = true;
            return this;
        }
        void getTables()
        {
            for(int a = 0; a < Annexes.Count; a++)
            {
                var firstRootItem = Annexes[a].RootElements.FirstOrDefault();
                if(firstRootItem != null)
                {
                    if(firstRootItem.NodeType == NodeType.Таблица)
                    {
                        Annexes[a].Annex.Table = firstRootItem.Table;
                        Annexes[a].RootElements.Remove(firstRootItem);
                    }
                    else
                    {
                        var table = firstRootItem.FindForward(t=>t.NodeType == NodeType.Таблица, 1);
                        if(table.IsOk)
                        {
                            Annexes[a].Annex.Table = table.Value.Table;
                            Annexes[a].RootElements.Remove(table.Value);
                        }
                    }
                }
            }
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

        private bool parseApproved(Token<AnnexTokenType> token)
        {
           
            var approvedDataToken = token.Next(AnnexTokenType.Дата);
            if(approvedDataToken.IsError)
                return AddError("Дата утверждения приложения не найдена");
            var approvedNumberToken = approvedDataToken.Value.Next(AnnexTokenType.Номер);
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
            if(date.IsError)
                return AddError($"В приложении № {Annexes.Count + 1} " + date.Error.Message);
            newAnnex.Annex.ApprovedPrefix.Date = date.Value.Value;
            newAnnex.Annex.ApprovedPrefix.Organ = extractor.GetUnicodeString(approvedElement, new TextIndex(token.Length, (approvedDataToken.Value.StartIndex - token.Length - approvedElement.StartIndex))).Trim();
            //возможно надо взять в расчет indentation(left) у этих абзацев он примерно 5000
            //прверяем следующий может такое быть что это типа лоп информации
            // у него такой же отступ как у первго
            var nextPar = approvedElement.Next();
            //найден дополнительный параграф с какими то комментами
            if(nextPar.IsOk && nextPar.IsOk && nextPar.Value.ParagraphProperties.Ind?.Left > 300)
            {
                newAnnex.Annex.ApprovedPrefix.ExtendedInfo = extractor.GetUnicodeString(nextPar.Value);
                extractor.SetElementNode(nextPar.Value, NodeType.Приложение);
            }
            var nameToken = approvedNumberToken.Value.FindForward(AnnexTokenType.ТипПриложения, 3);
            if(!searchName(nameToken, newAnnex))
                return false;
            Annexes.Add(newAnnex);
            Count++;
            return true;
        }

        private bool parseAnnex(Token<AnnexTokenType> token)
        {
            var annex = new AnnexParserModel();
            annex.Annex.AnnexPrefix = new AnnexPrefix();
            var numberToken = token.Next(AnnexTokenType.Номер);
            var annexParentToken = getAnnexParent(token, annex);
            if(annexParentToken.IsError)
            {
                var n = numberToken.IsOk ? numberToken.Value.Value : numberToken.Error.Message;
                return AddError($"Не могу определить поле \"Утверждено...\" {token.Value} {n} {annex.Annex.AnnexPrefix.AnnexTo}",annexParentToken.Error);
            }
                
            if(annexParentToken.Value.TokenType == AnnexTokenType.ПриложениеКПриложению)
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
            var nameToken = annexParentToken.Value.FindForward(AnnexTokenType.ТипПриложения, 3);
            if(!searchName(nameToken, annex))
                return false;
            Annexes.Add(annex);
            Count++;
            return true;
        }

        private Result<Token<AnnexTokenType>, TokenException> getAnnexParent(Token<AnnexTokenType> token, AnnexParserModel annex)
        {
            var numberToken = token.Next(AnnexTokenType.Номер);
            if(numberToken.IsOk)
            {
                annex.Annex.AnnexPrefix.Number = extractor.GetUnicodeString(numberToken.Value.CustomGroups[0]);
                var annexParentToken = numberToken.Value.Next(AnnexTokenType.ПриложениеКДокументу);
                if(annexParentToken.IsError)
                    annexParentToken = numberToken.Value.Next(AnnexTokenType.ПриложениеКПриложению);
                return annexParentToken;
            }
            else
            {
                var annexParentToken = token.Next(AnnexTokenType.ПриложениеКДокументу);
                if(annexParentToken.IsError)
                    annexParentToken = token.Next(AnnexTokenType.ПриложениеКПриложению);
                return annexParentToken;
            } 
        }

        private bool searchName(Result<Token<AnnexTokenType>, TokenException> nameToken, AnnexParserModel annex)
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
            if(withMeta && meta.IsOk && meta.IsOk && meta.Value.MetaInfo.FullIsMeta)
            {
                annex.Annex.Meta = meta.Value.MetaInfo;
                annex.LastElement = meta.Value;
                annex.StartIndex = meta.Value.ElementIndex + 1;
            }
            return true;
        }

        /// <summary>
        /// Перемещение приложений согласно их иерархии (могут быть приложения к приложению)
        /// Этот метод вызывается последним! после заголовков и итемов, а то потом 
        /// появятся вложенные приложения и уже ничего не найдется
        /// Автоматические вызывается в ItemsParser
        /// </summary>
        public void SortAnnexByHierarchy()
        {
            AnnexesCount = Annexes.Count;
             //Рассовываем все приложения согласно иерархии
            List<AnnexParserModel> forRemove = new List<AnnexParserModel>();
            List<AnnexParserModel> except = new List<AnnexParserModel>();
            Annexes.Reverse();
            for (int i = 0; i < Annexes.Count; i++)
            {
                var first = Annexes.Except(except).FirstOrDefault(f=>f.Hierarchy < Annexes[i].Hierarchy);
                if(first != null)
                {
                    if(Annexes.IndexOf(first) < i)
                    {
                        except.Add(first);
                        i--;
                        continue;
                    }
                    if(first.Annex.Annexes == null)
                            first.Annex.Annexes = new List<DocumentElements.Annex>();
                    first.Annex.Annexes.Insert(0, Annexes[i].Annex);
                    forRemove.Add(Annexes[i]);
                }

            }
            Annexes.RemoveAll(r=>forRemove.Contains(r));
            Annexes.Reverse();
        }

        
    }
}