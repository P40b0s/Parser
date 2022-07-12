using Lexer;
using DocumentParser.Parsers;
using SettingsWorker;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;
using Actualizer.Source.Extensions;
using Actualizer.Structure;

namespace Actualizer.Source;

public class SourceParser
{
    ISettings settings {get;}
    string filePath {get;}
    //SourceOperations operations {get;}
    public Operation operations {get;}
    
    public SourceParser(string filePath, ISettings settings)
    {
        this.filePath = filePath;
        this.settings = settings;
        //operations = new SourceOperations(settings);
        operations = new Operation(settings);
    }
    public async ValueTask<Result<SourceDocumentParserResult, Status>> Parse()
    {
        //var filePath = "/home/phobos/Документы/actualizer/02_07_2021.docx";
        List<StructureNode> structures = new List<StructureNode>();
        var parser = new Parser(filePath);
        var isOk = await parser.ParseDocument();
        var sourceDocumentRequisites = getSourceDocReq(parser.document);
        foreach(var e in parser.word.GetElementsList)
        {
            var lexer = new Lexer<ActualizerTokenType>();
            if(e.IsParsed || e.IsChange)
                continue;
            var text = e.WordElement.Text;
            var tokenSequence = lexer.Tokenize(text, new ActualizerTokensDefinition(settings.TokensDefinitions.ActualizerTokenDefinitions.TokenDefinitionSettings)).ToList();
            var operation = operations.GetOperationType(tokenSequence);
            //Внести
            if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.In))
            {
               
                if(operation == OperationType.Represent)
                {
                    var node = operations.NewEdition(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    structures.Add(node.Value);
                    //operations.NewEditionChange(e, tokenSequence, parser, structures, operation);
                }
                if(operation == OperationType.NextChangeSequence)
                {
                    var node = operations.ChangesSequence(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    structures.Add(node.Value);
                }
            }
            //Если изменение находится в то же параграфе что и реквизиты изменяемого документа
            //скорее всего изменение в одном абзаце
            // if(operation == OperationType.ChangeAndRequisitesInOneParagraph)
            // {
            //     var node = operations.NewEdition(parser, tokenSequence, e, operation);
            //     if(node.IsNone)
            //         return Result<SourceDocumentParserResult, Status>.Err(operations.status);
            //     structures.Add(node.Value);
            // }
            else if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.ChangedActRequisites))
            {
                var requsiteNode = operations.TargetDocumentRequisitesParagraph(parser, tokenSequence, e, operation);
                if (requsiteNode.IsNone)
                {
                    operations.status.AddError("Был обнаружен параграф с реквизитами изменяемого документа, но возникла ошибка их извленчения", e.WordElement.Text);
                    return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                }
                if(operation == OperationType.Represent)
                {
                    var node = operations.NewEdition(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    structures.Add(node.Value);
                    //operations.NewEditionChange(e, tokenSequence, parser, structures, operation);
                }
                if(operation == OperationType.AddNewElement)
                {
                    var node = operations.AddNewElement(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    structures.Add(node.Value);
                    //operations.NewEditionChange(e, tokenSequence, parser, structures, operation);
                }
              
                if(operation == OperationType.ReplaceWords)
                {
                    var node = operations.ReplaceWords(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    node.Value.Path = requsiteNode.Value.Path;
                    node.Value.TargetDocumentRequisites = requsiteNode.Value.TargetDocumentRequisites;
                    //var str = Structure.GetTokensSequence(tokenSequence);
                    //var newNode = new StructureNode(e, operation);
                    //newNode.ChangePartName = Structure.GetPathArray(str, parser, newNode, e);
                    //e.IsParsed = true;
                    //wordOperations.Recognize(operation, newNode, tokenSequence, e, parser);
                    //FIXME после атомарных операций со словами делаем общую операцию ноды - ОПЕРАЦИИ СО СЛОВАМИ!
                    node.Value.StructureOperation = OperationType.WordsOperations;
                    structures.Add(node.Value);
                    
                }
            }
        }
        return Result<SourceDocumentParserResult, Status>.Ok(new SourceDocumentParserResult(parser, structures, sourceDocumentRequisites));
    }

    
    
    private DocumentRequisites getSourceDocReq(DocumentParser.DocumentElements.Document doc) =>
        new DocumentRequisites(){SignDate = doc.SignDate,
                                    Name = doc.Name,
                                    ActType = doc.Type,
                                    Number = doc.Numbers.FirstOrDefault()?.val};
    
    
}