using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.Metas;
public class MetaTokenDefinition : TokenDefinitionBase<MetaTokenType>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
    public MetaTokenDefinition()
    {
        
        addToken(MetaTokenType.ТекущийАбзац, "[(]", 2);
        addToken(MetaTokenType.НовыйАбзац, "(?<=\n\\s*)[(]");
        addToken(MetaTokenType.Конец, "[)]");
        //AddToken(MetaTokenType.Разделитель, "[;]\\s+");

        addToken(MetaTokenType.Редакции, "в\\s*редакции");
        addToken(MetaTokenType.Утратил, "утратил[ао]?\\s+силу");
        addToken(MetaTokenType.Дополнен, "дополнен[о]?\\s");
        addToken(MetaTokenType.Дополнен, "введен[о]?\\s");

        addToken(MetaTokenType.Наименование, "наименование");
        addToken(MetaTokenType.Приложение, "приложение");
        addToken(MetaTokenType.Статья, "cтать[яи]");
        addToken(MetaTokenType.Пункт, "пункт");
        addToken(MetaTokenType.Пункт, "подпункт");
        addToken(MetaTokenType.Пункт, "часть");
        addToken(MetaTokenType.Раздел, "раздел");
        addToken(MetaTokenType.Глава, "глава");
        addToken(MetaTokenType.Абзац, "абзац"); 
    }
}

 
            