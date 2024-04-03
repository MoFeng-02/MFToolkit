using MFToolkit.Extensions;
using Microsoft.AspNetCore.Http;

namespace MFToolkit.FileManagement.Upload.Utils;
public class UploadUtil
{
    public static async Task DownloadAsync(HttpContext httpContext)
    {
        var sharding = GetShardingAsync(httpContext);
    }
    /// <summary>
    /// 是否分片（没值就是不支持分片）
    /// </summary>
    /// <returns></returns>
    public static string? GetShardingAsync(HttpContext httpContext)
    {
        var sharding = httpContext.GetHeaderString("Content-Range") ?? httpContext.GetHeaderString("Accept-Ranges");
        return sharding;
    }
}
