using System.Text.RegularExpressions;

namespace Settings;
public struct CustomRule<T>
{
    public string Organ {get;set;}
    public Regex OrganRX() => Organ!=null ? new Regex(Organ, RegexOptions.IgnoreCase | RegexOptions.Compiled) : new Regex("");
    public string Type {get;set;}
    public Regex TypeRX() => Type!=null ? new Regex(Type, RegexOptions.IgnoreCase | RegexOptions.Compiled) : new Regex("");
    public T Rules {get;set;}
}