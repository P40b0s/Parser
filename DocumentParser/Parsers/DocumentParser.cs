using DocumentParser.Workers;
using System.Threading.Tasks;
using DocumentParser.DocumentElements;
using DocumentParser.Parsers.Requisites;
using System.Linq;
using DocumentParser.Parsers.Headers;
using DocumentParser.Parsers.Annex;
using DocumentParser.Parsers.Items;
using System.IO;

namespace DocumentParser.Parsers
{
    /// <summary>
    /// Центральный модуль парсинга документа, один на каждый док, отсюда вызываются все остальные модули
    /// Но остальные парсеры довольно самодостаточные и могут в неготорых случаях использоваться отдельно
    /// Стандартная очередность рпботы парсеров:
    /// 1. RequisitesParser
    /// 2. ChangesParser
    /// 3. MetaParser 
    /// 4. AnnexParser
    /// 5. HeadersParser
    /// 6. FootNodeParser
    /// 7. TableParser
    /// 10. itemsParser.Parse(headersParser, annexParser);
    /// 11. annexParser.MoveAnnexByHierarchy();
    /// </summary>
    public class Parser : ParserBase
    {
        public Parser(string filePath)
        {
            this.filePath = filePath;
            settings = new SettingsWorker.Settings();
            settings.Load();
            word = new WordProcessing(settings);
        }
        public Document document {get;} = new Document();
        public WordProcessing word {get;}
        //RequisiteTokensModel tokensModel {get;set;}
        private string filePath {get;}
        
        public async ValueTask<bool> ParseDocument()
        {
            if(string.IsNullOrEmpty(filePath))
                return AddError("Не выбран файл для обработки");
            var f = new FileInfo(filePath);
            document.FileName = f.Name;
            await word.LoadDocument(filePath);
            if(!AddError(word))
                return false;

            var requisites = new RequisitesParser(word, document);
            requisites.UpdateCallback+= c => UpdateStatus(c);
            requisites.ErrorCallback+= e => AddError(e.Message, null, e.ErrorType);
            requisites.Parse();
            if(!AddError(requisites))
                return false;
            var changesParser = new ChangesParser(word);
            changesParser.UpdateCallback+= c => UpdateStatus(c);
            changesParser.ErrorCallback+= e => AddError(e);
            changesParser.Parse();
            AddError(changesParser);

            var metaParser = new MetaParser(word);
            metaParser.UpdateCallback+= c => UpdateStatus(c);
            metaParser.ErrorCallback+= e => AddError(e);
            metaParser.Parse();
            //БЕз парсера реквизитов мы не узнаем где еачало  конец тела документа
            //без парсера изменений мы не узнаем является ли заголовок частью изменения 
            //или это заголовок самого документа
            //без парсера мета информации мы не найдем системовские комментарии и не сможем их привязать к нашим
            //абзацам или зашоловкам
            //Таблицу ищем после футнотов а футноты после хедеров и приложений...
            //FIXME ПОЧЕМУ? через 2 масяа уже не ясно
            //замкнутый круг
            var tableParser = new TableParser(word);
            tableParser.UpdateCallback+= c => UpdateStatus(c);
            tableParser.ErrorCallback+= e => AddError(e);
            //Передаем, чтоб можно было вызвать пару методов headerParser после поиска таблиц
            tableParser.Parse();
            var annexParser = new AnnexParser(word);
            annexParser.UpdateCallback+= c => UpdateStatus(c);
            annexParser.ErrorCallback+= e => AddError(e);
            annexParser.WithMeta()
                .WithChanges()
                .WithTables()
                .Parse();
           
            var headersParser = new HeadersParser(word, requisites.BeforeBodyElement);
            headersParser.UpdateCallback += c => UpdateStatus(c);
            headersParser.ErrorCallback += e => AddError(e);
            //без поиска заголовков мы не сможем их извечь 
            headersParser.WithMeta()
                .WithChanges()
                .WithTables()
                .WithAnnexes(annexParser)
                .Parse();
            
            
            //
            //FIXME проблемы с выборкой итемов из примечаний и сносок
            //и вообще надо это переосмыслить
            //необходимо это делать после поиска заголовков, чтоб проставить конечные точки
            var footNodeParser = new FootNoteParser(word);
            footNodeParser.UpdateCallback+= c => UpdateStatus(c);
            footNodeParser.ErrorCallback+= e => AddError(e);
            footNodeParser.Parse(headersParser, annexParser);

            var itemsParser = new ItemsParser(word);
            itemsParser.UpdateCallback+= c => UpdateStatus(c);
            itemsParser.ErrorCallback+= e => AddError(e);
            itemsParser.Parse(headersParser, annexParser);

            //Создаем документ
            UpdateStatus("Формирование документа...");
            document.Body = new DocumentBody(headersParser.Headers.Select(s=>s.Header).ToList(),
                                            headersParser.BodyIndents,
                                            headersParser.BodyItems,
                                            annexParser.Annexes.Select(s=>s.Annex).ToList(),
                                            word.Comments.Select(s=>s.ToComment).ToList());
            foreach(var el in word.GetElementsList)
            {
                if(el.Table != null)
                {
                    var prev = word.GetElement(el.CurrentIndex -1);
                    if(prev.IsOk)
                    {
                        prev.Value().Table = el.Table;
                    }
                }
            }
            if(word.DocumentImages.Count > 0)
            {
                document.Images = word.DocumentImages;
                document.ImagesLength = word.ImagesLenth;
            }
            var dd = word.GetElement(0);
            //var items = word.GetElementsExcept(annexParser.Annexes.SelectMany(s=>s.RootItems), headersParser.Headers.SelectMany(s=>s.Items));
            //TEST
            //word.SaveDocument("/home/phobos/Документы/354_hl.docx");
            return !HasFatalError;
        }
    }
}