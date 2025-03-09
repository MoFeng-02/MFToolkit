// See https://aka.ms/new-console-template for more information
using MFToolkit.Download.Extensions.DependencyInjection;
using MFToolkit.Download.Services;
using MFToolkit.Http;
using MFToolkit.Http.Extensions.DependencyInjection;
using MFToolkit.Loggers.MFLogger.Extensions.DependencyInjection;
using MFToolkit.Utility;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new ServiceCollection();
services.AddDownloadService()
    .AddDownloadPauseInfoHandler();
services.AddHttpClientService<HttpClientService>();
services.AddLogging(options =>
{
    options.AddMFLocalFileLogger();
});


var serviceProvider = services.BuildServiceProvider();
var downloadService = serviceProvider.GetRequiredService<IDownloadService>();
downloadService.DownloadProgress = (current, total) =>
{
    Console.WriteLine($"下载进度：现：{current},总：{total}，百分比：{AppUtil.CalculateDownloadProgress(current, total):0.00}%");
};
_ = Task.Run(async () =>
{
    downloadService.DownloadStateAction = (isDownloading, state, exception) =>
    {
        Console.WriteLine($"下载状态：{state}");
        if (exception != null)
        {
            Console.WriteLine($"下载异常：{exception.Message}");
        }
    };
    await Task.Delay(3000);
    await downloadService.PauseDownloadAsync();
    Console.WriteLine("3秒后重新下载");
    await Task.Delay(3000);
    await downloadService.ResumeDownloadAsync();
    await downloadService.StartDownloadAsync();

});
if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads")))
{
    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"));
}
await downloadService.DownloadAsync(new()
{
    DownloadUrl = "https://vjs.zencdn.net/v/oceans.mp4",
    FileSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads", "test.mp4")
});

Console.ReadKey();