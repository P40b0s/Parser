using System;
using System.Collections.Generic;
using DocumentParser.Parsers;

namespace Actualizer.Structure;

public struct SourceDocumentParserResult
{
    public SourceDocumentParserResult(Parser parser, List<StructureNode> structures, DocumentRequisites sourceDocumentRequisites)
    {
        Parser = parser;
        Structures = structures;
        SourceDocumentRequisites = sourceDocumentRequisites;
    }
    public Parser Parser {get;set;}
    public List<StructureNode> Structures {get;set;}
    public DocumentRequisites SourceDocumentRequisites {get;set;}
}
