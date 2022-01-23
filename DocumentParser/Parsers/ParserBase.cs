using Lexer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Utils;
using System.Runtime.CompilerServices;

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
        protected SettingsWorker.ISettings settings {get;init;}
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
        List<ParserException> exceptions { get; set; } = new List<ParserException>();
        List<string> statuses { get; set; } = new List<string>();
        public bool HasError => exceptions.Count > 0;
        public bool HasFatalError => exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
        public List<ParserException> GetExceptions() => exceptions;
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
        /// <param name="message">Ошибка парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(string message,  TokenException tokenException, ErrorType errorType = ErrorType.Fatal, [CallerMemberName]string callerMemberName = null) =>
           AddError(new ParserException($"Метод: \"{callerMemberName}\" \n {message}. \n TokenError: {tokenException?.Message}", errorType));

        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="error">Ошибка парсера</param>
        /// <returns>Всегда возвращает false!</returns>
        protected bool AddError(IError error, [CallerMemberName]string callerMemberName = null) =>
            AddError(new ParserException($"Метод: \"{callerMemberName}\" \n {error.Message}"));

        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="message">Ошибка парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(string message, ErrorType errorType = ErrorType.Fatal, [CallerMemberName]string callerMemberName = null) =>
            AddError(new ParserException($"Метод: \"{callerMemberName}\" \n {message}"));

        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="exception">Ошибка парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        bool AddError(ParserException exception)
        {
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
        /// <param name="exception">Ошибка парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(Exception exception, [CallerMemberName]string callerMemberName = null) =>
            AddError(new ParserException($"Метод: \"{callerMemberName}\" \n Системное исключение: {exception.Message} \n StackTrace: {exception.StackTrace}"));
        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="exceptions">Ошибки парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(IEnumerable<ParserException> exceptions, [CallerMemberName]string callerMemberName = null)
        {
            var fatal = exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
            foreach(var e in exceptions)
                AddError(new ParserException($"Метод: \"{callerMemberName}\" \n {e.Message}"));
            return !fatal;
        }
        /// <summary>
        /// Добавляет ошибку в список ошибок
        /// </summary>
        /// <param name="exceptions">Ошибки парсера</param>
        /// <returns>Если ошибка критическая возвращает false в иных случаях true</returns>
        protected bool AddError(ParserBase p, [CallerMemberName]string callerMemberName = null)
        {
            var exceptions = p.GetExceptions();
            var fatal = exceptions.Any(a=>a.ErrorType == ErrorType.Fatal);
            foreach(var e in exceptions)
                AddError(new ParserException($"Метод: \"{callerMemberName}\" \n {e.Message}"));
            return !fatal;
        }
       
        
      
        int getPercentage(int all, int current)
        {
            var result = (current * 100) / all;
            return result > 100 ? 100 : result;
        }
    }
}
