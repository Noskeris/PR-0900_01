namespace BattleShipAPI.Adapter.Logs
{
    public class FileLoggerAdapter : ILoggerInterface
    {
        private readonly FileLoggingService _fileLoggingService;

        public FileLoggerAdapter(string fileName)
        {
            var solutionDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var filePath = Path.Combine(solutionDirectory, fileName);

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            _fileLoggingService = new FileLoggingService(filePath);
        }

        public void WriteLog(LogEntry logEntry)
        {
            _fileLoggingService.WriteLog(logEntry);
        }

        public List<LogEntry> GetLogs() => _fileLoggingService.GetLogs();
    }
}
