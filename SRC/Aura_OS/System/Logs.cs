using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System
{
    public enum LogLevel
    {
        Kernel,
        OS
    }

    public class LogEntry
    {
        public LogLevel Level;
        public string Log;
        public DateTime DateTime;

        public LogEntry(LogLevel level, string log, DateTime dateTime)
        {
            Level = level;
            Log = log;
            DateTime = dateTime;
        }
    }

    public static class Logs
    {
        public static List<LogEntry> LogList = new List<LogEntry>();

        public static void DoKernelLog(string log)
        {
            LogList.Add(new LogEntry(LogLevel.Kernel, log, DateTime.Now));
        }

        public static void DoOSLog(string log)
        {
            LogList.Add(new LogEntry(LogLevel.OS, log, DateTime.Now));
        }
    }
}
