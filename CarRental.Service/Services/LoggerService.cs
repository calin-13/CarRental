using CarRental.Service.Interfaces;

namespace CarRental.Service.Services;

public class LoggerService : ILoggerService
{
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();

    public LoggerService(string logFilePath = "application.log")
    {
        _logFilePath = logFilePath;
    }

    public void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }

    public void LogWarning(string message)
    {
        WriteLog("WARNING", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        var fullMessage = exception != null 
            ? $"{message} | Exception: {exception.Message} | StackTrace: {exception.StackTrace}" 
            : message;
        WriteLog("ERROR", fullMessage);
    }

    public void LogDebug(string message)
    {
        WriteLog("DEBUG", message);
    }

    private void WriteLog(string level, string message)
    {
        lock (_lockObject)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}
