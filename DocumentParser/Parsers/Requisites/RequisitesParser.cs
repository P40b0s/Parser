using System;
using System.Linq;
using Utils.Extensions;
using DocumentParser.DocumentElements;
using Lexer;
using DocumentParser.Workers;
using DocumentParser.Elements;
using SettingsWorker.Requisites;
using SettingsWorker;

namespace DocumentParser.Parsers.Requisites;

public class RequisitesParser : LexerBase<SettingsWorker.Requisites.RequisitesTokenType>
{
    WordProcessing extractor {get;}
    RequisiteTokensModel tokensRequisiteModel {get;} = new RequisiteTokensModel();
    DocumentElements.Document doc {get;}
    public ElementStructure BeforeBodyElement {get;set;}
    public bool HasErrors => exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
    public bool HasWarnings => exceptions.Any(a=>a.ErrorType == ErrorType.Warning);
    ISettings rules {get;}
    public RequisitesParser(WordProcessing extractor, DocumentElements.Document doc)
    {
        this.extractor = extractor;
        this.doc = doc;
        rules = extractor.Settings;
        Tokenize(extractor.FullText, new RequisitesTokensDefinition(rules.TokensDefinitions.RequisitesTokenDefinition.TokenDefinitionSettings));
    }

    Predicate<Token<RequisitesTokenType>> IsOrgan = d => 
        d != null 
        && (d.TokenType == RequisitesTokenType.Орган);

