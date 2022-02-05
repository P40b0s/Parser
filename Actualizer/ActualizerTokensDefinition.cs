using DocumentParser;

namespace Actualizer;

public class ActualizerTokensDefinition : TokensDefinitionBase<SettingsWorker.Actualizer.ActualizerTokenType>
{
    public ActualizerTokensDefinition(List<SettingsWorker.TokenDefinitionSettings<SettingsWorker.Actualizer.ActualizerTokenType>> tokens) 
    : base(tokens){ }
}