using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Lexer;

public partial class Token<T>
{
    public Result<Token<T>, TokenException> Next()
    {
        var index = Position + 1;
        if(tokens.Count < index)
            return new Result<Token<T>, TokenException>(outOfRangeException(index));
        return new Result<Token<T>, TokenException>(tokens[index]);
    }
    /// <summary>
    /// Переход на следующий токен (если он такой как мы ожидаем)
    /// </summary>
    /// <param name="nextIs"></param>
    /// <returns></returns>
    public Result<Token<T>, TokenException> Next(T nextIs) => Next(nextIs, 0);
    /// <summary>
    /// Возврат следующего токена если мы токенизируем не весь текст а только какую то часть
    /// </summary>
    /// <returns></returns>
    public Result<Token<T>, TokenException> NextLocal()
    {
        for(int i = 0; i < tokens.Count; i++)
        {
            if(tokens[i].StartIndex == StartIndex)
            {
                if(i+1 < tokens.Count)
                    return new Result<Token<T>, TokenException>(tokens[i+1]);
                else return new Result<Token<T>, TokenException>(outOfRangeException(i-1));
                    
            }
        }
        return new Result<Token<T>, TokenException>(neverException());
    }
   
    /// <summary>
    /// Следующий токен, используется когда нужно пропустить некоторое количество токенов, и мы знаем это количество.
    /// </summary>
    /// <param name="nextIs">Искомый токен</param>
    /// <param name="skip">Количество пропускаемых токенов</param>
    /// <returns></returns>
    public Result<Token<T>, TokenException> Next(T nextIs, int skip)
    {
        var index = Position + 1 + skip;
        if(tokens.Count < index)
            return new Result<Token<T>, TokenException>(outOfRangeException(index));
        if(tokens[index].TokenType.Equals(nextIs))
            return new Result<Token<T>, TokenException>(tokens[index]);
        else
        {
            var found = tokens[index].TokenType;
            return new Result<Token<T>, TokenException>(wrongFoundException(index, nextIs, found));
        }
    }
        /// <summary>
    /// Поиск токенов из перечня предиката
    /// </summary>
    /// <param name="nextIs">Искомый токен</param>
    /// <param name="skip">Количество пропускаемых токенов</param>
    /// <returns></returns>
    public Result<Token<T>, TokenException> FindForward(Predicate<Token<T>> oneOf, int maxDeep = 0, bool withSelf = false)
    {
        var index = withSelf? Position : Position + 1;
        if(tokens.Count == index)
            return new Result<Token<T>, TokenException>(outOfRangeException(index));
        for (int i = index; i <= maxDeep + index; i++)
        {
            if(tokens.Count < i)
                return new Result<Token<T>, TokenException>(outOfRangeException(i));
            
            if(oneOf(tokens[i]))
                return new Result<Token<T>, TokenException>(tokens[i]);   
        }
        return new Result<Token<T>, TokenException>(notFountOnPositionException(index, index + maxDeep));
    }
        /// <summary>
    /// Ищем то незнаем что, занем что не ищем) Берем только тот токен что не значиться в предикате
    /// </summary>
    /// <param name="ignore">Токены которые проходим</param>
    /// <returns></returns>
    public Result<Token<T>, TokenException> FindForward(Predicate<Token<T>> ignore)
    {
        var index = Position + 1;
        if(tokens.Count == index)
            return new Result<Token<T>, TokenException>(outOfRangeException(index));
        for (int i = index; i < tokens.Count; i++)
        {
            if(tokens.Count < i)
                return new Result<Token<T>, TokenException>(outOfRangeException(i));
            if(ignore(tokens[index]))
                return new Result<Token<T>, TokenException>(tokens[index]);    
        }
        return new Result<Token<T>, TokenException>(customException("Не найдено ни одного токена"));
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
    public Result<Token<T>, TokenException> FindForward(T searchedToken, int maxDeep = 0)
    {
        var index = Position + 1;
        if(tokens.Count == index)
            return new Result<Token<T>, TokenException>(outOfRangeException(index));
        for (int i = index; i <= maxDeep + index; i++)
        {
            if(tokens.Count < i)
                return new Result<Token<T>, TokenException>(outOfRangeException(i));
           
            if(tokens[index].TokenType.Equals(searchedToken))
                return new Result<Token<T>, TokenException>(tokens[index]);
        }
        return new Result<Token<T>, TokenException>(notFountOnPositionException(index, index + maxDeep, searchedToken));
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
}
