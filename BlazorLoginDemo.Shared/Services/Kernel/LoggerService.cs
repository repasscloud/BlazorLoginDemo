using BlazorLoginDemo.Shared.Models.Kernel.SysVar;
using BlazorLoginDemo.Shared.Models.Static;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using Microsoft.Extensions.Configuration;

namespace BlazorLoginDemo.Shared.Services.Kernel;

public class LoggerService : ILoggerService
{
    private readonly ApplicationDbContext _context;
    private readonly LogLevelType _minLogLevel;

    public LoggerService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;

        // Read the minimum log level from configuration.
        // For example, "Logging:LogLevelType:Default" might be "Information".
        var levelString = configuration["Logging:LogLevel:Default"];
        if (!Enum.TryParse<LogLevelType>(levelString, true, out var configLogLevel))
        {
            // Default to Information if parsing fails.
            configLogLevel = LogLevelType.Information;
        }
        _minLogLevel = configLogLevel;
    }

    private async Task LogAsync(string level, string message)
    {
        // Convert our custom level string to the LogLevelType enum.
        if (!Enum.TryParse<LogLevelType>(level, true, out var messageLogLevel))
        {
            // Fallback if parsing fails.
            messageLogLevel = LogLevelType.Information;
        }

        // Only log if the message's log level is equal to or more severe than the configured minimum.
        if (messageLogLevel < _minLogLevel)
        {
            // Skipping log: message level (e.g. TRACE, DEBUG) is below our threshold.
            return;
        }

        // LogEntry class is assumed to have Level, Message, and Timestamp properties.
        var logEntry = new AvaSystemLog
        {
            Level = level,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        _context.AvaSystemLogs.Add(logEntry);
        await _context.SaveChangesAsync();
    }

    public async Task LogTraceAsync(string message) => await LogAsync("Trace", message);
    public async Task LogDebugAsync(string message) => await LogAsync("Debug", message);
    public async Task LogInfoAsync(string message) => await LogAsync("Information", message);
    public async Task LogWarningAsync(string message) => await LogAsync("Warning", message);
    public async Task LogErrorAsync(string message) => await LogAsync("Error", message);
    public async Task LogCriticalAsync(string message) => await LogAsync("Critical", message);
}
