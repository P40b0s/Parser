using SettingsWorker.Regexes;
using SettingsWorker.Requisites;
namespace SettingsWorker;
public abstract class TokenDefinitionBase<TEnum> where TEnum : Enum
{
    /// <summary>
    /// Определения токенов
    /// </summary>
    /// <returns></returns>
   public List<TokenDefinitionSettings<TEnum>> TokenDefinitionSettings  {get;set;} = new List<TokenDefinitionSettings<TEnum>>();
   protected void addToken(TEnum tt, string pattern, int queue = 1) => 
        TokenDefinitionSettings.Add(new TokenDefinitionSettings<TEnum>(){TokenType = tt, TokenPattern = pattern, TokenQueue = queue});
}