using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Extensions;
using Services.Documents.Core.DocumentElements;
using Services.Documents.Lexer;
using Services.Documents.Lexer.Tokens;
using Services.Documents.Parser.TokensDefinitions;
using Services.Documents.Parser.Workers;

namespace Services.Documents.Parser.Parsers.Requisites
{
    public class RequisitesParser : ParserBase<DocumentToken>
    {
        WordProcessing extractor {get;}
        RequisiteTokensModel tokensRequisiteModel {get;} = new RequisiteTokensModel();
        Services.Documents.Core.DocumentElements.Document doc {get;}
        public ElementStructure BeforeBodyElement {get;set;}
        public bool HasErrors => exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
        public bool HasWarnings => exceptions.Any(a=>a.ErrorType == ErrorType.Warning);
        public RequisitesParser(WordProcessing extractor, List<Token<DocumentToken>> tokens, Services.Documents.Core.DocumentElements.Document doc)
        {
            this.extractor = extractor;
            this.tokens = tokens;
            this.doc = doc;
        }

        Predicate<Token<DocumentToken>> IsOrgan = d => 
            d != null 
            && (d.TokenType == DocumentToken.Орган 
                || d.TokenType == DocumentToken.Орган_Президент
                || d.TokenType == DocumentToken.Орган_Правительство);

        Predicate<Token<DocumentToken>> IsDocType = d => 
            d != null 
            && (d.TokenType == DocumentToken.Вид 
                || d.TokenType == DocumentToken.Вид_Закон
                || d.TokenType == DocumentToken.Вид_Кодекс
                || d.TokenType == DocumentToken.Вид_Постановление
                || d.TokenType == DocumentToken.Вид_Распоряжение
                || d.TokenType == DocumentToken.Вид_Указ
                || d.TokenType == DocumentToken.Вид_ФЗ
                || d.TokenType == DocumentToken.Вид_ФКЗ);
        
        Predicate<Token<DocumentToken>> HasGdSfFields = d => 
            d != null 
            && (d.TokenType == DocumentToken.Вид_Кодекс
                || d.TokenType == DocumentToken.Вид_ФЗ
                || d.TokenType == DocumentToken.Вид_ФКЗ);

        public bool Parse()
        {
            Status("Определение реквизитов документа...");
            TypeBlock();
            OrganBlock();
            NameBlock();
            GDSFBlock();
            SignDateNumberExecutorBlock();
            return HasErrors;
        }

