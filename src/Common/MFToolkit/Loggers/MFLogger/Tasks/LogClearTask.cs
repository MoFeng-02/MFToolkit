using MFToolkit.Loggers.MFLogger.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MFToolkit.Loggers.MFLogger.Tasks;
public class LogClearTask(IOptionsMonitor<LoggerConfiguration> config) : BackgroundService
{
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
public class LogClear
{

    /// <summary>
    /// 在App中进行手动的清理过期日志，不注册服务
    /// </summary>
    /// <param name="configuration"></param>
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