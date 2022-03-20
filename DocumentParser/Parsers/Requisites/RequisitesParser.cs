using System;
using System.Linq;
using Utils.Extensions;
using DocumentParser.DocumentElements;
using Lexer;
using DocumentParser.Workers;
using DocumentParser.Elements;
using SettingsWorker.Requisite;
using SettingsWorker;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using Utils;

namespace DocumentParser.Parsers.Requisites;

public class RequisitesParser : LexerBase<SettingsWorker.Requisite.RequisiteTokenType>
{
    WordProcessing extractor {get;}
    RequisiteTokensModel tokensRequisiteModel {get;} = new RequisiteTokensModel();
    DocumentElements.Document doc {get;}
    /// <summary>
    /// Последний идентифицированный элемент в шапке документа
    /// </summary>
    /// <Value()></Value()>
    public ElementStructure BeforeBodyElement {get;set;}
    //FIXME была одна ошибка где он был NULL! надо все проверить, доки из перечня проверки реквизитов
    public RequisitesParser(WordProcessing extractor, DocumentElements.Document doc)
    {
        this.extractor = extractor;
        this.doc = doc;
        settings = extractor.Settings;
        Tokenize(extractor.FullText, new RequisitesTokensDefinition(settings.TokensDefinitions.RequisiteTokenDefinitions.TokenDefinitionSettings));
    }

    Predicate<Token<RequisiteTokenType>> IsOrgan = d => 
        d != null 
        && (d.TokenType == RequisiteTokenType.Орган);

    Predicate<Token<RequisiteTokenType>> IsDocType = d => 
        d != null 
        && (d.TokenType == RequisiteTokenType.Вид);
    
    public bool Parse()
    {
        UpdateStatus("Определение реквизитов документа...");
        if(!TypeBlock())
            return HasFatalError;
        if(!OrganBlock())
            return HasFatalError;
        if(!NameBlock())
            return HasFatalError;
        if(!GDSFBlock())
            return HasFatalError;
        if(!SignDateNumberExecutorBlock())
            return HasFatalError;
        return HasFatalError;
    }

    bool TypeBlock()
    {
        if(tokens.Count == 0)
            return AddError("Не найдено ни одного токена, поиск реквизитов невозможен");
        var first = tokens[0];
        var typeToken = first.FindForward(f=>f.TokenType == RequisiteTokenType.Вид, settings.DefaultRules.RequisiteRule.TypeSearchMaxDeep, true);
        if(typeToken.IsError)
            return AddError("Не удалось определить вид документа: ", typeToken.Error());
        var stringType = extractor.GetUnicodeString(typeToken.Value());
        if(stringType == null)
            return AddError($"Параграф вида документа \"{typeToken.Value().Value}\" не найден");
        stringType = stringType.NormalizeWhiteSpaces().NormalizeCase().Trim();
        //TODO ТЕСТ! еще не тестил
        var changed = settings.RequisiteChangers.Change(ChangeType.TypeToType, stringType);
        doc.Type = string.IsNullOrEmpty(changed) ? stringType : changed;
        tokensRequisiteModel.typeToken = typeToken.Value();
        return true;
    }
    bool OrganBlock ()
    {
        // if(settings.DefaultRules.RequisiteRule.CustomOrganName != "")
        // {
        //     doc.Organs.Add(new Organ(settings.DefaultRules.RequisiteRule.CustomOrganName));
        //     return true;
        // }
        var before = tokensRequisiteModel.typeToken.FindBackwardMany(p=> p.TokenType == RequisiteTokenType.Орган);
        foreach(var o in before)
            tokensRequisiteModel.organsTokens.Insert(0, o);
        if(before.Count == 0)
        {
            var next = tokensRequisiteModel.typeToken.FindForwardMany(p=> p.TokenType == RequisiteTokenType.Орган);
            foreach(var o in next)
                tokensRequisiteModel.organsTokens.Add(o);
        }
        if(tokensRequisiteModel.organsTokens.Count == 0)
            return AddError("Не найдено ни однго принявшего органа!");
        foreach(var o in tokensRequisiteModel.organsTokens)
        {
            var organ = new Organ(extractor.GetUnicodeString(o).NormalizeWhiteSpaces().NormalizeCase());
            var changedOrgan = settings.RequisiteChangers.Change(ChangeType.TypeToOrgan, organ.val);
            if(changedOrgan != "")
                organ.val = changedOrgan;
            changedOrgan = settings.RequisiteChangers.Change(ChangeType.OrganToOrgan, organ.val);
            if(changedOrgan != "")
                organ.val = changedOrgan;
            doc.Organs.Add(organ); 
        }
        loadCustomRules();
        return true;
    }

