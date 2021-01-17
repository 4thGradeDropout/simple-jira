using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SimpleJira.Interface.Logging
{
    public class ConsoleLogger : ILogger
    {
        private const string TRACE = "TRACE";
        private const string DEBUG = "DEBUG";
        private const string WARN = "WARN";

        public void LogTrace(string message, string stackTrace)
        {
            Log(TRACE, message);
        }

        public void LogDebug(string message, string stackTrace)
        {
            Log(DEBUG, message);
        }

        public void LogWarn(string message, string stackTrace)
        {
            var builder = new StringBuilder(message);
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("stack trace:");
            builder.AppendLine(stackTrace);
            Log(WARN, builder.ToString());
        }

        private static void Log(string level, string message)
        {
            switch (level)
            {
                case WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Log(Console.Out, level, message);
            switch (level)
            {
                case WARN:
                    Console.ResetColor();
                    break;
            }
        }

        private static void Log(TextWriter logger, string level, string message)
        {
            var builder = new StringBuilder(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            builder.Append(" [thrd:");
            builder.Append(Thread.CurrentThread.ManagedThreadId);
            builder.Append("] ");
            builder.Append(level);
            builder.Append(" ");
            builder.Append(message);
            logger.WriteLine(builder.ToString());
        }
    }
}