
namespace DocumentParser.Interfaces
{
    /// <summary>
    /// Параграф в конце имеет : значит это типа мини заголовка для нижних абзацев
    /// </summary>
    public interface IIsDefinition
    {
        bool IsDefinition {get;set;}
    }
}
