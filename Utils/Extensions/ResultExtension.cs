using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Utils.Extensions;
public static class ResultExtension
{

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    /// </summary>
    public static Result<T, E> Next<T, E>(this Result<T, E> result, Action<T> action, string error) where E : class, IError, new()
    {
        if (result.IsOk)
            action(result.Value);
        else
            return new Result<T, E>(Activator.CreateInstance(typeof(E), new object[] { error }) as E);
        return result;
    }

    public static Result<T> GetFirst<T>(this List<T> list, Func<T, bool> predicate)
    {
        for(int i = 0; i < list.Count; i++)
        {
            if(predicate(list[i]))
                return new Result<T>(list[i]);
        }
        return new Result<T>(new DefaultError() {Message = "Элемент не найден" });
    }
    public static Result<T> GetLast<T>(this List<T> list, Func<T, bool> predicate)
    {
        for(int i = list.Count - 1; i >= 0; i--)
        {
            if(predicate(list[i]))
                return new Result<T>(list[i]);
        }
        return new Result<T>(new DefaultError() {Message = "Элемент не найден" });
    }

    // public static Result<T, Exception> F<T>(this IEnumerable<T> en)
    // {
    //     if(en.Count() == 0)
    //         return new Result<T, Exception>(new Exception("В массиве нет ни одного элемента"));
    //     else return new Result<T, Exception>(en.ElementAt(0));
        
    // }
    // public static Result<T, E> F<T, E>(this List<Result<T, E>> en) where E : Exception
    // {
    //     if(en.Count() == 0)
    //         return new Result<T, E>(new Exception("В массиве нет ни одного элемента") as E);
    //     else return new Result<T, E>(en.ElementAt(0).Value);
        
    // }
    // /// <summary>
    // /// Возвращает результат с первым элементом из списка если он там есть.
    // /// </summary>
    // /// <param name="en"></param>
    // /// <typeparam name="T"></typeparam>
    // /// <returns></returns>
    // public static Result<T, CustomException<E>> F<T, E>(this List<T> en)
    // {
    //     if(en.Count() == 0)
    //         return new Result<T, CustomException<E>>(new Exception("В массиве нет ни одного элемента") as CustomException<E>);
    //     else return new Result<T, CustomException<E>>(en.ElementAt(0));
        
    // }
    // public static Result<T, E> L<T, E>(this List<Result<T, E>> en) where E : Exception
    // {
    //     if(en.Count == 0)
    //         return new Result<T, E>(new Exception("В массиве нет ни одного элемента") as E);
    //     else return new Result<T, E>(en.ElementAt(en.Count - 1).Value);
        
    // }
}