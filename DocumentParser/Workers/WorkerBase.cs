
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentParser.Workers
{
    public abstract class WorkerBase
    {
        /// <summary>
        /// UpdateCallback = o => CurrentStatus = o;
        /// </summary>
        /// <value></value>
        public StatusUpdate UpdateCallback { get; set; }
        List<Utils.IError> Errors { get; set; } = new List<Utils.IError>();
        //protected ILoggerService logger {get;} = new LoggerService();
        protected void Status(string status)
        {
            Task.Factory.StartNew(()=> this.UpdateCallback?.Invoke(status));
        }
        protected void Percentage(string message, int allCount, int current)
        {
            Status($"{message} {getPercentage(allCount, current)}%");
        }
        protected int getPercentage(int all, int current)
        {
            return (current * 100) / all;
        }
    }
}