    /// <summary>
    /// Проверяем кастомные правила для данного органа\типа документа, если они находятся то заменяем дефолтные на эти
    /// На будущее можно добавить еще какие то особенности и отсюда переопределять дефолтные правила
    /// </summary>
    private void loadCustomRules()
    {
        var findedRules = new List<CustomRule<AllRules>>();
        for(int c = 0; c < settings.CustomRules.Count; c++)
        {
            //Если регекс органа - любой, то не ищем по органам вообще а ищем только по видам
            if(settings.CustomRules[c].Organ == ".+")
            {
                if(settings.CustomRules[c].TypeRX().IsMatch(tokensRequisiteModel.typeToken.Value))
                {
                    findedRules.Add(settings.CustomRules[c]);
                }   
            }
            else
            {
                for(int o = 0; o < tokensRequisiteModel.organsTokens.Count; o++)
                {
                    if(settings.CustomRules[c].OrganRX().IsMatch(tokensRequisiteModel.organsTokens[o].Value))
                    {
                        if(settings.CustomRules[c].TypeRX().IsMatch(tokensRequisiteModel.typeToken.Value))
                        {
                            findedRules.Add(settings.CustomRules[c]);
                        }   
                    }     
                }
            }
           
        }
        //Сортируем по весу и выбираем с наименьшим
        var def = findedRules.OrderBy(o=>o.Weight).Count() > 0;
        if(def)
        {
            settings.DefaultRules = findedRules[0].Rules;
            UpdateStatus($"Установлены правила разбора для {findedRules[0].Organ} {findedRules[0].Type}");
        }
    }

    
    bool SignDateNumberExecutorBlock()
    {
        if(!settings.DefaultRules.RequisiteRule.NoExecutor)
        {
            //var firstPost = tokens.GetFirst(f=>f.TokenType == RequisiteTokenType.Должность).Select(s=>s.Next(RequisiteTokenType.Подписант));
            var posts = tokens.GetAll(f=>f.TokenType == RequisiteTokenType.Должность && f.Next(RequisiteTokenType.Подписант).IsOk);
            //var findCount = 0;
            //while(firstPost.IsError && findCount < 5)
            //{

            //}
            if(posts.Count() == 0)
                return AddError("С текущими настройками парсера не удалось определить подписанта");
            //var posts = firstPost.Value().FindForwardMany(f=>f.TokenType == RequisiteTokenType.Должность, ig=>ig.TokenType == RequisiteTokenType.Подписант, true);
            //Должностей то  несколько
            foreach(var p in posts)
            {
                var executor = p.Next(RequisiteTokenType.Подписант);
                if(executor.IsError)
                {
                    AddError($"Не удалось определить ФИО подписанта для должности {p.Value}", executor.Error());
                }
                else
                    tokensRequisiteModel.personToken.Add(new ExecutorRequisiteToken(){postToken = p, executorToken = executor.Value()});
            }
            if(tokensRequisiteModel.personToken.Count == 0)
                return false;
            foreach(var e in tokensRequisiteModel.personToken)
            {
                var POST = extractor.GetUnicodeString(e.postToken);
                var EXECUTOR = extractor.GetUnicodeString(e.executorToken);
                doc.Executors.Add(new Executor(EXECUTOR, POST.Trim()));
                extractor.SetElementNode(e.postToken, NodeType.stop);
                extractor.SetElementNode(e.executorToken, NodeType.stop);
            }
        }
        //Дату не находим, заканчиваем метод
        if(!getSignDate())
            return false;
        if(!settings.DefaultRules.RequisiteRule.NoNumber)
        {
            var number =  tokensRequisiteModel.signDateToken.Next(RequisiteTokenType.Номер);
            if(number.IsError)
                return AddError($"Не удалось определить номер документа ", number.Error());
            tokensRequisiteModel.numberToken = number.Value();
            var n = extractor.GetUnicodeString(tokensRequisiteModel.numberToken.CustomGroups[0]);
            doc.Numbers = coNumberExtractor(n).ToList();
            extractor.SetElementNode(tokensRequisiteModel.numberToken, NodeType.stop);
            /// Если не вcтретился слеш и несколько органов, то один орган - материнский
            /// типа министретсва и под ним его подведомственный - какая-нибудь федеральная служба
            /// нет слеша = оставляем только последний орган
            if(doc.Numbers.Count == 1 && doc.Organs.Count > 1)
            for(int o = doc.Organs.Count - 2; o >= 0; o--)
                doc.Organs.RemoveAt(o);
                //FIXME Если орган 1 и есть слеш то берем номер прямо так
                //иначе проверяем номер через функцию coNumberExtractor
        }
        return true;  
    }

    

