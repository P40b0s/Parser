using System.Collections.Generic;
using System.Linq;
using DocumentParser.Parsers.Annex;

namespace DocumentParser.Reports;
public class HeadersParserReport : ReportBase<Parsers.Headers.HeadersParser>
{
    protected override string ParserName => "Отчет парсера заголовков";
    public HeadersParserReport(Parsers.Headers.HeadersParser parser) : base(parser)
    {
        
    }
    protected override void GetReport()
    {
        var headersByType = parser.Headers.OrderBy(o=>o.Header.Type);
        foreach(var h in headersByType)
            AddInfo("Найдено приложений: " + parser.AnnexesCount);
        AddInfo("Найдено приложений: " + parser.AnnexesCount);
    }

    private void getAnnexCount(List<DocumentElements.Annex> annexes)
    {
        int count = 0;
        
        foreach(var a in annexes)
        {
            AddInfo("Приложение: " + a.AnnexType + " " + a.Name);
            AddInfo("- Заголовков: " + a.Headers.Count);
            count++;
        }
        AddInfo("Всего найдено приложений: " + count);
    }


}