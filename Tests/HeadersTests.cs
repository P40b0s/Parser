using NUnit.Framework;
using System.Threading.Tasks;
using DocumentParser.Workers;
using DocumentParser;
using DocumentParser.DocumentElements;
using SettingsWorker;
using System.Linq;
using System.Collections.Generic;
using DocumentParser.Parsers;
using DocumentParser.Parsers.Requisites;
using DocumentParser.Parsers.Annex;
using DocumentParser.Parsers.Headers;

namespace Tests;

public class HeadersTests : BaseTest<HeadersParser>
{
    protected override List<Files<HeadersParser>> files {get;} = new List<Files<HeadersParser>>()
    {
        //0
        new Files<HeadersParser>()
        {
            FilePath = "указ1237_тест_приложений_2_вложенных.docx",
            Description = "2 приложения в корне 2 приложения вложенны в 1 приложение "+
            "приложение 1 - 8 глав 34 статьи" +
                "приложение 1-1 - 5 итемов во втором итеме 3 субитема " +
                "приложение 1-2 6 пунктов" + 
            "приложение 2",
            DirPath = Paths.CurrentDir + Paths.AnnexHeadersTestPath,
            // PredicateParserTests = new List<PredicateTest<HeadersParser>>()
            // {
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.AnnexesCount == 4,
            //         ResultDescription = "Количество приложений всего - 4"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 2 && p.Annexes[0].Annex.Annexes.Count == 2,
            //         ResultDescription = "Количество приложений в корне - 2шт, вложенных приложений в приложении 1 - 2шт."
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 2 && p.Annexes[0].Annex.ApprovedPrefix.Number == "1237",
            //         ResultDescription = "Приложение 1 утверждено указом 1237"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 2 && p.Annexes[0].Annex.AnnexType.ToLower() == "положение",
            //         ResultDescription = "Приложение 1 - тип ПОЛОЖЕНИЕ"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 2 && p.Annexes[0].Annex.Annexes.Count == 2 && p.Annexes[0].Annex.Annexes[0].AnnexType.ToLower() == "типовая форма",
            //         ResultDescription = "Приложение 1-1 - типовая форма"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 2 && p.Annexes[0].Annex.Annexes.Count == 2 && p.Annexes[0].Annex.Annexes[1].AnnexType.ToLower() == "порядок",
            //         ResultDescription = "Приложение 1-2 - порядок"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 2 && p.Annexes[1].Annex.AnnexType.ToLower() == "перечень",
            //         ResultDescription = "Приложение 2 - тип ПЕРЕЧЕНЬ"
            //     },
            // }
        },
        //1
        new Files<HeadersParser>()
        {
            FilePath = "П-1243-17_08_2020.docx",
            Description = "Постановление с приложением требования",
            DirPath = Paths.CurrentDir + Paths.AnnexHeadersTestPath,
            // PredicateParserTests = new List<PredicateTest<HeadersParser>>()
            // {
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.AnnexesCount == 1,
            //         ResultDescription = "Количество приложений всего - 1"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p => p.Annexes.Count == 1 && p.Annexes[0].Annex.ApprovedPrefix.Number == "1243" && p.Annexes[0].Annex.ApprovedPrefix.Date != null && p.Annexes[0].Annex.ApprovedPrefix.Date == new System.DateTime(2020, 8, 17),
            //         ResultDescription = "Приложение 1 утверждено указом 1243 от 17.08.2020"
            //     },
            //     new PredicateTest<HeadersParser>()
            //     {
            //         Predicate = p  => p.Annexes.Count == 1 && p.Annexes[0].Annex.AnnexType.ToLower() == "требования",
            //         ResultDescription = "Приложение 1 - тип ТРЕБОВАНИЯ"
            //     },
            // }
        },

    };

    //П-1243-17_08_2020.docx
    
    [Test]
    public async ValueTask TestChanges()
    {
        for(int i = 0; i< files.Count; i++)
        {
            await settings.Load();
            document = new Document();
            word = new WordProcessing(settings);
            var fi = new System.IO.FileInfo(files[i].GetPath);
            document.FileName = fi.Name;
            await word.LoadDocument(files[i].GetPath);
            Assert.True(!word.HasFatalError);
            if(word.GetExceptions().Count > 0)
            {
                foreach(var e in word.GetExceptions())
                    System.Console.WriteLine(e.Message);
            }

            var requisites = new RequisitesParser(word, document);
            requisites.Parse();
            var changesParser = new ChangesParser(word);
            changesParser.Parse();
            var annexParser = new AnnexParser(word);
            annexParser
                .WithChanges()
                .Parse();
            var headersParser = new HeadersParser(word, requisites.BeforeBodyElement);
            headersParser.WithChanges().WithAnnexes(annexParser).Parse();
            //var rParser = new DocumentParser.Parsers.Requisites.RequisitesParser(word, document);
            //rParser.Parse();
            //Сортируем приложения, потому что больше мы их использовать нигде не будем, а вообще сортировка будет проводиться в ItemsParser
            //Иначе потом во вложенных приложениях хрен что найдешь...
            annexParser.SortAnnexByHierarchy();
            AddResult(files[i], headersParser , document);
           
        }
        Assert.True(IsPassed());
    }

    
}