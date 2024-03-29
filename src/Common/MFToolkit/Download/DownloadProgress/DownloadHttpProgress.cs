using System.Net.Http.Handlers;
using MFToolkit.Download.Models;

namespace MFToolkit.Download.DownloadProgress;

internal partial class DownloadHttpProgress
{
    internal static async Task FileSaveAsync(DownloadModel file)
    {
        using var progressMessageHandler = new ProgressMessageHandler(new HttpClientHandler());
        progressMessageHandler.HttpReceiveProgress += (_, e) =>
        {
            file.SaveHandle?.Invoke(e.ProgressPercentage, e.TotalBytes.Value);
        };
    }
}
