using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Log
{
    public class MyLogEventInfo : LogEventInfo
    {
        public MyLogEventInfo() { }
        public MyLogEventInfo(LogLevel level, string loggerName, string message) : base(level, loggerName, message)
        { }

        public override string ToString()
        {
            //Message format
            //Log Event: Logger='XXX' Level=Info Message='XXX' SequenceID=5
            return FormattedMessage;
        }
    }
}
