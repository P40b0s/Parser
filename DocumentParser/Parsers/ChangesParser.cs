using DocumentParser.Workers;
using System.Linq;
using SettingsWorker.Changes;
using SettingsWorker;
using Utils.Extensions;
using DocumentParser.Elements;

namespace DocumentParser.Parsers
{
    public class ChangesParser : LexerBase<ChangesTokenType>
    {
        public ChangesParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        public bool Parse()
        {
            UpdateStatus("Поиск изменений...");
            Tokenize(extractor.FullText, new ChangesTokensDefinition(settings.TokensDefinitions.ChangesTokenDefinitions.TokenDefinitionSettings));
           
            var lastAnchor = tokens.GetLast(l=>l.TokenType == ChangesTokenType.Ancor);
            var count = tokens.Count();
            if(lastAnchor.IsOk)
                count = tokens.IndexOf(lastAnchor.Value());
            int percent = 0;
            foreach(var t in tokens)
            {
                var paragraphBefore = Utils.Result<ElementStructure>.Err("Значение не присвоено!");
                var paragraphAfter = Utils.Result<ElementStructure>.Err("Значение не присвоено!");
                if(t.TokenType == ChangesTokenType.NextIsChange)
                {
                    paragraphBefore = extractor.GetElement(t);
                    bool breakCycle = false;
                    for (int i = t.Position + 1; i < count && !breakCycle; i++)
                    {
                        //var stopToken = tokens[i];
                        if(tokens[i].TokenType == ChangesTokenType.Stop || tokens[i].TokenType == ChangesTokenType.NextIsChange)
                        {
                            //FIXME сделать подсчет статей в изменяющем документе и риентироваться по ним изначально потом по стопам
                            // обнаружен токен статья, если это предыдущий параграф то на нем и заканчиваем изменение
                            // if(tokens[i - 1].TokenType == ChangesTokenType.Article)
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
                    if(paragraphAfter.IsError && lastAnchor.IsOk)
                    {
                        //теперь подключаем анкоры
                        var par = extractor.GetElement(lastAnchor.Value());
                        if(par.IsOk)
                        {
                            paragraphAfter = extractor.GetElement(par.Value().ElementIndex + 1);
                        }
                       
                    }
                }
                if(paragraphAfter.IsOk && paragraphBefore.IsOk)
                {
                    var changePars = extractor.GetElements(paragraphBefore.Value().ElementIndex +1, paragraphAfter.Value().ElementIndex - 1);
                    Count =+ changePars.Count();
                    extractor.SetChange(changePars);
                    foreach (var item in changePars)
                    {
                        if(item.NodeType != NodeType.Таблица)
                            extractor.SetElementNode(item.WordElement.Element, NodeType.Абзац);
                    }
                }
            percent++;
            UpdateStatus("Поиск изменений...", count, percent);
            }
            return true;
        }
    }
}