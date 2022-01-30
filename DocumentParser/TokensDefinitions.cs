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

public class RequisitesTokensDefinition : TokensDefinitionBase<SettingsWorker.Requisite.RequisiteTokenType>
{
    public RequisitesTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Requisite.RequisiteTokenType>> tokens) 
    : base(tokens){ }
}
public class ChangesTokensDefinition : TokensDefinitionBase<SettingsWorker.Changes.ChangesTokenType>
{
    public ChangesTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Changes.ChangesTokenType>> tokens) 
    : base(tokens){ }
}

public class AnnexTokensDefinition : TokensDefinitionBase<SettingsWorker.Annex.AnnexTokenType>
{
    public AnnexTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Annex.AnnexTokenType>> tokens) 
    : base(tokens){ }
}

public class HeaderTokensDefinition : TokensDefinitionBase<SettingsWorker.Header.HeaderTokenType>
{
    public HeaderTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Header.HeaderTokenType>> tokens) 
    : base(tokens){ }
}

public class FootNoteTokensDefinition : TokensDefinitionBase<SettingsWorker.FootNote.FootNoteTokenType>
{
    public FootNoteTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.FootNote.FootNoteTokenType>> tokens) 
    : base(tokens){ }
}

public class MetaTokensDefinition : TokensDefinitionBase<SettingsWorker.Meta.MetaTokenType>
{
    public MetaTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Meta.MetaTokenType>> tokens) 
    : base(tokens){ }
}

public class ItemTokensDefinition : TokensDefinitionBase<SettingsWorker.Item.ItemTokenType>
{
    public ItemTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Item.ItemTokenType>> tokens) 
    : base(tokens){ }
}
