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

public static class ResultExtensions
{
    public static Result<T> Lift<T>(this T value) => new Result<T>(value);
     /// функтор
        public static Result<TResult> Select<TSource, TResult>(this Result<TSource> result, Func<TSource, TResult> functor)
        {
            return result.IsOk
            ? functor(result.Value()).Lift()
            : Result<TResult>.Err(result.Error());
        }

        /// монада
        public static Result<TResult> Select<TSource, TResult>(this Result<TSource> result, Func<TSource, Result<TResult>> arrow)
        {
            return result.IsOk
            ? arrow(result.Value())
            : Result<TResult>.Err(result.Error());
        }

        public static Result<TProjection> SelectMany<TSource, TResult, TProjection>(this Result<TSource> result, 
        Func<TSource, Result<TResult>> arrow,
        Func<TSource, TResult, TProjection> projection)
        {
            return result.Select(
                result => arrow(result).Select(
                    projectionResult => projection(result, projectionResult)
                )
            );
        }
}



