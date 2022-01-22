using System;
namespace Utils;

public interface IError
{
    DateTime Date {get;}
    string Message {get;set;}
}

public class DefaultError : IError
{
    public DefaultError(){}
    public DateTime Date {get;}
    public string Message {get;set;}
}

[Serializable]
public abstract class CustomError<T> : IError
{
    public T ErrorType {get;}
    public DateTime Date {get;}
    public string Message {get;set;}
     public CustomError()
    {
        ErrorType = default(T);
        Date = DateTime.Now;
        Message = "Сообщение об ошибке не инициализировано";
    }
    public CustomError(string message)
    {
        ErrorType = default(T);
        Date = DateTime.Now;
        Message = message;
    }
    public CustomError(string message, T errorType)
    {
        ErrorType = errorType;
        Date = DateTime.Now;
        Message = message;
    }


}