    Predicate<Token<RequisitesTokenType>> IsDocType = d => 
        d != null 
        && (d.TokenType == RequisitesTokenType.Вид);
    
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
        var typeToken = first.FindForward(f=>f.TokenType == RequisitesTokenType.Вид, rules.DefaultRules.RequisiteRule.TypeSearchMaxDeep);
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
        var before = tokensRequisiteModel.typeToken.FindBackwardMany(p=> p.TokenType == RequisitesTokenType.Орган);
        var next = tokensRequisiteModel.typeToken.FindForwardMany(p=> p.TokenType == RequisitesTokenType.Орган);
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
        //На будущее можно добавить еще какие то особенности и отсюда переопределять дефолтные правила
        foreach(var c in rules.CustomRules)
        {
            var org = c.OrganRX();
            var tp = c.TypeRX();
            if(tp.IsMatch(tokensRequisiteModel.typeToken.Value))
            {
                foreach(var o in tokensRequisiteModel.organsTokens)
                    if(org.IsMatch(o.Value))
                        rules.DefaultRules = c.Rules;
            }
        }
        return true;
    }

    
    bool SignDateNumberExecutorBlock()
    {
        var posts = tokens.FindAll(f=>f.TokenType == RequisitesTokenType.Должность);
        if(posts.Count == 0)
            return AddError("Не удалось определить должность подписанта");
        //Должностей то несколько
        foreach(var p in posts)
        {
            var executor = p.Next(RequisitesTokenType.Подписант);
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
            
        var number =  tokensRequisiteModel.signDateToken.Next(RequisitesTokenType.Номер);
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
        if(rules.DefaultRules.RequisiteRule.SearchSignDateOnHeader)
        {   
            var signD = tokensRequisiteModel.typeToken.FindForward(RequisitesTokenType.ДлиннаяДата, rules.DefaultRules.RequisiteRule.SignDateSearchMaxDeep);
            if(signD.IsOk)
            {
                header = true;
                tokensRequisiteModel.signDateToken = signD.Value;
            }
        }
        if(rules.DefaultRules.RequisiteRule.SearchSignDateOnFooter)
        {
            var signD = tokensRequisiteModel.personToken.Last().executorToken.FindForward(RequisitesTokenType.ДлиннаяДата, rules.DefaultRules.RequisiteRule.SignDateSearchMaxDeep);
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
        //Парсить данные совфеда и госдумы ненадо, возвращаем true
        if (!rules.DefaultRules.RequisiteRule.ParseGDSFAttributes)
            return true;
        //не всешда присутствует дата принятия совфедом!!!
        var gd = tokens.GetFirst(f=>f.TokenType == RequisitesTokenType.ПринятГД);
        if (gd.IsError)
            return AddError("Дата принятия в Государственной Думе не найдена");
        var gdDate = gd.Value.Next(RequisitesTokenType.ДлиннаяДата);
        if (gdDate.IsError)
            return AddError("Неверный формат или отсуствует дата принятия в Государственной Думе ", gdDate.Error);
        var GDDATE = gdDate.Value.GetDate();
        if (GDDATE.IsError)
            return AddError($"Дата принятия в Государственной Думе не распознана: {GDDATE.Error.Message}");
        doc.GDDate = GDDATE.Value.Value;
        extractor.SetElementNode(gdDate.Value, NodeType.stop);
        var sfToken = gdDate.Value.Next(RequisitesTokenType.ОдобренСФ);
        if (sfToken.IsError)
            AddError("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error , ErrorType.Warning);
        var sfDate = sfToken.Value.Next();
        if (sfDate.IsError)
            return AddError("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error);
        var SFDATE = sfDate.Value.GetDate();
        if (!SFDATE.IsError)
            return AddError($"Дата одобрения в Совете Федерации не распознана: {SFDATE.Error.Message}");
        doc.SFDate = SFDATE.Value.Value;
        extractor.SetElementNode(sfDate.Value, NodeType.stop);
        var before = extractor.GetSingleElement(sfDate.Value);
        if(before.IsOk)
            BeforeBodyElement = before.Value;
        return true;
    }

   

   
    /// <summary>
    /// Парсинг наименования
    /// </summary>
    bool NameBlock()
    {
        if(rules.DefaultRules.RequisiteRule.NameInTypeString)
        {
            var nameToken = extractor.GetElements(tokensRequisiteModel.typeToken).FirstOrDefault();
            if(nameToken == null)
                return AddError("Наименование не найдено, внимание включен флаг NameInTypeString=true - наименование находится в абзаце вида документа!");
            doc.Name = extractor.GetUnicodeString(nameToken);
            return true;
        }
        Token<RequisitesTokenType> lastToken = tokensRequisiteModel.typeToken.Position > tokensRequisiteModel.organsTokens.Last().Position 
            ? tokensRequisiteModel.typeToken 
            : tokensRequisiteModel.organsTokens.Last();

        var mayBeName = lastToken.FindForward(RequisitesTokenType.НачалоПредложения, rules.DefaultRules.RequisiteRule.NameSearchMaxDeep);

        if(mayBeName.IsError)
            return AddError("Наименование документа не найдено ", mayBeName.Error);
        var name = extractor.GetSingleElement(mayBeName.Value);
        if(name.IsError)
            return AddError(name.Error);
        var nameIsBold = extractor.Properties.IsBold(name.Value);
        if(rules.DefaultRules.RequisiteRule.RequiredName && !nameIsBold)
            return AddError("Не найдено наименование выделеное жирным шрифтом, при установленом флаге RequiredName=true это является критической ошибкой");
        if(!rules.DefaultRules.RequisiteRule.RequiredName && !nameIsBold)
        {
            var beforeName = name.Value.Before();
            if(beforeName.IsOk)
            {
                extractor.SetElementNode(beforeName.Value, NodeType.stop);
                BeforeBodyElement = beforeName.Value;
            }
            tokensRequisiteModel.NotHaveName = true;
            doc.Name = "";
            return AddError("Не найдено наименование выделеное жирным шрифтом, возможно оно отсутсвует", ErrorType.Warning);
        }
        extractor.SetElementNode(name.Value, NodeType.stop);
        BeforeBodyElement = name.Value;
        doc.Name = extractor.GetUnicodeString(name.Value);
        tokensRequisiteModel.nameToken = mayBeName.Value;
        return true;
    }
}