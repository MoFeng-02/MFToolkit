using System.Collections.Concurrent;
using System.Text;
using MFToolkit.Exceptions;
using MFToolkit.Loggers.MFLogger.Configurations;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Loggers.MFLogger;
public class LocalFileLogger : ILogger
{
    /// <summary>
    /// 用于线程安全
    /// </summary>
    private static readonly ConcurrentQueue<WaitWirteInfo> queue = new();
    private static readonly CancellationTokenSource cancellationTokenSource = new();
    private static Task writeTask = null!;
    private string _name;
    private readonly Func<LoggerConfiguration> _getCurrentConfig;

    public LocalFileLogger(string name, Func<LoggerConfiguration> getCurrentConfig)
    {
        _name = name;
        _getCurrentConfig = getCurrentConfig;
        writeTask ??= Task.Run(StartWriteTaskAsync);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel logLevel)
    {
        return _getCurrentConfig().LogLevels.Contains(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        var config = _getCurrentConfig() ?? throw new ArgumentNullException("config", "未提供配置参数，请提供配置参数");
        var dateTime = DateTime.Now;
        var groupFileInfo = GroupFile(logLevel, config, dateTime, exception);
        var content = formatter.Invoke(state, exception);
        //await FileWriteAsync(config, savePath, FormatterContent(content, logLevel));
        var waitWirteInfo = new WaitWirteInfo()
        {
            Content = FormatterContent(content, logLevel, dateTime),
            GroupFileInfo = groupFileInfo,
            LogLevel = logLevel,
        };
        queue.Enqueue(waitWirteInfo);
    }
    class WaitWirteInfo
    {
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public LogLevel LogLevel { get; set; }
        public string? Content { get; set; }
        public GroupFileInfo? GroupFileInfo { get; set; }
    }
    /// <summary>
    /// 格式化文件
    /// </summary>
    /// <param name="content"></param>
    /// <param name="logLevel"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    private string FormatterContent(string content, LogLevel logLevel, DateTime? dateTime = null, Exception? exception = null)
    {
        return $"[{dateTime ?? DateTime.Now}] {logLevel} {_name}  {content}\n";
    }
    private async Task StartWriteTaskAsync()
    {
        while (true)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }
            if (queue.TryDequeue(out var info))
            {
                var config = _getCurrentConfig() ?? throw new ArgumentNullException("logger config");
                var group = info.GroupFileInfo ?? GroupFile(info.LogLevel, config);
                await FileWriteAsync(config, group, info.Content!);
            }
            else
            {
                await Task.Delay(1000);
            }
        }
    }
    public static async Task StopWriteTask()
    {
        cancellationTokenSource.Cancel();
        await writeTask;
    }
    /// <summary>
    /// 返回分组后的路径
    /// </summary>
    /// <param name="logLevel">日志级别</param>
    /// <param name="config">配置信息</param>
    /// <param name="dateTime">日志时间</param>
    /// <param name="exception">异常</param>
    /// <returns></returns>
    private GroupFileInfo GroupFile(LogLevel logLevel, LoggerConfiguration config, DateTime? dateTime = null, Exception? exception = null)
    {
        DateTime time = dateTime ?? DateTime.Now;
        var dateStr = config.SaveTimeType switch
        {
            SaveTimeType.Day => $"{time:yyyy_MM_dd}",
            SaveTimeType.Month => $"{time:yyyy_MM}",
            SaveTimeType.Hour => $"{time:yyyy_MM_dd}",
            _ => $"{time:yyyy_MM_dd}"
        };
        var rePath = Path.Combine(config.BasePath, dateStr);
        // 如果启动了类型分组
        if (config.OpenGroupLevel)
        {
            if (exception != null && exception is MFCommonException baseException)
            {
                rePath = Path.Combine(rePath, logLevel.ToString(), baseException.ExceptionLevel.ToString());
            }
            else
                rePath = Path.Combine(rePath, logLevel.ToString());
        }
        return new()
        {
            Path = rePath,
            Hour = time.ToString("HH")
        };
    }
    /// <summary>
    /// 尾部插入斜杠
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    //private string LastInsertStr(string origin, string newValue)
    //{
    //    StringBuilder sb = new();
    //    var lastStr = origin.Substring(origin.Length - 1, 1);
    //    if (lastStr != "/" && lastStr != @"\") sb.Append("/" + newValue);
    //    else sb.Append(newValue);
    //    return sb.ToString();
    //}
    /// <summary>
    /// 分组后的文件路径和当前区间小时
    /// </summary>
    private class GroupFileInfo
    {
        public string? Path { get; set; }
        public string? Hour { get; set; }
    }
    /// <summary>
    /// 写入文件方法
    /// </summary>
    /// <param name="config">保存配置</param>
    /// <param name="info">保存信息</param>
    /// <param name="content">保存内容</param>
    /// <param name="retry">重新尝试</param>
    /// <returns></returns>
    private async Task FileWriteAsync(LoggerConfiguration config, GroupFileInfo info, string content, int retry = 0, string? path = null)
    {
        if (retry == 0)
        {
            if (!Directory.Exists(info.Path))
            {
                Directory.CreateDirectory(info.Path ?? throw new("路径值为空"));
            }
            if (config.SaveTimeType == SaveTimeType.Hour)
            {
                var name = info.Hour + config.SaveSuffix;
                path = Path.Combine(info.Path, name);
            }
            else
            {
                path = info.Path + config.SaveSuffix;
            }
        }
        try
        {
            //await File.AppendAllTextAsync(path, content, Encoding.UTF8);
            using FileStream fileStream = new(path!, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true);
            byte[] encodedText = Encoding.UTF8.GetBytes(content);
            await fileStream.WriteAsync(encodedText);

        }
        catch (Exception)
        {
            if (retry == 5) return;
            await Task.Delay(1000);
            await FileWriteAsync(config, info, content, retry++, path);
        }
    }
}
