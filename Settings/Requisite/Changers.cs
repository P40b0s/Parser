using System.Text.RegularExpressions;
using SettingsWorker;
using SettingsWorker.Regexes;

namespace SettingsWorker.Requisite;

public struct Changer
{
    public string From {get;set;}
    public Regex FromRx() => new Regex(From, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public string To {get;set;}
}

public enum ChangeType
{
    OrganToOrgan,
    TypeToOrgan,
    TypeToType,
    OrganToType
}

/// <summary>
/// Изменение органов или типов документов согласно шаблону
/// </summary>
[PropertyAttribute(about: "Изменение органов или типов документов согласно шаблону")]
[Serializable]
public class RequisiteChangers
{
    public RequisiteChangers(){}
    public RequisiteChangers(bool  withDefault)
    {
        OrganToOrgan  = new List<Changer>()
        {
            new Changer()
            {
                From = $"^российской{Templates.WsOrBr}федерации",
                To = "Российская Федерация"
            },
            new Changer()
            {
                From = $"^президента{Templates.WsOrBr}российской{Templates.WsOrBr}федерации",
                To = "Президент Российской Федерации"
            }
        };
        TypeToOrgan = new List<Changer>()
        {
            new Changer()
            {
                From = $"^соглашение",
                To = "Российская Федерация"
            }
        };
        OrganToType = new List<Changer>();
        TypeToType = new List<Changer>();
    }
    
    public List<Changer> OrganToOrgan {get;set;}
    
    public List<Changer> OrganToType {get;set;} 

    public List<Changer> TypeToOrgan {get;set;} 
    public List<Changer> TypeToType {get;set;}

    public string Change(ChangeType ct, string from)
    {
        var changer = SelectChanger(ct);
        for(int i = 0; i < changer.Count; i++)
        {
            if(changer[i].FromRx().IsMatch(from))
                return changer[i].FromRx().Replace(from, changer[i].To);
        }
        return "";
    }

    private List<Changer> SelectChanger(ChangeType ch) => ch switch 
    {
        ChangeType.OrganToOrgan => OrganToOrgan,
        ChangeType.TypeToOrgan => TypeToOrgan,
        ChangeType.OrganToType => OrganToType,
        ChangeType.TypeToType => TypeToType,
        _ => OrganToOrgan
    };
}