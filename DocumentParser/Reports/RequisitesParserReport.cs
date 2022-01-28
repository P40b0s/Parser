using DocumentParser.Parsers;

namespace DocumentParser.Reports;
public class RequisitesParserReport : ReportBase<Parsers.Annex.AnnexParser>
{
    public AnnexParserReport(Parsers.Annex.AnnexParser parser) : base(parser)
    {
        
    }
    protected override void GetReport()
    {
        
        AddInfo("Найдено приложений: " + parser.AnnexesCount);
    }


}