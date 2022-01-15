using System;
using System.Collections.Generic;
using System.Linq;
using Lexer.Tokenizer;

namespace Lexer
{
    public class Token<T> : ITextIndex
    {
        public Token(bool isLast = false){ IsLast = isLast;}
        public Token(T tokenType)
        {
            TokenType = tokenType;
            Value = string.Empty;
            CustomGroups = new List<GroupMatch>();
        }

        public Token(T tokenType, int startIndex, int endIndex, string value, List<GroupMatch> c_groups, int position, List<Token<T>> tokensList, string converted)
        {
            TokenType = tokenType;
            StartIndex = startIndex;
            EndIndex = endIndex;
            Value = value;
            CustomGroups = c_groups;
            Position = position;
            tokens = tokensList;
            ConvertedValue = converted;
        }
        private List<Token<T>> tokens {get;}
        /// <summary>
        /// Тип токена
        /// </summary>
        /// <value></value>
        public T TokenType { get; }
        /// <summary>
        /// Наименование токена
        /// </summary>
        /// <returns></returns>
        public string TypeName => Enum.GetName(typeof(T), TokenType);
        /// <summary>
        /// Значение токена
        /// </summary>
        /// <value></value>
        public string Value {get;}
        /// <summary>
        /// Начальный индекс токена в текущей строке
        /// </summary>
        /// <value></value>
        public int StartIndex {get;}
        /// <summary>
        /// Конечный индекс токена в текущей строке
        /// </summary>
        /// <value></value>
        public int EndIndex {get;}
        /// <summary>
        /// Длинна занчения токена
        /// </summary>
        /// <value></value>
        public int Length => EndIndex - StartIndex;
        public bool IsLast {get;}
        /// <summary>
        /// Номер позиции в массиве
        /// </summary>
        /// <value></value>
        public int Position {get;}
        /// <summary>
        /// Конвертированное значение токена (если был задан конвертер)
        /// </summary>
        /// <value></value>
        public string ConvertedValue {get;set;}
        /// <summary>
        /// Массив регекс групп (если они были заданны в определениях токенов)
        /// </summary>
        /// <value></value>
        public List<GroupMatch> CustomGroups {get;}

        public override string ToString()
        {
            return Value;
        }

