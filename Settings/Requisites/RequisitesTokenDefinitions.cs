using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.Requisites;
public class RequisitesTokenDefinition : TokenDefinitionBase<RequisitesTokenType>
{
    public RequisitesTokenDefinition()
    {
        addToken(RequisitesTokenType.Орган, $"российская{Templates.WsOrBr}федерация", 1);
        addToken(RequisitesTokenType.Орган, $"российской{Templates.WsOrBr}федерации", 2);
        addToken(RequisitesTokenType.Орган, $"президента{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", 1);
        //addToken(RequisitesTokenType.Орган, $"правительство{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", 1);
        //addToken(RequisitesTokenType.Орган, $"министерство.+федерации$", 1);
        //addToken(RequisitesTokenType.Орган, $"правительство.+(области|края|республики)$", 1);
        //addToken(RequisitesTokenType.Орган, $"правительство{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", 1);
        addToken(RequisitesTokenType.Орган, $"министерство(.|\u00ad)+", 1);
        addToken(RequisitesTokenType.Орган, $"правительство(.|\u00ad)+", 1);

        addToken(RequisitesTokenType.Вид, $"федеральный{Templates.WsOrBr}закон", 1);
        addToken(RequisitesTokenType.Вид, $"закон{Templates.WsOrBr}российской{Templates.WsOrBr}федерации{Templates.WsOrBr}о{Templates.WsOrBr}поправке{Templates.WsOrBr}к{Templates.WsOrBr}конституции{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", 1);
        addToken(RequisitesTokenType.Вид, $"федеральный{Templates.WsOrBr}конституционный{Templates.WsOrBr}закон", 1);
        addToken(RequisitesTokenType.Вид, "закон", 2);
        addToken(RequisitesTokenType.Вид, "кодекс|указ|распоряжение|постановление|приказ", 1);
        
        addToken(RequisitesTokenType.ОдобренСФ, $"одобрен{Templates.WsOrBr}советом{Templates.WsOrBr}федерации", 1);
        addToken(RequisitesTokenType.ПринятГД, $"принят{Templates.WsOrBr}государственной{Templates.WsOrBr}думой", 1);
        
        addToken(RequisitesTokenType.Должность, $"Президент{Templates.WsOrBr}Российской{Templates.WsOrBr}Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"Председатель{Templates.WsOrBr}Правительства{Templates.WsOrBr}Российской{Templates.WsOrBr}Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"Исполняющий{Templates.WsOrBr}полномочия{Templates.WsOrBr}Президента{Templates.WsOrBr}Российской{Templates.WsOrBr}Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"министр{Templates.WsOrBrPlus}(\\S+{Templates.WsOrBrPlus})+\\S+", 1);
        addToken(RequisitesTokenType.Должность, $"губернатор{Templates.WsOrBrPlus}(\\S+{Templates.WsOrBrPlus})+\\S+", 1);
        
        
        addToken(RequisitesTokenType.ДлиннаяДата, $"(?:от\\s*)?(?<date>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)?", 1);
        addToken(RequisitesTokenType.КороткаяДата, $"(?<date>\\d{{1,2}})[.](?<month>\\d{{1,2}})[.](?<year>\\d{{4}})", 1);
        addToken(RequisitesTokenType.Подписант, $"[А-Я]{{1}}[.][А-Яа-я]+", 1);
        addToken(RequisitesTokenType.Подписант, $"[А-Я]{{1}}[.][А-Я]{{1}}[.][А-Яа-я]+", 1);
        addToken(RequisitesTokenType.Номер, $"(?:N|№)\\s*(?<number>[^\\s]+)", 1);
        addToken(RequisitesTokenType.Часть, "часть\\s+(первая|вторая|третья|четвертая|пятая|шестая|седьмая)", 1);
        //addToken(RequisitesTokenType.Слово, "\\S+", 10);
        //addToken(RequisitesTokenType.НачалоПредложения, "\\S+\\s+\\S+", 3);
        //addToken(RequisitesTokenType.ТекстВСкобках, $"[(][^)]+[)]", 2);
        //addToken(RequisitesTokenType.Игнор, $"москва", 1);
    }
}

 
            