using System;
using System.Collections.Generic;
using System.Linq;
using Lexer.Tokenizer;
using System.Runtime.CompilerServices;
namespace Lexer
{
    public class Token<T> : ITextIndex
    {
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
                return new TokenResult<T>(null, outOfRangeException(index));
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
                    else return new TokenResult<T>(null, outOfRangeException(i-1));
                        
                }
            }
            return new TokenResult<T>(null, neverException());
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
                return new TokenResult<T>(null, outOfRangeException(index));
            if(tokens[index].TokenType.Equals(nextIs))
            {
                return new TokenResult<T>(tokens[index]);
            } 
            else
            {
                var found = tokens[index].TokenType;
                return new TokenResult<T>(null, wrongFoundException(index, nextIs, found));
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
                return new TokenResult<T>(null, outOfRangeException(index));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index + i;
                if(tokens.Count > index)
                {
                    if(oneOf.Invoke(tokens[index]))
                        return new TokenResult<T>(tokens[index]);
                }
                else return new TokenResult<T>(null, outOfRangeException(index));     
            }
            return new TokenResult<T>(null, notFountOnPositionException(index, index + maxDeep));
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
                return new TokenResult<T>(null, outOfRangeException(index));
            for (int i = 0; i < tokens.Count; i++)
            {
                index = index + i;
                if(tokens.Count > index)
                {
                    if(ignore.Invoke(tokens[index]))
                        return new TokenResult<T>(tokens[index]);
                }
                else return new TokenResult<T>(null, outOfRangeException(index));     
            }
            return new TokenResult<T>(null, customException("Не найдено ни одного токена"));
        }

        //TODO На потом:
        // passing expression, accessing value
        // public static IEnumerable<T> Filter<T>(this IEnumerable<T> collection, Expression<Func<T, bool>> predicate)
        // {
        //     var binExpr = predicate.Body as BinaryExpression;
        //     var value = binExpr.Right;

        //     var func = predicate.Compile();
        //     return collection.Where(func);
        // }
        public TokenResult<T> FindForward(T searchedToken, int maxDeep = 0)
        {
            var index = Position + 1;
            if(tokens.Count == index)
                return new TokenResult<T>(null, outOfRangeException(index));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index + i;
                if(tokens.Count > index)
                {
                    if(tokens[index].TokenType.Equals(searchedToken))
                        return new TokenResult<T>(tokens[index]);
                }
                else return new TokenResult<T>(null, outOfRangeException(index));     
            }
             return new TokenResult<T>(null, notFountOnPositionException(index, index + maxDeep, searchedToken));
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
                return new TokenResult<T>(null, outOfRangeException(index));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index - i;
                if(index >= 0)
                {
                    if(tokens[index].TokenType.Equals(searchedToken))
                        return new TokenResult<T>(tokens[index]);
                }
                else
                    return new TokenResult<T>(null, outOfRangeException(index));     
            }
            return new TokenResult<T>(null, notFountOnPositionException(index- maxDeep, index, searchedToken));
                    
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
                return new TokenResult<T>(null, outOfRangeException(index));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index - i;
                if(index >= 0)
                {
                    if(oneOf.Invoke(tokens[index]))
                        return new TokenResult<T>(tokens[index]);
                }
                else
                    return new TokenResult<T>(null, outOfRangeException(index));     
            }
            return new TokenResult<T>(null, notFountOnPositionException(index- maxDeep, index));
        }
       
        public TokenResult<T> Before()
        {
            var index = Position - 1;
            if(index < 0)
                return new TokenResult<T>(tokens[index]);
            return new TokenResult<T>(null, outOfRangeException(index));
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
                        else return new TokenResult<T>(null, outOfRangeException(i-1));
                }
            }
            return new TokenResult<T>(null, neverException());
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
                    return new TokenResult<T>(null, wrongFoundException(index, beforeIs, found));
                }
            }  
            else return new TokenResult<T>(null, outOfRangeException(index));
        }
       

        public Token<T> Clone()
        {
            return new Token<T>(TokenType, StartIndex, EndIndex, Value, CustomGroups, Position, tokens, ConvertedValue);
        }

        public TokenResult<T> ToResult()
        {
            return new TokenResult<T>(this);
        }
        public bool HaveCustomGroups => this.CustomGroups.Count > 0;
        public TokenResult<T> GetDate()
        {
            if(this.CustomGroups.Count != 3)
                return new TokenResult<T>(null, customException($"Группа данного токена не совпадает с сигнатурой даты: {this.Value}"));
            var date = getDate(this.CustomGroups[0].Value, this.CustomGroups[1].Value, this.CustomGroups[2].Value);
            if(date == null)
                return new TokenResult<T>(null, customException($"Ошибка преобразования даты: {this.Value}"));
            return new TokenResult<T>(this, date);
        }


        private string getTokenName(T token) => Enum.GetName(typeof(T), token);

        private TokenException customException(string message, [CallerMemberName]string callerMemberName = null) =>
            new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - {message}", TokenErrorType.NotFound);
        private TokenException outOfRangeException(int index, [CallerMemberName]string callerMemberName = null) =>
            new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", TokenErrorType.Range);
        
        private TokenException wrongFoundException(int index, T waitingToken, T foundedToken, [CallerMemberName]string callerMemberName = null) =>
         new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - на позиции {index} вместо ожидаемого {getTokenName(waitingToken)} обнаружен {getTokenName(foundedToken)}", TokenErrorType.WrongFound);
        
        private TokenException notFountOnPositionException(int startIndex, int endIndex, [CallerMemberName]string callerMemberName = null) => 
            new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - на позициях {startIndex} - {endIndex} токены не найдены");
        private TokenException notFountOnPositionException(int startIndex, int endIndex, T token, [CallerMemberName]string callerMemberName = null) => 
            new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - на позициях {startIndex} - {endIndex} токен {getTokenName(token)} не найден");
        private TokenException neverException([CallerMemberName]string callerMemberName = null) =>
            new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - токен не смог найти сам себя у себя в массиве! парадокс)", TokenErrorType.WrongFound);
        private string currentToken => $"При операции с токеном {TypeName} на позиции {Position}";

        
         /// <summary>
        /// Получение даты из строки
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="month">Месяц</param>
        /// <param name="year">Год</param>
        /// <returns></returns>
        private DateTime? getDate(string date, string month, string year)
        {
            System.Text.RegularExpressions.Regex isDigit = new System.Text.RegularExpressions.Regex("\\d+");
            bool monthIsWord = !isDigit.IsMatch(month);
            DateTime? signDate = null;
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(year) || string.IsNullOrEmpty(month))
                return signDate;
            try
            {
                var y = int.Parse(year);
                var m = monthIsWord ? MonthToNumberConverter(month) : int.Parse(month);
                var d = int.Parse(date);
                signDate = new DateTime(y, m, d);
                return signDate;
            }
            catch
            {
                return signDate;
            }
        }

        /// <summary>
        /// Конвертация текущего месяца в его номер
        /// </summary>
        /// <param name="month">Месяц в виде: января февраля марта итд...</param>
        /// <returns></returns>
        int MonthToNumberConverter(string month)
        {
            switch (month.ToLower().Trim())
            {
                default:
                case "января":
                    return 1;
                case "февраля":
                    return 2;
                case "марта":
                    return 3;
                case "апреля":
                    return 4;
                case "мая":
                    return 5;
                case "июня":
                    return 6;
                case "июля":
                    return 7;
                case "августа":
                    return 8;
                case "сентября":
                    return 9;
                case "октября":
                    return 10;
                case "ноября":
                    return 11;
                case "декабря":
                    return 12;
            }
        }
    }
}