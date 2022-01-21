
using System.Collections.Generic;
using System.Threading.Tasks;
using Settings.Annexes;
using Settings.Requisites;
namespace Settings;


public interface ISettings
{
    bool IsDefault {get;set;}
    string Status();
    ImageSettings ImageSettings {get;set;}
    ParserRules ParserRules {get;set;}
    AnnexParserRules AnnexParserRules {get;set;}
    RequisitesParserRules RequisitesParserRules {get;set;}

}
public partial class Settings : ISettings
{
    public Settings()
    {
        IsDefault = true;
    }
    public bool IsDefault {get;set;} = false;
    public string Status() => status;
    private string fileName => "settings.json";
    private System.DateTime loadDate {get;set;} = DateTime.Now;
    string status {get;set;} = "Модуль настроек";
    public ImageSettings ImageSettings {get;set;} = new ImageSettings();
    public ParserRules ParserRules {get;set;} = new ParserRules();
    public AnnexParserRules AnnexParserRules {get;set;} = new AnnexParserRules();
    public RequisitesParserRules RequisitesParserRules {get;set;} = new RequisitesParserRules();
   

    private void update(Settings settings)
    {
        this.loadDate = System.DateTime.Now;
        this.ImageSettings = settings.ImageSettings;
        this.ParserRules = settings.ParserRules;
        this.AnnexParserRules = settings.AnnexParserRules;
        this.RequisitesParserRules = settings.RequisitesParserRules;
        this.status = $"Загружены настройки из файла {fileName} {loadDate.ToShortDateString} {loadDate.ToShortTimeString}";
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
    // /// <summary>
    // ///Наименование документа должно быть выделено жирным шрифтом, иначе считается что его нет
    // /// </summary>
    // /// <value></value>
    // public bool BoldNameRule {get;set;} = true;
    
    // public override string ToString()
    // {
    //     return  $"Наименование документа должно быть выделено жирным шрифтом, иначе считается что его нет: {BoldNameRule}" + "\n" +
    //             "";
    // }
}



    // public override string ToString()
    // {
    //     return  $"Максимальная глубина поиска типа документа (от нулевого токена): {TypeSearchMaxDeep}" + "\n" +
    //             "";
    // }







