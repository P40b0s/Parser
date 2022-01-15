using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Services.Documents.Parser.Parsers.Requisites
{
    public static class RequisitesChanger
    {
        private const string OrgansFile = "configs/OrgansChanger.json";
        private const string TypesFile = "configs/TypesChanger.json";
        private const string TypeByOrganFile = "configs/TypeByOrganChanger.json";

        public static void LoadChangers()
        {
            if(System.IO.File.Exists(OrgansFile))
            {
                var org = System.IO.File.ReadAllText(OrgansFile);
                RequisitesChanger.OrgansChanger = JsonConvert.DeserializeObject<List<(string, string)>>(org);
                foreach(var c in RequisitesChanger.OrgansChanger)
                    RequisitesChanger.OrgansChangerRx.Add((new Regex(c.source), c.target));
            }
            if(System.IO.File.Exists(TypesFile))
            {
                var tp = System.IO.File.ReadAllText(TypesFile);
                RequisitesChanger.TypesChanger = JsonConvert.DeserializeObject<List<(string, string)>>(tp);
                foreach(var c in RequisitesChanger.TypesChanger)
                    RequisitesChanger.TypesChangerRx.Add((new Regex(c.source), c.target));
            }
            if(System.IO.File.Exists(TypeByOrganFile))
            {
                var tporg = System.IO.File.ReadAllText(TypeByOrganFile);
                RequisitesChanger.TypeByOrganChanger = JsonConvert.DeserializeObject<List<(string, string)>>(tporg);
                foreach(var c in RequisitesChanger.TypeByOrganChanger)
                    RequisitesChanger.TypeByOrganChangerRx.Add((new Regex(c.source), c.target));
            }
        }
        public static void SaveChangers()
        {
            System.IO.Directory.CreateDirectory("configs");
            if(RequisitesChanger.OrgansChanger.Count > 0)
            {
                var org = JsonConvert.SerializeObject(RequisitesChanger.OrgansChanger);
                System.IO.File.WriteAllText(OrgansFile, org);
            }
            if(RequisitesChanger.TypesChanger.Count > 0)
            {
                var tp = JsonConvert.SerializeObject(RequisitesChanger.TypesChanger);
                System.IO.File.WriteAllText(TypesFile, tp);
            }
            if(RequisitesChanger.TypeByOrganChanger.Count > 0)
            {
                var orgtp = JsonConvert.SerializeObject(RequisitesChanger.TypeByOrganChanger);
                System.IO.File.WriteAllText(TypeByOrganFile, orgtp);
            }
        }
        public static List<(string source, string target)> TypeByOrganChanger = new List<(string source, string target)>();
        public static List<(string source, string target)> OrgansChanger = new List<(string source, string target)>();
        public static List<(string source, string target)> TypesChanger = new List<(string source, string target)>();
        static List<(Regex source, string target)> OrgansChangerRx = new List<(Regex source, string target)>();
        static List<(Regex source, string target)> TypesChangerRx = new List<(Regex source, string target)>();
        static List<(Regex source, string target)> TypeByOrganChangerRx = new List<(Regex source, string target)>();
        public static void Change(Services.Documents.Core.DocumentElements.Document d)
        {
            //Заменяем органы
            foreach(var ch in OrgansChangerRx)
                foreach(var organ in d.Organs)
                    ch.source.Replace(organ.val, ch.target);
            //Зменяем типы документов в завимости от органов
            foreach(var ch in TypeByOrganChangerRx)
                foreach(var organ in d.Organs)
                    if(ch.source.IsMatch(organ.val))
                        d.Type = ch.target;
            //Хаменяем типы документов
            foreach(var ch in TypesChangerRx)
                ch.source.Replace(d.Type, ch.target);
        }
    }
}