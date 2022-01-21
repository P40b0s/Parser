using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexer;
public static class ResultExtension
{
    public static Result<T, Exception> F<T>(this IEnumerable<T> en)
    {
        if(en.Count() == 0)
            return new Result<T, Exception>(new Exception("В массиве нет ни одного элемента"));
        else return new Result<T, Exception>(en.ElementAt(0));
        
    }
    public static Result<T, E> F<T, E>(this List<Result<T, E>> en) where E : Exception
    {
        if(en.Count() == 0)
            return new Result<T, E>(new Exception("В массиве нет ни одного элемента") as E);
        else return new Result<T, E>(en.ElementAt(0).Value);
        
    }
    /// <summary>
    /// Возвращает результат с первым элементом из списка если он там есть.
    /// </summary>
    /// <param name="en"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Result<T, CustomException<E>> F<T, E>(this List<T> en)
    {
        if(en.Count() == 0)
            return new Result<T, CustomException<E>>(new Exception("В массиве нет ни одного элемента") as CustomException<E>);
        else return new Result<T, CustomException<E>>(en.ElementAt(0));
        
    }
    public static Result<T, E> L<T, E>(this List<Result<T, E>> en) where E : Exception
    {
        if(en.Count == 0)
            return new Result<T, E>(new Exception("В массиве нет ни одного элемента") as E);
        else return new Result<T, E>(en.ElementAt(en.Count - 1).Value);
        
    }
}