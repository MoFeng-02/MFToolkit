#nullable disable
using Microsoft.Extensions.Logging;

namespace MFToolkit.Loggers.MFLogger.Configurations;

/// <summary>
/// 日志配置
/// </summary>
public sealed class LoggerConfiguration
{
    /// <summary>
    /// 基础路径
    /// </summary>
    public string BasePath { get; set; }
    /// <summary>
    /// 保存名称
    /// </summary>
    public string SaveName { get; set; }
    /// <summary>
    /// 启用文件夹分组
    /// 对应每个类型都不同的分组文件夹，避免Info里面出现Error信息
    /// </summary>
    public bool OpenGroupLevel { get; set; }
    /// <summary>
    /// 保存时间段区分
    /// </summary>
    public SaveTimeType SaveTimeType { get; set; } = SaveTimeType.Day;
    /// <summary>
    /// 保存天数
    /// </summary>
    public int SaveDays { get; set; } = 7;
    /// <summary>
    /// 保存后缀
    /// </summary>
    public string SaveSuffix { get; set; } = ".log";
    /// <summary>
    /// 启用的日志级别
    /// </summary>
    public List<LogLevel> LogLevels { get; private set; }
    /// <summary>
    /// 添加启用的日志级别
    /// </summary>
    /// <param name="level"></param>
    public void AddStartLogLevel(LogLevel level)
    {
        LogLevels ??= [];
        if (LogLevels.Contains(level)) return;
        LogLevels.Add(level);
    }
    /// <summary>
    /// 添加启用的日志级别
    /// </summary>
    /// <param name="levels"></param>
    public void AddStartLogLevel(params LogLevel[] levels)
    {
        LogLevels ??= [];
        foreach (var level in levels)
        {
            if (LogLevels.Contains(level)) continue;
            LogLevels.Add(level);
        }
    }
}
/// <summary>
/// 保存时间段区分
/// <para>即是按天创建文件还是按消失创建还是按月创建</para>
/// </summary>
public enum SaveTimeType
{
    /// <summary>
    /// 每小时区分一次
    /// </summary>
    Hour,
    /// <summary>
    /// 每天区分一次
    /// </summary>
    Day,
    /// <summary>
    /// 每月区分一次
    /// </summary>
    Month
}