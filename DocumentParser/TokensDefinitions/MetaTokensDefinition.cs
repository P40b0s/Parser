using Services.Documents.Lexer.Tokenizer;
using Services.Documents.Parser.Regexes;

namespace Services.Documents.Parser.TokensDefinitions
{
    public enum MetaToken
    {
        None,
        Наименование,
        Утратил,
        Статья,
        Пункт,
        Раздел,
        Глава,
        Абзац,
        Приложение,
        Редакции,
        Дополнен,
        //Разделитель,
        ТекущийАбзац,
        НовыйАбзац,
        Конец
    }
    public class MetaTokensDefinition : ListTokensDefinition<MetaToken>
    {
        private string ws = Templates.WsOrBr;
        private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
        public MetaTokensDefinition()
        {
            AddToken(MetaToken.ТекущийАбзац, "[(]", 2);
            AddToken(MetaToken.НовыйАбзац, "(?<=\n\\s*)[(]");
            AddToken(MetaToken.Конец, "[)]");
            //AddToken(MetaToken.Разделитель, "[;]\\s+");

            AddToken(MetaToken.Редакции, "в\\s*редакции");
            AddToken(MetaToken.Утратил, "утратил[ао]?\\s+силу");
            AddToken(MetaToken.Дополнен, "дополнен[о]?\\s");
            AddToken(MetaToken.Дополнен, "введен[о]?\\s");

            AddToken(MetaToken.Наименование, "наименование");
            AddToken(MetaToken.Приложение, "приложение");
            AddToken(MetaToken.Статья, "cтать[яи]");
            AddToken(MetaToken.Пункт, "пункт");
            AddToken(MetaToken.Пункт, "подпункт");
            AddToken(MetaToken.Пункт, "часть");
            AddToken(MetaToken.Раздел, "раздел");
            AddToken(MetaToken.Глава, "глава");
            AddToken(MetaToken.Абзац, "абзац"); 
        }
    }
}