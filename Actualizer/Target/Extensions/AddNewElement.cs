using Actualizer.Structure;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Utils;
using Utils.Extensions;

namespace Actualizer.Target.Extensions;
public static class AddNewElementExt
{
    public static async ValueTask<bool> AddNewElement(this StructureNode node, ElementStructure beforeElement, Parser parser, SourceDocumentParserResult source, Func<ValueTask> reload)
    {  
        Option<Paragraph> first = Option.None<Paragraph>();
        Option<Paragraph> last = Option.None<Paragraph>();
        Option<Paragraph> insertAfter = Option.None<Paragraph>();
        for(int n = 0; n < node.ChangesNodes.Count(); n++)
        {
            Paragraph newPar = node.ChangesNodes[n].WordElement.Element.CloneNode(true) as Paragraph;
            if(insertAfter.IsNone)
                beforeElement.WordElement.Element.InsertAfterSelf(newPar);
            else
                insertAfter.Value.InsertAfterSelf(newPar);
            node.ChangesNodes[n].WordElement.Element.CopyParagraphStyle(newPar, source, parser);
            newPar.CopyAllRunsStyles(source, parser);
            newPar.CopyAllImages(source, parser);
            //Выбираем первый и последний параграфы для выделения коментариями
            if(n == 0)
                first = newPar.OptionFromValueOrDefault();
            //if(n == node.ChangesNodes.Count() - 1)
            last = newPar.OptionFromValueOrDefault();
            insertAfter = newPar.OptionFromValueOrDefault();
        }
        first.Try(f=>
            last.Try(l=>
            {
                var comment = new CommentModel(parser.word.Document.MainDocumentPart,
                                    "Скайнетов К.О.",
                                    "абырвалГ" + source.SourceDocumentRequisites.GetCommentLabel(),
                                    f,
                                    l);
                comment.AddParagraphComment();
            })
        );
        // var comment = new CommentModel(parser.word.Document.MainDocumentPart,
        //                             "Скайнетов К.О.",
        //                             "абырвалГ" + source.SourceDocumentRequisites.GetCommentLabel(),
        //                             first,
        //                             last);
        // comment.AddParagraphComment();
        await reload();
        return true;
        //await Reload();
    }
}