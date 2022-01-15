using Lexer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentParser.Parsers
{
    public abstract class ParserBase<T> : ParserBase
    {
        protected ILexerService<T> lexer {get;} = new Lexer<T>();
        public List<Token<T>> tokens = new List<Token<T>>();
    }

    public abstract class ParserBase
    {
        /// <summary>
        /// UpdateCallback = o => CurrentStatus = o;
        /// </summary>
        /// <value></value>
        public StatusUpdate UpdateCallback { get; set; }
        public ErrorUpdate ErrorCallback { get; set; }
        public List<ParserException> exceptions { get; set; } = new List<ParserException>();
        protected void Status(string status, bool debug = false)
        {
            if(debug)
                Console.WriteLine(status);
            Task.Factory.StartNew(()=> this.UpdateCallback?.Invoke(status));
        }
        protected void Error(List<ParserException> pe)
        {
            Task.Factory.StartNew(()=> this.ErrorCallback?.Invoke(pe));
        }
        protected void Percentage(string message, int allCount, int current, bool debug = false)
        {
            Status($"{message} {getPercentage(allCount, current)}%", debug);
        }
        protected int getPercentage(int all, int current)
        {
            var result = (current * 100) / all;
            return result > 100 ? 100 : result;
        }
    }
}
