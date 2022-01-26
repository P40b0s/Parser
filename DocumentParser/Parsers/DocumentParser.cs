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
    public class DocumentParser : ParserBase
    {
        public DocumentParser(string filePath)
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

            AddError(metaParser);
            var annexParser = new AnnexParser(word);
            annexParser.UpdateCallback+= c => UpdateStatus(c);
            annexParser.ErrorCallback+= e => AddError(e);
            annexParser.WithMetaNodes(metaParser).Parse();
           
            var headersParser = new HeadersParser(word, requisites.BeforeBodyElement);
            headersParser.UpdateCallback+= c => UpdateStatus(c);
            headersParser.ErrorCallback+= e => AddError(e);
            //без поиска заголовков мы не сможем их извечь 
            headersParser.WithMetaNodes(metaParser).WithAnnexes(annexParser).Parse();
            
            
            //
            //FIXME проблемы с выборкой итемов из примечаний и сносок
            //необходимо это делать после поиска заголовков, чтоб проставить конечные точки
            var footNodeParser = new FootNoteParser(word);
            footNodeParser.UpdateCallback+= c => UpdateStatus(c);
            footNodeParser.ErrorCallback+= e => AddError(e);
            footNodeParser.Parse();
            AddError(footNodeParser);
            headersParser.GetFootNotes(footNodeParser);
            //Таблицу ищем после футнотов а футноты после хедеров и приложений...
            //замкнутый круг
            var tableParser = new TableParser(word);
            //Передаем, чтоб можно было вызвать пару методов headerParser после поиска таблиц
            tableParser.Parse(headersParser, annexParser);
            var itemsParser = new ItemsParser(word);
            itemsParser.Parse(headersParser, annexParser);
            
            AddError(headersParser);
            AddError(itemsParser);
            AddError(tableParser);

            // List<AnnexParserModel> forRemove = new List<AnnexParserModel>();
            // var index = -1;
            // for (int i = 0; i < annexParser.Annexes.Count; i++)
            // {
            //     
            //     //var items = annexParser.Annexes
            //     if(index >=0)
            //     {
            //         if(annexParser.Annexes[index].Hierarchy < annexParser.Annexes[i].Hierarchy)
            //         {
            //             if(annexParser.Annexes[index].Annex.Annexes == null)
            //                 annexParser.Annexes[index].Annex.Annexes = new List<Core.DocumentElements.Annex>();
            //             annexParser.Annexes[index].Annex.Annexes.Add(annexParser.Annexes[i].Annex);
            //             forRemove.Add(annexParser.Annexes[i]);
            //             i++;
            //             continue;
            //         }
            //     }
            //     index = i-1;
            // }
            // annexParser.Annexes.RemoveAll(r=>forRemove.Contains(r));
            //Создаем документ
            UpdateStatus("Формирование документа...");
            document.Body = new DocumentBody(headersParser.Headers.Select(s=>s.Header).ToList(),
                                            headersParser.BodyIndents,
                                            headersParser.BodyItems,
                                            annexParser.Annexes.Select(s=>s.Annex).ToList());
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