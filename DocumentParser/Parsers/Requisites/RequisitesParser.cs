using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Extensions;
using DocumentParser.DocumentElements;
using Lexer;
using DocumentParser.TokensDefinitions;
using DocumentParser.Workers;
using DocumentParser.Elements;
using CSharpFunctionalExtensions;
using System.Text.RegularExpressions;

namespace DocumentParser.Parsers.Requisites;

public class RequisitesParser : LexerBase<DocumentToken>
{
    WordProcessing extractor {get;}
    RequisiteTokensModel tokensRequisiteModel {get;} = new RequisiteTokensModel();
    DocumentElements.Document doc {get;}
    public ElementStructure BeforeBodyElement {get;set;}
    public bool HasErrors => exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
    public bool HasWarnings => exceptions.Any(a=>a.ErrorType == ErrorType.Warning);
    RequisitesParserRules rules {get;}
    public RequisitesParser(WordProcessing extractor, DocumentElements.Document doc)
    {
        this.extractor = extractor;
        this.doc = doc;
        rules = extractor.Settings.RequisitesParserRules;
        Tokenize(extractor.FullText, new DocumentsTokensDefinition());
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
        UpdateStatus("Определение реквизитов документа...");
        if(!TypeBlock())
            return HasErrors;
        if(!OrganBlock())
            return HasErrors;
        if(NameBlock())
            return HasErrors;
        if(GDSFBlock())
            return HasErrors;
        if(!SignDateNumberExecutorBlock())
            return HasErrors;
        return HasErrors;
    }

    bool TypeBlock()
    {
        if(tokens.Count == 0)
            return AddError("Не найдено ни одного токена, поиск реквизитов невозможен");
        var first = tokens[0];
        var typeToken = first.FindForward(IsDocType, rules.DefaultRules.TypeSearchMaxDeep);
        if(typeToken.IsError)
            return AddError("Не удалось определить вид документа: ", typeToken.Error);
        var stringType = extractor.GetUnicodeString(typeToken.Value);
        if(stringType == null)
            return AddError($"Параграф вида документа \"{typeToken.Value.Value}\" не найден");
        stringType = stringType.NormalizeWhiteSpaces().NormalizeCase().Trim();
        doc.Type = stringType;
        tokensRequisiteModel.typeToken = typeToken.Value; 
        return true;
    }
     bool OrganBlock ()
    {
        var before = tokensRequisiteModel.typeToken.FindBackwardMany(IsOrgan);
        var next = tokensRequisiteModel.typeToken.FindForwardMany(IsOrgan);
        if(before.Count == 0 && next.Count == 0)
        {
            exceptions.Add(new ParserException($"Не найдено ни одного принявшего органа, поиск осуществлялся от токена \"{tokensRequisiteModel.typeToken.Value}\", индекс: {tokensRequisiteModel.typeToken.StartIndex}"));
            return false;
        }
        if(before.Count > 0)
        {
            foreach(var o in before)
                tokensRequisiteModel.organsTokens.Add(o);
        }
        if(next.Count > 0)
        {
            foreach(var o in next)
                tokensRequisiteModel.organsTokens.Add(o);
        }
        foreach(var o in tokensRequisiteModel.organsTokens)
        {
            var organ = new Organ(extractor.GetUnicodeString(o).NormalizeWhiteSpaces().NormalizeCase());
            doc.Organs.Add(organ); 
        }
        //Проверяем кастомные правила для данного органа\типа документа, если они находятся то заменяем дефолтные на эти
        foreach(var c in rules.CustomRequisiteRules)
        {
            var org = new Regex(c.Organ);
            var tp = new Regex(c.Type);
            if(tp.IsMatch(tokensRequisiteModel.typeToken.Value))
            {
                foreach(var o in tokensRequisiteModel.organsTokens)
                    if(org.IsMatch(o.Value))
                        rules.DefaultRules = c.RequisiteRule;
            }
        }
        return true;
    }

    
    bool SignDateNumberExecutorBlock()
    {
        var posts = tokens.FindAll(f=>f.TokenType == DocumentToken.Должность);
        if(posts.Count == 0)
            return AddError("Не удалось определить должность подписанта");
        //Должностей то несколько
        foreach(var p in posts)
        {
            var executor = p.Next(DocumentToken.Подписант);
            if(executor.IsError)
            {
                AddError($"Не удалось определить ФИО подписанта для должности {p.Value}", executor.Error);
            }
            else
                tokensRequisiteModel.personToken.Add(new ExecutorRequisiteToken(){postToken = p, executorToken = executor.Value});
        }
        if(tokensRequisiteModel.personToken.Count == 0)
            return false;
        foreach(var e in tokensRequisiteModel.personToken)
        {
            var POST = extractor.GetUnicodeString(e.postToken);
            var EXECUTOR = extractor.GetUnicodeString(e.executorToken);
            doc.Executors.Add(new Executor(EXECUTOR, POST.Trim()));
        }
        //Дату не находим, заканчиваем метод
        if(!getSignDate())
            return false;
            
        var number =  tokensRequisiteModel.signDateToken.Next(DocumentToken.Номер);
        if(number.IsError)
            return AddError($"Не удалось определить номер документа ", number.Error);
        var n = extractor.GetUnicodeString(tokensRequisiteModel.numberToken.CustomGroups[0]);
        doc.Numbers.Add(new Number(n));

        var signDate = tokensRequisiteModel.signDateToken.GetDate();
        if(signDate.IsError)
            return AddError(signDate.Error.Message, number.Error);
        doc.SignDate = signDate.Value.Value;
        foreach(var p in tokensRequisiteModel.personToken)
        {
            extractor.SetElementNode(p.postToken, NodeType.stop);
            extractor.SetElementNode(p.executorToken, NodeType.stop);
        }
        extractor.SetElementNode(tokensRequisiteModel.numberToken, NodeType.stop);
        extractor.SetElementNode(tokensRequisiteModel.signDateToken, NodeType.stop);
        return true;  
    }

