namespace Settings;
public abstract class RulesBase<TDef,TEnum> where TDef : new() where TEnum : Enum
{
    /// <summary>
    /// Дефолтные правила для реквизитов
    /// </summary>
    /// <returns></returns>
   public TDef DefaultRules {get;set;} = new TDef();
   public abstract List<CustomRule<TDef>> CustomRequisiteRules {get;set;}
   public List<TokenDefinitionSettings<TEnum>> TokenDefinitionSettings  {get;set;} = new List<TokenDefinitionSettings<TEnum>>();
   protected void addToken(TEnum tt, string pattern, int queue) => 
        TokenDefinitionSettings.Add(new TokenDefinitionSettings<TEnum>(){TokenType = tt, TokenPattern = pattern, TokenQueue = queue});
}