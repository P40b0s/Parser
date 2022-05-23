using Actualizer.Source.Operations;
using Lexer;
using SettingsWorker;
using SettingsWorker.Actualizer;

namespace Actualizer;

public class Operation
{
    public ISettings settings {get;}
    public Status status {get;}
    public Operation(ISettings settings)
    {
        this.settings = settings;
        this.status = new Status();
    }
    
}