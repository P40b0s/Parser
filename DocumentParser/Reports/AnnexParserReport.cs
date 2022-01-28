using System.Collections.Generic;
using DocumentParser.Parsers.Annex;

namespace DocumentParser.Reports;
public class AnnexParserReport : ReportBase<Parsers.Annex.AnnexParser>
{
    protected override string ParserName => "Отчет парсера приложений";
    public AnnexParserReport(Parsers.Annex.AnnexParser parser) : base(parser)
    {
        
    }
    protected override void GetReport()
    {
        
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