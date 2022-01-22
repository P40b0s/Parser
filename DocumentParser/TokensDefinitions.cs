using System;
using System.Collections.Generic;
using Lexer.Tokenizer;

namespace DocumentParser;

public abstract class TokensDefinitionBase<T> : ListTokensDefinition<T> where T : Enum
{
    public TokensDefinitionBase(List<SettingsWorker.TokenDefinitionSettings<T>> tokens)
    {
        foreach(var t in tokens)
            AddToken(t.TokenType, t.TokenPattern, t.TokenQueue);
    }
}

public class RequisitesTokensDefinition : TokensDefinitionBase<SettingsWorker.Requisites.RequisitesTokenType>
{
    public RequisitesTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Requisites.RequisitesTokenType>> tokens) 
    : base(tokens){ }
}
public class ChangesTokensDefinition : TokensDefinitionBase<SettingsWorker.Changes.ChangesTokenType>
{
    public ChangesTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Changes.ChangesTokenType>> tokens) 
    : base(tokens){ }
}

public class AnnexTokensDefinition : TokensDefinitionBase<SettingsWorker.Annexes.AnnexTokenType>
{
    public AnnexTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Annexes.AnnexTokenType>> tokens) 
    : base(tokens){ }
}

public class HeaderTokensDefinition : TokensDefinitionBase<SettingsWorker.Headers.HeaderTokenType>
{
    public HeaderTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Headers.HeaderTokenType>> tokens) 
    : base(tokens){ }
}

public class FootNoteTokensDefinition : TokensDefinitionBase<SettingsWorker.FootNotes.FootNoteTokenType>
{
    public FootNoteTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.FootNotes.FootNoteTokenType>> tokens) 
    : base(tokens){ }
}

public class MetaTokensDefinition : TokensDefinitionBase<SettingsWorker.Metas.MetaTokenType>
{
    public MetaTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Metas.MetaTokenType>> tokens) 
    : base(tokens){ }
}

public class ItemTokensDefinition : TokensDefinitionBase<SettingsWorker.Items.ItemTokenType>
{
    public ItemTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Items.ItemTokenType>> tokens) 
    : base(tokens){ }
}
