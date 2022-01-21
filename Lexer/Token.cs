using System;
using System.Collections.Generic;
using System.Linq;
using Lexer.Tokenizer;
using System.Runtime.CompilerServices;
namespace Lexer
{
    public partial class Token<T> : ITextIndex
    {
        public Token(T tokenType)
        {
            TokenType = tokenType;
            Value = string.Empty;
            CustomGroups = new List<GroupMatch>();
        }

        public Token(T tokenType, int startIndex, int endIndex, string value, List<GroupMatch> c_groups, int position, List<Token<T>> tokensList, string converted)
        {
            TokenType = tokenType;
            StartIndex = startIndex;
            EndIndex = endIndex;
            Value = value;
            CustomGroups = c_groups;
            Position = position;
            tokens = tokensList;
            ConvertedValue = converted;
        }
        private List<Token<T>> tokens {get;}
        /// <summary>
        /// Тип токена
        /// </summary>
        /// <value></value>
        public T TokenType { get; }
        /// <summary>
        /// Наименование токена
        /// </summary>
        /// <returns></returns>
        public string TypeName => Enum.GetName(typeof(T), TokenType);
        /// <summary>
        /// Значение токена
        /// </summary>
        /// <value></value>
        public string Value {get;}
        /// <summary>
        /// Начальный индекс токена в текущей строке
        /// </summary>
        /// <value></value>
        public int StartIndex {get;}
        /// <summary>
        /// Конечный индекс токена в текущей строке
        /// </summary>
        /// <value></value>
        public int EndIndex {get;}
        /// <summary>
        /// Длинна занчения токена
        /// </summary>
        /// <value></value>
        public int Length => EndIndex - StartIndex;
        /// <summary>
        /// Номер позиции в массиве
        /// </summary>
        /// <value></value>
        public int Position {get;}
        /// <summary>
        /// Конвертированное значение токена (если был задан конвертер)
        /// </summary>
        /// <value></value>
        public string ConvertedValue {get;set;}
        /// <summary>
        /// Массив регекс групп (если они были заданны в определениях токенов)
        /// </summary>
        /// <value></value>
        public List<GroupMatch> CustomGroups {get;}

        public override string ToString() => Value;
       
        public Token<T> Clone() => new Token<T>(TokenType, StartIndex, EndIndex, Value, CustomGroups, Position, tokens, ConvertedValue);
       
        public bool HaveCustomGroups => this.CustomGroups.Count > 0;

      
    }
}