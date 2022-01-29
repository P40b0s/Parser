// using NUnit.Framework;
// using System.Threading.Tasks;
// using DocumentParser.Workers;
// using DocumentParser;
// using DocumentParser.DocumentElements;
// using SettingsWorker;
// using System.Linq;
// using System.Collections.Generic;

// namespace Tests;

// public class ChangesParserTests : BaseTest<>
// {
//     protected override List<Files> files {get;} = new List<Files>()
//     {
//         //0
//         new Files()
//         {
//             FilePath = "соглашение.docx",
//             Description = "Соглашение МИД",
//             DirPath = Paths.ChangesTestPath
//         },

//     };
    
//     [Test]
//     public async ValueTask TestChanges()
//     {
//         for(int i = 0; i< files.Count; i++)
//         {
//             await settings.Load();
//             document = new Document();
//             word = new WordProcessing(settings);
//             var fi = new System.IO.FileInfo(files[i].GetPath);
//             document.FileName = fi.Name;
//             await word.LoadDocument(files[i].GetPath);
//             Assert.True(!word.HasFatalError);
//             if(word.GetExceptions().Count > 0)
//             {
//                 foreach(var e in word.GetExceptions())
//                     System.Console.WriteLine(e.Message);
//             }

//             var rParser = new DocumentParser.Parsers.Requisites.RequisitesParser(word, document);
//             rParser.Parse();
//             AddResult(files[i].Description, rParser, document);
//         }
//         Assert.True(IsPassed());
//     }

    
// }