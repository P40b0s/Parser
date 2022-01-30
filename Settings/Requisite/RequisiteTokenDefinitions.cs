using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.Requisite;
public class RequisiteTokenDefinitions : TokenDefinitionBase<RequisiteTokenType>
{
    public RequisiteTokenDefinitions()
    {
        addToken(RequisiteTokenType.Орган, $"российская{Templates.WsBr}+федерация", 1);
        addToken(RequisiteTokenType.Орган, $"российской{Templates.WsBr}+федерации", 2);
        addToken(RequisiteTokenType.Орган, $"президента{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        //addToken(RequisiteTokenType.Орган, $"правительство{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        //addToken(RequisiteTokenType.Орган, $"министерство.+федерации$", 1);
        //addToken(RequisiteTokenType.Орган, $"правительство.+(области|края|республики)$", 1);
        //addToken(RequisiteTokenType.Орган, $"правительство{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        addToken(RequisiteTokenType.Орган, $"министерство{Templates.WsBrWord}+", 1);
        addToken(RequisiteTokenType.Орган, $"правительство{Templates.WsBrWord}+", 1);
        addToken(RequisiteTokenType.Орган, $"федеральн[аяое]{{2}}{Templates.WsBrWord}+", 1);

        addToken(RequisiteTokenType.Вид, $"федеральный{Templates.WsBr}+закон", 1);
        addToken(RequisiteTokenType.Вид, $"закон{Templates.WsBr}+российской{Templates.WsBr}+федерации{Templates.WsBr}+о{Templates.WsBr}+поправке{Templates.WsBr}+к{Templates.WsBr}+конституции{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        addToken(RequisiteTokenType.Вид, $"федеральный{Templates.WsBr}+конституционный{Templates.WsBr}+закон", 1);
        addToken(RequisiteTokenType.Вид, "закон", 2);
        addToken(RequisiteTokenType.Вид, "кодекс|указ|распоряжение|постановление|приказ|соглашение", 1);
       
        
        addToken(RequisiteTokenType.ОдобренСФ, $"одобрен{Templates.WsBr}+советом{Templates.WsBr}+федерации", 1);
        addToken(RequisiteTokenType.ПринятГД, $"принят{Templates.WsBr}+государственной{Templates.WsBr}+думой", 1);
        
        addToken(RequisiteTokenType.Должность, $"президент{Templates.WsBr}+Российской{Templates.WsBr}+Федерации", 1);
        addToken(RequisiteTokenType.Должность, $"председатель{Templates.WsBr}+Правительства{Templates.WsBr}+Российской{Templates.WsBr}+Федерации", 1);
        addToken(RequisiteTokenType.Должность, $"исполняющий{Templates.WsBr}+(полномочия|обязанности){Templates.WsBrWord}+", 1);
        addToken(RequisiteTokenType.Должность, $"министр{Templates.WsBrWord}+", 1);
        addToken(RequisiteTokenType.Должность, $"губернатор{Templates.WsBrWord}+", 1);
        addToken(RequisiteTokenType.Должность, $"губернатор{Templates.WsBrWord}+", 1);
        addToken(RequisiteTokenType.Должность, $"руководитель{Templates.WsBr}+федеральн[ойого]+{Templates.WsBrWord}+", 1);
       
        
        
        addToken(RequisiteTokenType.ДлиннаяДата, $"(?:от\\s*)?(?<date>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)?", 1);
        addToken(RequisiteTokenType.КороткаяДата, $"(?<date>\\d{{1,2}})[.](?<month>\\d{{1,2}})[.](?<year>\\d{{4}})", 1);
        addToken(RequisiteTokenType.ПередДатойПодписания, $"совершено{Templates.WsBr}+в{Templates.WsBr}+городе{Templates.WsBr}+\\S+", 1);
        addToken(RequisiteTokenType.Подписант, $"[А-Я][.][А-Яа-я]+", 2);
        addToken(RequisiteTokenType.Подписант, "[А-Я][.][А-Я][.][А-Яа-я]+", 1);
        addToken(RequisiteTokenType.Номер, $"(?:N|№)\\s*(?<number>[^\\s]+)", 1);
        addToken(RequisiteTokenType.Часть, "часть\\s+(первая|вторая|третья|четвертая|пятая|шестая|седьмая)", 1);
        //addToken(RequisiteTokenType.Слово, "\\S+", 10);
        //addToken(RequisiteTokenType.НачалоПредложения, "\\S+\\s+\\S+", 3);
        //addToken(RequisiteTokenType.ТекстВСкобках, $"[(][^)]+[)]", 2);
        //addToken(RequisiteTokenType.Игнор, $"москва", 1);
    }
}

 
            