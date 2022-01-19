using Lexer.Tokenizer;
using DocumentParser.Regexes;

namespace DocumentParser.TokensDefinitions
{
    public enum ItemToken
    {
        None,
        Number0_0,
        Number1_0,
        Number1_1,
        Number1_2,
        Number1_3,
        Number1_4,
        Number1_5,
        Number1_6,
        Number1_7,
        Number2_0,
        Number2_1,
        Number2_2,
        Number3_0,
        Number3_1,
        Number4_0,
    }
    //(дополнить\s+(пункт|стать|част|раздел))|((абзац[ы]?|пункт[ы]?|стать[ию]|част[ьи]|разд[елы]+)\s+\S+\s+изложить)
    public class ItemTokensDefinition : ListTokensDefinition<ItemToken>
    {
        private string ws = Templates.WsOrBr;
        private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
        public ItemTokensDefinition()
        {
            AddToken(ItemToken.Number0_0, "(?<=\n\\s*)(?<number>\\d+)(?<postfix>[.])", 6);
            AddToken(ItemToken.Number0_0, "(?<=\n\\s*)(?<number>\\d+[-]\\d+)(?<postfix>[.])", 6);

            AddToken(ItemToken.Number1_0, "(?<=\n\\s*)(?<number>\\d+[.]\\d+)(?<postfix>[.]?)", 5);
            AddToken(ItemToken.Number1_1, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 4);
            AddToken(ItemToken.Number1_2, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 3);
            AddToken(ItemToken.Number1_3, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 2);
            AddToken(ItemToken.Number1_4, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+[.]\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 1);
            AddToken(ItemToken.Number2_0, "(?<=\n\\s*)(?<number>\\d+)(?<postfix>[)])", 2);
            AddToken(ItemToken.Number2_1, "(?<=\n\\s*)(?<number>\\d+[.]\\d+)(?<postfix>[)])", 1);
            AddToken(ItemToken.Number3_0, "(?<=\n\\s*)(?<number>[а-я](\\d{0,2}))(?<postfix>[)])", 1);
            AddToken(ItemToken.Number3_0, "(?<=\n\\s*)(?<number>[а-я](\\d{0,2}[-]\\d{0,2}))(?<postfix>[)])", 1);
        }
    }
}