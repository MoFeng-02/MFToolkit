using System.Net.Http.Headers;
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

        var result = header.ToString();
        if (string.IsNullOrWhiteSpace(result)) return null;
        return result;
    }
    /// <summary>
    /// 提取对应Header（Key集合的第一个）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="headers"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? GetHeaderValuesFirst<T>(this HttpHeaders headers, string key)
    {
        var value = headers.TryGetValues("Content-Length", out var content);
        if (!value || content == null) return default;
        var rValue = content.FirstOrDefault();
        if (rValue == null) return default;
        try
        {
            return rValue.ConvertTo<T>();
        }
        catch (Exception)
        {
            return default;
        }
    }
}
