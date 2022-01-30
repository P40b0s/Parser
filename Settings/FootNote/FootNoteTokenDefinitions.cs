using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.FootNote;
public class FootNoteTokenDefinitions : TokenDefinitionBase<FootNoteTokenType>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
    public FootNoteTokenDefinitions()
    {
        
        addToken(FootNoteTokenType.Подчеркивания, "_{4,}");
        addToken(FootNoteTokenType.Сноска, "(?<=\n\\s*)(?<number>[0-9]{1,2})\\s*[А-Я]");
        addToken(FootNoteTokenType.Сноска, "(?<=\n\\s*)(?<number>[*]{1,4})\\s*[А-Я]", 2);
        addToken(FootNoteTokenType.Сноска, "(?<=\n\\s*)[<]?(?<number>[*]{1,4})[>]?\\s*[А-Я]");
        addToken(FootNoteTokenType.СсылкаНаСноску, "([а-я]{3,}|[)])(?<number>[0-9]{1,2})");
        addToken(FootNoteTokenType.СсылкаНаСноску, "([а-я]{3,}|[)])(?<number>[*]{1,4})");
        addToken(FootNoteTokenType.СсылкаНаСноску, "([а-я]{3,}|[)])[<]?(?<number>[*]{1,4})[>]?");
        addToken(FootNoteTokenType.Примечание, $"(?<=\n\\s*)примечани[ея.]+", 1);
    }
}

 
            