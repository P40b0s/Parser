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
            FilePath = "476-тест_комментов.docx",
            Description = "тест прохода коментариев",
            DirPath = Paths.CurrentDir + Paths.RootTestPath,
        },
    };
    
    [Test]
    public async ValueTask TestChanges()
    {
        for(int i = 0; i< files.Count; i++)
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
           
        }
        Assert.True(IsPassed());
    }

    
}