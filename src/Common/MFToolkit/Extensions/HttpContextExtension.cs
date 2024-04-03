using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MFToolkit.Extensions;
public static class HttpContextExtension
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

        var result = header.ToString();
        if (string.IsNullOrWhiteSpace(result)) return null;
        return result;
    }
    /// <summary>
    /// 添加HttpContext返回的标头
    /// </summary>
    /// <param name="context"></param>
    /// <param name="headerName"></param>
    /// <param name="value"></param>
    public static void SetHttpHender(this HttpContext context, string headerName, string? value)
    {
        context.Response.Headers.Append(headerName, value);
    }
    /// <summary>
    /// 获取User.FindFirstValue 的快捷方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="claimType"></param>
    /// <returns></returns>
    public static string? GetUserValue(this HttpContext context, string claimType)
    {
        return context.User.FindFirstValue(claimType);
    }
}
