namespace BattleShipAPI.Adapter.Logs
{
    public class ConsoleLoggerAdapter : ILoggerInterface
    {
        private readonly ConsoleLoggingService _consoleLoggingService;

        public ConsoleLoggerAdapter()
        {
            _consoleLoggingService = new ConsoleLoggingService();
        }

        public void WriteLog(LogEntry logEntry)
        {
            _consoleLoggingService.WriteLog(logEntry);
        }

        public List<LogEntry> GetLogs() => _consoleLoggingService.GetLogs();
    }
}
