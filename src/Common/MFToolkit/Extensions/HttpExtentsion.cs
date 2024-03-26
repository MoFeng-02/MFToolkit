using Microsoft.AspNetCore.Http;

namespace MFToolkit.Extensions;
public static class HttpExtentsion
{
    /// <summary>
    /// 获取指定请求头
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? GetHeaderString(this HttpContext httpContext, string key)
    {
        var header = httpContext.Request.Headers[key];
        return header.ToString();
    }
}
