using SettingsWorker.Regexes;
namespace SettingsWorker.Changes;
public class ChangesTokenDefinitions : TokenDefinitionBase<ChangesTokenType>
{
    public ChangesTokenDefinitions()
    {
        addToken(ChangesTokenType.NextIsChange, "следующего\\s*содержания\\s*:\\s*\n");
        addToken(ChangesTokenType.NextIsChange, "(изложить|изложив\\s*его)\\s*в\\s*следующей\\s*редакции\\s*:\\s*\n");
        addToken(ChangesTokenType.Stop, "(?<=\n\\s*)[а-я0-9]+([).])\\s*(в\\s+)?(част|стать|пункт|подпун|разд|правилах)");
        addToken(ChangesTokenType.Stop, "(?<=\n\\s*)[а-я0-9]+([).])\\s*[(]");
        addToken(ChangesTokenType.Stop, "(?<=\n\\s*)[а-я0-9]+([).])\\s*после\\s+(част|стать|пункт|подпун|разд)");
        addToken(ChangesTokenType.Stop, "(?<=\nв?\\s*)(част[ьи]|стать[июе]|пункт[ае]?|подпункт[ае]?|разд[ела]*|правилах)+\\s+\\S+");
        //Напимер Статья 2
        addToken(ChangesTokenType.Stop, "(?<=\n\\s*)статья\\s+\\d+\\s*\n");
        addToken(ChangesTokenType.Ancor, $"[.]{Templates.RightQuotationMark}[.]");
        addToken(ChangesTokenType.Ancor, $"[;]{Templates.RightQuotationMark}[.]");
        addToken(ChangesTokenType.Ancor, $"[:]{Templates.RightQuotationMark}[.]");
        addToken(ChangesTokenType.Ancor, $"[:]{Templates.RightQuotationMark}[;]");
    }
}

 
            