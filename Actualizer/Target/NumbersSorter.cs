using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Actualizer.Source;
using Actualizer.Structure;

namespace Actualizer.Target;

public class NumbersSorter
{
    CultureInfo ci {get;}
    public NumbersSorter()
    {
        ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
    }
    /// <summary>
    /// Получаем номер который будет находится преред итемом который необходимо добавить
    /// </summary>
    /// <param name="numbers">массив номеров к которым необходимо добавить новый</param>
    /// <param name="target">номер который необходимо добавить</param>
    /// <returns></returns>
    public string GetItemNumberBefore(IEnumerable<string> numbers, PathUnit target)
    {
        var items = converter(numbers);
        var item = converter(target.Number);
        var lastItem = items.Where(w=>w.Item1 < item.Item1).Last();
        //Последний итем не этот а последний его вложенный узел!
        return lastItem.Item2;
    }
    List<(float, string)> converter(IEnumerable<string> numbers)
    {
        List<(float, string)> items = new List<(float, string)>();
        foreach(var n in numbers)
        {
            var cort = converter(n);
            items.Add(cort);
            
        }
        items = items.OrderBy(o=>o.Item1).ToList();
        return items;
    }
    
    (float, string) converter(string number)
    {
        (float, string) item = (0, "");
        
            string num = "";
            bool dot = false;
            bool romeOk = false;
            float floatPart = 0.0F;
            foreach(var ch in number)
            {
                if(CharDictionary.ContainsKey(ch))
                    num += CharDictionary[ch];
                //если точка уже есть то меняем ее на 0
                if(dot && ch == '.')
                    num = num.Replace(ch, '0');
                if(ch == '.')
                    dot = true;
                if(ch == 'I' || ch == 'X' || ch == 'V' || ch == 'L')
                {
                    if(!romeOk)
                    {
                        var rome = romeRx.Match(number);
                        var val = rome.Value;
                        if(RomeDictionary.ContainsKey(val))
                            num += RomeDictionary[val];
                        romeOk = true;
                    }
                    continue;
                }
                if(SSConverter.ContainsKey(ch))
                {
                    var str0 = "";
                    var str1 = "";
                    var ss = superScriptRegex.Match(number);
                    if(ss.Success)
                    {
                        var firstGroup = ss.Groups["digit"].Value;
                        var secondGroup =  ss.Groups["digit2"].Value == "" ? null : ss.Groups["digit2"].Value;
                        foreach(var d1 in firstGroup)
                        {
                            str0+= SSConverter[ch];
                        }
                        var fl0 = int.Parse(str0);
                        var r0 = "0.";
                        //делаем из числа тысячные - 0001; и добавляем целые, получается строка 0.0001
                        r0 = r0 +  fl0.ToString("D3");
                        //парсим ее и прибавляем к основному номеру
                        floatPart = floatPart + float.Parse(r0,NumberStyles.Any,ci);
                        if(secondGroup != null)
                        {
                            foreach(var d2 in secondGroup)
                            {
                                str1+= SSConverter[ch];
                            }
                            var fl1 = int.Parse(str1);
                            var r1 = "0.";
                            r1 = r1 +  fl1.ToString("D4");
                            floatPart = floatPart + float.Parse(r1,NumberStyles.Any,ci);
                        }
                    }
                    break;
                }
                num += ch;
            }
            var resultFloat = float.Parse(num,NumberStyles.Any,ci) + floatPart;
            item = (resultFloat, number);
        return item;
    }
    Regex superScriptRegex = new Regex("(?<digit>(\u207C|\u2070|\u00B9|\u00B2|\u00B3|\u2074|\u2075|\u2076|\u2077|\u2078|\u2079|\u207A)+)(\u207B)?(?<digit2>(\u207C|\u2070|\u00B9|\u00B2|\u00B3|\u2074|\u2075|\u2076|\u2077|\u2078|\u2079|\u207A)*)");

    readonly Dictionary<char, char> SSConverter = new Dictionary<char, char>
    {
        { '\u2070','0' },
        { '\u00B9','1' },
        { '\u00B2','2' },
        { '\u00B3','3' },
        { '\u2074','4' },
        { '\u2075','5' },
        { '\u2076','6' },
        { '\u2077','7' },
        { '\u2078','8' },
        { '\u2079','9' },
        // { '\u207A','+' },
            { '\u207B','-' },
        // { '\u207C','=' },
            { '\u207D','(' },
            { '\u207E',')' },
        // { '\u2071','i' },
        // { '\u2090','a' },
        // { '\u2091','e' },
        // { '\u2092','o' },
        // { '\u2093','x' },
        // { '\u2095','h' },
        // { '\u2096','k' },
        // { '\u2097','l' },
        // { '\u2098','m' },
        // { '\u2099','n' },
        // { '\u209A','p' },
        // { '\u209B','s' },
        // { '\u209C','t' },
        // { '\u1D2C','A' },
        
    };
    readonly Dictionary<char, string> CharDictionary = new Dictionary<char, string>
    {
        {'а',"1"},
        {'б',"2"},
        {'в',"3"},
        {'г',"4"},
        {'д',"5"},
        {'е',"6"},
        {'ж',"7"},
        {'з',"8"},
        {'и',"9"},
        {'к',"10"},
        {'л',"11"},
        {'м',"12"},
        {'н',"13"},
        {'о',"14"},
        {'п',"15"},
        {'р',"16"},
        {'с',"17"},
        {'т',"18"},
        {'у',"19"},
        {'ф',"20"},
        {'х',"21"},
        {'ц',"22"},
        {'ч',"23"},
        {'ш',"24"},
        {'щ',"25"},
        {'э',"26"},
        {'ю',"27"},
        {'я',"28"},
    };
    Regex romeRx = new Regex("[XVIL]+");
    readonly Dictionary<string, string> RomeDictionary = new Dictionary<string, string>
    {
        {"I","1"},
        {"II","2"},
        {"III","3"},
        {"IV","4"},
        {"V","5"},
        {"VI","6"},
        {"VII","7"},
        {"VIII","8"},
        {"IX","9"},
        {"X","10"},
        {"XI","11"},
        {"XII","12"},
        {"XIII","13"},
        {"XIV","14"},
        {"XV","15"},
        {"XVI","16"},
        {"XVII","17"},
        {"XVIII","18"},
        {"XIX","19"},
        {"XX","20"},
        {"XXI","21"},
        {"XXII","22"},
        {"XXIII","23"},
        {"XXIV","24"},
        {"XXV","25"},
        {"XXVI","26"},
        {"XXVII","27"},
        {"XXVIII","28"},
        {"XXIX","29"},
        {"XXX","30"},
        {"XXXI","31"},
        {"XXXII","32"},
        {"XXXIII","33"},
        {"XXXIV","34"},
        {"XXXV","35"},
        {"XXXVI","36"},
        {"XXXVII","37"},
        {"XXXVIII","38"},
        {"XXXIX","39"},
        {"XL","40"},
        {"XLI","41"},
        {"XLII","42"},
        {"XLIII","43"},
        {"XLIV","44"},
        {"XLV","45"},
        {"XLVI","46"},
        {"XLVII","47"},
        {"XLVIII","48"},
        {"XLIX","49"},
        {"L","50"},
    };
}