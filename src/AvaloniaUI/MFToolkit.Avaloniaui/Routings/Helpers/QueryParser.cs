using System.Net;

namespace MFToolkit.Avaloniaui.Routings.Helpers;

/// <summary>
/// 自定义查询参数解析器（替代QueryHelpers.ParseQuery）
/// </summary>
public static class QueryParser
{
    /// <summary>
    /// 解析查询字符串到字典集合
    /// </summary>
    /// <param name="queryString">查询字符串（不带问号）</param>
    /// <returns>参数键值对字典</returns>
    public static Dictionary<string, object?> Parse(string queryString)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(queryString))
            return parameters;

        // 拆分键值对
        var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            var key = keyValue[0].Trim();

            if (string.IsNullOrEmpty(key))
                continue;

            // URL解码并处理空值
            var value = keyValue.Length > 1 ?
                WebUtility.UrlDecode(keyValue[1]) :
                string.Empty;

            // 只保留第一个出现的参数（与ASP.NET Core默认行为一致）
            if (!parameters.ContainsKey(key))
            {
                parameters.Add(key, value);
            }
        }

        return parameters;
    }
}