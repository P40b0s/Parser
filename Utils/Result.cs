using System;
using System.Runtime.CompilerServices;

namespace Utils;

public interface IResult<T>
{
    T Value([CallerMemberName]string caller = null);
    IError Error([CallerMemberName]string caller = null);
    bool IsOk {get;}
    bool IsError {get;}
}

public struct Result<T> : IResult<T>
{
    public Result(T result)
    {
        value = result;
        error = null;
    }
    
    public Result(IError error)
    {
        this.error = error;
        value = default(T);
    }
    /// <summary>
    /// Значение результата
    /// </summary>
    /// <returns></returns>
    public T Value([CallerMemberName]string caller = null)
    {
        if(this.IsError)
            throw new Exception($"Метод {caller} пытается получить результат операции, со статусом ERROR: {Error().Message}");
        else return value;
    }
    private T value {get;init;}
    public IError Error([CallerMemberName]string caller = null)
    {
        if(!this.IsError)
            throw new Exception($"Метод {caller} пытается получить ошибку операции, но операция завершена положительно и у нее есть результат: " + Value().ToString());
        else return error;
    }
    private IError error {get;init;}
    public bool IsOk => error == null;
    public bool IsError => error != null;
    public static Result<T> Err(IError error)
    {
        return new Result<T>(error);
    }
    public static Result<T>Err(string error)
    {
        return new Result<T>(new DefaultError(){Message = error, ErrorType = ErrorType.Fatal});
    }
    public static Result<T> Err(string error, ErrorType errorType)
    {
        return new Result<T>(new DefaultError(){Message = error, ErrorType = errorType});
    }
    public static Result<T> Ok(T value)
    {
        return new Result<T>(value);
    }
}



// public struct Result<T,E> : IResult<T,E> where E : IError, new()
// {
//     public Result(T result)
//     {
//         value = result;
//         error = default(E);
//     }
    
//     public Result(E error)
//     {
//         this.error = error;
//         value = default(T);
//     }
//     /// <summary>
//     /// Значение результата
//     /// </summary>
//     /// <returns></returns>
//      public T Value([CallerMemberName]string caller = null)
//     {
//         if(this.IsError)
//             throw new Exception($"Метод {caller} пытается получить результат операции, со статусом ERROR: {Error().Message}");
//         else return value;
//     }
//     private T value {get;init;}
//     public E Error([CallerMemberName]string caller = null)
//     {
//         if(!this.IsError)
//             throw new Exception($"Метод {caller} пытается получить ошибку операции, но операция завершена положительно и у нее есть результат: " + Value().ToString());
//         else return error;
//     }
//     private E error {get;init;}
//     public bool IsOk => error == null;
//     public bool IsError => error != null;
// }



