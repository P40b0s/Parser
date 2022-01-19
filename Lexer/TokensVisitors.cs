using System;
using System.Collections.Generic;
using System.Linq;
namespace Lexer
{
    public static class TokensVisitors
    {
       
        public static List<T> TakeTokenWhile<T,E>(this List<T> tokens, T fromToken, E searchToken, bool isReverse = false) 
        where T : Token<E>
        where E : Enum
        {
            int modif = 1;
            List<T> nodes = new List<T>();
            if(isReverse)
            {
                var curr = tokens.IndexOf(fromToken);
                while(tokens.Count > curr - modif && tokens[curr - modif].TokenType.Equals(searchToken))
                {
                    nodes.Add(tokens[curr + modif]);
                    modif--;
                }
            }
            else
            {
                var curr = tokens.IndexOf(fromToken);
                while(tokens.Count > curr + modif && tokens[curr + modif].TokenType.Equals(searchToken))
                {
                    nodes.Add(tokens[curr + modif]);
                    modif++;
                }
            }
            return nodes;
        }
        
        public static T GetToken<T,E>(this List<T> tokens, T afterToken, E searchToken, int deep = 1) 
        where T : Token<E>
        where E : Enum
        {
            T returnToken = afterToken;
            int modif = 1;
            var curr = tokens.IndexOf(afterToken);
            while(tokens.Count > curr + modif && !returnToken.TokenType.Equals(searchToken) && modif <= deep)
            {
                returnToken = tokens[curr + modif];
                modif++;
            }
            return returnToken;
        }

        public static T GetToken<T,E>(this List<T> tokens, E searchToken, int deep) 
        where T : Token<E>
        where E : Enum
        {
            return tokens.FirstOrDefault(f=> f.TokenType.Equals(searchToken) && f.Position <= deep);
        }
       

        
          /// <summary>
        /// Удаление токена из списка (удаляем после успешной обработки этого токена)
        /// </summary>
        /// <param name="tokens"></param>
        public static bool DropToken<T,E>(this List<T> tokens, T token ) where T: Token<E> => tokens.Remove(token);
    }
}