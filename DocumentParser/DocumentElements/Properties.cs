using Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentParser.DocumentElements
{

    public enum TextAlignmentEnum
    {
        center,
        justify,
        left,
        right
    }
    
    public interface IParagraphProperties : IEquatable<IParagraphProperties>
    {
        RunProperties RProperties { get; set; }
        ParagraphSpacing Spacing { get; set; }
        TextAlignmentEnum Alignment { get; set; }
        Indentation Ind { get; set; }

    }

    public interface IRunProperties : IEquatable<IRunProperties>
    {
        RunFontsType RunFonts { get; set; }
        RunSpacing Spacing { get; set; }
        /// <summary>
        ///  <w:vertAlign w:val="superscript"/> subscript/superscript (Верхняя и нижняя сноска для текста)
        /// </summary>
        string VerticalAligment { get; set; }
        //RunStyleProperties runStyleProperties { get; set; }
        /// <summary>
        /// w:b
        /// </summary>
        bool IsBold { get; set; }
        /// <summary>
        /// w:i
        /// </summary>
        bool IsItalic { get; set; }
        /// <summary>
        /// w:caps
        /// </summary>
        bool Caps { get; set; }
        /// <summary>
        /// Specifies that any lowercase characters are to be displayed as their uppercase equivalents in a font size two points smaller than the specified font size:<para/> w:smallCaps w:val="true". It cannot appear with caps in the same text run. This is a toggle property.
        /// </summary>
        bool SmallCaps { get; set; }
        /// <summary>
        /// Цвет в шеснадцатиричном формате RGB (000000)
        /// </summary>
        string Color { get; set; }
        /// <summary>
        /// Для высоты в пикселях разделить на 2
        /// </summary>
        int FontSize { get; set; }
        double NormalFontSize { get; }
        /// <summary>
        /// Если это формула то сюда ставим true
        /// </summary>
        bool IsMathFormula { get; set; }
    }

    /// <summary>
    /// w:rFonts
    /// </summary>
    public class RunFontsType : IEquatable<RunFontsType>
    {
       // public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Определяет шрифт для форматирования текста в диапазоне юникода (U+0000-U+007F)
        /// </summary>
        public string Ascii { get; set; }
        /// <summary>
        /// Определяет шрифт для форматирования текста в диапазоне юникода который не подходит под остальные категории
        /// </summary>
        public string HighAnsi { get; set; }
        /// <summary>
        /// Определяет шрифт для форматирования текста в диапазоне комплексных скриптов юникода (арабский язык)
        /// </summary>
        public string ComplexScript { get; set; }
        /// <summary>
        /// Определяет шрифт для форматирования текста в диапазоне юникода восточной азии (иероглифы всякие)
        /// </summary>
        public string EastAsia { get; set; }

        public override bool Equals(object other)
        {
            //Последовательность проверки должна быть именно такой.
            //Если не проверить на null объект other, то other.GetType() может выбросить //NullReferenceException.            
            if (other == null)
                return false;

            //Если ссылки указывают на один и тот же адрес, то их идентичность гарантирована.
            if (object.ReferenceEquals(this, other))
                return true;

            //Если класс находится на вершине иерархии или просто не имеет наследников, то можно просто
            //сделать StructRunFonts tmp = other as StructRunFonts; if(tmp==null) return false; 
            //Затем вызвать экземплярный метод, сразу передав ему объект tmp.
            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as RunFontsType);
        }
        public bool Equals(RunFontsType other)
        {
            if (other == null)
                return false;

            //Здесь сравнение по ссылкам необязательно.
            //Если уверены, что многие проверки на идентичность будут отсекаться на проверке по ссылке - //можно имплементировать.
            if (object.ReferenceEquals(this, other))
                return true;

            //Если по логике проверки, экземпляры родительского класса и класса потомка могут считаться равными,
            //то проверять на идентичность необязательно и можно переходить сразу к сравниванию полей.
            if (this.GetType() != other.GetType())
                return false;

            if(string.Compare(this.Ascii ?? string.Empty, other.Ascii ?? string.Empty) == 0
            && string.Compare(this.ComplexScript ?? string.Empty, other.ComplexScript ?? string.Empty) == 0
            && string.Compare(this.EastAsia ?? string.Empty, other.EastAsia ?? string.Empty) == 0
            && string.Compare(this.HighAnsi ?? string.Empty, other.HighAnsi ?? string.Empty) == 0)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int) 2166136261;
                hash = (16777619 * hash) ^ (Ascii?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (ComplexScript?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (EastAsia?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (HighAnsi?.GetHashCode() ?? 0);
                return hash;
            }
        }

    }
    /// <summary>
    /// w:shd
    /// </summary>
    public class Shading : IEquatable<Shading>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// clear (no pattern), pct10, pct12, pct15 . . ., diagCross, diagStripe, horzCross, horzStripe, nil, thinDiagCross, solid, etc.
        /// </summary>
        public string Val { get; set; }
        public string Color { get; set; }
        public string Fill { get; set; }
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as Shading);
        }
        public bool Equals(Shading other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (string.Compare(this.Val ?? string.Empty, other.Val ?? string.Empty) == 0
            && string.Compare(this.Color ?? string.Empty, other.Color ?? string.Empty) == 0
            && string.Compare(this.Fill ?? string.Empty, other.Fill ?? string.Empty) == 0)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (Val?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (Color?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (Fill?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
    /// <summary>
    /// w:bdr
    /// </summary>
    public class TextBorder : IEquatable<TextBorder>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// <para/>single - a single line
        /// <para/>dashDotStroked - a line with a series of alternating thin and thick strokes
        /// <para>dashed - a dashed line</para> 
        /// <para>dashSmallGap - a dashed line with small gaps</para> 
        /// <para>dotDash - a line with alternating dots and dashes</para> 
        /// <para>dotDotDash - a line with a repeating dot - dot - dash sequence</para> 
        /// <para>dotted - a dotted line</para> 
        /// <para>double - a double line</para> 
        /// <para>doubleWave - a double wavy line</para> 
        /// <para>inset - an inset set of lines</para> 
        /// <para>nil - no border</para> 
        /// <para>none - no border</para> 
        /// <para>outset - an outset set of lines</para> 
        /// <para>thick - a single line</para> 
        /// <para>thickThinLargeGap - a thick line contained within a thin line with a large-sized intermediate gap</para> 
        /// <para>thickThinMediumGap - a thick line contained within a thin line with a medium-sized intermediate gap</para> 
        /// <para>thickThinSmallGap - a thick line contained within a thin line with a small intermediate gap</para> 
        /// <para>thinThickLargeGap - a thin line contained within a thick line with a large-sized intermediate gap</para> 
        /// <para>thinThickMediumGap - a thick line contained within a thin line with a medium-sized intermediate gap</para> 
        /// <para>thinThickSmallGap - a thick line contained within a thin line with a small intermediate gap</para> 
        /// <para>thinThickThinLargeGap - a thin-thick-thin line with a large gap</para> 
        /// <para>thinThickThinMediumGap - a thin-thick-thin line with a medium gap</para> 
        /// <para>thinThickThinSmallGap - a thin-thick-thin line with a small gap</para> 
        /// <para>threeDEmboss - a three-staged gradient line, getting darker towards the paragraph</para> 
        /// <para>threeDEngrave - a three-staged gradient like, getting darker away from the paragraph</para> 
        /// <para>triple - a triple line</para> 
        /// <para>wave - a wavy line</para> 
        /// </summary>
        public string Val { get; set; }
        public int Space { get; set; }
        public int Size { get; set; }
        public string Color { get; set; }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as TextBorder);
        }
        public bool Equals(TextBorder other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (string.Compare(this.Val ?? string.Empty, other.Val ?? string.Empty) == 0
            && this.Space == other.Space
            && this.Size == other.Size
            && string.Compare(this.Color ?? string.Empty, other.Color ?? string.Empty) == 0)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (Val?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (Space.GetHashCode());
                hash = (16777619 * hash) ^ (Size.GetHashCode());
                hash = (16777619 * hash) ^ (Color?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    /// <summary>
    /// w:highlight
    /// </summary>
    public class Highlight :  IEquatable<Highlight>
    {
       // public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// black, blue, cyan, darkBlue, darkCyan, darkGray, darkGreen, darkMagenta, darkRed, darkYellow, green, lightGray, magenta, none, red, white, yellow
        /// </summary>
        public string Val { get; set; }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as Highlight);
        }
        public bool Equals(Highlight other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (string.Compare(this.Val ?? string.Empty, other.Val ?? string.Empty) == 0)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (Val?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
    /// <summary>
    /// w:spacing
    /// </summary>
    public class ParagraphSpacing : IEquatable<ParagraphSpacing>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        //public string Val { get; set; }
        public long Line { get; set; }
        public long After { get; set; }
        private string _rule { get; set; } = null;
        /// <summary>
        /// atLeast, exactly, auto
        /// </summary>
        public string LineRule {
            get => _rule;
            set
            {
                if(value is "auto" or null or "atLast" or "exactly")
                {
                    _rule = value;
                }
            }
        }



        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as ParagraphSpacing);
        }
        public bool Equals(ParagraphSpacing other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (string.Compare(this.LineRule ?? string.Empty, other.LineRule ?? string.Empty) == 0
                && this.Line == other.Line
                && this.After == other.After)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (Line.GetHashCode());
                hash = (16777619 * hash) ^ (After.GetHashCode());
                hash = (16777619 * hash) ^ (LineRule?.GetHashCode() ?? 0);
                return hash;
            }
        }

    }
    /// <summary>
    /// w:spacing (Растягивание текста по ширине)
    /// </summary>
    public class RunSpacing :  IEquatable<RunSpacing>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// from 1% to the maximum 600%
        /// </summary>
        public long Val { get; set; }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as RunSpacing);
        }
        public bool Equals(RunSpacing other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (this.Val == other.Val)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (Val.GetHashCode());
                return hash;
            }
        }
    }
    /// <summary>
    /// Отступ текста от границы w:ind
    /// </summary>
    public class Indentation : IEquatable<Indentation>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public long Left { get; set; }
        public long Right { get; set; }
        public long FirstLine { get; set; }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as Indentation);
        }
        public bool Equals(Indentation other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if ((this.Left != other.Left) || (this.Right != other.Right) || (this.FirstLine != other.FirstLine))
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (Left.GetHashCode());
                hash = (16777619 * hash) ^ (Right.GetHashCode());
                hash = (16777619 * hash) ^ (FirstLine.GetHashCode());
                return hash;
            }
        }
    }

    ///// <summary>
    ///// w:rPr
    ///// </summary>
    //public class RunStyleProperties :  IEquatable<RunStyleProperties>
    //{
    //    //public Guid Id { get; set; } = Guid.NewGuid();
    //    /// <summary>
    //    /// subscript/superscript (Верхняя и нижняя сноска для текста)
    //    /// </summary>
    //    public string VerticalAligment { get; set; }
    //    public bool Bold { get; set; }
    //    public override bool Equals(object other)
    //    {
    //        if (other == null)
    //            return false;

    //        if (object.ReferenceEquals(this, other))
    //            return true;

    //        if (this.GetType() != other.GetType())
    //            return false;

    //        return this.Equals(other as RunStyleProperties);
    //    }
    //    public bool Equals(RunStyleProperties other)
    //    {
    //        if (other == null)
    //            return false;

    //        if (object.ReferenceEquals(this, other))
    //            return true;

    //        if (this.GetType() != other.GetType())
    //            return false;
    //        if (this.VerticalAligment?.Equals(other.VerticalAligment) ?? other.VerticalAligment == null
    //            && this.Bold == other.Bold)
    //            return true;
    //        else return false;
    //    }

    //    public override int GetHashCode()
    //    {
    //        unchecked
    //        {
    //            // https://stackoverflow.com/a/263416/4340086
    //            int hash = (int)2166136261;
    //            hash = (16777619 * hash) ^ (VerticalAligment?.GetHashCode() ?? 0);
    //            hash = (16777619 * hash) ^ (Bold.GetHashCode());
    //            return hash;
    //        }
    //    }
    //}

    /// <summary>
    /// w:rPr
    /// </summary>
    public class RunProperties : IRunProperties
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public RunFontsType RunFonts { get; set; }
        public RunSpacing Spacing { get; set; }
        //public RunStyleProperties runStyleProperties {get;set;}
        /// <summary>
        /// w:b
        /// </summary>
        public bool IsBold { get; set; } = false;
        /// <summary>
        /// w:i
        /// </summary>
        public bool IsItalic { get; set; } = false;
        /// <summary>
        /// w:caps
        /// </summary>
        public bool Caps { get; set; } = false;
        /// <summary>
        /// Specifies that any lowercase characters are to be displayed as their uppercase equivalents in a font size two points smaller than the specified font size:<para/> w:smallCaps w:val="true". It cannot appear with caps in the same text run. This is a toggle property.
        /// </summary>
        public bool SmallCaps { get; set; } = false;
        public string Color { get; set; } = "000000";
        /// <summary>
        /// Для высоты в пикселях разделить на 2
        /// </summary>
        public int FontSize { get; set; } = 0;
        //1 pt = 1.3281472327365 px
        public double NormalFontSize
        {
            get
            {
                if (FontSize != 0)
                    return (FontSize / 2) * 1.33;
                else
                    return 0;
             }
        }
        /// <summary>
        ///  <w:vertAlign w:val="superscript"/> subscript/superscript (Верхняя и нижняя сноска для текста)
        /// </summary>
        public string VerticalAligment { get; set; } = null;
        public bool IsMathFormula { get; set; } = false;
        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as IRunProperties);
        }
        public bool Equals(IRunProperties other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if ((this.RunFonts?.Equals(other.RunFonts) ?? other.RunFonts == null)
           && (this.Spacing?.Equals(other.Spacing) ?? other.Spacing == null)
           && this.IsBold == other.IsBold
           && this.IsItalic == other.IsItalic
           && this.Caps == other.Caps
           && this.SmallCaps == other.SmallCaps
           && string.Compare(this.Color, other.Color) == 0 //По умолчанию если нет значения то считаем его черным цветом
           && this.FontSize == other.FontSize
           && this.VerticalAligment == other.VerticalAligment
           && this.IsMathFormula == other.IsMathFormula)
              return true;
            else
              return false;
        }

        public string GetMD5()
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(this);
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
               return  serialized.GetHash();
                //byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(serialized);
                //byte[] hashBytes = md5.ComputeHash(inputBytes);
                //string value = "";
                //foreach (var byt in hashBytes)
                //    value += String.Format("{0:X2} ", byt);
                //return value;
            }
        }

        public override int GetHashCode()
        {
           

            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (RunFonts?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (Spacing?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (IsBold.GetHashCode());
                hash = (16777619 * hash) ^ (IsItalic.GetHashCode());
                hash = (16777619 * hash) ^ (Caps.GetHashCode());
                hash = (16777619 * hash) ^ (SmallCaps.GetHashCode());
                hash = (16777619 * hash) ^ (Color?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (FontSize.GetHashCode());
                hash = (16777619 * hash) ^ (IsMathFormula.GetHashCode());
                return hash;
            }
        }

        public void UpdateFromParagraphStyle(RunProperties rPr)
        {
            if(rPr != null)
            {
                if (RunFonts == null)
                    RunFonts = rPr.RunFonts;
                if (Spacing == null)
                    Spacing = rPr.Spacing;
                if (FontSize == 0)
                    FontSize = rPr.FontSize;
                if (VerticalAligment == null)
                    VerticalAligment = rPr.VerticalAligment;
                if (!IsBold)
                    IsBold = rPr.IsBold;
                if (!IsMathFormula)
                    IsMathFormula = rPr.IsMathFormula;
                if (!IsItalic)
                    IsItalic = rPr.IsItalic;
                if (!Caps)
                    Caps = rPr.Caps;
                if (!SmallCaps)
                    SmallCaps = rPr.SmallCaps;
                if (Color == "000000")
                    Color = rPr.Color;
            }
        }
    }
   
    /// <summary>
    /// w:pPr
    /// </summary>
    public class ParagraphProperties : IParagraphProperties
    {
        //если мы добавляем сюда id то каждлое свойство станет уникальным а нам нужно находить одинаковые....
        //public Guid Id { get; set; } = Guid.NewGuid();
        public RunProperties RProperties {get;set;}
        public ParagraphSpacing Spacing { get; set; }
        public TextAlignmentEnum Alignment { get; set; }
        public Indentation Ind { get; set; }
        public bool IsChange { get; set; }
        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as IParagraphProperties);
        }
        public bool Equals(IParagraphProperties other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (this.RProperties?.Equals(other.RProperties) ?? other.RProperties == null
            && (this.Spacing?.Equals(other.Spacing) ?? other.Spacing == null)
            && this.Alignment == other.Alignment
            && (this.Ind?.Equals(other.Ind) ?? other.Ind == null))
                return true;
            else
                return false;
        }

        public string GetMD5()
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(this);
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                return serialized.GetHash();
                //byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(serialized);
                //byte[] hashBytes = md5.ComputeHash(inputBytes);
                //string value = "";
                //foreach (var byt in hashBytes)
                //    value += String.Format("{0:X2} ", byt);
                //return value;
            }
        }
        public override int GetHashCode()
        {
            unchecked
            {
                // https://stackoverflow.com/a/263416/4340086
                int hash = (int)2166136261;
                hash = (16777619 * hash) ^ (RProperties?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (Spacing?.GetHashCode() ?? 0);
                hash = (16777619 * hash) ^ (Alignment.GetHashCode());
                hash = (16777619 * hash) ^ (Ind?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
