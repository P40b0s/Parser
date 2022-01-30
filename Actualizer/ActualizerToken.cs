
using System.Collections.Generic;

namespace Actualizer;


    public class ActualizerTokenDefinition : ListTokensDefinition<ActualizerToken>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
    private string oneToNine = "перв[ыйгомую]+|втор[ымгойую]+|трет[ьегомийю]+|четверт[ымйогую]+|пят[ыогмйую]+|шест[гымойую]+|седьм[гымойую]+|восьм[гымойую]+|девят[ыогмйую]+";
    private string tenToNineten = "десят[ымйогую]+|одиннадцат[огымйую]+|двенадцат[огымйую]+|тринадцат[огымйую]+|четырнадцат[огымйую]+|пятнадцат[огымйую]+|шестнадцат[огымйую]+|семнадцат[огымйую]+|восемнадцат[огымйую]+|девятнадцат[огымйую]+";
    private string tens = "двадца[тйогоымую]+|тридца[тйогоымую]+|сороков[ойгымую]+|пятидеся[тйогоымую]+";
    private string allTens() => "(двадцать|тридцать|сорок|пятьдесят)\\s+" +"(" + oneToNine + ")";
    private Dictionary<string, string> numbersDict {get;} = new NumbersDictionary();
    
    public ActualizerTokenDefinition()
    {
        AddToken(ActualizerToken.NewEdition, "(изложить|изложив\\s*(его)?)\\s*в\\s*следующей\\s*редакции\\s*:\\s*\n", 1);
        AddToken(ActualizerToken.In, "Внести", 1);
        AddToken(ActualizerToken.NextChanges, "следующие\\s*изменения:", 1);
        AddToken(ActualizerToken.Definition, ":", 2);
        AddToken(ActualizerToken.ChangedActRequisites, $"(?<type>федеральн[ыйогм]+\\s*закон[аом]*)\\s*от\\s*(?<date>\\d+)\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*года\\s*(?:N|№)\\s*(?<number>[^\\s]+)\\s*\"(?<name>([^\"])+)\"", 1);
        AddToken(ActualizerToken.ChangedActRequisites, $"(?<type>указ[ом]*\\s*президента\\s*российской\\s*федерации)\\s*от\\s*(?<date>\\d+)\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(года|г[.])\\s*(?:N|№)\\s*(?<number>[^\\s]+)\\s*\"(?<name>([^\"])+)\"", 1);
        AddToken(ActualizerToken.AnnexRequisites, "(положение|состав|стратегия|уведомление|реестр|требования|правила|список|перечень|порядок|форма|план\\s*-\\s*график|типовой\\s*договор|типовая\\s*форма|расчет|условия\\s*и\\s*порядок|изменения)", 1);
        AddToken(ActualizerToken.AnnexRequisitesStop, $",\\s*утвержденн[оеую]", 1);
        AddToken(ActualizerToken.Header, "стать[еию]", 2);
        AddToken(ActualizerToken.Item0, "част[иь]", 2);
        AddToken(ActualizerToken.Item1, "пункт[е]*", 2);
        AddToken(ActualizerToken.Item2, "подпункт[е]*", 2);
        AddToken(ActualizerToken.Indent, "абзац[е]*", 2);
        AddToken(ActualizerToken.Quoted, "\"(?<word>([^\"])+)\"", 1);
        AddToken(ActualizerToken.Name, "наименовани[ие]", 1);
        //с чем производить действия
        AddToken(ActualizerToken.OperationUnitHeader, "стать[ейями]{2,3}", 1);
        AddToken(ActualizerToken.OperationUnitItem0, "част[ьюями]{2,3}", 1);
        AddToken(ActualizerToken.OperationUnitItem1, "пункт[омаи]{2,3}", 1);
        AddToken(ActualizerToken.OperationUnitItem2, "подпункт[омаи]{2,3}", 1);
        AddToken(ActualizerToken.OperationUnitWord, "слов[оами]*", 1);
        AddToken(ActualizerToken.OperationUnitWord, "цифр[ыами]*", 1);
            //Действия
        AddToken(ActualizerToken.Replace, "заменить", 1);
        AddToken(ActualizerToken.Represent, "изложить", 1);
        AddToken(ActualizerToken.Represent, "изложив", 1);
        AddToken(ActualizerToken.Add, "дополнить", 1);
        AddToken(ActualizerToken.Remove, "исключить", 1);
        AddToken(ActualizerToken.Admit, "признать", 1);
        AddToken(ActualizerToken.LostStrength, "утратившим\\s+силу", 1);
        AddToken(ActualizerToken.After, "после", 1);
        AddToken(ActualizerToken.NumbersRange, "(?<n1>[0-9.XVIxviABCabc]+)\\s?-\\s?(?<n2>[0-9.XVIxviABCabc]+)", 1);
        AddToken(ActualizerToken.Number, $"[0-9-.XVIxviABCabc]+{UPPER}(?:(?<![.]))", 2);
        AddToken(ActualizerToken.StringNumber, oneToNine + "|" + tenToNineten, 3, numbersDict);
        AddToken(ActualizerToken.StringNumber, allTens(), 2, numbersDict);
    }

    string UPPER => "("+ Templates.GetUnicodeChar('\u2070') 
                + "|" + Templates.GetUnicodeChar('\u00B9')
                + "|" + Templates.GetUnicodeChar('\u00B2')
                + "|" + Templates.GetUnicodeChar('\u00B3')
                + "|" + Templates.GetUnicodeChar('\u2074')
                + "|" + Templates.GetUnicodeChar('\u2075')
                + "|" + Templates.GetUnicodeChar('\u2076')
                + "|" + Templates.GetUnicodeChar('\u2077')
                + "|" + Templates.GetUnicodeChar('\u2078')
                + "|" + Templates.GetUnicodeChar('\u2079') + ")*";

        

    
}