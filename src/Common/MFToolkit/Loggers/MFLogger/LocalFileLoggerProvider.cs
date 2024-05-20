using System.Collections.Concurrent;
using MFToolkit.Loggers.MFLogger.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MFToolkit.Loggers.MFLogger;
public class LocalFileLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private LoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, LocalFileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    public LocalFileLoggerProvider(
        IOptionsMonitor<LoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new LocalFileLogger(name, GetCurrentConfig));
    private LoggerConfiguration GetCurrentConfig() => _currentConfig;
    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}
