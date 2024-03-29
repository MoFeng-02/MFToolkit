using Microsoft.AspNetCore.Http;

namespace MFToolkit.Upload.Services;
/// <summary>
/// 上传处理Handler
/// </summary>
public interface IUploadHandler
{
    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    Task GetFileInfoAsync(HttpContext httpContext);
    /// <summary>
    /// 保存文件操作
    /// </summary>
    /// <returns></returns>
    Task<bool> SaveFileAsync();
}
