namespace SettingsWorker;
public struct TokenDefinitionSettings<T> where T : Enum
{
    /// <summary>
    /// Тип токена означает enul его типа - все токены это enum
    /// сделал статически чтоб не тащить сюда циклиюческую зависимость от проекта DocumentParser
    /// Не очень удобно, но зато правильно
    /// </summary>
    /// <value></value>
    public T TokenType {get;set;}
    public string TokenName => Enum.GetName(typeof(T), TokenType);
    /// <summary>
    /// Regex паттерн для данного токена
    /// </summary>
    /// <value></value>
    public string TokenPattern {get;set;}
    /// <summary>
    /// Очередь, чем выше значение тем позже данный паттерн сработает
    /// например федеральный закон - 1 и закон - 2
    /// первым сработает паттерн федеральный закон.
    /// </summary>
    /// <value></value>
    public int TokenQueue {get;set;}
    public string Description {get;set;}
    public Dictionary<string, string> Converter {get;set;}
}