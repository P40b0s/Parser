using System;
namespace Utils;

public interface IResult<T, out E> where E : IError
{
    T Value {get;}
    E Error {get;}
    bool IsOk => Error == null;
    bool IsError => Error != null;
}
public interface IResult<T>
{
    T Value {get;}
    IError Error {get;}
    bool IsOk => Error == null;
    bool IsError => Error != null;
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
    public T Value => getValue();
    private T value {get;init;}
    public IError Error => getError();
    private IError error {get;init;}
    public bool IsOk => error == null;
    public bool IsError => error != null;
    private T getValue()
    {
        if(this.IsError)
            throw new Exception("Вы пытаетесь получить результат операции, со статусом ERROR: " + Error.Message);
        else return value;
    }
     private IError getError()
    {
        if(!this.IsError)
            throw new Exception("Вы пытаетесь получить ошибку операции, но операция завершена положительно и у нее есть результат: " + Value.ToString());
        else return error;
    }
}



public struct Result<T,E> : IResult<T,E> where E : IError, new()
{
    public Result(T result)
    {
        value = result;
        error = default(E);
    }
    
    public Result(E error)
    {
        this.error = error;
        value = default(T);
    }
    /// <summary>
    /// Значение результата
    /// </summary>
    /// <returns></returns>
    public T Value => getValue();
    private T value {get;init;}
    public E Error => getError();
    private E error {get;init;}
    public bool IsOk => error == null;
    public bool IsError => error != null;
    private T getValue()
    {
        if(this.IsError)
            throw new TryGetValueIfErrorExistsException("Вы не можете получить Value, так как результат операции: false " + Error.Message);
        else return value;
    }
     private E getError()
    {
        if(!this.IsError)
            throw new TryGetErrorIfResultExistsException("Вы не можете получить Error, так как результат операции: true " + Value.ToString());
        else return error;
    }
}