    /// <summary>
    /// Извлечение номеров с возможность разбития их по правым слешам на несколько
    /// Используются исключения добавленые в настройках программы
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    IEnumerable<Number> coNumberExtractor(string number)
    {
        var indexes = new Hashtable();
        var list = settings.DefaultRules.RequisiteRule.RightSlashExceptions.Select(s=>new Regex(s, RegexOptions.IgnoreCase));
        var matches = list.Select(s=>s.Match(number));
        foreach(var m in matches)
        if(m.Success)
        {
            indexes.Add(m.Index, m.Length);
        }
        string n = "";
        for (int c = 0; c < number.Length; c++)
        {
            //Если встречаем слеш, значит несколько номеров
            if(number[c] == '/')
            {
                yield return new Number(n);
                n = "";
                c++;
            }
            //Если находим исключение номера то обрабатываем его целиком и переходим дальше
            if(indexes.ContainsKey(c))
            {
                var length = (int)indexes[c];
                for (int e = 0; e < length; e++)
                {
                    n += number[c + e];
                }
                yield return new Number(n);
                n = "";
                c = c+ length;
                continue;
            }
            n += number[c];
        }
        //Если не встетидлось исключений и нет нескольких номеров то просто возвращаем номер
        if(n.Length > 0)
            yield return new Number(n);
    }

