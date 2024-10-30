using Newtonsoft.Json;

namespace BattleShipAPI.Adapter.Logs
{
    public class FileLoggingService
    {
        private readonly string _filePath;

        public FileLoggingService(string filePath)
        {
            _filePath = filePath;
        }

        public void WriteLog(LogEntry logEntry)
        {
            File.AppendAllText(_filePath, $"[LOG {DateTime.Now.ToString()}] {logEntry.Message}" + Environment.NewLine);
        }

        public List<LogEntry> GetLogs()
        {
            var logs = new List<LogEntry>();

            if (File.Exists(_filePath))
            {
                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    logs.Add(new LogEntry { Message = line });
                }
            }

            return logs;
        }
    }
}
