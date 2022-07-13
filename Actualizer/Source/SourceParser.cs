using Lexer;
using DocumentParser.Parsers;
using SettingsWorker;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;
using Actualizer.Source.Extensions;
using Actualizer.Structure;
using Actualizer.Source.Operations;

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
            var operation = operations.GetElementOperationType(tokenSequence);
            //Внести в федеральный закон .... изменения, обычно дальше идет перечень изменений
            if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.In))
            {
                if(operation == OperationType.Represent)
                {
                    var node = operations.NewEdition(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    structures.Add(node.Value);
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
            if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.ChangedActRequisites)
            &&  (tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.Definition)
                || tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.Remove)
                || tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.Replace)
                || tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.Add)
                || tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.After)))
            {
                var requsiteNode = Operations.SourceOperations.GetTargetDocumentRequisites(operations.status, tokenSequence, e, parser);
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
                }
                if(operation == OperationType.AddNewElement)
                {
                        var node = operations.AddNewElement(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
                    structures.Add(node.Value);
                }
                var wordsNode = operations.ProcessingWordsOperations(parser, tokenSequence, e, 0);
                if(wordsNode.HasValue)
                {
                    structures.Add(wordsNode.Value);
                }
                else
                {
                    if(operations.status.HaveErrors)
                        return Result<SourceDocumentParserResult, Status>.Err(operations.status);
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