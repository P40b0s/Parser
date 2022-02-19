using System.Collections.Generic;
using System.Linq;
using Utils;
namespace DocumentParser.Reports;
public abstract class ReportBase<P> where P : Parsers.ParserBase
{
    public List<string> Information {get;} = new List<string>();
    public List<ParserException> Errors {get;}
    public int CriticalErrorsCount => Errors?.Count(w=>w.ErrorType == ErrorType.Fatal) ?? 0; 
    public int WarningsCount => Errors?.Count(w=>w.ErrorType == ErrorType.Warning) ?? 0; 
    protected P parser {get;}

    protected ReportBase(P parser)
    {
        this.parser = parser;
        Errors = parser.GetExceptions();
        GetReport();
    }

    protected void AddInfo(string info)
    {
        Information.Add(info);
    }

    protected abstract void GetReport();
    protected abstract string ParserName {get;}

    public int BodyHeadersCount {get;set;}
    //кодичество заголовков итемов какого уровня приложений абзцев необраь=ботанных нод итд.

}