using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.Actualizer;
public class ActualizerTokenDefinitions : TokenDefinitionBase<ActualizerTokenType>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
    private string oneToNine = "перв[ыйгомую]+|втор[ымгойую]+|трет[ьегомийю]+|четверт[ымйогую]+|пят[ыогмйую]+|шест[гымойую]+|седьм[гымойую]+|восьм[гымойую]+|девят[ыогмйую]+";
    private string tenToNineten = "десят[ымйогую]+|одиннадцат[огымйую]+|двенадцат[огымйую]+|тринадцат[огымйую]+|четырнадцат[огымйую]+|пятнадцат[огымйую]+|шестнадцат[огымйую]+|семнадцат[огымйую]+|восемнадцат[огымйую]+|девятнадцат[огымйую]+";
    private string tens = "двадца[тйогоымую]+|тридца[тйогоымую]+|сороков[ойгымую]+|пятидеся[тйогоымую]+";
    private string allTens() => "(двадцать|тридцать|сорок|пятьдесят)\\s+" +"(" + oneToNine + ")";
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
    //private Dictionary<string, string> numbersDict {get;} = new NumbersDictionary();
    public ActualizerTokenDefinitions()
    {
        
        //addToken(ActualizerTokenType.NewEdition, "(изложить|изложив\\s*(его)?)\\s*в\\s*следующей\\s*редакции\\s*:\\s*\n", 1);
        //addToken(ActualizerTokenType.In, "Внести", 1);
        addToken(ActualizerTokenType.In);
        //addToken(ActualizerTokenType.NextChanges, "следующие\\s*изменения:", 1);
        addToken(ActualizerTokenType.NextChanges);
        addToken(ActualizerTokenType.Definition);
        //addToken(ActualizerTokenType.Definition, ":", 2);
        addToken(ActualizerTokenType.ChangedActRequisites);
        //addToken(ActualizerTokenType.ChangedActRequisites, $"(?<type>федеральн[ыйогм]+\\s*закон[аом]*)\\s*от\\s*(?<date>\\d+)\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*года\\s*(?:N|№)\\s*(?<number>[^\\s]+)\\s*\"(?<name>([^\"])+)\"", 1);
        //addToken(ActualizerTokenType.ChangedActRequisites, $"(?<type>указ[ом]*\\s*президента\\s*российской\\s*федерации)\\s*от\\s*(?<date>\\d+)\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(года|г[.])\\s*(?:N|№)\\s*(?<number>[^\\s]+)\\s*\"(?<name>([^\"])+)\"", 1);
        addToken(ActualizerTokenType.AnnexRequisites, "(положение|состав|стратегия|уведомление|реестр|требования|правила|список|перечень|порядок|форма|план\\s*-\\s*график|типовой\\s*договор|типовая\\s*форма|расчет|условия\\s*и\\s*порядок|изменения)", 1);
        addToken(ActualizerTokenType.AnnexRequisitesStop, $",\\s*утвержденн[оеую]", 1);
        addToken(ActualizerTokenType.Header, "стать[еию]", 2);
        addToken(ActualizerTokenType.Item0, "част[иь]", 2);
        addToken(ActualizerTokenType.Item1, "пункт[е]*", 2);
        addToken(ActualizerTokenType.Item2, "подпункт[е]*", 2);
        addToken(ActualizerTokenType.Indent, "абзац[е]*", 2);
        addToken(ActualizerTokenType.Quoted);
        //addToken(ActualizerTokenType.Quoted, "\"(?<word>([^\"])+)\"", 1);
        addToken(ActualizerTokenType.Name, "наименовани[ие]", 1);
        //с чем производить действия
        addToken(ActualizerTokenType.OperationUnitHeader, "стать[ейями]{2,3}", 1);
        addToken(ActualizerTokenType.OperationUnitItem0, "част[ьюями]{2,3}", 1);
        addToken(ActualizerTokenType.OperationUnitItem1, "пункт[омаи]{2,3}", 1);
        addToken(ActualizerTokenType.OperationUnitItem2, "подпункт[омаи]{2,3}", 1);
        //addToken(ActualizerTokenType.OperationUnitWord, "слов[оами]*", 1);
        //addToken(ActualizerTokenType.OperationUnitWord, "цифр[ыами]*", 1);
        addToken(ActualizerTokenType.OperationUnitWord);
            //Действия
        //addToken(ActualizerTokenType.Replace, "заменить", 1);
        addToken(ActualizerTokenType.Replace);
        //addToken(ActualizerTokenType.Represent, "изложить", 1);
        //addToken(ActualizerTokenType.Represent, "изложив", 1);
        addToken(ActualizerTokenType.Represent);
        addToken(ActualizerTokenType.Add);
        //addToken(ActualizerTokenType.Remove, "исключить", 1);
        addToken(ActualizerTokenType.Remove);
        addToken(ActualizerTokenType.Admit, "признать", 1);
        addToken(ActualizerTokenType.LostStrength, "утратившим\\s+силу", 1);
        //addToken(ActualizerTokenType.After, "после", 1);
        addToken(ActualizerTokenType.After);
        addToken(ActualizerTokenType.NumbersRange, "(?<n1>[0-9.XVIxviABCabc]+)\\s?-\\s?(?<n2>[0-9.XVIxviABCabc]+)", 1);
        addToken(ActualizerTokenType.Number, $"[0-9-.XVIxviABCabc]+{UPPER}(?:(?<![.]))", 2);
        //addToken(ActualizerTokenType.StringNumber, oneToNine + "|" + tenToNineten, 3, numbersDict);
        //addToken(ActualizerTokenType.StringNumber, allTens(), 2, numbersDict);
    }
}

 
            