using Services.Documents.Core;
using System;
using System.Linq;
using RunProperties = DocumentParser.DocumentElements.RunProperties;

namespace DocumentParser.Workers
{    
    
    public static class DataConverter
    {
        
        //1 cm = 2.54 inches
        //1 pt = inch * 72
        //1 dxa = pt * 20

        public static long EmuToPixels(double size)
        {
            return (long)Math.Round(size / 9525);
        }
        //1pt = 12700 EMU
        public static long PtToPixels(double size)
        {
            var emuSize = size * 12700;
            return (long)Math.Round(emuSize / 9525);
        }
        //1 px = 0.75 point
        public static long DxaToPixels(double size)
        {
            var pt = size / 20;
            var pixels = pt / 0.75; 
            return (long)Math.Round(pixels);
        }
        public static long EmuToPixels(string size)
        {
            return EmuToPixels(parserString(size));
        }
        /// <summary>
        /// 1 twip = 0.0666666667 pixel
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static long TwipsToPixels(string size)
        {
            //1 twip = 0.0666666667 pixel (X)
            var c = 0.0666666667;
            var pixels = parserString(size) * c;
            return (long)Math.Round(pixels);
        }
        //1pt = 12700 EMU
        public static long PtToPixels(string size)
        {
            return PtToPixels(parserString(size));
        }
        //1 px = 0.75 point
        public static long DxaToPixels(string size)
        {
            return DxaToPixels(parserString(size));
        }

        static double parserString(string val)
        {
            double d = 0;
            double.TryParse(val, out d);
            return d;
        }
        //FIXME если хоть один символ не проходить по словарю символов то не меняем ни один из последоватекльности
        public static (string, bool) ConvertText(string txt, RunProperties props)
        {
            (string, bool) t = ("", false);
            if (string.IsNullOrEmpty(txt))
                return t;
            if (props?.VerticalAligment == "superscript")
            {
                if(txt.All(a=>ScriptConverter.superscriptDictionary.ContainsKey(a)))
                {
                    t = (ScriptConverter.SuperScriptConverter(txt), true);
                    props.VerticalAligment = null;
                }
                else
                {
                    t = (txt, false);
                }
            }
            else if (props?.VerticalAligment == "subscript")
            {
                if(txt.All(a=>ScriptConverter.subscriptDictionary.ContainsKey(a)))
                {
                    t = (ScriptConverter.SubScriptConverter(txt), true);
                    props.VerticalAligment = null;
                }
                else
                {
                    t = (txt, false);
                }
            }
            else
            {
                t = (txt, false);
            }
            return t;
        }
    }
}
