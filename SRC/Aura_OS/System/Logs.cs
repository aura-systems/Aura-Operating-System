using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Diagnostics;

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

    /// <summary>
    /// Aura's log window entries, each also mirrored to the serial log.
    /// LogLevel.Kernel carries Aura's own kernel-level messages.
    /// GEN3-GAP(log-sink): there is no kernel log sink anymore (gen2 fed it from the Cosmos debugger plug).
    /// Not thread-safe: a non-UI thread (the FTP server) logs with Cosmos.Kernel.System.Diagnostics.Log only.
    /// </summary>
    public static class Logs
    {
        public static List<LogEntry> LogList = new List<LogEntry>();

        public static void DoKernelLog(string log)
        {
            Mirror("[Kernel] ", log);
            LogList.Add(new LogEntry(LogLevel.Kernel, log, DateTime.Now));
        }

        public static void DoOSLog(string log)
        {
            Mirror("", log);
            LogList.Add(new LogEntry(LogLevel.OS, log, DateTime.Now));
        }

        private static void Mirror(string prefix, string log)
        {
            try
            {
                Log.WriteString(prefix + log + "\n");
            }
            catch (Exception)
            {
            }
        }
    }
}
