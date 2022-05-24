
using Actualizer.Structure;
using DocumentParser.Parsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SettingsWorker;
using Utils;
using Actualizer.Target.Extensions;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace Actualizer.Target;

public class TargetParser
{
    ISettings settings {get;}
    string filePath {get;}
    public Status status {get;}
    //SourceOperations operations {get;}
    public Operation operations {get;}
    SourceDocumentParserResult source{get;}
    public TargetParser(string filePath, SourceDocumentParserResult source, ISettings settings)
    {
        this.filePath = filePath;
        this.settings = settings;
        this.source = source;
        operations = new Operation(settings);
        status = new Status();
        var templateFilePath = "tmp";
        File.Copy(filePath, templateFilePath, true);
        parser = new Parser(templateFilePath);
    }
    JObject JDoc {get;set;}
    Parser parser {get; set;}
    NumbersSorter sorter {get;}

    /// <summary>
    /// Разбирает документ в который будут вноситься изменения 
    /// </summary>
    /// <returns></returns>
    async ValueTask<Option<bool>> Parse()
    {
        
        var p =  await parser.ParseDocument();
        if(!p)
        {
            status.AddError("Ошибка парсинга изменяемого документа", filePath);
            var errors = parser.GetExceptions();
            foreach(var e in errors)
            {
                status.AddError(Enum.GetName(e.ErrorType), e.Message);
            }
            return Option.None<bool>();
        }
            
        var b = parser.document.Body;
        var json = JsonConvert.SerializeObject(b);
        JDoc = JObject.Parse(json);
        return Option.None<bool>();
    }
    
    void SaveDocument(string path = null)
    {
        if(path == null)
            parser.word.SaveDocument("/home/phobos/Документы/actualizer/actual.docx");
        else
            parser.word.SaveDocument(path);
    }
    /// <summary>
    /// После замены целого параграфа или добавления одного/нескольких параграфов необходимо разбить документ
    /// заново чтобы обновить структуру
    /// </summary>
    /// <returns></returns>
    private async ValueTask Reload()
    {
        var tmpFile = "tmp";
        //parser.word.SaveDocument(tmpFile, true);
        parser.word.Dispose();
        parser = new Parser(tmpFile);
        await Parse();
    }

    public async ValueTask<Result<bool, Status>> Actualize()
    {
        await Parse();
        //каждый изменяющий документ может вносить изменения сразу во много законов
        //Каждая нода относится к своему закону
        //и уже в ней все изменения которые вносятся в данный закон
        foreach(var mainChangeNode in source.Structures)
        {
            if(mainChangeNode.StructureOperation == OperationType.ReplaceWords)
            {
                var res = operations.ReplaceWord(parser, JDoc, mainChangeNode, source);
                if(!res)
                {
                    status.AddErrors(operations.status.statuses);
                    return Result<bool, Status>.Err(status);
                }
            }
            if(mainChangeNode.StructureOperation == OperationType.NextChangeSequence)
            {
                var res = await operations.ChangeSequence(parser, JDoc, mainChangeNode, source, Reload);
                if(!res)
                {
                    status.AddErrors(operations.status.statuses);
                    return Result<bool, Status>.Err(status);
                }
            }
            if(mainChangeNode.StructureOperation == OperationType.Represent)
            {
                var res = await operations.Represent(parser, JDoc, mainChangeNode, source, Reload);
                if(!res)
                {
                    status.AddErrors(operations.status.statuses);
                    return Result<bool, Status>.Err(status);
                }
            }  
        }
        SaveDocument();
        return Result<bool, Status>.Ok(true); 
    }
}