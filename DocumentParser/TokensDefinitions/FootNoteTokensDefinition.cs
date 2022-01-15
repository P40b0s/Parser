using Services.Documents.Lexer.Tokenizer;
using Services.Documents.Parser.Regexes;

namespace Services.Documents.Parser.TokensDefinitions
{
    public enum FootNoteToken
    {
        None,
        Подчеркивания,
        Сноска,
        СсылкаНаСноску,
        Примечание
    }
    public class FootNoteTokensDefinition : ListTokensDefinition<FootNoteToken>
    {
        private string ws = Templates.WsOrBr;
        private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
        public FootNoteTokensDefinition()
        {
           AddToken(FootNoteToken.Подчеркивания, "_{4,}");
           AddToken(FootNoteToken.Сноска, "(?<=\n\\s*)(?<number>[0-9]{1,2})\\s*[А-Я]");
           AddToken(FootNoteToken.Сноска, "(?<=\n\\s*)(?<number>[*]{1,4})\\s*[А-Я]", 2);
           AddToken(FootNoteToken.Сноска, "(?<=\n\\s*)[<]?(?<number>[*]{1,4})[>]?\\s*[А-Я]");
           AddToken(FootNoteToken.СсылкаНаСноску, "([а-я]{3,}|[)])(?<number>[0-9]{1,2})");
           AddToken(FootNoteToken.СсылкаНаСноску, "([а-я]{3,}|[)])(?<number>[*]{1,4})");
           AddToken(FootNoteToken.СсылкаНаСноску, "([а-я]{3,}|[)])[<]?(?<number>[*]{1,4})[>]?");
           AddToken(FootNoteToken.Примечание, $"(?<=\n\\s*)примечани[ея.]+", 1);
        }
    }
}