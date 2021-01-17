namespace SimpleJira.Interface.Logging
{
    public struct LoggingSettings
    {
        public LogLevel Level { get; set; }
        public ILogger Logger { get; set; }
    }
}