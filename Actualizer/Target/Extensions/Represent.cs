using Actualizer.Source.Operations;
using Actualizer.Structure;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using Newtonsoft.Json.Linq;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Target.Extensions;

public static class RepresentEx
{
    /// <summary>
    /// Операция изложения в новой редакции одного или нескольких абзацев
    /// </summary>
    /// <param name="node">Нода с изменением</param>
    /// <param name="targetDocumentJObject">Представлние JObject для документа в который будут вноситься изменения</param>
    /// <param name="parser">Парсер документа в который вносятся изменения</param>
    public static async ValueTask<bool> Represent(this Operation op, Parser parser, JObject JDoc, StructureNode node, SourceDocumentParserResult source, Func<ValueTask> reload)
    {
        var element = JDoc.GetTargetElement(node, parser);
        if(element.IsError)
        {
            op.status.AddError("Ошибка", element.Error().Message);
            return false;
        }
        parser.ClearChildrens(element.Value().ElementIndex);   
        var parent = element.Value().WordElement.Element.Parent;
        Option<Paragraph> first = Option.None<Paragraph>();
        Option<Paragraph> last = Option.None<Paragraph>();
        for(int n = 0; n < node.ChangesNodes.Count(); n++)
        {
            Paragraph newPar = node.ChangesNodes[n].WordElement.Element.CloneNode(true) as Paragraph;
            element.Value().WordElement.Element.InsertBeforeSelf(newPar);
            node.ChangesNodes[n].WordElement.Element.CopyParagraphStyle(newPar, source, parser);
            newPar.CopyAllRunsStyles(source, parser);
            newPar.CopyAllImages(source, parser);
            //Выбираем первый и последний параграфы для веделения коментариями
            if(n == 0)
                first = newPar.OptionFromValueOrDefault();
            //if(n == node.ChangesNodes.Count() - 1)
            //на последней итерации итак будет он, не нужно каждый раз условие проверять
            last = newPar.OptionFromValueOrDefault();
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
        element.Value().WordElement.Element.Remove();
        await reload();
        return true;
    }
}