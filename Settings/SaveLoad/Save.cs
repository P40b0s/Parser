using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using SettingsWorker.Requisite;

namespace SettingsWorker;

public partial class Settings
{
    JsonSerializerOptions getOptions()  
    {
        //Не работает аллоу для <>+
        //var encoderSettings = new TextEncoderSettings();
        //Символ который не пропускаем через фильтр
        //encoderSettings.AllowCharacters('<', '>', '+', '\u2116');
        //encoderSettings.ForbidCharacter('\u00ad');
        //encoderSettings.ForbidCharacters('<', '>', '+', '\u2116');
        //encoderSettings.AllowRanges(UnicodeRanges.Cyrillic, UnicodeRanges.BasicLatin);
        
        return new JsonSerializerOptions()
        {
            WriteIndented = true,
            //Encoder = JavaScriptEncoder.Create(encoderSettings),
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
    }
    /// <summary>
    /// Сериализация класса в файлы, каждая пропертя сериализуется в отдельный файл
    /// </summary>
    /// <param name="pathToDir"></param>
    /// <param name="deserializationObject"></param>
    /// <typeparam name="T"></typeparam>
    public void SerializeParts<T>(string pathToDir, T serializationObject) //AllTokensDefinitions
    {
        Paths.createPaths();
        var props = typeof(T).GetProperties();
        foreach(var p in props)
        {
            var tName = p.Name + ".json";
            var val = p.GetValue(serializationObject);
            var sr = System.Text.Json.JsonSerializer.Serialize(val, getOptions());
            System.IO.File.WriteAllText(Path.Combine(pathToDir, tName), sr);
        }
    }
     /// <summary>
    /// Сериализация класса в файлы, класс сериализуется целиком
    /// </summary>
    /// <param name="pathToDir"></param>
    /// <param name="deserializationObject"></param>
    /// <typeparam name="T"></typeparam>
    public void SerializeFull<T>(string pathToDir, T serializationObject, string name = null) //AllTokensDefinitions
    {
        Paths.createPaths();
        if(name == null)
            name = serializationObject.GetType().Name;
        name = name + ".json";
        var sr = System.Text.Json.JsonSerializer.Serialize<T>(serializationObject, getOptions());
        System.IO.File.WriteAllText(Path.Combine(pathToDir, name), sr);
        
    }
        
    public void Save()
    {
        SerializeParts<AllTokensDefinitions>(Paths.TokensDirPath, TokensDefinitions);
        SerializeFull<RequisiteChangers>(Paths.ChangersDirPath, RequisiteChangers);
        SerializeFull<List<CustomRule<AllRules>>>(Paths.CustomRulesDirPath, CustomRules, "CustomRules");
        SerializeFull<DocumentProcessing.PoolSettings>(Paths.RootCfgDirPath, PoolSettings, "PoolSettings");
        SerializeParts<Dictionaries.AllDictionaries>(Paths.DictionariesDirPath, Dictionaries);
    }
}