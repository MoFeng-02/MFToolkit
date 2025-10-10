using System.Security.Claims;
using MFToolkit.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MFToolkit.AspNetCore.Extensions;

/// <summary>
/// HttpContext 扩展
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 获取指定请求头
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? GetHeaderString(this HttpContext httpContext, string? key)
    {
        if(key.IsNullOrEmpty()) return null;
        var header = httpContext.Request.Headers[key];

        var result = header.ToString();
        if (string.IsNullOrWhiteSpace(result)) return null;
        return result;
    }
    /// <summary>
    /// 获取指定类型的Header值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpContext"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? GetHeaderValue<T>(this HttpContext httpContext, string key)
    {
        var header = httpContext.Request.Headers[key];
        var result = header.ToString();
        if (string.IsNullOrWhiteSpace(result)) return default;
        return result.ConvertTo<T>();
    }
    /// <summary>
    /// 获取指定的Enum值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpContext"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? GetHeaderEnum<T>(this HttpContext httpContext, string key) where T : struct, Enum
    {
        var header = httpContext.Request.Headers[key];
        var result = header.ToString();
        if (string.IsNullOrWhiteSpace(result)) return default;
        var r = Enum.Parse<T>(result);
        return r;
    }
    /// <summary>
    /// 添加HttpContext返回的标头
    /// </summary>
    /// <param name="context"></param>
    /// <param name="headerName"></param>
    /// <param name="value"></param>
    public static void SetResponseHender(this HttpContext context, string headerName, string? value)
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
    /// <summary>
    /// 获取IPV4地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string? GetIpV4(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
    /// <summary>
    /// 获取IPV6地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string? GetIpV6(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.MapToIPv6().ToString();
    }
    /// <summary>
    /// 获取访问日志
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    public static void VisitLog<T>(this HttpContext context, ILogger<T> logger, string? message = null)
    {
        var visitLog = $"IPV4:{context.GetIpV4()}，IPV6:{context.GetIpV6()}，访问方法:{context.Request.Path}" + (message != null ? "," + message : null);
        logger.LogInformation(visitLog);
    }
}
