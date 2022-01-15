
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace DocumentParser
{

    public interface ISettings
    {
        string Status {get;set;}
        ImageSettings ImageSettings {get;set;}
        ParserRules ParserRules {get;set;}

    }
    public class Settings : ISettings
    {
        public Settings()
        {
            loadSetings();
        }
        private string fileName => "parserSettings.json";
        private System.DateTime loadDate {get;set;}
        public string Status {get;set;}
        public ImageSettings ImageSettings {get;set;}
        public ParserRules ParserRules {get;set;}
        private async Task loadSetings()
        {
            if(System.IO.File.Exists(fileName))
            {
                var settings = await System.IO.File.ReadAllTextAsync(fileName);
                Settings deserialized = JsonConvert.DeserializeObject<Settings>(settings);
                update(deserialized);
            }
            else
            {
                ImageSettings = new ImageSettings();
                ParserRules = new ParserRules();
                Status = $"Файл настроек {fileName} не обнаружен, установлены настройки по умолчанию";
            }
               
           
        }

        private void update(Settings settings)
        {
            this.ImageSettings = settings.ImageSettings;
            this.ParserRules = settings.ParserRules;
            this.loadDate = System.DateTime.Now;
            this.Status = $"Загружены настройки из файла {fileName} {loadDate.ToShortDateString} {loadDate.ToShortTimeString}";
        }
        /// <summary>
        /// Список текущих настроек
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return    ImageSettings.ToString() + "\n"
                    + ParserRules.ToString();

        }
        
    }

    public class ImageSettings
    {
        /// <summary>
        /// Максимальный размер файла который создается в теле документа
        /// все изображения которые больше этого размера будут сохранятся в таблицу
        /// documents_images
        /// </summary>
        /// <value></value>
        public int MaxBodyImageSize {get;set;} = 240000;
         /// <summary>
        /// Размер файла предпросмотра
        /// </summary>
        /// <value></value>
        public int PreviewImegeThumbSizeX {get;set;} = 256;
         /// <summary>
        /// Размер файла предпросмотра
        /// </summary>
        /// <value></value>
        public int PreviewImegeThumbSizeY {get;set;} = 256;

        public override string ToString()
        {
            return  $"Максимальный размер изображения в теле документа: {MaxBodyImageSize /1000} кб" + "\n" +
                    $"Размер файла предпросмотра изображения по оси X: {PreviewImegeThumbSizeX} пикселей" + "\n" +
                    $"Размер файла предпросмотра изображения по оси Y: {PreviewImegeThumbSizeY} пикселей";
        }
    }

    public class ParserRules
    {
        /// <summary>
        ///Наименование документа должно быть выделено жирным шрифтом, иначе считается что его нет
        /// </summary>
        /// <value></value>
        public bool BoldNameRule {get;set;} = true;
      
        public override string ToString()
        {
            return  $"Наименование документа должно быть выделено жирным шрифтом, иначе считается что его нет: {BoldNameRule}" + "\n" +
                    "";
        }
    }

}