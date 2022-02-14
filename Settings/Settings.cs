
using System.Collections.Generic;
using System.Threading.Tasks;
using SettingsWorker.Annex;
using SettingsWorker.Requisite;
using SettingsWorker.Regexes;
using System.Text.Json.Serialization;

namespace SettingsWorker;


public interface ISettings
{
    bool IsDefault {get;set;}
    void AddStatus(string status);
    AllRules DefaultRules {get;set;}
    AllTokensDefinitions TokensDefinitions {get;set;}
    List<CustomRule<AllRules>> CustomRules {get;set;}
    RequisiteChangers RequisiteChangers {get;set;}
    Dictionaries.AllDictionaries Dictionaries {get;set;}
    DocumentProcessing.PoolSettings PoolSettings {get;set;}
    Paths Paths {get;}
    ValueTask<bool> Load();
    List<string> Status {get;}
    void Save();
}
public partial class Settings : ISettings
{
    public Settings()
    {
        IsDefault = true;
    }
    [JsonIgnore]
    public List<string> Status {get;} = new List<string>();
    public bool IsDefault {get;set;} = false;
    public void AddStatus(string status) => Status.Add(status);
    private System.DateTime loadDate {get;set;} = DateTime.Now;
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
        new CustomRule<AllRules>(){Organ = $"российская{Templates.WsOrBr}федерация", Type = "закон", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {
                ParseGDSFAttributes = true,
            },
        },Weight = 1},
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
        new CustomRule<AllRules>(){Organ = $".+", Type = "соглашение", Rules = new AllRules() 
        {
            RequisiteRule = new RequisiteRule()
            {   NameInTypeString = true,
                NoExecutor = true,
                NoNumber = true,
                SignDateAfterCustomToken = true,
            },
        }},
    };
    public RequisiteChangers RequisiteChangers {get;set;} =  new RequisiteChangers(true);
    public Dictionaries.AllDictionaries Dictionaries {get;set;} = new Dictionaries.AllDictionaries();

    [System.Text.Json.Serialization.JsonIgnore]
    public Paths Paths {get;} = new Paths();

    public DocumentProcessing.PoolSettings PoolSettings {get;set;} = new DocumentProcessing.PoolSettings();
   

    private void update(Settings settings)
    {
        this.loadDate = System.DateTime.Now;
        this.CustomRules = settings.CustomRules;
        this.DefaultRules = settings.DefaultRules;
        this.TokensDefinitions = settings.TokensDefinitions;
        //this.status = $"Загружены настройки из файла {fileName} {loadDate.ToShortDateString} {loadDate.ToShortTimeString}";
    }
    
}










