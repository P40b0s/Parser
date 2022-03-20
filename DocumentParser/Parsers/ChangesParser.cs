using DocumentParser.Workers;
using System.Linq;
using SettingsWorker.Changes;
using SettingsWorker;
using Utils.Extensions;
using DocumentParser.Elements;
using System.Threading.Tasks;

namespace DocumentParser.Parsers
{
    public class ChangesParser : LexerBase<ChangesTokenType>
    {
        public ChangesParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
        }
        /// <summary>
        /// Количество параграфов которые являются изменениями
        /// </summary>
        /// <value></value>
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
                    if(paragraphBefore.IsError)
                        return AddError($"Ошибка извлечения начального параграфа изменения, токен: {t.Value} позиция: {t.StartIndex}");
                    var nextToken = t.FindForward(f=>f.TokenType == ChangesTokenType.Stop || f.TokenType == ChangesTokenType.NextIsChange, 2);
                    if(nextToken.IsError)
                    {
                        nextToken = t.FindForward(f=>f.TokenType == ChangesTokenType.Ancor);
                        if(nextToken.IsError)
                            return AddError($"Не найдены границы окончания изменения {paragraphBefore.Value().WordElement.Text}, изменение начинается с позиции {paragraphBefore.Value().StartIndex}");
                    }
                    paragraphAfter = extractor.GetElement(nextToken.Value());
                    if(paragraphAfter.IsError)
                        return AddError($"Ошибка извлечения конечного параграфа изменения, токен: {nextToken.Value()} позиция: {nextToken.Value().StartIndex}");
                    var changes = paragraphBefore.Value().TakeTo(t=>t.ElementIndex == paragraphAfter.Value().ElementIndex);
                    Count =+ changes.Count();
                    extractor.SetChange(changes);
                }
            percent++;
            UpdateStatus("Поиск изменений...", count, percent);
            }
            return true;
            
        }
    }
}