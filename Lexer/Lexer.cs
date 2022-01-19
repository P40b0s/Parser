using Lexer.Tokenizer;
using System.Collections.Generic;
using System.Linq;

namespace Lexer
{
    public interface ILexerService<T> : ITokenizer<T>
    {

    }
   
    public class Lexer<T> : ILexerService<T>
    {
        public string UploadDirectory { get; }    
        public string RootDirectory { get; }
        public bool UploadInProgress { get; set; } = false;
       
        public Lexer(){}

        private List<Token<T>> tokens = new List<Token<T>>();
        private List<TokenDefinition<T>> _tokenDefinitions;
        public IEnumerable<Token<T>> Tokenize(string lqlText, List<TokenDefinition<T>> definitions)
        {
            //lqlText = NormalizeString(lqlText);
            _tokenDefinitions = definitions;
            var tokenMatches = FindTokenMatches(lqlText);
            int position = 0;
            var groupedByIndex = tokenMatches.GroupBy(x => x.StartIndex)
                .OrderBy(x => x.Key)
                .ToList();

            TokenMatch<T> lastMatch = null;
            for (int i = 0; i < groupedByIndex.Count; i++)
            {
                var bestMatch = groupedByIndex[i].OrderBy(x => x.Precedence).First();
                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                    continue;
                //добавляем ссылку на основной массив в токен
                var t = new Token<T>(bestMatch.TokenType, bestMatch.StartIndex, bestMatch.EndIndex, bestMatch.Value, bestMatch.Groups, position, tokens, bestMatch.Converted);
                tokens.Add(t); 
                position++;
                yield return t;
                lastMatch = bestMatch;
            }

            //yield return new Token<T>(true);
        }

        private List<TokenMatch<T>> FindTokenMatches(string lqlText)
        {
            var tokenMatches = new List<TokenMatch<T>>();

            foreach (var tokenDefinition in _tokenDefinitions)
                tokenMatches.AddRange(tokenDefinition.FindMatches(lqlText).ToList());

            return tokenMatches;
        }
    }
       
}
