using Settings.Regexes;
namespace Settings.Requisites;
public class RequisitesParserRules : RulesBase<RequisiteRule, RequisitesTokenType>
{
    public RequisitesParserRules()
    {
        addToken(RequisitesTokenType.Орган, $"российская{Templates.WsOrBr}федерация", 1);
        addToken(RequisitesTokenType.Орган, $"российской{Templates.WsOrBr}федерации", 2);
        addToken(RequisitesTokenType.Орган, $"президента{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", 1);
        addToken(RequisitesTokenType.Орган, $"правительство{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", 1);
        addToken(RequisitesTokenType.Вид, $"федеральный{Templates.WsOrBr}закон", 1);
        addToken(RequisitesTokenType.Вид, $"ЗАКОН{Templates.WsOrBr}РОССИЙСКОЙ{Templates.WsOrBr}ФЕДЕРАЦИИ{Templates.WsOrBr}О{Templates.WsOrBr}ПОПРАВКЕ{Templates.WsOrBr}К{Templates.WsOrBr}КОНСТИТУЦИИ{Templates.WsOrBr}РОССИЙСКОЙ{Templates.WsOrBr}ФЕДЕРАЦИИ", 1);
        addToken(RequisitesTokenType.Вид, $"федеральный{Templates.WsOrBr}конституционный{Templates.WsOrBr}закон", 1);
        addToken(RequisitesTokenType.Вид, "закон", 2);
        addToken(RequisitesTokenType.Вид, "кодекс", 1);
        addToken(RequisitesTokenType.Вид, "указ", 1);
        addToken(RequisitesTokenType.Вид, "распоряжение", 1);
        addToken(RequisitesTokenType.Вид, "постановление", 1);
        addToken(RequisitesTokenType.Вид, "приказ", 1);
        
        addToken(RequisitesTokenType.ОдобренСФ, $"одобрен{Templates.WsOrBr}советом{Templates.WsOrBr}федерации", 1);
        addToken(RequisitesTokenType.ПринятГД, $"принят{Templates.WsOrBr}государственной{Templates.WsOrBr}думой", 1);
        
        addToken(RequisitesTokenType.Должность, $"Президент{Templates.WsOrBr}Российской{Templates.WsOrBr}Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"Председатель{Templates.WsOrBr}Правительства{Templates.WsOrBr}Российской{Templates.WsOrBr}Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"Исполняющий{Templates.WsOrBr}полномочия{Templates.WsOrBr}Президента{Templates.WsOrBr}Российской{Templates.WsOrBr}Федерации", 1);
        
        addToken(RequisitesTokenType.ДлиннаяДата, $"(?:от\\s*)?(?<date>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)?", 1);
        addToken(RequisitesTokenType.КороткаяДата, $"(?<date>\\d{{1,2}})[.](?<month>\\d{{1,2}})[.](?<year>\\d{{4}})", 1);
        addToken(RequisitesTokenType.Подписант, $"[А-Я]{{1}}[.][А-Яа-я]+", 1);
        addToken(RequisitesTokenType.Подписант, $"[А-Я]{{1}}[.][А-Я]{{1}}[.][А-Яа-я]+", 1);
        addToken(RequisitesTokenType.Номер, $"(?:N|№)\\s*(?<number>[^\\s]+)", 1);
        addToken(RequisitesTokenType.Часть, "часть\\s+(первая|вторая|третья|четвертая|пятая|шестая|седьмая)", 1);
        addToken(RequisitesTokenType.Слово, "\\S+", 10);
        addToken(RequisitesTokenType.НачалоПредложения, "\\S+\\s+\\S+", 3);
        addToken(RequisitesTokenType.ТекстВСкобках, $"[(][^)]+[)]", 2);
        addToken(RequisitesTokenType.Игнор, $"москва", 1);

    }
    public override List<CustomRule<RequisiteRule>> CustomRequisiteRules {get;set;} = new List<CustomRule<RequisiteRule>>()
    {
        new CustomRule<RequisiteRule>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = "кодекс", Rules = new RequisiteRule() 
        {
            NameInTypeString = true,
            ParseGDSFAttributes = true,
        }},
        new CustomRule<RequisiteRule>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = $"федеральный{Templates.WsOrBr}закон", Rules = new RequisiteRule() 
        {
            ParseGDSFAttributes = true,
        }},
        new CustomRule<RequisiteRule>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = $"закон{Templates.WsOrBr}российской{Templates.WsOrBr}федерации{Templates.WsOrBr}о{Templates.WsOrBr}поправке{Templates.WsOrBr}к{Templates.WsOrBr}конституции{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", Rules = new RequisiteRule() 
        {
            ParseGDSFAttributes = true,
        }},
        new CustomRule<RequisiteRule>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = "закон", Rules = new RequisiteRule() 
        {
            ParseGDSFAttributes = true,
        }},
        new CustomRule<RequisiteRule>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = $"федеральный{Templates.WsOrBr}конституционный{Templates.WsOrBr}закон", Rules = new RequisiteRule() 
        {
            ParseGDSFAttributes = true,
        }},
        new CustomRule<RequisiteRule>(){Organ = $"правительство{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", Type = "распоряжение", Rules = new RequisiteRule() 
        {
            SearchSignDateOnFooter = false,
            SearchSignDateOnHeader = true,
            OverrideHeaderSignDateByFooterSignDate = false,
            RequiredName = false,
        }},
    }; 
}

 
            