using System;
namespace Lexer;

public interface IResult<T, out E> where E : Exception
{
    T Value {get;}
    E Error {get;}
    bool IsOk => Error == null;
    bool IsError => Error != null;
}



public struct Result<T,E> : IResult<T,E> where E : Exception
{
    public Result(T result)
    {
        value = result;
        error = default;
    }
    public Result(E error)
    {
        this.error = error;
        value = default;
    }
    /// <summary>
    /// Значение результата
    /// </summary>
    /// <returns></returns>
    public T Value => getValue();
    private T value {get;init;}
    public E Error => getError();
    private E error {get;init;}
    public bool IsOk => Error == null;
    public bool IsError => Error != null;
    private T getValue()
    {
        if(this.IsError)
            throw new Exception("Вы пытаетесь получить результат операции, со статусом ERROR: " + Error.Message);
        else return value;
    }
     private E getError()
    {
        if(!this.IsError)
            throw new Exception("Вы пытаетесь получить ошибку операции, но операция завершена положительно и у нее есть результат: " + Value.ToString());
        else return error;
    }
}


