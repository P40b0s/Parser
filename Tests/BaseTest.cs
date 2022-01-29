using NUnit.Framework;
using System.Threading.Tasks;
using DocumentParser.Workers;
using DocumentParser;
using DocumentParser.DocumentElements;
using SettingsWorker;
using System.Linq;
using System.Collections.Generic;
namespace Tests;

public struct Files<T>
{
    public string FilePath {get;set;}
    public string DirPath {get;init;}
    public string Description {get;set;}
    public string GetPath => DirPath + FilePath;
    public List<PredicateTest<Document>> PredicateDocumentTests {get;set;} = new List<PredicateTest<Document>>();
    public List<PredicateTest<T>> PredicateParserTests {get;set;} = new List<PredicateTest<T>>();

}
public struct PredicateTest<T>
{
    public string ResultDescription {get;set;}
    public System.Predicate<T> Predicate {get;set;}
}
public struct PredicateResult
{
    public bool Result {get;set;}
    public string Description {get;set;}
}

public struct ResultItem
{
    public bool HasFatalErrors {get;set;}
    public string Description {get;init;}
    public Document Document {get;set;}
    public List<ParserException> exceptions {get;set;}
    public List<PredicateResult> PredicateResults {get;set;}

}
public abstract class BaseTest<T> where T : DocumentParser.Parsers.ParserBase
{
    protected ISettings settings {get;}
    protected Document document {get;set;} = new Document();
    protected WordProcessing word {get;set;}
    protected List<ResultItem> Results = new List<ResultItem>();
    protected abstract List<Files<T>> files {get;}
    public BaseTest()
    {
        settings = new SettingsWorker.Settings();
        settings.Save();
        //settings.Load();
        word = new WordProcessing(settings);
    }

    protected void AddResult(Files<T> file, T parser, Document doc)
    {

        if(parser.GetExceptions().Count > 0)
            System.Console.WriteLine($"Ошибки в {file.Description}:");
        foreach(var e in parser.GetExceptions())
        {
            var error = e.ErrorType.Equals(ErrorType.Fatal) ? "fatal" : "warning";
            System.Console.WriteLine(e.Message + " статус: " + error );
        }
        List<PredicateResult> predicateResults = getPredicateResults(file, parser);
        if(predicateResults.Count > 0)
            System.Console.WriteLine($"Дополнительные тесты {file.Description}:");
        foreach(var p in predicateResults)
        {
            System.Console.WriteLine("Тест: "+ p.Description + " статус: " + p.Result );
        }
        Results.Add(new ResultItem(){HasFatalErrors = parser.HasFatalError, Description = file.Description, Document = doc, exceptions = parser.GetExceptions(), PredicateResults = predicateResults});
    }

    protected bool IsPassed() => Results.All(a=>!a.HasFatalErrors && a.PredicateResults.All(al=>al.Result == true));

    List<PredicateResult> getPredicateResults(Files<T> file, T parser)
    {
        var parserTest = file.PredicateParserTests.Select(s=> new PredicateResult(){ Result = s.Predicate(parser), Description = s.ResultDescription });
        var docTest = file.PredicateDocumentTests.Select(s=> new PredicateResult(){ Result = s.Predicate(document), Description = s.ResultDescription });
        return parserTest.Concat(docTest).ToList();
    }
        
    
}