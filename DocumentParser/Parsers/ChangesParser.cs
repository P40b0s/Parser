using DocumentParser.Workers;
using DocumentParser.TokensDefinitions;
using System.Linq;

namespace DocumentParser.Parsers
{
    public class ChangesParser : ParserBase<ChangesIndentsToken>
    {
        public ChangesParser(WordProcessing extractor)
        {
            this.extractor = extractor;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        public bool Parse()
        {
            Status("Поиск изменений...");
            tokens = lexer.Tokenize(extractor.FullText, new ChangesIndentsTokensDefinition()).ToList();
            var lastAnchor = tokens.LastOrDefault(l=>l.TokenType == ChangesIndentsToken.Ancor);
            var count = tokens.Count();
            if(lastAnchor!= null)
                count = tokens.IndexOf(lastAnchor);
            int percent = 0;
            foreach(var t in tokens)
            {
                ElementStructure paragraphBefore = null;
                ElementStructure paragraphAfter = null;
                if(t.TokenType == ChangesIndentsToken.NextIsChange)
                {
                    paragraphBefore = extractor.GetElement(t);
                    bool breakCycle = false;
                    for (int i = t.Position + 1; i < count && !breakCycle; i++)
                    {
                        //var stopToken = tokens[i];
                        if(tokens[i].TokenType == ChangesIndentsToken.Stop || tokens[i].TokenType == ChangesIndentsToken.NextIsChange)
                        {
                            //FIXME сделать подсчет статей в изменяющем документе и риентироваться по ним изначально потом по стопам
                            // обнаружен токен статья, если это предыдущий параграф то на нем и заканчиваем изменение
                            // if(tokens[i - 1].TokenType == ChangesIndentsToken.Article)
                            // {
                            //     var mbArticle = extractor.GetElements(tokens[i-1]).FirstOrDefault();
                            //     var par = extractor.GetElements(tokens[i]).FirstOrDefault();
                            //     if(mbArticle.ElementIndex == par.ElementIndex -1)
                            //         paragraphAfter = mbArticle;
                            //     else paragraphAfter = par;
                            // }
                            // else
                            paragraphAfter = extractor.GetElement(tokens[i]);
                            breakCycle = true;
                        }
                    }
                    if(paragraphAfter == null)
                    {
                        //теперь подключаем анкоры
                        var par = extractor.GetElement(lastAnchor);
                        paragraphAfter = extractor.GetElement(par.ElementIndex + 1);
                    }
                }
                if(paragraphAfter != null && paragraphBefore != null)
                {
                    var changePars = extractor.GetElements(paragraphBefore.ElementIndex +1, paragraphAfter.ElementIndex - 1);
                    Count =+ changePars.Count();
                    extractor.SetChange(changePars);
                    foreach (var item in changePars)
                    {
                        if(item.NodeType != NodeType.Таблица)
                            extractor.SetElementNode(item.WordElement.Element, NodeType.Абзац);
                    }
                }
            percent++;
            Percentage("Поиск изменений...", count, percent);
            }
            return true;
        }
    }
}