using System.Collections.Generic;
using DocumentParser.Workers;
using DocumentParser.DocumentElements;
using Lexer;
using System.Linq;
using System;
using DocumentParser.Elements;
using SettingsWorker.Items;
using SettingsWorker;

namespace DocumentParser.Parsers.Items;


public partial class ItemsParser : LexerBase<ItemTokenType>
{
    public List<ItemParserModel> Items {get;set;} = new List<ItemParserModel>();
    public ItemsParser(WordProcessing extractor)
    {
        this.extractor = extractor;
        settings = extractor.Settings;
        Tokenize(extractor.FullText, new ItemTokensDefinition(settings.TokensDefinitions.ItemTokenDefinitions.TokenDefinitionSettings));
    }
    public int Count {get;set;}
    private WordProcessing extractor {get;}
    public List<Item> Parse(List<ElementStructure> items)
    {
        Items = new List<ItemParserModel>();
        List<Token<ItemTokenType>> currentTokens = new List<Token<ItemTokenType>>();
        Count = 0;
        foreach(var i in items)
        {
            if(!i.IsChange)
            foreach(var t in tokens)
            {
                if((i.StartIndex <= t.StartIndex && (i.StartIndex + i.Length) >= t.EndIndex))
                    currentTokens.Add(t);
            }
        }
        if(currentTokens.Count == 0)
            return null;
        groupItems(currentTokens);
        var hy = getHierarchyItems(currentTokens, items);
        var cast = hy.items.Cast<Item>().ToList();
        //После этого все что не определено на 95% будет абзацами
        //но мы еще не искали по человечески футноты и примечания, с этим будет самая большая сложность....
        //они то как раз и попадут в эти 5% скорее всего
        items.RemoveAll(r=> hy.elements.FirstOrDefault(f=>f.ElementIndex == r.ElementIndex) != null);
        
        UpdateStatus("Поиск списочных элементов");
        var count = currentTokens.Count();
        return cast;
    }
    
    Predicate<ElementStructure> IsItem = i => i.NodeType == NodeType.item0 ||
                i.NodeType == NodeType.item1 ||
                i.NodeType == NodeType.item2 ||
                i.NodeType == NodeType.item3 ||
                i.NodeType == NodeType.item4 ||
                i.NodeType == NodeType.item5 ||
                i.NodeType == NodeType.item6;

    /// <summary>
    /// Группируем итемы по их типу итема
    /// </summary>
    /// <param name="currentTokens"></param>
    private void groupItems(List<Token<ItemTokenType>> currentTokens)
    {
        IEnumerable<IGrouping<ItemTokenType, Token<ItemTokenType>>> gr = currentTokens.GroupBy(g => g.TokenType);
        for(int i = 0; i < gr.Count(); i++)
        {
            var groppedItems = gr.ElementAt(i).ToList();
            var key = gr.ElementAt(i).Key;
            if(i == 0)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item0);
                }
            }
            if(i == 1)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item1);
                }
            }
            if(i == 2)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item2);
                }
            }
            if(i == 3)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item3);
                }
            }
            if(i == 4)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item4);
                }
            }
            if(i == 5)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item5);
                }
            }
            if(i == 6)
            {
                foreach(var itm in groppedItems)
                {
                    extractor.SetElementNode(itm, NodeType.item6);
                }
            }
        }
    }
    /// <summary>
    /// Получаем иерархию итемов
    /// </summary>
    /// <param name="currentTokens"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    private (List<ItemWrapper> items, List<ElementStructure> elements) getHierarchyItems(List<Token<ItemTokenType>> currentTokens, List<ElementStructure> items)
    {
        List<ItemWrapper> readyItems = new List<ItemWrapper>();
        List<ItemWrapper> outItems = new List<ItemWrapper>();
        List<ElementStructure> forDelete = new List<ElementStructure>();
        foreach(var i in items)
        {
            if(IsItem(i))
            {
                var fToken =  currentTokens.FirstOrDefault(f=>(i.StartIndex <= f.StartIndex && (i.StartIndex + i.Length) >= f.EndIndex));
                //Берет больше чем есть в списке итемов надо ограеничить его
                var pars = i.TakeWhile(NodeType.Сноска, IsItem, items);
                var itm = new ItemWrapper(i, pars, fToken, extractor);
                readyItems.Add(itm);
                forDelete.Add(i);
                forDelete.AddRange(pars);
            }
        }
        foreach(var rItem in readyItems)
        {
            if(rItem.nodeType == NodeType.item0)
            {
                outItems.Add(new ItemWrapper(rItem));
            }
            if(rItem.nodeType == NodeType.item1)
            {
                var h = outItems.Last(l=>l.nodeType == NodeType.item0);
                h.AddSubitem(rItem, h);
            }
            if(rItem.nodeType == NodeType.item2)
            {
                var h = outItems.Last(l=>l.nodeType == NodeType.item0);
                var h1 = h.Items.Last(l=>l.nodeType == NodeType.item1) as ItemWrapper;
                h1.AddSubitem(rItem, h1);
            }
            if(rItem.nodeType == NodeType.item3)
            {
                var h = outItems.Last(l=>l.nodeType == NodeType.item0);
                var h1 = h.Items.Last(l=>l.nodeType == NodeType.item1);
                var h2 = h1.Items.Last(l=>l.nodeType == NodeType.item2) as ItemWrapper;
                h2.AddSubitem(rItem, h2);
            }
            if(rItem.nodeType == NodeType.item4)
            {
                var h = outItems.Last(l=>l.nodeType == NodeType.item0);
                var h1 = h.Items.Last(l=>l.nodeType == NodeType.item1);
                var h2 = h1.Items.Last(l=>l.nodeType == NodeType.item2);
                var h3 = h2.Items.Last(l=>l.nodeType == NodeType.item3) as ItemWrapper;
                h3.AddSubitem(rItem, h3);
            }
            if(rItem.nodeType == NodeType.item5)
            {
                var h = outItems.Last(l=>l.nodeType == NodeType.item0);
                var h1 = h.Items.Last(l=>l.nodeType == NodeType.item1);
                var h2 = h1.Items.Last(l=>l.nodeType == NodeType.item2);
                var h3 = h2.Items.Last(l=>l.nodeType == NodeType.item3);
                var h4 = h3.Items.Last(l=>l.nodeType == NodeType.item4) as ItemWrapper;
                h4.AddSubitem(rItem, h4);
            }
            if(rItem.nodeType == NodeType.item6)
            {
                var h = outItems.Last(l=>l.nodeType == NodeType.item0);
                var h1 = h.Items.Last(l=>l.nodeType == NodeType.item1);
                var h2 = h1.Items.Last(l=>l.nodeType == NodeType.item2);
                var h3 = h2.Items.Last(l=>l.nodeType == NodeType.item3);
                var h4 = h3.Items.Last(l=>l.nodeType == NodeType.item4);
                var h5 = h4.Items.Last(l=>l.nodeType == NodeType.item5) as ItemWrapper;
                h5.AddSubitem(rItem, h5);
            }
        }
        return (outItems, forDelete);
    }
}