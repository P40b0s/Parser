using System.Collections.Generic;
using DocumentParser.Workers;
using DocumentParser.DocumentElements;
using DocumentParser.TokensDefinitions;
using Lexer;
using System.Linq;
using System;
using Utils.Extensions;

namespace DocumentParser.Parsers.Items
{
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
    public class ItemsParser : ParserBase<ItemToken>
    {
        public List<ItemParserModel> Items {get;set;} = new List<ItemParserModel>();
        public ItemsParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            tokens = lexer.Tokenize(extractor.FullText, new ItemTokensDefinition()).ToList();
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        public List<Item> Parse(List<ElementStructure> items)
        {
            Items = new List<ItemParserModel>();
            List<Token<ItemToken>> currentTokens = new List<Token<ItemToken>>();
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
            
            Status("Поиск списочных элементов");
            var percentage = 0;
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
        private void groupItems(List<Token<ItemToken>> currentTokens)
        {
            IEnumerable<IGrouping<ItemToken, Token<ItemToken>>> gr = currentTokens.GroupBy(g => g.TokenType);
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
        private (List<ItemWrapper> items, List<ElementStructure> elements) getHierarchyItems(List<Token<ItemToken>> currentTokens, List<ElementStructure> items)
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
}