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

public class FullTests : BaseTest<Parser>
{
    protected override List<Files<Parser>> files {get;} = new List<Files<Parser>>()
    {
        //0
        new Files<Parser>()
        {
            FilePath = "354.docx",
            Description = "тест большого правительства с 6 приложениями",
            DirPath = Paths.CurrentDir + Paths.AnnexHeadersTestPath,
        },
         //1
        new Files<Parser>()
        {
            FilePath = "476-тест_комментов.docx",
            Description = "Опять какие то траблы с метаинформацией...",
            DirPath = Paths.CurrentDir + Paths.RootTestPath,
        },
        //2
        new Files<Parser>()
        {
            FilePath = "ФЗ-476_не_парсится.docx",
            Description = "Говорит не парсится из-за новых строк",
            DirPath = Paths.CurrentDir + Paths.RootTestPath,
        },
        //3
        new Files<Parser>()
        {
            FilePath = "476-тест_комментов_3уровня_итемов.docx",
            Description = "Сборная солянка из всего чего можно",
            DirPath = Paths.CurrentDir + Paths.RootTestPath,
        },
        
    };
    
    [Test]
    public async ValueTask TestChanges()
    {
        for(int i = 3; i< files.Count; i++)
        {
            await settings.Load();
            var fi = new System.IO.FileInfo(files[i].GetPath);
            //await word.LoadDocument(files[i].GetPath);
            //Assert.True(!word.HasFatalError);
            //if(word.GetExceptions().Count > 0)
            //{
            //    foreach(var e in word.GetExceptions())
            //        System.Console.WriteLine(e.Message);
            //}
            var par = new Parser(files[i].GetPath);
            var ok = await par.ParseDocument();

            AddResult(files[i], par , par.document);
            var j = Newtonsoft.Json.JsonConvert.SerializeObject(par.document);
            System.IO.File.WriteAllText(files[i].FilePath + ".json", j);
           
        }
        Assert.True(IsPassed());
    }

    
}