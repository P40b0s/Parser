using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Documents.Parser.Extensions
{
    public static class RegexExtensions
    {
        /// <summary>
        /// Представить как группу
        /// </summary>
        /// <param name="s"></param>
        /// <param name="groupName">Если название группы пустое то группа будет обозначена как нерегестрируемая</param>
        /// <returns></returns>
        public static string AsRegexGroup(this string s, string groupName) => groupName == "" ? $"(?:{s})" : $"(?<{groupName}>{s})";
        public static string IsOptional(this string s) =>  "("+s+")"+"?";

        //public static string ReplaceWspaces(this string txt, string change) => new Regex(@"[ ]{1,}", RegexOptions.Compiled).Replace(txt, change);
        public static bool IsMatch(this string s, string pattern)
        {
            var r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return r.IsMatch(s);

        }


    }
}
