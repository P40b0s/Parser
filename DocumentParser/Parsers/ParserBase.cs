using Lexer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DocumentParser.Parsers
{
    public abstract class LexerBase<T> : ParserBase
    {
        ILexerService<T> lexer {get;} = new Lexer<T>();
        /// <summary>
        /// Массив токенов
        /// </summary>
        /// <returns></returns>
        public List<Token<T>> tokens {get; private set;} = new List<Token<T>>();
        /// <summary>
        /// Токинезация текста
        /// </summary>
        /// <param name="text">Текст для токинезации</param>
        /// <param name="list">Массив определений токенов</param>
        /// <returns></returns>
        protected List<Token<T>> Tokenize(string text, Lexer.Tokenizer.ListTokensDefinition<T> list)
        {
            tokens = lexer.Tokenize(text, list).ToList();
            return tokens;
        }
    }

    public abstract class ParserBase
    {
        /// <summary>
        /// Вызывается при обновлении статуса.
        /// Использование:
        /// UpdateCallback = o => CurrentStatus = o;
        /// </summary>
        /// <value></value>
        public StatusUpdate UpdateCallback { get; set; }
        /// <summary>
        /// Вызывается при обновлении статусов, содержит массив статусов за все время жизни.
        /// </summary>
        public StatusesUpdate StatusesUpdateCallback { get; set; }
        /// <summary>
        /// Вызывается при добавлении ошибки.
        /// </summary>
        public ErrorUpdate ErrorCallback { get; set; }
        /// <summary>
        /// Вызывается при обновлении ошибок, содержит массив ошибок за все время жизни.
        /// </summary>
        public ErrorsUpdate ErrorsCallback { get; set; }
        public List<ParserException> exceptions { get; set; } = new List<ParserException>();
        public List<string> statuses { get; set; } = new List<string>();
        public bool HasError => exceptions.Count > 0;
        public bool HasFatalError => exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
        /// <summary>
        /// Добавить статус
        /// </summary>
        /// <param name="status">Сообщения для добавления в список статусов</param>
        protected void UpdateStatus(string status)
        {
            #if DEBUG
            Console.WriteLine(status);
            #endif
            if(this.UpdateCallback != null)
                Task.Factory.StartNew(()=> this.UpdateCallback?.Invoke(status));
            statuses.Add(status);
            UpdateStatuses();
        }
        /// <summary>
        /// Добавить статус, с расчетом процента выполнения.
        /// </summary>
        /// <param name="status">Сообщения для добавления в список статусов</param>
        /// <param name="allCount">Всего</param>
        /// <param name="current">Текущее значение</param>
        protected void UpdateStatus(string status, int allCount, int current)
        {
            UpdateStatus($"{status} {getPercentage(allCount, current)}%");
        }
       
        void UpdateStatuses()
        {
           
            if(this.StatusesUpdateCallback != null)
                Task.Factory.StartNew(()=> this.StatusesUpdateCallback?.Invoke(statuses));
        }
        void UpdateErrors()
        {
            if(this.ErrorsCallback != null)
                Task.Factory.StartNew(()=> this.ErrorsCallback?.Invoke(exceptions));
        }
        void UpdateError(ParserException pe)
        {
            #if DEBUG
            Console.WriteLine(pe.Message);
            #endif
            if(this.ErrorCallback != null)
                Task.Factory.StartNew(()=> this.ErrorCallback?.Invoke(pe));
        }
        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="pe">Ошибка парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(string message,  TokenException tokenException, ErrorType errorType = ErrorType.Fatal)
        {
            ParserException  exception = new ParserException($"{message}. TokenError: {tokenException.Message}");
            exceptions.Add(exception);
            UpdateError(exception);
            UpdateErrors();
            if(exception.ErrorType == ErrorType.Fatal)
                return false;
            else return true;
        }
        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="pe">Ошибка парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(string message, ErrorType errorType = ErrorType.Fatal)
        {
            ParserException exception = new ParserException($"{message}");
            exceptions.Add(exception);
            UpdateError(exception);
            UpdateErrors();
            if(exception.ErrorType == ErrorType.Fatal)
                return false;
            else return true;
        }
        
      
        int getPercentage(int all, int current)
        {
            var result = (current * 100) / all;
            return result > 100 ? 100 : result;
        }
    }
}
