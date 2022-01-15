using System.Collections.Generic;

namespace Lexer.Tokenizer
{
    public interface ITokenizer<T>
    {
        IEnumerable<Token<T>> Tokenize(string query, List<TokenDefinition<T>> definitions);
    }
}