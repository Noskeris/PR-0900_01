using BattleShipAPI.Adapter.Logs;

namespace BattleShipAPI.Proxy
{
    public class LoggerProxy : ILoggerInterface, ILoggerOnReceive, ILoggerOnSend
    {
        private readonly ILoggerInterface _realLogger;
        private readonly List<LogEntry> _logCache = new List<LogEntry>();
        private readonly bool _useCache;

        public LoggerProxy(ILoggerInterface realLogger, bool useCache = false)
        {
            _realLogger = realLogger;
            _useCache = useCache;
        }

        public void WriteLog(LogEntry logEntry)
        {
            if (CheckIsValid(logEntry))
            {
                if (_useCache)
                {
                    _logCache.Add(logEntry);
                }
                else
                {
                    _realLogger.WriteLog(logEntry);
                }
            }
            else
            {
                Console.WriteLine("Log is wrong log entry: " + logEntry.Message);
            }
        }

        public List<LogEntry> GetCachedLogs()
        {
            return _logCache;
        }

        public void FlushCache()
        {
            foreach (var logEntry in _logCache)
            {
                _realLogger.WriteLog(logEntry);
            }
            _logCache.Clear();
        }

        private bool CheckIsValid(LogEntry logEntry)
        {
            return !string.IsNullOrEmpty(logEntry.Message);
        }
    }
}