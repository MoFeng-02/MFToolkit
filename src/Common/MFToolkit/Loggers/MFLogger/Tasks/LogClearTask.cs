using MFToolkit.Loggers.MFLogger.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MFToolkit.Loggers.MFLogger.Tasks;
/// <summary>
/// 日志清理任务
/// </summary>
/// <param name="config"></param>
public class LogClearTask(IOptionsMonitor<LoggerConfiguration> config) : BackgroundService
{
    /// <summary>
    /// 日志清理执行
    /// </summary>
    /// <param name="stoppingToken">停止令牌</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                LogClear.ClearLogs(config.CurrentValue);
            }
            catch
            {
            }

            await Task.Delay(1000 * 60 * 60 * 24, stoppingToken);
        }
    }
}
/// <summary>
/// 日志清理
/// </summary>
public class LogClear
{

    /// <summary>
    /// 在App中进行手动的清理过期日志，不注册服务
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="toPath"></param>
    public static void ClearLogs(LoggerConfiguration configuration, string? toPath = null)
    {
        var deleteTime = DateTime.UtcNow.AddDays(-1 * configuration.SaveDays);
        if (configuration == null) return;
        string basePath = configuration.BasePath;
        if (!Directory.Exists(basePath)) return;
        var filePaths = Directory.GetFiles(toPath ?? basePath);
        // 删除文件夹内的所有文件
        foreach (string filePath in filePaths)
        {
            FileInfo fileInfo = new(filePath);
            if (fileInfo.CreationTimeUtc < deleteTime)
            {
                File.Delete(filePath);
            }
        }
        var directories = Directory.GetDirectories(toPath ?? basePath);
        // 递归删除所有子文件夹
        foreach (string subFolderPath in directories)
        {
            ClearLogs(configuration, subFolderPath);
            DirectoryInfo directoryInfo = new(subFolderPath);
            if (directoryInfo.CreationTimeUtc < deleteTime)
            {
                Directory.Delete(subFolderPath, true);
            }
        }
    }
}