using System.Collections.Concurrent;
using MFToolkit.Loggers.MFLogger.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MFToolkit.Loggers.MFLogger;
/// <summary>
/// 本地文件日志记录器提供程序
/// </summary>
public class LocalFileLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private LoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, LocalFileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config"></param>
    public LocalFileLoggerProvider(
        IOptionsMonitor<LoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }
    /// <summary>
    /// 创建日志记录器
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new LocalFileLogger(name, GetCurrentConfig));
    private LoggerConfiguration GetCurrentConfig() => _currentConfig;
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}
