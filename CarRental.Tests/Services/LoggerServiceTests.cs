using CarRental.Service.Services;
using Xunit;

namespace CarRental.Tests.Services;

public class LoggerServiceTests : IDisposable
{
    private readonly string _testLogFile;
    private readonly LoggerService _loggerService;

    public LoggerServiceTests()
    {
        _testLogFile = $"test_log_{Guid.NewGuid()}.log";
        _loggerService = new LoggerService(_testLogFile);
    }

    public void Dispose()
    {
        if (File.Exists(_testLogFile))
        {
            File.Delete(_testLogFile);
        }
    }

    [Fact]
    public void LogInfo_ShouldWriteInfoMessageToFile()
    {
        var message = "Test info message";
        
        _loggerService.LogInfo(message);
        
        var logContent = File.ReadAllText(_testLogFile);
        Assert.Contains("[INFO]", logContent);
        Assert.Contains(message, logContent);
    }

    [Fact]
    public void LogWarning_ShouldWriteWarningMessageToFile()
    {
        var message = "Test warning message";
        
        _loggerService.LogWarning(message);
        
        var logContent = File.ReadAllText(_testLogFile);
        Assert.Contains("[WARNING]", logContent);
        Assert.Contains(message, logContent);
    }

    [Fact]
    public void LogError_WithoutException_ShouldWriteErrorMessageToFile()
    {
        var message = "Test error message";
        
        _loggerService.LogError(message);
        
        var logContent = File.ReadAllText(_testLogFile);
        Assert.Contains("[ERROR]", logContent);
        Assert.Contains(message, logContent);
    }

    [Fact]
    public void LogError_WithException_ShouldWriteErrorAndExceptionDetailsToFile()
    {
        var message = "Test error with exception";
        var exception = new InvalidOperationException("Test exception");
        
        _loggerService.LogError(message, exception);
        
        var logContent = File.ReadAllText(_testLogFile);
        Assert.Contains("[ERROR]", logContent);
        Assert.Contains(message, logContent);
        Assert.Contains("Test exception", logContent);
    }

    [Fact]
    public void LogDebug_ShouldWriteDebugMessageToFile()
    {
        var message = "Test debug message";
        
        _loggerService.LogDebug(message);
        
        var logContent = File.ReadAllText(_testLogFile);
        Assert.Contains("[DEBUG]", logContent);
        Assert.Contains(message, logContent);
    }

    [Fact]
    public void MultipleLogCalls_ShouldWriteAllMessagesToFile()
    {
        _loggerService.LogInfo("Info message");
        _loggerService.LogWarning("Warning message");
        _loggerService.LogError("Error message");
        
        var logContent = File.ReadAllText(_testLogFile);
        Assert.Contains("Info message", logContent);
        Assert.Contains("Warning message", logContent);
        Assert.Contains("Error message", logContent);
    }

    [Fact]
    public void LogEntry_ShouldContainTimestamp()
    {
        var message = "Test message with timestamp";
        
        _loggerService.LogInfo(message);
        
        var logContent = File.ReadAllText(_testLogFile);
        
        Assert.Matches(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\]", logContent);
    }

    [Fact]
    public void LogEntries_ShouldBeOnSeparateLines()
    {
        _loggerService.LogInfo("First message");
        _loggerService.LogInfo("Second message");
        _loggerService.LogInfo("Third message");
        
        var logLines = File.ReadAllLines(_testLogFile);
        Assert.Equal(3, logLines.Length);
        Assert.Contains("First message", logLines[0]);
        Assert.Contains("Second message", logLines[1]);
        Assert.Contains("Third message", logLines[2]);
    }
}
