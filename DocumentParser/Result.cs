using System;
namespace DocumentParser;

/// <summary>
/// Результат выполнения операции
/// </summary>
public struct Result<R,E> where E : Exception
{
    public  Result(R result, E exception = null)
    {
        Element = result;
        Error = exception;
    }
    public R Element {get;}
    public E Error {get;}
    public bool IsOk => Element != null;
    public bool IsError => Error != null;
}
