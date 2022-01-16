using System.Collections.Generic;
using DocumentParser.Workers;
using DocumentParser.DocumentElements;
using DocumentParser.TokensDefinitions;
using Lexer;
using Utils.Extensions;
namespace DocumentParser.Parsers.Items;
/// <summary>
/// Класс обертка для итемов, здесь мы получем номер и постфикс итема, на сколько нужно отрезать абзац
/// 
/// </summary>
public class ItemWrapper : Item
{
    ItemWrapper Parent {get;set;}
    int CuttingLenght {get;set;}
    Token<ItemToken> FirstItemToken {get;set;}
    ElementStructure First {get;set;}
    public void AddSubitem(ItemWrapper item, ItemWrapper parent)
    {
        Parent = parent;
        if(Items == null)
            Items = new List<Item>();
        Items.Add(new ItemWrapper(item));
    }
    /// <summary>
    /// Корректировка начала абзаца при обрезке (например обрезка номера итема)
    /// </summary>
    /// <value></value>
    
    public ItemWrapper(ItemWrapper item)
    {
        nodeType = item.nodeType;
        Number = item.Number;
        Postfix = item.Postfix;
        CuttingLenght = item.CuttingLenght;
        FirstItemToken = item.FirstItemToken;
        First = item.First;
        //Elements = item.Elements;
        Indents = item.Indents;
        ElementIndex = item.ElementIndex;
    }
    public ItemWrapper(ElementStructure first, IEnumerable<ElementStructure> paragraps, Token<ItemToken> firstItemToken, WordProcessing extractor)
    {
        
        First = first;
        FirstItemToken = firstItemToken;
        ElementIndex = first.ElementIndex;
        nodeType = First.NodeType;
        Number = extractor.GetUnicodeString(firstItemToken.CustomGroups[0]);
        Postfix = extractor.GetUnicodeString(firstItemToken.CustomGroups[1]);
        CuttingLenght = FirstItemToken.Length;
        extractor.SetElementNode(First, NodeType.Абзац);
        //Первый параграф мы не добавляем в спмисок, от него еще надо отрезать номер.
        //скорректировать значение отрезаного номера для гиперссылок и сносок если они там есть
        //First.HyperTextInfo.hyperLinks[0].LinkStartIndex
        //Потому что это значение будет тогда неверно
        //Это уже сделано внутри Indent
        // foreach(var l in First.HyperTextInfo.hyperLinks)
        // {
        //     l.LinkStartIndex = l.LinkStartIndex - CuttingLenght;
        // }
        var firstIndent = new Indent(First.ParagraphProperties,
                                            First.ElementIndex,
                                            First.WordElement.Text.GetHash(),
                                            First.WordElement.RunWrapper.GetCustRuns(CuttingLenght),
                                            First.MetaInfo,
                                            First.HyperTextInfo,
                                            First.Comment,
                                            First.FootNoteInfo,
                                            null,
                                            First.IsChange,
                                            CuttingLenght);
        Indents.Add(firstIndent);
        foreach (var p in  paragraps)
        {
                extractor.SetElementNode(p, NodeType.Абзац);
                Indents.Add(new Indent(p.ParagraphProperties,
                                                p.ElementIndex,
                                                p.WordElement.Text.GetHash(),
                                                p.WordElement.RunWrapper.GetCustRuns(),
                                                p.MetaInfo,
                                                p.HyperTextInfo,
                                                p.Comment,
                                                p.FootNoteInfo,
                                                null,
                                                p.IsChange));
                //Elements.Add(p);
        }
    }
    //List<ElementStructure> Elements {get;} = new List<ElementStructure>();
}