using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lexer.Tokenizer
{
    public class TokenDefinition<T>
    {
        private Regex _regex;
        private readonly T _returnsToken;
        private readonly int _precedence;
        private readonly Dictionary<string, string> _converter;

        public TokenDefinition(T returnsToken, string regexPattern, int precedence, Dictionary<string, string> converter = null)
        {
            _regex = new Regex(regexPattern, RegexOptions.IgnoreCase|RegexOptions.Compiled);
            _returnsToken = returnsToken;
            _precedence = precedence;
            _converter = converter;
        }

        public IEnumerable<TokenMatch<T>> FindMatches(string inputString)
        {
            var matches = _regex.Matches(inputString);
            for(int i=0; i<matches.Count; i++)
            {
                yield return new TokenMatch<T>()
                {
                    //TODO Сделать добавление групп в массив наряду с выводом просто данных

                    StartIndex = matches[i].Index,
                    EndIndex = matches[i].Index + matches[i].Length,
                    TokenType = _returnsToken,
                    Value = matches[i].Value,
                    Groups = _ReturnGroups(matches[i]),
                    Precedence = _precedence,
                    Converted = (_converter != null && _converter.ContainsKey(matches[i].Value)) ? _converter[matches[i].Value] : null
                };
            }
        }
        Regex _noCustomGroup => new Regex("^\\d{1,2}");
        private List<GroupMatch> _ReturnGroups(Match m)
        {
            List<GroupMatch> l = new List<GroupMatch>();
            if(m.Groups.Count > 0)
            {
                for (int i = 0; i < m.Groups.Count; i++)
                {
                    if(!_noCustomGroup.IsMatch(m.Groups[i].Name))
                        l.Add(new GroupMatch(m.Groups[i].Name, m.Groups[i].Value, m.Groups[i].Index, m.Groups[i].Length ));
                }        
            }
            return l;
        }
    }

    public class ListTokensDefinition<T> : List<TokenDefinition<T>>
    {
        protected void AddToken(T token, string regex, int prec = 1, Dictionary<string, string> converter = null) => Add(new TokenDefinition<T>(token, regex, prec, converter));

    }
}