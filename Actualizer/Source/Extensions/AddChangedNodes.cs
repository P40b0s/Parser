using Actualizer.Structure;
using DocumentParser.Elements;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Extensions;

public static class AddChangedNodesEx
{
        /// <summary>
    /// Добавляем в ChangesNodes список параграфов которые излагаются в новой редакции
    /// В певом и последнем абзацах удаляется символ кавычки "
    /// </summary>
    /// <param name="currentParagraph">Параграф - определение (пункт 5 изложить в следующей редации:)</param>
    /// <param name="node">нода для добавления изменений</param>
    public static Result<StructureNode> AddChangedNodes(this ElementStructure currentParagraph, StructureNode node)
    {
        node.ChangesNodes = currentParagraph.TakeTo(t=>!t.IsChange).ToList();
        if( node.ChangesNodes.Count == 0)
            return Result<StructureNode>.Err($"После параграфа #{currentParagraph.WordElement.Text}# не найдено ни одного изменения");
        var first = node.ChangesNodes.TryFirst();
        var last = node.ChangesNodes.TryLast();
        //удаляем кавычки в начале и конце фрагмента
        // var del1 = first.Try(f=> f.WordElement.Element.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>()
        //             .ElementAt(0)
        //             .OfType<DocumentFormat.OpenXml.Wordprocessing.Text>()
        //             .TryFirst()
        //             .Try(r=>r.Text
        //                     .Remove(0, 1))
        //         );
        // var del2 = last.Try(l=> l.WordElement.Element.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>()
        //             .ElementAt(0)
        //             .OfType<DocumentFormat.OpenXml.Wordprocessing.Text>()
        //             .TryLast()
        //             .Try(r=>r.Text
        //                     .Remove(r.Text.Length - 2, 1))
        //         );
        var firstRun = first.Try(f=>f.WordElement.Element
                                    .OfType<DocumentFormat.OpenXml.Wordprocessing.Run>()
                                    .TryElementAt(0)
                                    .Try(s=>s.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>().TryFirst()
                                    .Try(r=> r.Text.Remove(0, 1))));
        // if(first.HasValue)
        // {
        //     foreach(var r in c)
        //     {
        //         var txt = r.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>().FirstOrDefault();
        //         if(txt.Text.IndexOf("\"") != -1)
        //         {
        //             txt.Text = txt.Text.Remove(txt.Text.IndexOf("\""), 1);
        //             break;
        //         }
        //     }
        // }
        //Берем последний ран у которго длинна около 3 знаков, потому что последний ран может быть и пустой
        var lastRun =  last.SelectMany(l=> l.WordElement.Element
                                    .OfType<DocumentFormat.OpenXml.Wordprocessing.Run>()
                                    .TryLast()
                                    .SelectMany(r=> r.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>().TryLast(m=>m.Text.Length >= 3))
                                );
        int iterations = 0;
        if(lastRun.HasValue)
        {
            for(int i = lastRun.Value.Text.Length -1; i >= 0; i--)
            {
                if(lastRun.Value.Text[i] == Char.Parse("\""))
                {
                    lastRun.Value.Text.Remove(i, 1);
                    break;
                }
                if(iterations >=3)
                    return Result<StructureNode>.Err($"Ошибка определения окончания изменения в ране #{lastRun.Value.Text}# параграфа #{currentParagraph.WordElement.Text}#");
                lastRun.Value.Text.Remove(i, 1);
                iterations++;
            }
        }
        // if(last)
        // {
        //     var c = last.WordElement.Element.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>().Reverse();
        //     foreach(var r in c)
        //     {
        //         var txt = r.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>().FirstOrDefault();
        //         if(txt == null)
        //             continue;
        //         if(txt.Text.IndexOf("\".") != -1)
        //         {
        //             txt.Text = txt.Text.Remove(txt.Text.IndexOf("\"."), 2);
        //             break;
        //         }
        //         if(txt.Text.IndexOf("\";") != -1)
        //         {
        //             txt.Text = txt.Text.Remove(txt.Text.IndexOf("\";"), 2);
        //             break;
        //         }
        //     }
        node.ChangesNodes.ForEach(f=>f.IsParsed = true);
        return Result<StructureNode>.Ok(node);
    }
}