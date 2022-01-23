using NUnit.Framework;
using System.Threading.Tasks;
using DocumentParser.Workers;
using DocumentParser;
using DocumentParser.DocumentElements;
using SettingsWorker;
using System.Linq;
using System.Collections.Generic;
namespace Tests;

public struct Files
{
    public string FilePath {get;set;}
    string DirPath {get;} = "/home/phobos/Документы/docx/RequisitesTest/";
    public string Description {get;set;}
    public string GetPath => DirPath + FilePath;
    
}

public class Tests
{
    ISettings settings {get;}
    Document document {get;set;} = new Document();
    WordProcessing word {get;set;}
    private List<Files> files {get;} = new List<Files>()
    {
        new Files()
        {
            FilePath = "соглашение.docx",
            Description = "Соглашение МИД"
        },
        new Files()
        {
            FilePath = "закон.docx",
            Description = "Старый вид законов"
        },
        new Files()
        {
            FilePath = "кодекс.docx",
            Description = "Кодекс"
        },
        new Files()
        {
            FilePath = "кодекс_с_частью.docx",
            Description = "Кодекс с частью"
        },
        new Files()
        {
            FilePath = "постановлени_приложение.docx",
            Description = "Постановление правительства"
        },
        new Files()
        {
            FilePath = "прав_распр_без_назв.docx",
            Description = "Распоряжение без названия"
        },
        new Files()
        {
            FilePath = "приказ.docx",
            Description = "Приказ"
        },
        new Files()
        {
            FilePath = "указ.docx",
            Description = "указ президента"
        },
        new Files()
        {
            FilePath = "фз.docx",
            Description = "федеральный закон"
        },
        new Files()
        {
            FilePath = "фз_без_сф.docx",
            Description = "фз без совета федерации"
        },
    };

    public Tests()
    {
        settings = new SettingsWorker.Settings();
        settings.Save();
        //settings.Load();
        word = new WordProcessing(settings);
    }

    [SetUp]
    public void Setup()
    {
       
    }
        //RequisiteTokensModel tokensModel {get;set;}
        
        [Test]
        public async ValueTask ParseRequisites()
        {
            var list = new List<(bool, string, Document)>();
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

                var rParser = new DocumentParser.Parsers.Requisites.RequisitesParser(word, document);
                rParser.Parse();
                
                if(rParser.HasFatalError)
                    System.Console.WriteLine($"Ошибка в {files[i].Description}");
                list.Add((!rParser.HasFatalError, files[i].Description, document));
                //Assert.True(!rParser.HasErrors);
            }

            foreach(var v in list)
            {
                System.Console.WriteLine($"{v.Item1}, {v.Item2} \n" + $"{v.Item3.ToString()}");
            }
            Assert.True(list.All(a=>a.Item1));
        }

    [Test]
    public async ValueTask LoadDocAndParseRequisites()
    {
        //var sourcePath = "/home/phobos/Документы/actualizer/02_07_2021.docx";
        var sourcePath = "/home/phobos/Документы/docx/П-1465-16_09_2020.docx";
        
        var s = new DocumentParser.Parsers.DocumentParser(sourcePath);
        s.StatusesUpdateCallback = u => System.Console.WriteLine(u);
        s.ErrorCallback = e => System.Console.WriteLine(e.Message);
        var load = await s.ParseDocument();
        Assert.IsTrue(load);
    }

    
}