using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
namespace Lexer;

public partial class Token<T>
{
     /// <summary>
        /// Поиск токена в обратном направлении (назад)
        /// </summary>
        /// <param name="searchedToken">Искомый токен</param>
        /// <param name="maxDeep">Максимальная глубина поиска (максимальное количество пропускаемый токенов, пока не втретиться нужный)</param>
        /// <returns></returns>
        public Result<Token<T>> FindBackward(T searchedToken, int maxDeep = 0)
        {
            var index = Position - 1;
            if(index == 0)
                return Result<Token<T>>.Err(outOfRangeException(index));
            for (int i = 0; i <= maxDeep; i++)
            {
                index = index - i;
                if(index >= 0)
                {
                    if(tokens[index].TokenType.Equals(searchedToken))
                        return Result<Token<T>>.Ok(tokens[index]);
                }
                else
                    return Result<Token<T>>.Err(outOfRangeException(index));     
            }
            return Result<Token<T>>.Err(notFountOnPositionException(index- maxDeep, index, searchedToken));  
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
            while(index >= 0 && oneOf(tokens[index]))
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
                if(skip(tokens[index]))
                {
                    index--;
                    continue;
                }
                if(take(tokens[index]))
                    result.Add(tokens[index]);
                else break;
                index--;
            }
            return result;
        }
        /// <summary>
        /// ТЕСТ!!!
        /// Поиск токена в обратном направлении (назад) по предикату
        /// </summary>
        /// <param name="searchedToken">Предикат</param>
        /// <param name="maxDeep">Максимальная глубина поиска (максимальное количество пропускаемый токенов, пока не втретиться нужный)</param>
        /// <returns></returns>
        public Result<Token<T>> FindBackward(Predicate<Token<T>> oneOf, int maxDeep = 0)
        {
            var index = Position - 1;
            if(index == 0)
                return Result<Token<T>>.Err(outOfRangeException(index));
                //5        10    
            for (int i = index; i >= 0 && maxDeep >=0; i--)
            {
                if(oneOf.Invoke(tokens[index]))
                    return Result<Token<T>>.Ok(tokens[index]);
                maxDeep--;
            }
            return Result<Token<T>>.Err(notFountOnPositionException(index- maxDeep, index));
        }
       
        public Result<Token<T>> Before()
        {
            var index = Position - 1;
            if(index >= 0)
                return Result<Token<T>>.Ok(tokens[index]);
            return Result<Token<T>>.Err(outOfRangeException(index));
        }
        public Result<Token<T>> Before(T beforeIs) => Before(beforeIs, 0);
         /// <summary>
        /// Возврат предыдущего токена если мы токенизируем не весь текст а только какую то часть
        /// </summary>
        /// <returns></returns>
        public Result<Token<T>> BeforeLocal()
        {
            ///Такой перебор быстрее чем запрос linq
            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].StartIndex == StartIndex)
                {
                    if(i-1 >= 0)
                        return Result<Token<T>>.Ok(tokens[i-1]);
                    else return Result<Token<T>>.Err(outOfRangeException(i-1));
                }
            }
            return Result<Token<T>>.Err(neverException());
        }

        /// <summary>
        /// Предыдущий токен, используется когда нужно пропустить некоторое количество токенов, и мы знаем это количество.
        /// </summary>
        /// <param name="nextIs">Искомый токен</param>
        /// <param name="skip">Количество пропускаемых токенов</param>
        /// <returns></returns>
        public Result<Token<T>> Before(T beforeIs, int skip)
        {
            var index = Position - 1 - skip;
            if(index >= 0)
            {
                if(tokens[index].TokenType.Equals(beforeIs))
                    return Result<Token<T>>.Ok(tokens[index]);
                else
                {
                    var found = tokens[index].TokenType;
                    return Result<Token<T>>.Err(wrongFoundException(index, beforeIs, found));
                }
            }  
            else return Result<Token<T>>.Err(outOfRangeException(index));
        }
}
