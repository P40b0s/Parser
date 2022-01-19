   
   namespace Utils
   {
    public static class ScriptConverter
    {
        /// <summary>
        /// Такая сигнатура ответа, чтобы понимать что текст обработался
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SuperScriptConverter(string input)
        {
            if (input == null) return "";
            string output = "";
            foreach (var ch in input)
            {
                if(subscriptDictionary.ContainsKey(ch))
                     output += superscriptDictionary[ch];
                else output += ch;
            }
            return output;
        }

        public static string SubScriptConverter(string input)
        {
            if (input == null) return "";
            string output = "";
            foreach (var ch in input)
            {
                if(subscriptDictionary.ContainsKey(ch))
                     output += subscriptDictionary[ch];
                else output += ch;
            }
            return output;
        }
        public readonly static Dictionary<char, char> subscriptDictionary = new Dictionary<char, char>
        {
            { '0', '\u2080' },
            { '1', '\u2081' },
            { '2', '\u2082' },
            { '3', '\u2083' },
            { '4', '\u2084' },
            { '5', '\u2085' },
            { '6', '\u2086' },
            { '7', '\u2087' },
            { '8', '\u2088' },
            { '9', '\u2089' },
            { '+', '\u208A' },
            { '-', '\u208B' },
            { '=', '\u208C' },
            { '(', '\u208D' },
            { ')', '\u208E' },
        };

        public readonly static Dictionary<char, char> superscriptDictionary = new Dictionary<char, char>
        {
            { '0', '\u2070' },
            { '1', '\u00B9' },
            { '2', '\u00B2' },
            { '3', '\u00B3' },
            { '4', '\u2074' },
            { '5', '\u2075' },
            { '6', '\u2076' },
            { '7', '\u2077' },
            { '8', '\u2078' },
            { '9', '\u2079' },
            { '+', '\u207A' },
            { '-', '\u207B' },
            { '=', '\u207C' },
            { '(', '\u207D' },
            { ')', '\u207E' },
            { 'i', '\u2071' },
            { 'a', '\u2090' },
            { 'e', '\u2091' },
            { 'o', '\u2092' },
            { 'x', '\u2093' },
            { 'h', '\u2095' },
            { 'k', '\u2096' },
            { 'l', '\u2097' },
            { 'm', '\u2098' },
            { 'n', '\u2099' },
            { 'p', '\u209A' },
            { 's', '\u209B' },
            { 't', '\u209C' },
            { 'A', '\u1D2C' },
            //TODO дописать отсюда https://unicode-table.com/ru/blocks/phonetic-extensions/

        };
         
    }
}