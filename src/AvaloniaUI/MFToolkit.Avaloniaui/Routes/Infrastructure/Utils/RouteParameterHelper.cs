using System.Reflection;

namespace MFToolkit.Avaloniaui.Routes.Infrastructure.Utils;

/// <summary>
/// 路由参数帮助类，用于处理路由参数的转换和拼接
/// </summary>
public static class RouteParameterHelper
{
    /// <summary>
    /// 将参数拼接到路由中
    /// </summary>
    /// <param name="route">基础路由</param>
    /// <param name="parameters">参数字典</param>
    /// <returns>包含参数的完整路由</returns>
    public static string MergeParameters(string route, Dictionary<string, object?>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return route;
        }
        
        // 检查是否已有查询参数
        var hasQuery = route.Contains('?');
        var queryParameters = new List<string>();
        
        foreach (var (key, value) in parameters)
        {
            // 跳过空值
            if (value == null)
            {
                continue;
            }
            
            // 转换为字符串
            var stringValue = Convert.ToString(value) ?? string.Empty;
            queryParameters.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(stringValue)}");
        }
        
        if (queryParameters.Count == 0)
        {
            return route;
        }
        
        return $"{route}{(hasQuery ? "&" : "?")}{string.Join("&", queryParameters)}";
    }
    
    /// <summary>
    /// 解析路由中的查询参数
    /// </summary>
    /// <param name="route">包含查询参数的路由</param>
    /// <returns>解析后的参数字典</returns>
    public static Dictionary<string, object?> ParseQueryParameters(string route)
    {
        var parameters = new Dictionary<string, object?>();
        
        if (string.IsNullOrWhiteSpace(route))
        {
            return parameters;
        }
        
        var queryIndex = route.IndexOf('?');
        if (queryIndex == -1 || queryIndex >= route.Length - 1)
        {
            return parameters;
        }
        
        var queryString = route.Substring(queryIndex + 1);
        var queryParts = queryString.Split('&');
        
        foreach (var part in queryParts)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }
            
            var keyValue = part.Split('=', 2);
            var key = Uri.UnescapeDataString(keyValue[0]);
            var value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : null;
            
            parameters[key] = value;
        }
        
        return parameters;
    }
    
    /// <summary>
    /// 将参数转换为目标类型
    /// </summary>
    /// <param name="parameters">原始参数字典</param>
    /// <param name="targetType">目标类型</param>
    /// <returns>转换后的参数对象</returns>
    public static object? ConvertParametersToObject(Dictionary<string, object?>? parameters, Type? targetType)
    {
        if (parameters == null || parameters.Count == 0 || targetType == null)
        {
            return null;
        }
        
        var instance = Activator.CreateInstance(targetType);
        if (instance == null)
        {
            return null;
        }
        
        var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            if (parameters.TryGetValue(property.Name, out var value) && value != null)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch
                {
                    // 转换失败时忽略该参数
                }
            }
        }
        
        return instance;
    }
}
    