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

public static class ReplaceWordEx
{
    /// <summary>
    /// Операция замены слова
    /// </summary>
    /// <param name="node">Нода с изменением</param>
    /// <param name="targetDocumentJObject">Представлние JObject для документа в который будут вноситься изменения</param>
    /// <param name="parser">Парсер документа в который вносятся изменения</param>
    public static bool ReplaceWord(this Operation op, Parser parser, JObject JDoc, StructureNode node, SourceDocumentParserResult source)
    {
        var element = JDoc.GetTargetElement(node, parser);
        if(element.IsError)
        {
            op.status.AddError("Ошибка", element.Error().Message);
            return false;
        }
        foreach(var change in node.WordsOperations)
        {
            //TODO разделить для каждой операции! само собой пришло)
            if(change.StructureOperation == OperationType.ReplaceWords)
            {
                var start = element.Value().WordElement.Text.IndexOf(change.SourceText);
                var run = element.Value().DeleteText(start, start + change.SourceText.Length);
                var text = run.Descendants<Text>().First();
                var ws = char.IsWhiteSpace(text.Text.Last());
                if(ws)
                {
                    text.Text = text.Text.Remove(text.Text.Length -1);
                    start--;
                }
                var sourceRun = node.Element.GetRun(node.Element.WordElement.Text.IndexOf(change.TargetText));
                var newRun = sourceRun.CloneNode(true);
                
                if(ws)
                    (newRun as Run).GetFirstChild<Text>().Text = '\u0020' + change.TargetText;
                else
                    (newRun as Run).GetFirstChild<Text>().Text = change.TargetText;
                //если заменяемый текст находился в конце рана то просто добавляем его в новый текст
                if(run.Descendants<Text>().Sum(s=>s.Text.Length) == start)
                {
                    run.InsertAfterSelf(newRun);
                    var comment = new CommentModel(parser.word.Document.MainDocumentPart,
                                                    "Скайнетов К.О.",
                                                    "абырвалГ" + source.SourceDocumentRequisites.GetCommentLabel(),
                                                    run,
                                                    newRun);
                    comment.AddRunComment();
                }
                //если в середине то разбиваем на несколько ранов
                else
                {
                    var splited = run.Split( start, newRun);
                    splited.Reverse();
                    foreach(var r in splited)
                    {
                        run.InsertAfterSelf(r);
                    }
                    //удаляем оригинальный ран
                    run.Remove();
                }
                var tt = "";
            }
        }
        
        return true;
    }
}