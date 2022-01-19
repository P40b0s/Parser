using Lexer.Tokenizer;
using DocumentParser.Regexes;

namespace DocumentParser.TokensDefinitions
{
    public enum HeaderToken
    {
        None,
        Заголовок,
    }
    //(дополнить\s+(пункт|стать|част|раздел))|((абзац[ы]?|пункт[ы]?|стать[ию]|част[ьи]|разд[елы]+)\s+\S+\s+изложить)
    public class HeaderTokensDefinition : ListTokensDefinition<HeaderToken>
    {
        private string ws = Templates.WsOrBr;
        private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
        public HeaderTokensDefinition()
        {
            AddToken(HeaderToken.Заголовок, "(?<=\n\\s*)((?<type>раздел)\\s*)(?<number>[IVXML]{1}[^\\s.]*)(?<dot>[.])?", 1);
            AddToken(HeaderToken.Заголовок, "(?<=\n\\s*)(?<type>)(?<number>[IVXML]{1}[^\\s.]*)(?<dot>[.])\\s+(?=[^-0-9])", 1);
            AddToken(HeaderToken.Заголовок, "(?<=\n\\s*)((?<type>глава)\\s*)(?<number>[IVXML]{1}[^\\s.]*)(?<dot>[.])?", 1);
            AddToken(HeaderToken.Заголовок, "(?<=\n\\s*)(?<type>статья)\\s*(?<number>[^\\s.]+)(?<dot>[.])?", 1);
        }
    }
}