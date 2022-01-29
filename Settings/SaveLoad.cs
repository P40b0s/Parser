using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
        
    public void Save()
    {
        if(System.IO.File.Exists(fileName))
            System.IO.File.Delete(fileName);
        var serializer = System.Text.Json.JsonSerializer.Serialize<Settings>(this, getOptions());
        System.IO.File.WriteAllText(fileName, serializer);
        status = $"Файл настроек {fileName} успешно обновлен";
    }

    public async ValueTask<bool> Load()
    {
        if(System.IO.File.Exists(fileName))
        {
            var settings = await System.IO.File.ReadAllTextAsync(fileName);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<Settings>(settings, getOptions());
            if(deserialized != null)
            {
                update(deserialized);
                status = $"Настройки успешно загружены из файла {fileName}";
                IsDefault = false;
            }
            else
            {
                status = $"Ошибка десериализации настроек из файла {fileName}, Загружены настройки по умолчанию";
                IsDefault = true;
                Save();
            }
            return true;
        }
        else
        {
            status = $"Файл настроек {fileName} не обнаружен, установлены настройки по умолчанию";
            Save();
            IsDefault = true;
            return true;
        }
    }

}