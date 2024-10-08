using System;

namespace DocumentParser.DocumentElements
{
    
    public class Run
    {
        public Guid Id {get;} = Guid.NewGuid();
        public string Text {get;set;} = "";
        public string CommentId {get;set;}
        public RunProperties Properties {get;set;}
        public Image Image {get;set;}
        public string FormulaLatexFormat {get;set;}
        public string FormulaMathMlFormat {get;set;}
        public int Index { get; set; }
        public bool HaveFormula => !string.IsNullOrEmpty(FormulaLatexFormat);
        public bool HaveImage => Image.HaveImage;
    }

    public struct Thumb
    {
        public Thumb(byte[] data, long sizeX, long sizeY)
        {
            Data = data;
            SizeX = sizeX;
            SizeY = sizeY;
        }
        public byte[] Data { get; }
        public long SizeX { get; }
        public long SizeY { get; }
    }

    public struct Image
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public Image(Guid id, byte[] data, long sizeX, long sizeY, Thumb th)
        {
            Data = data;
            SizeX = sizeX;
            SizeY = sizeY;
            Id = id;
            if(Data != null)
            {
                HaveImage = true;
                Length = Data.Length;
            }
            else
            {
                HaveImage = false;
                Length = 0;
            }
            Thumbland = th;
        }
        /// <summary>
        /// эскиз не нужен потому что изображение маленькое и мы храним его тут целиком
        /// </summary>
        /// <param name="img"></param>
        /// <param name="id"></param>
        public Image(Image img, Guid id)
        {
            Data = img.Data;
            SizeX = img.SizeX;
            SizeY = img.SizeY;
            Id = id;
            
            if(Data != null)
            {
                HaveImage = true;
                Length = Data.Length;

            }
            else
            {
                HaveImage = false;
                Length = 0;
            }
            Thumbland = new Thumb(null, 0, 0);
        }
        /// <summary>
        /// Имадж есть но он еще не загружен с сервера
        /// </summary>
        /// <param name="id"></param>
        public Image(Guid id, int lenth, Thumb th)
        {
            Data = null;
            SizeX = 0;
            SizeY = 0;
            Id = id;
            HaveImage = true;
            Length = lenth;
            Thumbland = th;
        }
        public Guid Id {get;}
        public bool HaveImage {get;}
        public bool IsLoad => HaveImage && Data == null;
        public int Length {get;}
        public byte[] Data { get; }
        public long SizeX { get; }
        public long SizeY { get; }
        public Thumb Thumbland {get;}
    }
}
