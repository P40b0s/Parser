using Actualizer.Source;
using Actualizer.Target;

namespace Actualizer;
/// <summary>
/// Пркси для соединения двух классов, увеличим уровень абстракции
/// </summary>
public class DocumentActualizer
{
    SourceDocumentParser source {get;}
    TargetParser target {get;}

    public DocumentActualizer()
    {
        
    }
}