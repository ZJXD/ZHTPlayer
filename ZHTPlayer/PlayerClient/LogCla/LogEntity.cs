using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Log
{
    public class LogEntity
    {
        public long Id { get; set; }
        /// <summary>
        /// 日志级别 Trace|Debug|Info|Warn|Error|Fatal
        /// </summary>
        public string Level { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public string Amount { get; set; }
        public string StackTrace { get; set; }
        public DateTime Timestamp { get; set; }

        private LogEntity() { }
        public LogEntity(string level, string message, string action = null, string amount = null)
        {
            this.Level = level;
            this.Message = message;
            this.Action = action;
            this.Amount = amount;
        }
    }
}
