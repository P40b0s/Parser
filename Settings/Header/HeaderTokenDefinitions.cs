using SettingsWorker.Regexes;
namespace SettingsWorker.Header;
public class HeaderTokenDefinitions : TokenDefinitionBase<HeaderTokenType>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);

    public HeaderTokenDefinitions()
    {
             
        addToken(HeaderTokenType.Заголовок, "(?<=\n\\s*)((?<type>раздел)\\s*)(?<number>[IVXML]{1}[^\\s.]*)(?<dot>[.])?", 1);
        addToken(HeaderTokenType.Заголовок, "(?<=\n\\s*)(?<type>)(?<number>[IVXML]{1}[^\\s.]*)(?<dot>[.])\\s+(?=[^-0-9])", 1);
        addToken(HeaderTokenType.Заголовок, "(?<=\n\\s*)((?<type>глава)\\s*)(?<number>[IVXML]{1}[^\\s.]*)(?<dot>[.])?", 1);
        addToken(HeaderTokenType.Заголовок, "(?<=\n\\s*)(?<type>статья)\\s*(?<number>[^\\s.]+)(?<dot>[.])?", 1);
        
    }
}

 
            