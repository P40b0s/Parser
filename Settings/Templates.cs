
namespace SettingsWorker.Regexes
{
    class Quotations
    {
        /// <summary>
        /// Открывающая кавычка <<
        /// </summary>
        public const char LCav = '\u00AB';
        /// <summary>
        /// Закрывающая кавычка >>
        /// </summary>
        public const char RCav = '\u00BB';
        /// <summary>
        /// Обычная кавычка "
        /// </summary>
        public const char Cav = '\u0022';

        /// <summary>
        /// Открывающая кавычка в виде двух запятых
        /// </summary>
        public const char Cav3Left = '\u8220';
        /// <summary>
        /// Закрыващая кавычка в виде двух запятых
        /// </summary>
        public const char Cav3Right = '\u8221';
    }

    //(?<{RegexGroupNames.Date}>\d{{1,2}}){Ws}(?<{RegexGroupNames.Month}>{Months}){Ws}(?<{RegexGroupNames.Year}>\d{{4}}){Ws}(?:г.)
    public static class Templates
    {
        
        /// <summary>
        /// Все месяцы
        /// </summary>
        public const string Months = "январ[ья]|феврал[ья]|март[а]?|апрел[ья]|ма[йя]|июн[ья]|июл[ья]|август[а]?|сентябр[ья]|октябр[ья]|ноябр[ья]|декабр[ья]";
        /// <summary>
        /// Дата в формате 12 января 2021 г. (г, года, год)
        /// </summary>
        public static string FullDate => $@"(?<date>\d{{1,2}})\\s*(?<month>{Months})\\s*(?<year>\d{{4}})\\s*(?:[г.ода]*)";
        /// <summary>
        /// Форматирование символа юникода под группу регулярки, нипример - [\u8221]
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string GetUnicodeChar(char c) => $"[\\u{(int)c:x4}]";
        public static string GetEmptyUnicodeChar(char c) => $"\\u{(int)c:x4}";
        /// <summary>
        /// Левые кавычки
        /// </summary>
        public static string LeftQuotationMark => $@"(?:{GetUnicodeChar(Quotations.LCav)}|{GetUnicodeChar(Quotations.Cav)}|{GetUnicodeChar(Quotations.Cav3Left)})";
        
        /// <summary>
        /// Правые кавычки
        /// </summary>
        public static  string RightQuotationMark => $@"(?:{GetUnicodeChar(Quotations.RCav)}|{GetUnicodeChar(Quotations.Cav)}|{GetUnicodeChar(Quotations.Cav3Right)})";
        // \s + мягкий перенос, звезда
        public static string WsOrBr => $"(\\s|{GetUnicodeChar(Templates.BRChar)})*";
        /// <summary>
        /// Стандартный пробел + мягкий перенос, плюс
        /// </summary>
        /// <returns></returns>
        public static string WsOrBrPlus => $"(\u0020|{GetUnicodeChar(Templates.BRChar)})+";
        /// <summary>
        /// Пробел или мягкий перенос без модификаторов
        /// </summary>
        /// <returns></returns>
        public static string WsBr => $"(\u0020|{GetUnicodeChar(Templates.BRChar)})";
        public static char BRChar => Templates.NewLines.SplitNewLine;
        /// <summary>
        ///Пробелом или мягкий пернос (пробел и мягкий перенос не более 3 раз) + слово
        /// </summary>
        /// <returns></returns>
        public static string WsBrWord => $"({WsBr}{{1,3}}\\S+)";
        public static class NewLines
        {
            /// <summary>
            /// мягкий перенос строки (визуально перенос есть но абзац остается тот же)
            /// проблемы с ним одни, он как бы есть но его как бы нет, регекс его считает
            /// а substring например не считает, из-за этого возникают трудности с вытаскиванием 
            /// нужной строки, поменяю его на 200B он как бы пробел но с 0 шириной, потом может смогу заменить на перенос
            /// </summary>
            public const char SplitNewLine = '\u00ad';
            //CSS автоматически перенорсятся слова при таких значениях свйств
            // .long-read {
            //     -webkit-hyphens: auto;
            //     -moz-hyphens: auto;
            //     -ms-hyphens: auto;
            //      hyphens: auto;
            // }
            public const char CarretReturn = '\u000d';
            public const char NewLine = '\u000a';
            public const char ZeroWidthSpace = '\u200b';
            public const char IdeographicSpace = '\u3000';
        }

    }
}