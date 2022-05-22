using SettingsWorker.Regexes;
using SettingsWorker.Requisite;
namespace SettingsWorker;
public abstract class TokenDefinitionBase<TEnum> where TEnum : Enum
{
        /// <summary>
        /// Определения токенов
        /// </summary>
        /// <returns></returns>
    public List<TokenDefinitionSettings<TEnum>> TokenDefinitionSettings  {get;set;} = new List<TokenDefinitionSettings<TEnum>>();
    protected void addToken(TEnum tt, string pattern, int queue = 1, Dictionary<string, string> converter = null) => 
            TokenDefinitionSettings.Add(new TokenDefinitionSettings<TEnum>(){TokenType = tt, TokenPattern = pattern, TokenQueue = queue, Converter = converter});
    protected void addToken(TEnum tt, Dictionary<string, string> converter = null)
    {
        var def = System.Attribute.GetCustomAttributes(tt.GetType()).FirstOrDefault(f=>f is TokenDefinitionAttribute);
        if(def != null)
        {
            var d = (TokenDefinitionAttribute)def;
            TokenDefinitionSettings.Add(new TokenDefinitionSettings<TEnum>(){TokenType = tt, TokenPattern = d.pattern, TokenQueue = d.queue, Converter = converter});
        }
    }
}