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

public class DefaultError : CustomError
{
     public DefaultError() : base(){}
    public DefaultError(string message) : base(message){}
    public DefaultError(string message, ErrorType errorType) : base(message, errorType){}
    public override string ToString() =>
        Message;
    

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
    public override string ToString() =>
        Message;


}

