using NUnit.Framework;
using System.Threading.Tasks;
using SettingsWorker;
using Actualizer.Source;

namespace Tests;

public class ActualizerTest 
{

    ISettings settings {get;}
    public ActualizerTest()
    {
        settings = new SettingsWorker.Settings();
        settings.Save();
    }
    [Test]
    public async ValueTask TestSourceParser()
    {
       var file = Paths.CurrentDir + Paths.RootTestPath + "ФЗ-476_не_парсится.docx";

        SourceParser source = new SourceParser(file, settings);
        var result = await source.Parse();
        if(result.IsOk)
        {
            var j = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value().Structures, Newtonsoft.Json.Formatting.None,
            new Newtonsoft.Json.JsonSerializerSettings()
            { 
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None
            });
            //var j = Newtonsoft.Json.JsonConvert.SerializeObject(result.Value());
            System.IO.File.WriteAllText("476_Changemap.json", j);
            Assert.True(true);
        }
        //Assert.True(false);
           
    }

    
}