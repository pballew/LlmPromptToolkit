using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Service for capturing and storing application logs
/// </summary>
public class LoggingService
{
    private readonly List<LogEntry> _logs = new();
    private readonly object _lockObject = new();
    private const int MaxLogs = 1000;

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Add a log entry
    /// </summary>
    public void Log(string message, string level = "INFO")
    {
        lock (_lockObject)
        {
            _logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });

            // Keep only the last MaxLogs entries
            if (_logs.Count > MaxLogs)
            {
                _logs.RemoveRange(0, _logs.Count - MaxLogs);
            }
        }
    }

    /// <summary>
    /// Get all logs
    /// </summary>
    public List<LogEntry> GetLogs()
    {
        lock (_lockObject)
        {
            return new List<LogEntry>(_logs);
        }
    }

    /// <summary>
    /// Get logs since a specific timestamp
    /// </summary>
    public List<LogEntry> GetLogsSince(DateTime since)
    {
        lock (_lockObject)
        {
            return _logs.Where(l => l.Timestamp > since).ToList();
        }
    }

    /// <summary>
    /// Clear all logs
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _logs.Clear();
        }
    }

    /// <summary>
    /// Get the last N logs
    /// </summary>
    public List<LogEntry> GetLastLogs(int count)
    {
        lock (_lockObject)
        {
            return _logs.TakeLast(count).ToList();
        }
    }
}
