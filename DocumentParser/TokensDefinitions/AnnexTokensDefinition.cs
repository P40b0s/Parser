using Services.Documents.Lexer.Tokenizer;
using Services.Documents.Parser.Regexes;

namespace Services.Documents.Parser.TokensDefinitions
{
    public enum AnnexToken
    {
        None,
        Утверждено,
        Приложение,
        ТипПриложения,
        Номер,
        Дата,
        ПриложениеКПриложению,
        ПриложениеКДокументу
    }
    //(дополнить\s+(пункт|стать|част|раздел))|((абзац[ы]?|пункт[ы]?|стать[ию]|част[ьи]|разд[елы]+)\s+\S+\s+изложить)
    public class AnnexTokensDefinition : ListTokensDefinition<AnnexToken>
    {
        private string ws = Templates.WsOrBr;
        private string  brChar = Templates.GetEmptyUnicodeChar(Templates.BRChar);
        public AnnexTokensDefinition()
        {
           AddToken(AnnexToken.Дата, $"(?:от\\s*)?(?<day>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)");
           AddToken(AnnexToken.Номер, $"(?:N|№)\\s*(?<number>[^\\s{brChar}]+)");
           AddToken(AnnexToken.Утверждено, $"(?<=\n\\s*)утвержден[оы]{ws}");
           AddToken(AnnexToken.Приложение, $"(?<=\n\\s*)приложение{ws}");
           AddToken(AnnexToken.ПриложениеКДокументу, $"{ws}к{ws}постановлению|распоряжению|приказу|указу");
           AddToken(AnnexToken.ПриложениеКПриложению, $"{ws}к{ws}положению|составу|стратегии|уведомлению|реестру|требованиям|правилам|списку|перечню|плану\\s*-\\s*графику|изменениям|типовому\\s*договору|расчету|условиям\\s*и\\s*порядку|изменениям");
           AddToken(AnnexToken.ТипПриложения, $"(?<=\n\\s*)(?<type>положение|состав|стратегия|уведомление|реестр|требования|правила|список|перечень|порядок|форма|план\\s*-\\s*график|типовой\\s*договор|типовая\\s*форма|расчет|условия\\s*и\\s*порядок|изменения)");
        }
    }
}