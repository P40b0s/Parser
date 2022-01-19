
using DocumentParser.DocumentElements.FootNotes;

namespace DocumentParser.Interfaces
{
    /// <summary>
    /// Может быть обьектом для гиперссылок
    /// </summary>
    public interface IFootNote
    {
       FootNoteInfo FootNote {get;set;}
    }
    
}