        void TypeBlock()
        {
            try
            {
                var typeToken = tokens.FirstOrDefault(f=>IsDocType(f) && f.Position <= 2);
                if(typeToken == null)
                    throw new ParserException("Не удалось определить вид документа", ErrorType.Fatal);
                var stringType = extractor.GetUnicodeString(typeToken);
                if(stringType == null)
                    throw new ParserException($"Параграф вида документа \"{typeToken.Value}\" не найден", ErrorType.Fatal);
                stringType = stringType.NormalizeWhiteSpaces().NormalizeCase().Trim();
                doc.Type = stringType;
                tokensRequisiteModel.typeToken = typeToken;
            }
            catch(ParserException pe)
            {
                exceptions.Add(pe);
            }
        }

        
        void SignDateNumberExecutorBlock()
        {
            try
            {
                tokensRequisiteModel.postToken = tokens.FirstOrDefault(f=>f.TokenType == DocumentToken.Должность);
                while(tokensRequisiteModel.postToken != null)
                {
                    tokensRequisiteModel.executorToken = tokens.Next(tokensRequisiteModel.postToken);
                    if(tokensRequisiteModel.executorToken == null || tokensRequisiteModel.executorToken.TokenType != DocumentToken.Подписант)
                        tokensRequisiteModel.postToken = tokens.FirstOrDefault(f=>f.TokenType == DocumentToken.Должность 
                                                            && f.Position > tokensRequisiteModel.postToken.Position);
                    else break;
                }
                if(tokensRequisiteModel.postToken == null)
                    throw new ParserException($"Не удалось определить должность подписанта", ErrorType.Fatal);
                if(tokensRequisiteModel.executorToken == null)
                    throw new ParserException($"Не удалось определить ФИО подписанта", ErrorType.Fatal);
                var POST = extractor.GetUnicodeString(tokensRequisiteModel.postToken);
                if(POST == null)
                    throw new ParserException($"Параграф должности не найден", ErrorType.Fatal);
                
                var EXECUTOR = extractor.GetUnicodeString(tokensRequisiteModel.executorToken);
                if(EXECUTOR == null)
                    throw new ParserException("Параграф ФИО не найден", ErrorType.Fatal);
                if(tokensRequisiteModel.organToken.TokenType == DocumentToken.Орган_Правительство)
                    tokensRequisiteModel.signDateToken = tokens.Next(tokensRequisiteModel.typeToken);
                else
                    tokensRequisiteModel.signDateToken = tokens.GetToken(tokensRequisiteModel.executorToken, DocumentToken.ДлиннаяДата, 8);
                if(tokensRequisiteModel.signDateToken == null)
                    throw new ParserException("Не удалось определить дату подписания", ErrorType.Fatal);
                tokensRequisiteModel.numberToken = tokens.Next(tokensRequisiteModel.signDateToken);
                if(tokensRequisiteModel.numberToken == null)
                    throw new ParserException("Не удалось определить номер документа", ErrorType.Fatal);
                var signDate = DateTimeHelpers.GetDate(tokensRequisiteModel.signDateToken.CustomGroups[0].Value,
                                                        tokensRequisiteModel.signDateToken.CustomGroups[1].Value,
                                                        tokensRequisiteModel.signDateToken.CustomGroups[2].Value,
                                                        true);
                doc.Executors.Add(new Executor(EXECUTOR, POST.Trim()));
                doc.SignDate = signDate.Value;
                var number = extractor.GetUnicodeString(tokensRequisiteModel.numberToken.CustomGroups[0]);
                doc.Numbers.Add(new Number(number));
                extractor.SetElementNode(tokensRequisiteModel.postToken, Core.NodeType.stop);
                extractor.SetElementNode(tokensRequisiteModel.executorToken, Core.NodeType.stop);
                extractor.SetElementNode(tokensRequisiteModel.numberToken, Core.NodeType.stop);
                extractor.SetElementNode(tokensRequisiteModel.signDateToken, Core.NodeType.stop);
               

        }
        catch(ParserException pe)
        {
            exceptions.Add(pe);
        }       
        }
        /// <summary>
        /// Парсинг блоков
        /// одобрен сф
        /// принят гд
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokens"></param>
        /// <param name="doc"></param>
        void GDSFBlock()
        {
            List<ParserException> exceptions = new List<ParserException>();
            if (!HasGdSfFields(tokensRequisiteModel.typeToken))
                return;
            try
            {
                //TODO не всешда присутствует дата принятия совфедом!!!
                var gd = tokens.FirstOrDefault(f => f.TokenType == DocumentToken.ПринятГД);
                if (gd == null)
                    throw new ParserException($"После наименования документа должна идти дата принятия в Государственной Думе, но она не найдена", ErrorType.Fatal);
                var gdDate = tokens.Next(gd);
                if (gdDate == null)
                    throw new ParserException($"После наименования документа должна идти дата принятия в Государственной Думе, но она не найдена", ErrorType.Fatal);
                var GDDATE = DateTimeHelpers.GetDate(gdDate.CustomGroups[0].Value, gdDate.CustomGroups[1].Value, gdDate.CustomGroups[2].Value, true);
                if (!GDDATE.HasValue)
                    throw new ParserException($"Дата принятия в Государственной Думе не распознана: {gdDate.Value} - неправильный формат даты", ErrorType.Fatal);
                doc.GDDate = GDDATE;
                extractor.SetElementNode(gdDate, Core.NodeType.stop);
                var sfToken = tokens.Next(gdDate);
                if (sfToken == null || sfToken.TokenType != DocumentToken.ОдобренСФ)
                    throw new ParserException($"После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена", ErrorType.Warning);
                var sfDate = tokens.Next(sfToken);
                if (sfDate == null)
                    throw new ParserException($"После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена", ErrorType.Warning);
                var SFDATE = DateTimeHelpers.GetDate(sfDate.CustomGroups[0].Value, sfDate.CustomGroups[1].Value, sfDate.CustomGroups[2].Value, true);
                if (!SFDATE.HasValue)
                    throw new ParserException($"Дата одобрения в Совете Федерации не распознана: {sfDate.Value} - неправильный формат даты", ErrorType.Fatal);
                doc.SFDate = SFDATE;
                extractor.SetElementNode(sfDate, Core.NodeType.stop);
                BeforeBodyElement = extractor.GetElements(sfDate).FirstOrDefault();
            }
            catch (ParserException pe)
            {
                exceptions.Add(pe);
            }
        }