        public TokenResult<T> Next()
        {
            var index = Position + 1;
            if(tokens.Count <= index)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            return new TokenResult<T>(tokens[index]);
        }
        public TokenResult<T> Next(T nextIs) => Next(nextIs, 0);
        /// <summary>
        /// Возврат следующего токена если мы токенизируем не весь текст а только какую то часть
        /// </summary>
        /// <returns></returns>
        public TokenResult<T> NextLocal()
        {
            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].StartIndex == StartIndex)
                {
                    if(i+1 < tokens.Count)
                        return new TokenResult<T>(tokens[i+1]);
                    else return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {i-1}", TokenErrorType.Range));
                        
                }
            }
            return new TokenResult<T>(null, new TokenException($"Этой ошибки не должно существовыть! Токен не смог найти сам себя у себя в массиве! парадокс) {getTokenName(this.TokenType)}", TokenErrorType.WrongFound));
        }
        public bool LastIs(T last)
        {
            var l = tokens.LastOrDefault();
            if(l == null)
                return false;
            return l.TokenType.Equals(last);
        }
            
        /// <summary>
        /// Следующий токен, используется когда нужно пропустить некоторое количество токенов, и мы знаем это количество.
        /// </summary>
        /// <param name="nextIs">Искомый токен</param>
        /// <param name="skip">Количество пропускаемых токенов</param>
        /// <returns></returns>
        public TokenResult<T> Next(T nextIs, int skip)
        {
            var index = Position + 1 + skip;
            if(tokens.Count <= index)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            if(tokens[index].TokenType.Equals(nextIs))
            {
                return new TokenResult<T>(tokens[index]);
            } 
            else
            {
                var found = tokens[index].TokenType;
                return new TokenResult<T>(null, new TokenException($"На позиции {index} вместо ожидаемого {getTokenName(nextIs)} обнаружен {getTokenName(found)}", TokenErrorType.WrongFound));
            }
        }
         /// <summary>
        /// Поиск токенов из перечня предиката
        /// </summary>
        /// <param name="nextIs">Искомый токен</param>
        /// <param name="skip">Количество пропускаемых токенов</param>
        /// <returns></returns>
        public TokenResult<T> FindForward(Predicate<Token<T>> oneOf, int maxDeep = 0)
        {
            var index = Position + 1;
            if(tokens.Count == index)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index + i;
                if(tokens.Count > index)
                {
                    if(oneOf.Invoke(tokens[index]))
                        return new TokenResult<T>(tokens[index]);
                }
                else return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));     
            }
            return new TokenResult<T>(null, new TokenException($"На позициях {index} - {index + maxDeep} токены не найдены", TokenErrorType.NotFound));
        }
         /// <summary>
        /// Ищем то незнаем что, занем что не ищем) Берем только тот токен что не значиться в предикате
        /// </summary>
        /// <param name="ignore">Токены которые проходим</param>
        /// <returns></returns>
        public TokenResult<T> FindForward(Predicate<Token<T>> ignore)
        {
            var index = Position + 1;
            if(tokens.Count == index)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            for (int i = 0; i < tokens.Count; i++)
            {
                index = index + i;
                if(tokens.Count > index)
                {
                    if(ignore.Invoke(tokens[index]))
                        return new TokenResult<T>(tokens[index]);
                }
                else return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));     
            }
            return new TokenResult<T>(null, new TokenException($"Токены не найдены", TokenErrorType.NotFound));
        }
        public TokenResult<T> FindForward(T searchedToken, int maxDeep = 0)
        {
            var index = Position + 1;
            if(tokens.Count == index)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index + i;
                if(tokens.Count > index)
                {
                    if(tokens[index].TokenType.Equals(searchedToken))
                        return new TokenResult<T>(tokens[index]);
                }
                else return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));     
            }
             return new TokenResult<T>(null, new TokenException($"На позициях {index} - {index + maxDeep} токен {getTokenName(searchedToken)} не найден", TokenErrorType.NotFound));
        }
        /// <summary>
        /// Получаем массив искомых токенов, метод прерывается если встречается токен отличный от искомых 
        /// </summary>
        /// <param name="searchedToken">Искомый токен</param>
        /// <param name="withSelf">Осуществлять поиск включая искомый токен</param>
        /// <returns></returns>
        // public List<Token<T>> TakeForward(T searchedToken, bool withSelf = false)
        // {
        //     var index = withSelf ? Position : Position + 1;
        //     var result = new List<Token<T>>();
        //     while(tokens.Count > index && tokens[index].TokenType.Equals(searchedToken))
        //     {
        //         result.Add(tokens[index]);
        //         index++;
        //     }
        //     return result;
        // }

        /// <summary>
        /// Получаем массив искомых токенов, метод прерывается если встречается токен отличный от искомых 
        /// </summary>
        /// <param name="searchedToken">Искомый токен</param>
        ///  /// <param name="withSelf">Осуществлять поиск включая искомый токен</param>
        /// <returns></returns>
        public List<Token<T>> FindForwardMany(Predicate<Token<T>> oneOf, bool withSelf = false)
        {
            var index = withSelf ? Position : Position + 1;
            var result = new List<Token<T>>();
            while(tokens.Count > index && oneOf.Invoke(tokens[index]))
            {
                result.Add(tokens[index]);
                index++;
            }
            return result;
        }
        /// <summary>
        /// Поиск токена в обратном направлении (назад)
        /// </summary>
        /// <param name="searchedToken">Искомый токен</param>
        /// <param name="maxDeep">Максимальная глубина поиска (максимальное количество пропускаемый токенов, пока не втретиться нужный)</param>
        /// <returns></returns>
        public TokenResult<T> FindBackward(T searchedToken, int maxDeep = 0)
        {
            var index = Position - 1;
            if(index == 0)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index - i;
                if(index >= 0)
                {
                    if(tokens[index].TokenType.Equals(searchedToken))
                        return new TokenResult<T>(tokens[index]);
                }
                else
                    return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));     
            }
            return new TokenResult<T>(null, new TokenException($"На позициях {index- maxDeep} - {index} токен {getTokenName(searchedToken)} не найден", TokenErrorType.NotFound));
                    
        }
        /// <summary>
        /// Получаем массив искомых токенов, метод прерывается если встречается токен отличный от искомых 
        /// </summary>
        /// <param name="searchedToken">Искомый токен</param>
        ///  /// <param name="withSelf">Осуществлять поиск включая искомый токен</param>
        /// <returns></returns>
        public List<Token<T>> FindBackwardMany(Predicate<Token<T>> oneOf, bool withSelf = false)
        {
            var index = withSelf ? Position : Position - 1;
            var result = new List<Token<T>>();
            while(index >= 0 && oneOf.Invoke(tokens[index]))
            {
                result.Add(tokens[index]);
                index--;
            }
            return result;
        }
          /// <summary>
        /// Берет искомый токен вверх по массиву, останавливается на любых токенах кроме тех что заданы skip.
        /// </summary>
        /// <param name="take">Какой тип токена будем брать</param>
        /// <param name="skip">Токен который будем пропускать, если попадется токен отличный от заданных то метод остановиться</param>
        /// <returns></returns>
        public List<Token<T>> TakeWhileBackward(Predicate<Token<T>> take, Predicate<Token<T>> skip)
        {
            var index = Position - 1;
            if(index == 0)
                return new List<Token<T>>();
            var result = new List<Token<T>>();
            for(int i = index; i >= 0; i--)
            {
                if(skip.Invoke(tokens[index]))
                {
                    index--;
                    continue;
                }
                if(take.Invoke(tokens[index]))
                    result.Add(tokens[index]);
                else break;
                index--;
            }
            return result;
        }
        /// <summary>
        /// Поиск токена в обратном направлении (назад) по предикату
        /// </summary>
        /// <param name="searchedToken">Предикат</param>
        /// <param name="maxDeep">Максимальная глубина поиска (максимальное количество пропускаемый токенов, пока не втретиться нужный)</param>
        /// <returns></returns>
        public TokenResult<T> FindBackward(Predicate<Token<T>> oneOf, int maxDeep = 0)
        {
            var index = Position - 1;
            if(index == 0)
                return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index - i;
                if(index >= 0)
                {
                    if(oneOf.Invoke(tokens[index]))
                        return new TokenResult<T>(tokens[index]);
                }
                else
                    return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));     
            }
            return new TokenResult<T>(null, new TokenException($"На позициях {index- maxDeep} - {index} токены не найдены", TokenErrorType.NotFound));
        }
       
        public TokenResult<T> Before()
        {
            var index = Position - 1;
            if(index < 0)
                return new TokenResult<T>(tokens[index]);
            return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
        }
        public TokenResult<T> Before(T beforeIs) => Before(beforeIs, 0);
         /// <summary>
        /// Возврат предыдущего токена если мы токенизируем не весь текст а только какую то часть
        /// </summary>
        /// <returns></returns>
        public TokenResult<T> BeforeLocal()
        {
            ///Такой перебор быстрее чем запрос linq
            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].StartIndex == StartIndex)
                {
                        if(i-1 >= 0)
                            return new TokenResult<T>(tokens[i-1]);
                        else return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {i-1}", TokenErrorType.Range));
                }
            }
            return new TokenResult<T>(null, new TokenException($"Токен не смог найти сам себя у себя в массиве! парадокс) {getTokenName(this.TokenType)}", TokenErrorType.WrongFound));
        }

        /// <summary>
        /// Предыдущий токен, используется когда нужно пропустить некоторое количество токенов, и мы знаем это количество.
        /// </summary>
        /// <param name="nextIs">Искомый токен</param>
        /// <param name="skip">Количество пропускаемых токенов</param>
        /// <returns></returns>
        public TokenResult<T> Before(T beforeIs, int skip)
        {
            var index = Position - 1 - skip;
            if(index >= 0)
            {
                if(tokens[index].TokenType.Equals(beforeIs))
                    return new TokenResult<T>(tokens[index]);
                else
                {
                    var found = tokens[index].TokenType;
                    return new TokenResult<T>(null, new TokenException($"На позиции {index} вместо ожидаемого {getTokenName(beforeIs)} обнаружен {getTokenName(found)}", TokenErrorType.WrongFound));
                }
            }  
            else return new TokenResult<T>(null, new TokenException($"Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range));
        }
       

        public Token<T> Clone()
        {
            return new Token<T>(TokenType, StartIndex, EndIndex, Value, CustomGroups, Position, tokens, ConvertedValue);
        }

        public TokenResult<T> ToResult()
        {
            return new TokenResult<T>(this);
        }


        private string getTokenName(T token) => Enum.GetName(typeof(T), token);
    }
}