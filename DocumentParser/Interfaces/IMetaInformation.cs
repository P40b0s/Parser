
using DocumentParser.DocumentElements.MetaInformation;

namespace DocumentParser.Interfaces
{
    /// <summary>
    /// Может иметь метаинформацию (та что НТЦ Система проставляет в скобках - (Абзац дополнен - Постановление правительства от.....))
    /// </summary>
    public interface IMetaInformation
    {
        MetaInfo Meta {get;set;}
    }
}
