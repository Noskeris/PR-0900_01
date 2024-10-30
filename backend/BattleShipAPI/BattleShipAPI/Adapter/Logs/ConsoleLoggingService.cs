namespace BattleShipAPI.Adapter.Logs
{
    public class ConsoleLoggingService
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();

        public void WriteLog(LogEntry logEntry)
        {
            Console.WriteLine($"[LOGENTRY] {logEntry.Message}" + Environment.NewLine);
            _logs.Add(logEntry);
        }

        public List<LogEntry> GetLogs()
        {
            return _logs;
        }
    }
}
