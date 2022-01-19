namespace Lexer
{
    public interface ITextIndex
    {
        int StartIndex {get;}
        int EndIndex {get;}
        int Length {get;}
    }
    public class TextIndex : ITextIndex
    {
        public TextIndex(int start, int length)
        {
            StartIndex = start;
            Length = length;
        }
        public int StartIndex {get;}
        public int EndIndex {get;}
        public int Length {get;}
    }
}