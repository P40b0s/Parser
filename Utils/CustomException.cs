using System;
namespace Utils;

public enum ErrorType
{
    Fatal,
    Warning,
    Info
}

public interface IError
{
    DateTime Date {get;}
    string Message {get;set;}
    ErrorType ErrorType {get;set;}
    Exception Exception {get;set;}
}

public class DefaultError : IError
{
    public DefaultError(){}
    public DateTime Date {get;}
    public string Message {get;set;}
    public ErrorType ErrorType {get;set;}
    public Exception Exception {get;set;}

}

[Serializable]
public abstract class CustomError : IError
{
    public DateTime Date {get;}
    public string Message {get;set;}
    public ErrorType ErrorType {get;set;}
    public Exception Exception {get;set;}
     public CustomError()
    {
        ErrorType = ErrorType.Fatal;
        Date = DateTime.Now;
        Message = "Сообщение об ошибке не инициализировано";
    }
    public CustomError(string message)
    {
        ErrorType = ErrorType.Fatal;
        Date = DateTime.Now;
        Message = message;
    }
    public CustomError(string message, ErrorType errorType)
    {
        ErrorType = errorType;
        Date = DateTime.Now;
        Message = message;
    }


}

