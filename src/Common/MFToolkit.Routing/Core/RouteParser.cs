using System.Text.RegularExpressions;

namespace MFToolkit.Routing;

/// <summary>
/// 路由路径解析器，用于解析路由路径中的参数
/// </summary>
public static class RouteParser
{
    // 匹配 :paramName 格式的参数
    private static readonly Regex ParameterPattern = new(@":(\w+)", RegexOptions.Compiled);

    /// <summary>
    /// 从路由路径中提取参数名列表
    /// </summary>
    /// <param name="routePath">路由路径，如 "/user/:id/profile"</param>
    /// <returns>参数名列表</returns>
    public static IReadOnlyList<string> GetParameterNames(string routePath)
    {
        if (string.IsNullOrEmpty(routePath))
            return Array.Empty<string>();

        var matches = ParameterPattern.Matches(routePath);
        return matches.Select(m => m.Groups[1].Value).ToList();
    }

    /// <summary>
    /// 检查路由路径是否包含参数
    /// </summary>
    public static bool HasParameters(string routePath)
    {
        if (string.IsNullOrEmpty(routePath))
            return false;

        return ParameterPattern.IsMatch(routePath);
    }

    /// <summary>
    /// 解析实际路径，提取参数值
    /// </summary>
    /// <param name="routePath">注册的路由路径，如 "/user/:id"</param>
    /// <param name="actualPath">实际访问路径，如 "/user/123"</param>
    /// <returns>参数字典，如果路径不匹配返回 null</returns>
    public static Dictionary<string, object?>? ParseParameters(string routePath, string actualPath)
    {
        if (string.IsNullOrEmpty(routePath) || string.IsNullOrEmpty(actualPath))
            return null;

        // 清理路径
        routePath = routePath.TrimEnd('/');
        actualPath = actualPath.TrimEnd('/');

        // 按 / 分割
        var routeParts = routePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var actualParts = actualPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // 段数不匹配
        if (routeParts.Length != actualParts.Length)
            return null;

        var parameters = new Dictionary<string, object?>();

        for (int i = 0; i < routeParts.Length; i++)
        {
            var routePart = routeParts[i];
            var actualPart = actualParts[i];

            // 检查是否是参数段
            if (routePart.StartsWith(':'))
            {
                var paramName = routePart[1..]; // 去掉冒号
                parameters[paramName] = actualPart;
            }
            else if (!routePart.Equals(actualPart, StringComparison.OrdinalIgnoreCase))
            {
                // 非参数段必须完全匹配
                return null;
            }
        }

        return parameters.Count > 0 ? parameters : null;
    }

    /// <summary>
    /// 从路由路径生成正则表达式模式
    /// </summary>
    public static string ToRegexPattern(string routePath)
    {
        if (string.IsNullOrEmpty(routePath))
            return string.Empty;

        var pattern = Regex.Escape(routePath.TrimEnd('/'));
        pattern = ParameterPattern.Replace(pattern, @"([^/]+)");

        return $"^{pattern}$";
    }

    /// <summary>
    /// 检查路径是否匹配路由模式
    /// </summary>
    public static bool IsMatch(string routePath, string actualPath)
    {
        return ParseParameters(routePath, actualPath) != null;
    }

    /// <summary>
    /// 根据参数生成实际路径
    /// </summary>
    /// <param name="routePath">路由路径，如 "/user/:id/profile"</param>
    /// <param name="parameters">参数值</param>
    /// <returns>替换后的路径</returns>
    public static string BuildPath(string routePath, Dictionary<string, object?>? parameters)
    {
        if (string.IsNullOrEmpty(routePath) || parameters == null)
            return routePath;

        var result = routePath;

        foreach (var param in parameters)
        {
            result = result.Replace($":{param.Key}", param.Value?.ToString() ?? string.Empty);
        }

        return result;
    }
}
