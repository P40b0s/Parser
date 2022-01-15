using Lexer.Tokenizer;
using DocumentParser.Regexes;

namespace DocumentParser.TokensDefinitions
{
    public enum ChangesIndentsToken
    {
        None,
        NextIsChange,
        Stop,
        // изменение обычно заканчивается чем товроде .". или ."; использовать только как вспомогательный инструмент
        // очень часто из-за этого были ошибки.
        Ancor,

    }
    //(дополнить\s+(пункт|стать|част|раздел))|((абзац[ы]?|пункт[ы]?|стать[ию]|част[ьи]|разд[елы]+)\s+\S+\s+изложить)
    public class ChangesIndentsTokensDefinition : ListTokensDefinition<ChangesIndentsToken>
    {
        public ChangesIndentsTokensDefinition()
        {
           //AddToken(ChangesIndentsToken.NextIsChange, "дополнить\\s+(абзац|част|пункт|стать|раздел)");
           AddToken(ChangesIndentsToken.NextIsChange, "следующего\\s*содержания\\s*:\\s*\n");
           AddToken(ChangesIndentsToken.NextIsChange, "(изложить|изложив\\s*его)\\s*в\\s*следующей\\s*редакции\\s*:\\s*\n");
           AddToken(ChangesIndentsToken.Stop, "(?<=\n\\s*)[а-я0-9]+([).])\\s*(в\\s+)?(част|стать|пункт|подпун|разд|правилах)");
           AddToken(ChangesIndentsToken.Stop, "(?<=\n\\s*)[а-я0-9]+([).])\\s*[(]");
           AddToken(ChangesIndentsToken.Stop, "(?<=\nв?\\s*)(част[ьи]|стать[июе]|пункт[ае]?|подпункт[ае]?|разд[ела]*|правилах)+\\s+\\S+");
           AddToken(ChangesIndentsToken.Stop, "(?<=\n\\s*)статья\\s+\\d+\\s*\n");
           AddToken(ChangesIndentsToken.Ancor, $"[.]{Templates.RightQuotationMark}[.]");
           AddToken(ChangesIndentsToken.Ancor, $"[;]{Templates.RightQuotationMark}[.]");
           AddToken(ChangesIndentsToken.Ancor, $"[:]{Templates.RightQuotationMark}[.]");
           AddToken(ChangesIndentsToken.Ancor, $"[:]{Templates.RightQuotationMark}[;]");
        }
    }
}