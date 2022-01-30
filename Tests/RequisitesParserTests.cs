using NUnit.Framework;
using System.Threading.Tasks;
using DocumentParser.Workers;
using DocumentParser;
using DocumentParser.DocumentElements;
using SettingsWorker;
using System.Linq;
using System.Collections.Generic;

namespace Tests;




public class RequisitesParserTests : BaseTest<DocumentParser.Parsers.Requisites.RequisitesParser>
{
    protected override List<Files<DocumentParser.Parsers.Requisites.RequisitesParser>> files {get;} = new List<Files<DocumentParser.Parsers.Requisites.RequisitesParser>>()
    {
        //0
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "соглашение.docx",
            Description = "Соглашение МИД",
            DirPath =  Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //1
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "закон.docx",
            Description = "Старый вид законов",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //2
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "кодекс.docx",
            Description = "Кодекс",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //3
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "кодекс_с_частью.docx",
            Description = "Кодекс с частью",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //4
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "постановлени_приложение.docx",
            Description = "Постановление правительства",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //5
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "прав_распр_без_назв.docx",
            Description = "Распоряжение без названия",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //6
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "приказ.docx",
            Description = "Приказ",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //7
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "указ.docx",
            Description = "указ президента",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //8
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "фз.docx",
            Description = "федеральный закон",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //9
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "фз_без_сф.docx",
            Description = "фз без совета федерации",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath
        },
        //10
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "совместный_приказ_слеш.docx",
            Description = "Совместный приказ с одним номером-исключением",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath,
            PredicateDocumentTests = new List<PredicateTest<Document>>()
            {
                new PredicateTest<Document>()
                {
                    Predicate = p  => p.Numbers.Count == 2 && p.Numbers[0].val == "317" && p.Numbers[1].val == "ММВ-7-2/481@",
                    ResultDescription = "Номер документа раскладывается по слешам с одним исключением, был 1 номер - 317/ММВ-7-2/481@, должно получиться 2 номера - 317 и ММВ-7-2/481@"
                },
                new PredicateTest<Document>()
                {
                    Predicate = p  => p.Organs.Count == 2 && p.Organs[0].val.ToUpper() == "МИНИСТЕРСТВО ВНУТРЕННИХ ДЕЛ РОССИЙСКОЙ ФЕДЕРАЦИИ" && p.Organs[1].val.ToUpper() == "ФЕДЕРАЛЬНАЯ НАЛОГОВАЯ СЛУЖБА",
                    ResultDescription = "Должно быть 2 органа - МИНИСТЕРСТВО ВНУТРЕННИХ ДЕЛ РОССИЙСКОЙ ФЕДЕРАЦИИ и ФЕДЕРАЛЬНАЯ НАЛОГОВАЯ СЛУЖБА"
                }
            }
        },
        //11
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "совместный_приказ_материнский_орган.docx",
            Description = "Приказ с одним материнским органом, на выходе должен быть только один орган",
            DirPath = Paths.CurrentDir + Paths.RequisitesTestPath,
            PredicateDocumentTests = new List<PredicateTest<Document>>()
            {
                new PredicateTest<Document>()
                {
                    Predicate = p  => p.Organs.Count == 1 && p.Organs[0].val.ToUpper() == "ФЕДЕРАЛЬНАЯ СЛУЖБА ИСПОЛНЕНИЯ НАКАЗАНИЙ",
                    ResultDescription = "Тут есть материнский орган, при первоначальном парсинге их будет 2, но после парсинга номера и слешей в номере должен остаться один - ФЕДЕРАЛЬНАЯ СЛУЖБА ИСПОЛНЕНИЯ НАКАЗАНИ"
                }
            }
        },
         //12
        new Files<DocumentParser.Parsers.Requisites.RequisitesParser>()
        {
            FilePath = "указ1237_тест_приложений_2_вложенных.docx",
            Description = "Указ о прохождении военной службы с 4 приложениями",
            DirPath = Paths.CurrentDir + Paths.AnnexHeadersTestPath,
            PredicateDocumentTests = new List<PredicateTest<Document>>()
            {
                new PredicateTest<Document>()
                {
                    Predicate = p  => p.Organs.Count == 1 && p.Organs[0].val.ToLower() == "президент российской федерации",
                    ResultDescription = "Осуществляется замена президента российской федерации на президент российской федерации"
                }
            }
        },
    };

    //
    

        [Test]
        public async ValueTask ParseRequisites()
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

                var rParser = new DocumentParser.Parsers.Requisites.RequisitesParser(word, document);
                rParser.Parse();
                //Проверка внутренних ошибок парсеров
                AddResult(files[i], rParser, document);

            }
            Assert.True(IsPassed());
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