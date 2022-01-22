
using System.Collections.Generic;
using System.Threading.Tasks;
using SettingsWorker.Annexes;
using SettingsWorker.Requisites;
using SettingsWorker.Regexes;
namespace SettingsWorker;


public interface ISettings
{
    bool IsDefault {get;set;}
    string Status();
    AllRules DefaultRules {get;set;}
    AllTokensDefinitions TokensDefinitions {get;set;}
    List<CustomRule<AllRules>> CustomRules {get;set;}
    Paths Paths {get;}
    ValueTask<bool> Load();
    void Save();
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
    public AllRules DefaultRules {get;set;} = new AllRules();
    public AllTokensDefinitions TokensDefinitions {get;set;} = new AllTokensDefinitions();
    public List<CustomRule<AllRules>> CustomRules {get;set;} = new List<CustomRule<AllRules>>()
    {
        new CustomRule<AllRules>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = "кодекс", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                NameInTypeString = true,
                ParseGDSFAttributes = true,
            },
        }},
        new CustomRule<AllRules>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = $"федеральный{Templates.WsOrBr}закон", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                ParseGDSFAttributes = true,
            },
        }},
        new CustomRule<AllRules>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = $"закон{Templates.WsOrBr}российской{Templates.WsOrBr}федерации{Templates.WsOrBr}о{Templates.WsOrBr}поправке{Templates.WsOrBr}к{Templates.WsOrBr}конституции{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                ParseGDSFAttributes = true,
            },
        }},
        new CustomRule<AllRules>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = "закон", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                ParseGDSFAttributes = true,
            },
        }},
        new CustomRule<AllRules>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = $"федеральный{Templates.WsOrBr}конституционный{Templates.WsOrBr}закон", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                ParseGDSFAttributes = true,
            },
        }},
        new CustomRule<AllRules>(){Organ = $"правительство{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", Type = "распоряжение", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                SignDateAfterType= true,
                RequiredName = false,
                NamePositionAfterTypeCorrection = 3,
            },
        }},
        new CustomRule<AllRules>(){Organ = $"правительство{Templates.WsOrBr}российской{Templates.WsOrBr}федерации", Type = "постановление", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                SignDateAfterType= true,
                NamePositionAfterTypeCorrection = 3,
            },
        }},
        new CustomRule<AllRules>(){Organ = $".+", Type = "приказ", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                SignDateAfterType= true,
                NamePositionAfterTypeCorrection = 3,
            },
        }, Weight = 5},
    }; 

    [System.Text.Json.Serialization.JsonIgnore]
    public Paths Paths {get;} = new Paths();
   

    private void update(Settings settings)
    {
        this.loadDate = System.DateTime.Now;
        this.CustomRules = settings.CustomRules;
        this.DefaultRules = settings.DefaultRules;
        this.TokensDefinitions = settings.TokensDefinitions;
        this.status = $"Загружены настройки из файла {fileName} {loadDate.ToShortDateString} {loadDate.ToShortTimeString}";
    }
    
}

public class Paths
{
    public string RootDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    public string DocumentsDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Documents");
    public string FilesUploadingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/UploadedFiles");
    public string XmlDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Xml");
    public string ConfigurationsDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Configuration");
}








