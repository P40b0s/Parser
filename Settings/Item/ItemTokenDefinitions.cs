using SettingsWorker.Regexes;
namespace SettingsWorker.Item;
public class ItemTokenDefinitions : TokenDefinitionBase<ItemTokenType>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);

    public ItemTokenDefinitions()
    {
        addToken(ItemTokenType.Number0_0, "(?<=\n\\s*)(?<number>\\d+)(?<postfix>[.])", 6);
        addToken(ItemTokenType.Number0_0, "(?<=\n\\s*)(?<number>\\d+[-]\\d+)(?<postfix>[.])", 6);
        addToken(ItemTokenType.Number1_0, "(?<=\n\\s*)(?<number>\\d+[.]\\d+)(?<postfix>[.]?)", 5);
        addToken(ItemTokenType.Number1_1, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 4);
        addToken(ItemTokenType.Number1_2, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 3);
        addToken(ItemTokenType.Number1_3, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 2);
        addToken(ItemTokenType.Number1_4, "(?<=\n\\s*)(?<number>\\d+[.]\\d+[.]\\d+[.]\\d+[.]\\d+[.]\\d+)(?<postfix>[.]?)", 1);
        addToken(ItemTokenType.Number2_0, "(?<=\n\\s*)(?<number>\\d+)(?<postfix>[)])", 2);
        addToken(ItemTokenType.Number2_1, "(?<=\n\\s*)(?<number>\\d+[.]\\d+)(?<postfix>[)])", 1);
        addToken(ItemTokenType.Number3_0, "(?<=\n\\s*)(?<number>[а-я](\\d{0,2}))(?<postfix>[)])", 1);
        addToken(ItemTokenType.Number3_0, "(?<=\n\\s*)(?<number>[а-я](\\d{0,2}[-]\\d{0,2}))(?<postfix>[)])", 1);
        
    }
}

 
            