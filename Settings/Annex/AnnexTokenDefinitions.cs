using SettingsWorker.Regexes;
using SettingsWorker;
namespace SettingsWorker.Annex;
public class AnnexTokenDefinitions : TokenDefinitionBase<AnnexTokenType>
{
    private string ws = Templates.WsOrBr;
    private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
    public AnnexTokenDefinitions()
    {
        
        addToken(AnnexTokenType.Дата, $"(?:от\\s*)?(?<day>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)");
        addToken(AnnexTokenType.Номер, $"(?:N|№)\\s*(?<number>[^\\s{brChar}]+)");
        addToken(AnnexTokenType.Утверждено, $"(?<=\n\\s*)утвержден[оы]{ws}");
        addToken(AnnexTokenType.Приложение, $"(?<=\n\\s*)приложение{ws}");
        addToken(AnnexTokenType.ПриложениеКДокументу, $"{ws}к{ws}постановлению|распоряжению|приказу|указу");
        addToken(AnnexTokenType.ПриложениеКПриложению, $"{ws}к{ws}положению|составу|стратегии|уведомлению|реестру|требованиям|правилам|списку|перечню|плану\\s*-\\s*графику|изменениям|типовому\\s*договору|расчету|условиям\\s*и\\s*порядку|изменениям");
        addToken(AnnexTokenType.ТипПриложения, $"(?<=\n\\s*)(?<type>положение,?|состав,?|стратегия,?|уведомление,?|реестр,?|требования,?|правила,?|список,?|перечень,?|порядок,?|форма,?|план\\s*-\\s*график,?|типовой\\s*договор,?|типовая\\s*форма,?|расчет,?|условия\\s*и\\s*порядок,?|изменения,?)");
    }
}

 
            