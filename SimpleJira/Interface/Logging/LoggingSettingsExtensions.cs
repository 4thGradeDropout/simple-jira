using System;

namespace SimpleJira.Interface.Logging
{
    internal static class LoggingSettingsExtensions
    {
        public static bool NeedLogging(this LoggingSettings? settings, LogLevel level)
        {
            return settings?.Logger != null && settings.Value.Level <= level;
        }

        public static void Log(this LoggingSettings? settings, LogLevel level, string message)
        {
            if (settings.NeedLogging(level))
                switch (level)
                {
                    case LogLevel.Trace:
                        settings.Value.Logger.LogTrace(message, Environment.StackTrace);
                        break;
                    case LogLevel.Debug:
                        settings.Value.Logger.LogDebug(message, Environment.StackTrace);
                        break;
                    case LogLevel.Warning:
                        settings.Value.Logger.LogWarn(message, Environment.StackTrace);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
        }
    }
}