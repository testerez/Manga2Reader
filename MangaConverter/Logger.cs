using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaConverter
{
    public abstract class Logger
    {
        public enum LogLevel {
            Error=0,
            Info=1,
            Verbose=2,
            Debug=3,
        }
        protected abstract void OnLog(LogLevel l, String message);

        private void OnLog(LogLevel l, String message, object[] args)
        {
            OnLog(l, String.Format(message ?? "", args));
        }

        public void D(String message, params object[] args)
        {
            OnLog(LogLevel.Debug, message, args);
        }
        public void V(String message, params object[] args)
        {
            OnLog(LogLevel.Verbose, message, args);
        }
        public void I(String message, params object[] args)
        {
            OnLog(LogLevel.Info, message, args);
        }
        public void E(String message, params object[] args)
        {
            OnLog(LogLevel.Error, message, args);
        }
    }

    public class ConsoleLogger : Logger
    {
        public LogLevel MaxLogLevel = LogLevel.Debug;
        protected override void OnLog(Logger.LogLevel l, string message)
        {
            if (l > MaxLogLevel)
                return;
            (l == LogLevel.Error ? Console.Error : Console.Out).WriteLine(message);
        }
    }
}
