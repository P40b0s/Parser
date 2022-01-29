using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.Requisites;
public class RequisitesTokenDefinition : TokenDefinitionBase<RequisitesTokenType>
{
    public RequisitesTokenDefinition()
    {
        addToken(RequisitesTokenType.Орган, $"российская{Templates.WsBr}+федерация", 1);
        addToken(RequisitesTokenType.Орган, $"российской{Templates.WsBr}+федерации", 2);
        addToken(RequisitesTokenType.Орган, $"президента{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        //addToken(RequisitesTokenType.Орган, $"правительство{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        //addToken(RequisitesTokenType.Орган, $"министерство.+федерации$", 1);
        //addToken(RequisitesTokenType.Орган, $"правительство.+(области|края|республики)$", 1);
        //addToken(RequisitesTokenType.Орган, $"правительство{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        addToken(RequisitesTokenType.Орган, $"министерство{Templates.WsBrWord}+", 1);
        addToken(RequisitesTokenType.Орган, $"правительство{Templates.WsBrWord}+", 1);
        addToken(RequisitesTokenType.Орган, $"федеральн[аяое]{{2}}{Templates.WsBrWord}+", 1);

        addToken(RequisitesTokenType.Вид, $"федеральный{Templates.WsBr}+закон", 1);
        addToken(RequisitesTokenType.Вид, $"закон{Templates.WsBr}+российской{Templates.WsBr}+федерации{Templates.WsBr}+о{Templates.WsBr}+поправке{Templates.WsBr}+к{Templates.WsBr}+конституции{Templates.WsBr}+российской{Templates.WsBr}+федерации", 1);
        addToken(RequisitesTokenType.Вид, $"федеральный{Templates.WsBr}+конституционный{Templates.WsBr}+закон", 1);
        addToken(RequisitesTokenType.Вид, "закон", 2);
        addToken(RequisitesTokenType.Вид, "кодекс|указ|распоряжение|постановление|приказ|соглашение", 1);
       
        
        addToken(RequisitesTokenType.ОдобренСФ, $"одобрен{Templates.WsBr}+советом{Templates.WsBr}+федерации", 1);
        addToken(RequisitesTokenType.ПринятГД, $"принят{Templates.WsBr}+государственной{Templates.WsBr}+думой", 1);
        
        addToken(RequisitesTokenType.Должность, $"президент{Templates.WsBr}+Российской{Templates.WsBr}+Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"председатель{Templates.WsBr}+Правительства{Templates.WsBr}+Российской{Templates.WsBr}+Федерации", 1);
        addToken(RequisitesTokenType.Должность, $"исполняющий{Templates.WsBr}+(полномочия|обязанности){Templates.WsBrWord}+", 1);
        addToken(RequisitesTokenType.Должность, $"министр{Templates.WsBrWord}+", 1);
        addToken(RequisitesTokenType.Должность, $"губернатор{Templates.WsBrWord}+", 1);
        addToken(RequisitesTokenType.Должность, $"губернатор{Templates.WsBrWord}+", 1);
        addToken(RequisitesTokenType.Должность, $"руководитель{Templates.WsBr}+федеральн[ойого]+{Templates.WsBrWord}+", 1);
       
        
        
        addToken(RequisitesTokenType.ДлиннаяДата, $"(?:от\\s*)?(?<date>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)?", 1);
        addToken(RequisitesTokenType.КороткаяДата, $"(?<date>\\d{{1,2}})[.](?<month>\\d{{1,2}})[.](?<year>\\d{{4}})", 1);
        addToken(RequisitesTokenType.ПередДатойПодписания, $"совершено{Templates.WsBr}+в{Templates.WsBr}+городе{Templates.WsBr}+\\S+", 1);
        addToken(RequisitesTokenType.Подписант, $"[А-Я][.][А-Яа-я]+", 2);
        addToken(RequisitesTokenType.Подписант, "[А-Я][.][А-Я][.][А-Яа-я]+", 1);
        addToken(RequisitesTokenType.Номер, $"(?:N|№)\\s*(?<number>[^\\s]+)", 1);
        addToken(RequisitesTokenType.Часть, "часть\\s+(первая|вторая|третья|четвертая|пятая|шестая|седьмая)", 1);
        //addToken(RequisitesTokenType.Слово, "\\S+", 10);
        //addToken(RequisitesTokenType.НачалоПредложения, "\\S+\\s+\\S+", 3);
        //addToken(RequisitesTokenType.ТекстВСкобках, $"[(][^)]+[)]", 2);
        //addToken(RequisitesTokenType.Игнор, $"москва", 1);
    }
}

 
            