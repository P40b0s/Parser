using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils.Extensions
{
    public static class StringExtensions
    {
        static readonly Regex replaceRx = new Regex($"(\\s+|(\n|\r\n)|[\"]|[.]|,|№|-|[\u00ad])", RegexOptions.Compiled);
        // public static string GetHash(this string text)
        // {
        //     string rstring = replaceRx.Replace(text, "").ToLower();
        //     if (!string.IsNullOrEmpty(rstring))
        //     {
        //         using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        //         {
        //             byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(rstring);
        //             byte[] hashBytes = md5.ComputeHash(inputBytes);
        //             StringBuilder sb = new StringBuilder();
        //             for (int i = 0; i < hashBytes.Length; i++)
        //             {
        //                 sb.Append(hashBytes[i].ToString("X2"));
        //             }
        //             return sb.ToString();
        //         }
        //     }
        //     else return string.Empty;
        // }
        public static string ToSearchString(this string txt) => replaceRx.Replace(txt, "").ToLower();
        /// <summary>
        /// Вычистяет хэш строки
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string? GetHash(this string text)
        {
            string rstring = text.ReplaceWspaces("").ToLower();
            if (!string.IsNullOrEmpty(rstring))
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(rstring);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    //StringBuilder sb = new StringBuilder();
                    string sb = "";
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb+=(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            else return null;
        }
        /// <summary>
        /// Удаляем все знаки пробела
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="change"></param>
        /// <returns></returns>
        public static string ReplaceWspaces(this string txt, string change) => new Regex(@"[\s]{1,}", RegexOptions.Compiled).Replace(txt, change);
        /// <summary>
        /// Оставляет между словами только один знак пробела
        /// </summary>
        public static string NormalizeWhiteSpaces(this string txt) => new Regex(@"[\s]{1,}", RegexOptions.Compiled).Replace(txt, " ");
        /// <summary>
        /// Первая буква большая остальные маленькие можно несколько слов, только после номализации пробелов
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        // public static string NormalizeCase(this string txt)
        // {
        //     var arr = txt.ToArray();
        //     for(int i = 0; i < arr.Length; i++)
        //     {
        //         if (i == 0)
        //             arr[i] = char.ToUpper(arr[i]);
        //         else arr[i] = char.ToLower(arr[i]);
        //     }
        //     return new string(arr);
        // }
        /// <summary>
        /// У каждого слова делаем первую букву большой, остальные маленькими
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string NormalizeCase(this string txt)
        {
            var arr = txt.ToArray();
            for(int i = 0; i < arr.Length; i++)
            {
                if (i == 0 || (i - 1 >= 0 && char.IsWhiteSpace(arr[i-1])))
                    arr[i] = char.ToUpper(arr[i]);
                else arr[i] = char.ToLower(arr[i]);
            }
            return new string(arr);
        }

        /// <summary>
        /// Вставляем пробел между буквами например - постановлениеПравительства
        /// здесь из-за мягкого переноса слипаются слова, их надо расклеить
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string SplitCase(this string txt)
        {
            int pref = 3;
            string pattern = $@"([а-я]{{{pref}}})([А-Я]{{1}})";
            Regex r = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);
            r.Replace(txt, "$1 $2");
            //var matches = r.Matches(txt);
            //var m = matches.Select(s => s.Index + pref);
            //foreach (var m1 in m)
            //{
            //    txt = txt.Insert(m1, " ");
            //}
            return txt;
        }


    }
}