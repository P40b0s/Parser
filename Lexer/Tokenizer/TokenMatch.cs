using System;
using System.Collections.Generic;

namespace Lexer.Tokenizer
{
    public struct GroupMatch :  ITextIndex
    {
        public GroupMatch(string name, string val, int start, int length)
        {
            Name = name;
            Value = val;
            StartIndex = start;
            Length = length;
            EndIndex = -1;
        }
        public string Name {get;}
        public string Value {get;set;}
        public int StartIndex { get;}
        public int EndIndex{get;}
        public int Length { get;}
    }
    public class TokenMatch<T>
    {
        /// <summary>
        /// Тип токена
        /// </summary>
        /// <value></value>
        public T TokenType { get; set; }
        /// <summary>
        /// Значение токена
        /// </summary>
        /// <value></value>
        public string Value { get; set; }
        /// <summary>
        /// Массив групп если они были определены в определениях токенов
        /// </summary>
        /// <value></value>
        public List<GroupMatch> Groups {get;set;}
        /// <summary>
        /// Конвертированое значение (если был задан конвертер в определениях токенов)
        /// </summary>
        /// <value></value>
        public string Converted {get;set;}
        /// <summary>
        /// Начальный индекс токена в текущей строке
        /// </summary>
        /// <value></value>
        public int StartIndex { get; set; }
        /// <summary>
        /// Конечный индекс токена в текущей строке
        /// </summary>
        /// <value></value>
        public int EndIndex { get; set; }
        /// <summary>
        /// Приоритет
        /// </summary>
        /// <value>При нахождении токенов с одинаковым вхождением приоритет будет отдаваться тому токену у которого меньше значение</value>
        public int Precedence { get; set; }
    }
}