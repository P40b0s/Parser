using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
namespace SettingsWorker;

public partial class Settings
{
    /// <summary>
    /// Десериализация файлов в класс из определенной директории (собираем один класс из нескольких файлов)
    /// </summary>
    /// <param name="dirPath">Директория из которой будут десериализовываться файлы</param>
    /// <param name="removedStringToNamespace">Какая часть имени типа должна быть убрана чтоб получился namespace десериализуемого типа</param>
    /// <param name="deserializationValue">Куда будем десериализовывать файлы</param>
    /// <typeparam name="DT">Тип десериализации</typeparam>
    /// <returns></returns>
    public async ValueTask<bool> DeserializePart<DT>(string dirPath, string removedStringToNamespace, DT deserializationValue)
    {
        if(Directory.Exists(dirPath))
        {
            foreach(var f in Directory.GetFiles(dirPath).Select(s=>new FileInfo(s)))
            {
                var settings = await System.IO.File.ReadAllTextAsync(f.FullName);
                var name = f.Name.Replace(f.Extension,"");
                var ns = name.Replace(removedStringToNamespace, ""); //TokenDefinitions
                Type type = Type.GetType($"SettingsWorker.{ns}.{name}");
                if(type != null)
                {
                     var deserialized = System.Text.Json.JsonSerializer.Deserialize(settings, type, getOptions());
                    if(deserialized != null)
                    {
                        var props = typeof(DT).GetProperties();
                        foreach(var p in props)
                        {
                            if(p.Name == name)
                            {
                                //p.SetValue(TokensDefinitions, Convert.ChangeType(deserialized, p.PropertyType));
                                p.SetValue(deserializationValue, deserialized);
                                AddStatus($"Настройки {name} успешно загружены из файла {f.Name}");
                            }
                        }
                    }
                }
                else
                    AddStatus($"Не могу десериализовать файл {f.Name}, тип SettingsWorker.{ns}.{name} не существует, настройки {name} будут загружены по умолчанию");
            }
        }
        else
        {
            AddStatus($"Директория кофигурации {dirPath} не найдена, настройки {deserializationValue.GetType().Name} будут загружены по умолчанию");
            Save();
        }
        return true;
    }
    /// <summary>
    /// Десериализация файлов в класс из определенной директории (десериализуем один файл в один класс)
    /// </summary>
    /// <param name="dirPath">Директория из которой будут десериализовываться файлы</param>
    /// <param name="removedStringToNamespace">Какая часть имени типа должна быть убрана чтоб получился namespace десериализуемого типа</param>
    /// <param name="deserializationValue">Куда будем десериализовывать файлы</param>
    /// <typeparam name="DT">Тип десериализации</typeparam>
    /// <returns></returns>
    public async ValueTask<bool> DeserializeFull<DT>(string dirPath, DT deserializationValue)
    {
        var ns = typeof(DT).Namespace;
        var name = typeof(DT).Name;
        if(Directory.Exists(dirPath))
        {
            var filePath = new FileInfo(Path.Combine(dirPath, name+".json"));
            var stringFp = Path.Combine(dirPath, name+".json");
            var files = Directory.GetFiles(dirPath).Select(s=>new FileInfo(s));
            if(!filePath.Exists)
            {
                AddStatus($"Не могу десериализовать класс {name}, файл {stringFp} не найден, настройки {name} будут загружены по умолчанию");
                Save();
                return true;
            }
            var settings = await System.IO.File.ReadAllTextAsync(filePath.FullName);
            //FIXME System.Collections.Generic.List`1[SettingsWorker.Requisite.Changer]' to type 'SettingsWorker.Requisite.RequisiteChangers'
            var deserialized = System.Text.Json.JsonSerializer.Deserialize(settings, typeof(DT), getOptions());
            if(deserialized != null)
            {
                deserializationValue = (DT)deserialized;
                AddStatus($"Настройки {name} успешно загружены из файла {stringFp}");
            }
            else
                AddStatus($"Невозможно десериализовать файл {stringFp}, ошибка структуры файла, настройки {name} будут загружены по умолчанию");
        }
        else
        {
            AddStatus($"Директория кофигурации {dirPath} не найдена, настройки {name} будут загружены по умолчанию");
            Save();
        }
        return true;
    }

   
    public async ValueTask<bool> Load()
    {
        //await LoadFiles<AllTokensDefinitions>(Paths.TokensDirPath, "TokenDefinitions", TokensDefinitions);
        return await DeserializeFull<Requisite.RequisiteChangers>(Paths.ChangersDirPath, RequisiteChangers);
    }



}