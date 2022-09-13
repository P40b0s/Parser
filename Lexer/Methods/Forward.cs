using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Lexer;

public partial class Token<T>
{
    public Result<Token<T>> Next()
    {
        var index = Position + 1;
        if(tokens.Count < index)
            return Result<Token<T>>.Err(outOfRangeException(index));
        return Result<Token<T>>.Ok(tokens[index]);
    }
    /// <summary>
    /// Переход на следующий токен (если он такой как мы ожидаем)
    /// </summary>
    /// <param name="nextIs"></param>
    /// <returns></returns>
    public Result<Token<T>> Next(T nextIs) => Next(nextIs, 0);
    /// <summary>
    /// Возврат следующего токена если мы токенизируем не весь текст а только какую то часть
    /// </summary>
    /// <returns></returns>
    public Result<Token<T>> NextLocal()
    {
        for(int i = 0; i < tokens.Count; i++)
        {
            if(tokens[i].StartIndex == StartIndex)
            {
                if(i+1 < tokens.Count)
                    return Result<Token<T>>.Ok(tokens[i+1]);
                else return Result<Token<T>>.Err(outOfRangeException(i+1));
                    
            }
        }
        return Result<Token<T>>.Err(neverException());
    }
   
    /// <summary>
    /// Следующий токен, используется когда нужно пропустить некоторое количество токенов, и мы знаем это количество.
    /// </summary>
    /// <param name="nextIs">Искомый токен</param>
    /// <param name="skip">Количество пропускаемых токенов</param>
    /// <returns></returns>
    public Result<Token<T>> Next(T nextIs, int skip)
    {
        var index = Position + 1 + skip;
        if(tokens.Count <= index)
            return Result<Token<T>>.Err(outOfRangeException(index));
        if(tokens[index].TokenType.Equals(nextIs))
            return Result<Token<T>>.Ok(tokens[index]);
        else
        {
            var found = tokens[index].TokenType;
            return Result<Token<T>>.Err(wrongFoundException(index, nextIs, found));
        }
    }
    /// <summary>
    /// Поиск токенов из перечня предиката
    /// </summary>
    /// <param name="nextIs">Искомый токен</param>
    /// <param name="skip">Количество пропускаемых токенов</param>
    /// <returns></returns>
    public Result<Token<T>> FindForward(Predicate<Token<T>> oneOf, int maxDeep = 0, bool withSelf = false)
    {
        var index = withSelf? Position : Position + 1;
        if(tokens.Count <= index)
            return Result<Token<T>>.Err(outOfRangeException(index));
        for (int i = index; i < (maxDeep + index) && i < tokens.Count(); i++)
        {
            if(tokens.Count < i)
                return Result<Token<T>>.Err(outOfRangeException(i));
            
            if(oneOf(tokens[i]))
                return Result<Token<T>>.Ok(tokens[i]);   
        }
        return Result<Token<T>>.Err(notFountOnPositionException(index, index + maxDeep));
    }
        /// <summary>
    /// Ищем то незнаем что, занем что не ищем) Берем только тот токен что не значиться в предикате
    /// </summary>
    /// <param name="ignore">Токены которые проходим</param>
    /// <returns></returns>
    public Result<Token<T>> FindForward(Predicate<Token<T>> ignore)
    {
        var index = Position + 1;
        if(tokens.Count <= index)
            return Result<Token<T>>.Err(outOfRangeException(index));
        for (int i = index; i < tokens.Count; i++)
        {
            if(tokens.Count < i)
                return Result<Token<T>>.Err(outOfRangeException(i));
            if(ignore(tokens[i]))
                return Result<Token<T>>.Ok(tokens[i]);    
        }
        return Result<Token<T>>.Err(customException("Не найдено ни одного токена"));
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
    public Result<Token<T>> FindForward(T searchedToken, int maxDeep = 0)
    {
        var index = Position + 1;
        if(tokens.Count <= index)
            return Result<Token<T>>.Err(outOfRangeException(index));
        for (int i = index; i <= maxDeep + index; i++)
        {
            if(tokens.Count <= i)
                return Result<Token<T>>.Err(outOfRangeException(i));
           
            if(tokens[i].TokenType.Equals(searchedToken))
                return Result<Token<T>>.Ok(tokens[i]);
        }
        return Result<Token<T>>.Err(notFountOnPositionException(index, index + maxDeep, searchedToken));
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
    /// <param name="oneOf">один из искомых токенов</param>
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
    /// Получаем массив искомых токенов, метод прерывается если встречается токен отличный от искомых 
    /// </summary>
    /// <param name="oneOf">Список искомых токенов</param>
    /// <param name="ignore">Какие токены пропускаем</param>
    /// <param name="withSelf">Осуществлять поиск включая искомый токен</param>
    /// <returns></returns>
    public IEnumerable<Token<T>> FindForwardMany(Predicate<Token<T>> oneOf, Predicate<Token<T>> ignore, bool withSelf = false)
    {
        var index = withSelf ? Position : Position + 1;
        while(tokens.Count > index && (oneOf(tokens[index]) || ignore(tokens[index])))
        {   if(oneOf(tokens[index]))
                yield return tokens[index];
            index++;
        }
    }
}