        void OrganBlock ()
        {
            try
            {
                var before = tokensRequisiteModel.typeToken.Before();
                if(IsOrgan(before))
                {
                    tokensRequisiteModel.organToken = before;
                }
                var next = tokensRequisiteModel.typeToken.Next();
                if(IsOrgan(next))
                {
                    tokensRequisiteModel.organToken = next;
                }
                if(tokensRequisiteModel.organToken == null)
                    throw new ParserException($"В найденых ключах: {before?.Value ?? ""} {next?.Value ?? ""} Принявший орган не обнаружен", ErrorType.Fatal);
                var organ = extractor.GetElements(tokensRequisiteModel.organToken).FirstOrDefault();
                if(organ == null)
                    throw new ParserException($"Параграф с принявшим органом не найден", ErrorType.Fatal);
                doc.Organs.Add(new Organ(extractor.GetUnicodeString(organ).NormalizeWhiteSpaces().NormalizeCaseAll())); 
            }
            catch (ParserException pe)
            {
                exceptions.Add(pe);
            }
        }
        /// <summary>
        /// Парсинг наименования
        /// </summary>
        void NameBlock()
        {
            try
            {
                Token<DocumentToken> lastToken = null;
                if(tokensRequisiteModel.typeToken.Position > tokensRequisiteModel.organToken.Position)
                    lastToken = tokensRequisiteModel.typeToken;
                else lastToken = tokensRequisiteModel.organToken;
                var mayBeName = tokens.Next(lastToken);
                if(tokensRequisiteModel.organToken.TokenType == DocumentToken.Орган_Правительство)
                {
                    mayBeName = tokens.Next(lastToken, 3);
                }

                if(mayBeName == null && mayBeName.TokenType != DocumentToken.Слово)
                    throw new ParserException($"После \"{tokens.Before(mayBeName ?? lastToken).Value}\" должно начинаться наименование документа, но оно не найдено", ErrorType.Fatal);
                var name = extractor.GetElements(mayBeName).FirstOrDefault();
                if(name == null)
                    throw new ParserException($"Параграф с наименованием документа не найден", ErrorType.Fatal);
                if(tokensRequisiteModel.organToken.TokenType == DocumentToken.Орган_Правительство)
                {
                    if(!extractor.Properties.IsBold(name.WordElement.Element))
                    {
                        name.Before().NodeType = Core.NodeType.stop;
                        BeforeBodyElement = name.Before();
                        throw new ParserException($"Наименование должно быть выделено жирным шрифтом", ErrorType.Warning);
                    }

                }
                name.NodeType = Core.NodeType.stop;
                BeforeBodyElement = name;
                doc.Name = extractor.GetUnicodeString(name);
                tokensRequisiteModel.nameToken = mayBeName;
            }
            catch (ParserException pe)
            {
                exceptions.Add(pe);
            }
        }
    }
}