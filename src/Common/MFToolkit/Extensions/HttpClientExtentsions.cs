using System.Net.Http.Headers;

namespace MFToolkit.Extensions;
/// <summary>
/// HttpClient 拓展
/// </summary>
public static class HttpClientExtentsions
{
    
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
