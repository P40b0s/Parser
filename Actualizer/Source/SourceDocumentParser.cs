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

namespace Actualizer.Source;

public class SourceDocumentParser
{
    ISettings settings {get;}
    string filePath {get;}
    SourceOperations operations {get;}
    
    public SourceDocumentParser(string filePath, ISettings settings)
    {
        this.filePath = filePath;
        this.settings = settings;
        operations = new SourceOperations();
    }
    public async ValueTask<SourceDocumentParserResult> Parse()
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
            var tokenSequence = lexer.Tokenize(text, new ActualizerTokensDefinition(settings.TokensDefinitions.ActualizerTokenDefinitions.TokenDefinitionSettings));
            if(tokenSequence.Any(a=>a.TokenType == ActualizerTokenType.In))
            {
                var operation = operations.GetNodeOperation(tokenSequence);
                if(operation == Operation.Represent)
                {
                    operations.NewEditionChange(e, tokenSequence, parser, structures, operation);
                }
                if(operation == Operation.NextChangeSequence)
                {
                    operations.NextSequenceChange(e, tokenSequence, parser, structures, operation);
                }
            }
            if(tokenSequence.Any(a=>a.TokenType == ActualizerToken.ChangedActRequisites))
            {
                var operation = operations.GetNodeOperation(tokenSequence);
                if(tokenSequence.Any(a=>a.TokenType == ActualizerToken.Add || a.TokenType == ActualizerToken.After))
                {
                    operations.OneParagraphChange(e, tokenSequence, parser, structures, operation);
                }
                if(operation == Operation.Replace)
                {
                    var str = operations.GetTokensSequence(tokenSequence);
                    var newNode = new StructureNode(e, operation);
                    newNode.ChangePartName = operations.GetPathArray(str, parser, newNode, e);
                    e.IsParsed = true;
                    operations.WordOperation(operation, newNode, tokenSequence, e, parser);
                    structures.Add(newNode);
                }
            }
        }
        var result = new SourceDocumentParserResult(parser, structures, sourceDocumentRequisites);
        return result;
    }

    
    
    private DocumentRequisites getSourceDocReq(DocumentParser.DocumentElements.Document doc) =>
        new DocumentRequisites(){SignDate = doc.SignDate,
                                    Name = doc.Name,
                                    ActType = doc.Type,
                                    Number = doc.Numbers.FirstOrDefault()?.val};
    
    
}