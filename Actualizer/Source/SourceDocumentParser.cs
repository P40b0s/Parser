using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentParser.DocumentElements;
using Lexer;
using Lexer.Tokenizer;
using DocumentParser.Parsers;
using SettingsWorker;
using SettingsWorker.Actualizer;
using Actualizer.Source.Operations;
using Utils;
using Utils.Extensions;
using Actualizer.Source.Extensions;
using Actualizer.Structure;

namespace Actualizer.Source;

public class SourceDocumentParser
{
    ISettings settings {get;}
    string filePath {get;}
    //SourceOperations operations {get;}
    public Operation operations {get;}
    
    public SourceDocumentParser(string filePath, ISettings settings)
    {
        this.filePath = filePath;
        this.settings = settings;
        //operations = new SourceOperations(settings);
        operations = new Operation(settings);
    }
    public async ValueTask<Option<SourceDocumentParserResult>> Parse()
    {
        //var filePath = "/home/phobos/Документы/actualizer/02_07_2021.docx";
        List<StructureNode> structures = new List<StructureNode>();
        var parser = new Parser(filePath);
        var isOk = await parser.ParseDocument();
        var sourceDocumentRequisites = getSourceDocReq(parser.document);
        foreach(var e in parser.word.GetElementsList)
        {
            var lexer = new Lexer<ActualizerTokenType>();
            if(e.IsParsed)
                continue;
            var text = e.WordElement.Text;
            var tokenSequence = lexer.Tokenize(text, new ActualizerTokensDefinition(settings.TokensDefinitions.ActualizerTokenDefinitions.TokenDefinitionSettings)).ToList();
            if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.In))
            {
                var operation = operations.GetOperationType(tokenSequence);
                if(operation == OperationType.Represent)
                {
                    var node = operations.NewEdition(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Option.None<SourceDocumentParserResult>();
                    structures.Add(node.Value);
                    //operations.NewEditionChange(e, tokenSequence, parser, structures, operation);
                }
                if(operation == OperationType.NextChangeSequence)
                {
                    var node = operations.ChangesSequence(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Option.None<SourceDocumentParserResult>();
                    structures.Add(node.Value);
                }
            }
            //Если изменение находится в то же параграфе что и реквизиты изменяемого документа
            //скорее всего изменение в одном абзаце
            if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.ChangedActRequisites))
            {
                var operation = operations.GetOperationType(tokenSequence);
                //TODO разобраться для чего тут это условие
                if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.Add || a.TokenType == ActualizerTokenType.After))
                {
                    var node = operations.TargetDocumentRequisitesParagraph(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Option.None<SourceDocumentParserResult>();
                    //if(!operations.OneParagraphChange(e, tokenSequence, parser, structures, operation))
                    //    return Option.None<SourceDocumentParserResult>();
                    structures.Add(node.Value);
                }
                if(operation == OperationType.ReplaceWords)
                {
                    var node = operations.ReplaceWords(parser, tokenSequence, e, operation);
                    if(node.IsNone)
                        return Option.None<SourceDocumentParserResult>();
                    
                    //var str = Structure.GetTokensSequence(tokenSequence);
                    //var newNode = new StructureNode(e, operation);
                    //newNode.ChangePartName = Structure.GetPathArray(str, parser, newNode, e);
                    //e.IsParsed = true;
                    //wordOperations.Recognize(operation, newNode, tokenSequence, e, parser);
                    structures.Add(node.Value);
                }
            }
        }
        return new SourceDocumentParserResult(parser, structures, sourceDocumentRequisites).OptionFromValueOrDefault();
    }

    
    
    private DocumentRequisites getSourceDocReq(DocumentParser.DocumentElements.Document doc) =>
        new DocumentRequisites(){SignDate = doc.SignDate,
                                    Name = doc.Name,
                                    ActType = doc.Type,
                                    Number = doc.Numbers.FirstOrDefault()?.val};
    
    
}