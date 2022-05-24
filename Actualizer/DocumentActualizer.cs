using Actualizer.Source;
using Actualizer.Target;
using SettingsWorker;

namespace Actualizer;
/// <summary>
/// Пркси для соединения двух классов, увеличим уровень абстракции
/// </summary>
public class DocumentActualizer
{
    Status status {get;}
    ISettings settings {get;}
    SourceParser source {get; set;}
    TargetParser target {get; set;}

    string sourceFilePath {get;}

    string targetFilePath {get;}

    public DocumentActualizer(ISettings settings, string sourceFilePath, string targetFilePath)
    {
        this.sourceFilePath = sourceFilePath;
        this.targetFilePath = targetFilePath;
        this.settings = settings;
        status = new Status();
        
        
    }

    public async ValueTask<bool> Actualize()
    {
        if(!File.Exists(sourceFilePath))
        {
            status.AddError("Ошибка пути", $"Файл изменяющего документа по пути {sourceFilePath} не найден");
            return false;
        }
        if(!File.Exists(sourceFilePath))
        {
            status.AddError("Ошибка пути", $"Файл изменяемого документа по пути {targetFilePath} не найден");
            return false;
        }
        source = new SourceParser(this.sourceFilePath, this.settings);
        var sourceResult = await source.Parse();
        if(sourceResult.IsError)
        {
            status.AddError("Ошибки изменяющего документа", "При обработке изменяющего документа возникли ошибки:");
            status.AddErrors(sourceResult.Error().statuses);
            return false;
        }
        target = new TargetParser(targetFilePath, sourceResult.Value(), settings);
        var targetResult = await target.Actualize();
        if(targetResult.IsError)
        {
            status.AddError("Ошибки изменяемого документа", "При обработке изменяемого документа возникли ошибки:");
            status.AddErrors(targetResult.Error().statuses);
            return false;
        }
        else return true;
    }
}