    private bool getSignDate()
    {
        bool ok = false;
        if(settings.DefaultRules.RequisiteRule.SignDateAfterCustomToken)
        {
            var custom = tokens.GetFirst(f=>f.TokenType == RequisiteTokenType.ПередДатойПодписания);
            if(custom.IsOk)
            {
                var signD = custom.Value().Next(RequisiteTokenType.ДлиннаяДата);
                if(signD.IsOk)
                {
                    tokensRequisiteModel.signDateToken = signD.Value();
                    ok = true;
                }
            }
        }
        else
        {
            if(settings.DefaultRules.RequisiteRule.SignDateAfterType)
            {   
                var signD = tokensRequisiteModel.typeToken.FindForward(RequisiteTokenType.ДлиннаяДата, settings.DefaultRules.RequisiteRule.SignDateSearchMaxDeep);
                if(signD.IsOk)
                {
                    ok = true;
                    tokensRequisiteModel.signDateToken = signD.Value();
                    extractor.SetElementNode(signD.Value(), NodeType.stop);
                }
            }
            else
            {
                var signD = tokensRequisiteModel.personToken.Last().executorToken.FindForward(RequisiteTokenType.ДлиннаяДата, settings.DefaultRules.RequisiteRule.SignDateSearchMaxDeep);
                if(signD.IsOk)
                {
                    ok =  true;
                    tokensRequisiteModel.signDateToken = signD.Value();
                    extractor.SetElementNode(signD.Value(), NodeType.stop);
                }
            }
        }
        if(!ok)
            return AddError("Не удалось найти дату подписания");
        var signDate = tokensRequisiteModel.signDateToken.GetDate();
        if(signDate.IsError)
            return AddError(signDate.Error().Message, signDate.Error());
        doc.SignDate = signDate.Value().Value;
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
        //Парсить данные совфеда и госдумы ненадо, возвращаем true
        if (!settings.DefaultRules.RequisiteRule.ParseGDSFAttributes)
            return true;
        //не всешда присутствует дата принятия совфедом!!!
        var gd = tokens.GetFirst(f=>f.TokenType == RequisiteTokenType.ПринятГД);
        if (gd.IsError)
            return AddError("Дата принятия в Государственной Думе не найдена");
        var gdDate = gd.Value().Next(RequisiteTokenType.ДлиннаяДата);
        if (gdDate.IsError)
            return AddError("Неверный формат или отсуствует дата принятия в Государственной Думе ", gdDate.Error());
        var GDDATE = gdDate.Value().GetDate();
        if (GDDATE.IsError)
            return AddError($"Дата принятия в Государственной Думе не распознана: {GDDATE.Error().Message}");
        doc.GDDate = GDDATE.Value().Value;
        extractor.SetElementNode(gdDate.Value(), NodeType.stop);
        var sfToken = gdDate.Value().Next(RequisiteTokenType.ОдобренСФ);
        if (sfToken.IsError)
            return AddError("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error() , Utils.ErrorType.Warning);
        var sfDate = sfToken.Value().Next();
        if (sfDate.IsError)
            return AddError("После даты принятия в Государственной Думе должна идти дата одобрения в Совете Федерации, но она не найдена ", sfToken.Error());
        var SFDATE = sfDate.Value().GetDate();
        if (SFDATE.IsError)
            return AddError($"Дата одобрения в Совете Федерации не распознана: {SFDATE.Error().Message}");
        doc.SFDate = SFDATE.Value().Value;
        extractor.SetElementNode(sfDate.Value(), NodeType.stop);
        var before = extractor.GetElement(sfDate.Value());
        if(before.IsOk)
            BeforeBodyElement = before.Value();
        return true;
    }

   

   
    /// <summary>
    /// Парсинг наименования
    /// </summary>
    bool NameBlock()
    {
        if(settings.DefaultRules.RequisiteRule.NameInTypeString)
        {
            var nameToken = extractor.GetElement(tokensRequisiteModel.typeToken);
            if(nameToken.IsError)
                return AddError("Наименование не найдено, внимание включен флаг NameInTypeString=true - наименование находится в абзаце вида документа!");
            doc.Name = extractor.GetUnicodeString(nameToken.Value());
            var part = tokensRequisiteModel.typeToken.FindForward(f=>f.TokenType == RequisiteTokenType.Часть, 1);
            if(part.IsOk)
            {
                var p = extractor.GetElement(part.Value());
                if(p.IsOk)
                {
                    doc.Part = extractor.GetUnicodeString(p.Value());
                    extractor.SetElementNode(p.Value(), NodeType.stop);
                } 
            }
            extractor.SetElementNode(tokensRequisiteModel.typeToken, NodeType.stop);
            return true;
        }
        Token<RequisiteTokenType> lastToken = tokensRequisiteModel.typeToken.Position > tokensRequisiteModel.organsTokens.Last().Position 
            ? tokensRequisiteModel.typeToken 
            : tokensRequisiteModel.organsTokens.Last();

        // if(settings.DefaultRules.RequisiteRule.SignDateAfterType)
        // {
        //     var dateField = lastToken.Next();
        //     if(dateField.IsOk)
        //         lastToken = dateField.Value();
        // }

        var beforeNameElement = extractor.GetElement(lastToken);
        var mayBeName = beforeNameElement.Value().Next(settings.DefaultRules.RequisiteRule.NamePositionAfterTypeCorrection);
        if(mayBeName.IsError)
            return AddError("Наименование документа не найдено");
        var name = mayBeName.Value();
        var nameIsBold = extractor.Properties.IsBold(name);
        if(settings.DefaultRules.RequisiteRule.RequiredName && !nameIsBold)
            return AddError("Не найдено наименование выделеное жирным шрифтом, при установленом флаге RequiredName=true это является критической ошибкой");
        if(!settings.DefaultRules.RequisiteRule.RequiredName && !nameIsBold)
        {
            extractor.SetElementNode(beforeNameElement.Value(), NodeType.stop);
            BeforeBodyElement = beforeNameElement.Value();
            tokensRequisiteModel.NotHaveName = true;
            doc.Name = "";
            return AddError("Не найдено наименование выделеное жирным шрифтом, возможно оно отсутсвует", Utils.ErrorType.Warning);
        }
        extractor.SetElementNode(name, NodeType.stop);
        BeforeBodyElement = name;
        doc.Name = extractor.GetUnicodeString(name);
        tokensRequisiteModel.nameElement = mayBeName.Value();
        return true;
    }
}