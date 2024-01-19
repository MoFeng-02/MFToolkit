using System.Net.Http.Handlers;

namespace MFToolkit.Utils.FileExtensions.FileProgress;

internal partial class FileHttpProgress
{
    internal static async Task FileSaveAsync(FileModel file)
    {
        using var progressMessageHandler = new ProgressMessageHandler(new HttpClientHandler());
        progressMessageHandler.HttpReceiveProgress += (_, e) =>
        {
            file.SaveHandle?.Invoke(e.ProgressPercentage, e.TotalBytes.Value);
        };
    }
}
