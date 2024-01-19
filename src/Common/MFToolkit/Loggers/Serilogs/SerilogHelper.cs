using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MFToolkit.Loggers.Serilogs;
/// <summary>
/// 由通义千问和CHATGPT互相补足而成
/// </summary>
public class SerilogHelper
{
    /// <summary>
    /// 全局单例日志处理器，只可本类中赋值
    /// </summary>
    public static Logger Logger { get; private set; }
    // 设置日志级别和文件名模板
    static string logFileNameTemplate = "{Level}/{Hour}.log";

    /// <summary>
    /// 创建一个 Serilog 日志记录器，并且有相应配置
    /// </summary>
    /// <param name="logDirectory">保存基路径，可以是绝对路径也可以是相对路径</param>
    /// <param name="maxFileSizeInMB">最大限制文件大小，默认10MB</param>
    /// <param name="maxFilesInFolder">最大限制文件数量，默认20个</param>
    /// <param name="saveFileTimeOut">文件保存天数</param>
    /// <returns></returns>
    public static Logger ConfigureLogger(string logDirectory = "logs/", int maxFileSizeInMB = 10, int maxFilesInFolder = 20, int saveFileTimeOut = 7)
    {
        var logger = new LoggerConfiguration()
             .MinimumLevel.Verbose() // 设置最低日志级别为 Verbose，以确保所有级别的日志都被记录
             .WriteTo.Logger(lc => lc
                 .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information) // 只记录 Information 级别的日志
                 .WriteTo.File(
                     GetLogFilePath(logDirectory + DateTime.Now.ToString("yyyy_MM_dd"), logFileNameTemplate, LogEventLevel.Information),
                     rollingInterval: RollingInterval.Day, // 按天轮换
                     fileSizeLimitBytes: maxFileSizeInMB * 1024 * 1024, // 文件大小限制
                     retainedFileCountLimit: maxFilesInFolder, // 最多保留几个文件
                     shared: true, // 允许多个程序实例共享日志文件
                     flushToDiskInterval: TimeSpan.FromSeconds(1), // 刷新到磁盘的间隔时间
                     retainedFileTimeLimit: TimeSpan.FromDays(saveFileTimeOut))) // 删除旧日志文件的时间限制
             .WriteTo.Logger(lc => lc
                 .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error) // 只记录 Error 级别的日志
                 .WriteTo.File(
                     GetLogFilePath(logDirectory + DateTime.Now.ToString("yyyy_MM_dd"), logFileNameTemplate, LogEventLevel.Error),
                     rollingInterval: RollingInterval.Day, // 按天轮换
                     fileSizeLimitBytes: maxFileSizeInMB * 1024 * 1024, // 文件大小限制
                     retainedFileCountLimit: maxFilesInFolder, // 最多保留几个文件
                     shared: true, // 允许多个程序实例共享日志文件
                     flushToDiskInterval: TimeSpan.FromSeconds(1), // 刷新到磁盘的间隔时间
                     retainedFileTimeLimit: TimeSpan.FromDays(saveFileTimeOut))) // 删除旧日志文件的时间限制
             .WriteTo.Logger(lc => lc
                 .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning) // 只记录 Warning 级别的日志
                 .WriteTo.File(
                     GetLogFilePath(logDirectory + DateTime.Now.ToString("yyyy_MM_dd"), logFileNameTemplate, LogEventLevel.Warning),
                     rollingInterval: RollingInterval.Day, // 按天轮换
                     fileSizeLimitBytes: maxFileSizeInMB * 1024 * 1024, // 文件大小限制
                     retainedFileCountLimit: maxFilesInFolder, // 最多保留几个文件
                     shared: true, // 允许多个程序实例共享日志文件
                     flushToDiskInterval: TimeSpan.FromSeconds(1), // 刷新到磁盘的间隔时间
                     retainedFileTimeLimit: TimeSpan.FromDays(saveFileTimeOut))) // 删除旧日志文件的时间限制
             .CreateLogger();
        Logger = logger;
        return logger;
    }

    // 自定义日志文件路径生成器
    private static string GetLogFilePath(string rootPath, string fileNameTemplate, LogEventLevel logLevel)
    {
        // 获取当前小时
        string currentHour = DateTime.Now.ToString("HH");

        // 构建完整的日志文件路径
        string logFilePath = Path.Combine(rootPath, currentHour);

        // 确保目录存在
        Directory.CreateDirectory(logFilePath);

        // 构建日志文件名
        string fileName = fileNameTemplate
            .Replace("{Date:yyyy_MM_dd}", DateTime.Now.ToString("yyyy_MM_dd"))
            .Replace("{Level}", logLevel.ToString())
            .Replace("{Hour}", currentHour);

        return Path.Combine(logFilePath, fileName);
    }
}
