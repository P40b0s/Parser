using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Extensions;
using DocumentParser.DocumentElements;
using Lexer;
using DocumentParser.TokensDefinitions;
using DocumentParser.Workers;
namespace DocumentParser.Parsers.Requisites;

public class RequisitesParser : ParserBase<DocumentToken>
{
    WordProcessing extractor {get;}
    RequisiteTokensModel tokensRequisiteModel {get;} = new RequisiteTokensModel();
    DocumentElements.Document doc {get;}
    public ElementStructure BeforeBodyElement {get;set;}
    public bool HasErrors => exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
    public bool HasWarnings => exceptions.Any(a=>a.ErrorType == ErrorType.Warning);
    public RequisitesParser(WordProcessing extractor, List<Token<DocumentToken>> tokens, DocumentElements.Document doc)
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
        {
            exceptions.Add(new ParserException("Не найдено ни одного токена, поиск реквизитов невозможен", ErrorType.Fatal));
            return false;
        }
        var first = tokens[0];
        var typeToken = first.FindForward(IsDocType, 2);
        if(!typeToken.IsOk)
        {
            exceptions.Add(new ParserException("Не удалось определить вид документа: " + typeToken.Error, ErrorType.Fatal));
            return false;
        }
        var stringType = extractor.GetUnicodeString(typeToken.Token);
        if(stringType == null)
        {
            exceptions.Add(new ParserException($"Параграф вида документа \"{typeToken.Token.Value}\" не найден", ErrorType.Fatal));
            return false;
        }
        stringType = stringType.NormalizeWhiteSpaces().NormalizeCase().Trim();
        doc.Type = stringType;
        tokensRequisiteModel.typeToken = typeToken.Token;
        return true;
    }

    
    bool SignDateNumberExecutorBlock()
    {
        var posts = tokens.FindAll(f=>f.TokenType == DocumentToken.Должность);
        if(posts.Count == 0)
        {
            exceptions.Add(new ParserException("Не удалось определить должность подписанта", ErrorType.Fatal));
            return false;
        }
        //Должностей то несколько
        foreach(var p in posts)
        {
            var executor = p.Next(DocumentToken.Подписант);
            if(!executor.IsOk)
            {
                exceptions.Add(new ParserException($"Не удалось определить ФИО подписанта для должности {p.Value}", executor.Error));
            }
            else
                tokensRequisiteModel.personToken.Add(new ExecutorRequisiteToken(){postToken = p, executorToken = executor.Token});
        }
        if(tokensRequisiteModel.personToken.Count == 0)
            return false;
        foreach(var e in tokensRequisiteModel.personToken)
        {
            var POST = extractor.GetUnicodeString(e.postToken);
            var EXECUTOR = extractor.GetUnicodeString(e.executorToken);
            doc.Executors.Add(new Executor(EXECUTOR, POST.Trim()));
        }
        
        //Ветвление для правительства
        if(tokensRequisiteModel.organsTokens.FirstOrDefault(c=> c.TokenType == DocumentToken.Орган_Правительство) != null)
        {   
            var signD =  tokensRequisiteModel.typeToken.Next(DocumentToken.ДлиннаяДата);
            if(!signD.IsOk)
            {
                exceptions.Add(new ParserException($"Не удалось найти дату подписания", signD.Error));
                return false;
            }
            tokensRequisiteModel.signDateToken = signD.Token;
        }
        else
        {
            var signD = tokensRequisiteModel.personToken.Last().executorToken.FindForward(DocumentToken.ДлиннаяДата, 8);
            if(!signD.IsOk)
            {
                exceptions.Add(new ParserException("Не удалось найти дату подписания", signD.Error));
                return false;
            }    
            tokensRequisiteModel.signDateToken = signD.Token;
        }
            
        var number =  tokensRequisiteModel.signDateToken.Next(DocumentToken.Номер);
        if(!number.IsOk)
        {
            exceptions.Add(new ParserException($"Не удалось определить номер документа", number.Error));
            return false;
        }
        var n = extractor.GetUnicodeString(tokensRequisiteModel.numberToken.CustomGroups[0]);
        doc.Numbers.Add(new Number(n));

        var signDate = Utils.Extensions.DateTimeExtensions.GetDate(tokensRequisiteModel.signDateToken.CustomGroups[0].Value,
                                                tokensRequisiteModel.signDateToken.CustomGroups[1].Value,
                                                tokensRequisiteModel.signDateToken.CustomGroups[2].Value,
                                                true);
        if(!signDate.HasValue)
        {
            exceptions.Add(new ParserException($"Не удалось привести дату к формату даты\\времени. {tokensRequisiteModel.signDateToken.Value}", number.Error));
            return false;
        }
        doc.SignDate = signDate.Value;
        foreach(var p in tokensRequisiteModel.personToken)
        {
            extractor.SetElementNode(p.postToken, NodeType.stop);
            extractor.SetElementNode(p.executorToken, NodeType.stop);
        }
        extractor.SetElementNode(tokensRequisiteModel.numberToken, NodeType.stop);
        extractor.SetElementNode(tokensRequisiteModel.signDateToken, NodeType.stop);
        return true;  
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
        {
            exceptions.Add(new ParserException("Дата принятия в Государственной Думе не найдена"));
            return false;
        }
        var gdDate = gd.Next(DocumentToken.ДлиннаяДата);
        if (!gdDate.IsOk)
        {
            exceptions.Add(new ParserException("Неверный формат или отсуствует дата принятия в Государственной Думе ", gdDate.Error));
            return false;
        }
        var GDDATE = Utils.Extensions.DateTimeExtensions.GetDate(gdDate.Token.CustomGroups[0].Value, gdDate.Token.CustomGroups[1].Value, gdDate.Token.CustomGroups[2].Value, true);
        if (!GDDATE.HasValue)
        {
            exceptions.Add(new ParserException($"Дата принятия в Государственной Думе не распознана: {gdDate.Token.Value} - неправильный формат даты"));
            return false;
        }
        doc.GDDate = GDDATE;
        extractor.SetElementNode(gdDate.Token, NodeType.stop);
        var sfToken = gdDate.Token.Next(DocumentToken.ОдобренСФ);
        if (!sfToken.IsOk)
        {
            exceptions.Add(new ParserException("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error , ErrorType.Warning));
            return true;
        }
        var sfDate = sfToken.Token.Next();
        if (!sfDate.IsOk)
        {
            exceptions.Add(new ParserException("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error));
            return false;
        }

        var SFDATE = Utils.Extensions.DateTimeExtensions.GetDate(sfDate.Token.CustomGroups[0].Value, sfDate.Token.CustomGroups[1].Value, sfDate.Token.CustomGroups[2].Value, true);
        if (!SFDATE.HasValue)
        {
            exceptions.Add(new ParserException($"Дата одобрения в Совете Федерации не распознана: {sfDate.Token.Value} - неправильный формат даты", ErrorType.Fatal));
            return true;
        }
        doc.SFDate = SFDATE;
        extractor.SetElementNode(sfDate.Token, NodeType.stop);
        BeforeBodyElement = extractor.GetElements(sfDate.Token).FirstOrDefault();
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
        return true;
    }
    /// <summary>
    /// Парсинг наименования
    /// </summary>
    bool NameBlock()
    {
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
        if(!mayBeName.IsOk)
        {
            exceptions.Add(new ParserException("Наименование документа не найдено ", mayBeName.Error));
            return false;
        }
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