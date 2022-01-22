namespace Utils;
public class TryGetErrorIfResultExistsException : Exception
{
    public TryGetErrorIfResultExistsException(string message = "Вы не можете получить Error, так как результат операции: true") : base(message){}
}

public class TryGetValueIfErrorExistsException : Exception
{
    public TryGetValueIfErrorExistsException(string message = "Вы не можете получить Value, так как результат операции: false") : base(message){}
}