    private bool getSignDate()
    {
        bool header = false;
        bool footer = false;
        if(rules.SearchSignDateOnHeader)
        {   
            var signD = tokensRequisiteModel.typeToken.FindForward(DocumentToken.ДлиннаяДата, rules.SignDateSearchMaxDeep);
            if(signD.IsOk)
            {
                header = true;
                tokensRequisiteModel.signDateToken = signD.Value;
            }
        }
        if(rules.SearchSignDateOnFooter)
        {
            var signD = tokensRequisiteModel.personToken.Last().executorToken.FindForward(DocumentToken.ДлиннаяДата, rules.SignDateSearchMaxDeep);
            if(signD.IsOk)
            {
                footer =  true;
                tokensRequisiteModel.signDateToken = signD.Value;
            }
        }
        if(!header && !footer)
            return AddError("Не удалось найти дату подписания");
        else return true;
    }



    /// <summary>
    /// Парсинг блоков
    /// одобрен сф
    /// принят гд
    /// </summary>
    /// <param name="token"></param>
    /// <param name="tokens"></param>
    /// <param name="doc"></param>
    bool GDSFBlock()
    {
       
        if (!HasGdSfFields(tokensRequisiteModel.typeToken))
            return true;
        //TODO не всешда присутствует дата принятия совфедом!!!
        var gd = tokens.FirstOrDefault(f => f.TokenType == DocumentToken.ПринятГД);
        if (gd == null)
            return AddError("Дата принятия в Государственной Думе не найдена");
        var gdDate = gd.Next(DocumentToken.ДлиннаяДата);
        if (gdDate.IsError)
            return AddError("Неверный формат или отсуствует дата принятия в Государственной Думе ", gdDate.Error);
        var GDDATE = gdDate.Token.GetDate();
        if (GDDATE.IsError)
            return AddError($"Дата принятия в Государственной Думе не распознана: {GDDATE.Error.Message}");
        doc.GDDate = GDDATE.Date.Value;
        extractor.SetElementNode(gdDate.Token, NodeType.stop);
        var sfToken = gdDate.Token.Next(DocumentToken.ОдобренСФ);
        if (sfToken.IsError)
            AddError("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error , ErrorType.Warning);
        var sfDate = sfToken.Token.Next();
        if (sfDate.IsError)
            return AddError("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error);
        var SFDATE = sfDate.Token.GetDate();
        if (!SFDATE.IsError)
            return AddError($"Дата одобрения в Совете Федерации не распознана: {SFDATE.Error.Message}");
        doc.SFDate = SFDATE.Date.Value;
        extractor.SetElementNode(sfDate.Token, NodeType.stop);
        BeforeBodyElement = extractor.GetElements(sfDate.Token).FirstOrDefault();
        return true;
    }

   

   
    /// <summary>
    /// Парсинг наименования
    /// </summary>
    bool NameBlock()
    {
        if(rules.DefaultRules.NameInTypeString)
        {
            var nameToken = extractor.GetElements(tokensRequisiteModel.typeToken).FirstOrDefault();
            if(nameToken == null)
                return AddError("Наименование не найдено, внимание включен параметр - наименование находится в абзаце типа документа!");
            doc.Name = extractor.GetUnicodeString(nameToken);
            return true;
        }
        Token<DocumentToken> lastToken = null;
        if(tokensRequisiteModel.typeToken.Position > tokensRequisiteModel.organsTokens.Last().Position)
            lastToken = tokensRequisiteModel.typeToken;
        else lastToken = tokensRequisiteModel.organsTokens.Last();
        
        var mayBeName = lastToken.FindForward(DocumentToken.НачалоПредложения, 2);
        bool isGov = false;
        //FIXME хз будет так работать или нет ПРОВЕРИТЬ!
        if(tokensRequisiteModel.organsTokens.Any(a=> a.TokenType == DocumentToken.Орган_Правительство))
        {
            mayBeName = lastToken.FindForward(DocumentToken.НачалоПредложения, 4);
            isGov = true;
        }
        if(mayBeName.IsError)
            return AddError("Наименование документа не найдено ", mayBeName.Error);
        var name = extractor.GetElements(mayBeName.Token).FirstOrDefault();
        if(isGov || extractor.Settings.ParserRules.BoldNameRule)
        {
            if(!extractor.Properties.IsBold(name.WordElement.Element))
            {
                name.Before().NodeType = NodeType.stop;
                BeforeBodyElement = name.Before();
                exceptions.Add(new ParserException("Наименование должно быть выделено жирным шрифтом, если наименование не выделено, то считается что его нет", ErrorType.Warning));
            }
        }
        name.NodeType = NodeType.stop;
        BeforeBodyElement = name;
        doc.Name = extractor.GetUnicodeString(name);
        tokensRequisiteModel.nameToken = mayBeName.Token;
        return true;
    }
}