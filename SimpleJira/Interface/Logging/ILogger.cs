namespace SimpleJira.Interface.Logging
{
    public interface ILogger
    {
        void LogTrace(string message, string stackTrace);
        void LogDebug(string message, string stackTrace);
        void LogWarn(string message, string stackTrace);
    }
}