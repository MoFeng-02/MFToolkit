using System.Collections.Immutable;
using MFToolkit.Avaloniaui.Routes.Core.Entities;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;

namespace MFToolkit.Avaloniaui.Routes.Infrastructure.Utils;

/// <summary>
/// 路由解析器实现类，负责路由的解析和匹配
/// </summary>
public class RouteParser : IRouteParser
{
    /// <summary>
    /// 默认提供器
    /// </summary>
    public static readonly RouteParser Default = new RouteParser();
    /// <summary>
    /// 尝试解析路由字符串为路由段集合
    /// </summary>
    /// <param name="route">路由字符串</param>
    /// <param name="segments">解析后的路由段集合</param>
    /// <returns>是否解析成功</returns>
    public bool TryParse(string route, out ImmutableArray<RouteSegment> segments)
    {
        segments = ImmutableArray<RouteSegment>.Empty;
        
        if (string.IsNullOrWhiteSpace(route))
        {
            return false;
        }
        
        var routeSegments = new List<RouteSegment>();
        var parts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            if (part.StartsWith("{") && part.EndsWith("}"))
            {
                // 参数段，如 {id}
                var paramName = part[1..^1]; // 去除首尾的大括号
                routeSegments.Add(new RouteSegment(paramName, true, false));
            }
            else if (part.StartsWith("*"))
            {
                // 通配符段，如 *path
                var wildcardName = part[1..]; // 去除星号
                routeSegments.Add(new RouteSegment(wildcardName, false, true));
            }
            else
            {
                // 普通段
                routeSegments.Add(new RouteSegment(part, false, false));
            }
        }
        
        segments = routeSegments.ToImmutableArray();
        return true;
    }
    
    /// <summary>
    /// 匹配路由
    /// </summary>
    /// <param name="route">要匹配的路由</param>
    /// <param name="pattern">路由模式</param>
    /// <param name="parameters">匹配到的参数</param>
    /// <returns>是否匹配成功</returns>
    public bool Match(string route, string pattern, out Dictionary<string, string>? parameters)
    {
        parameters = null;
        
        if (string.IsNullOrWhiteSpace(route) || string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }
        
        // 解析路由模式和实际路由
        if (!TryParse(pattern, out var patternSegments) || 
            !TryParse(route, out var routeSegments))
        {
            return false;
        }
        
        // 检查基本匹配条件
        if (patternSegments.Length > routeSegments.Length &&
            !patternSegments.Last().IsWildcard)
        {
            return false;
        }
        
        if (patternSegments.Length < routeSegments.Length &&
            !patternSegments.Any(s => s.IsWildcard))
        {
            return false;
        }
        
        parameters = new Dictionary<string, string>();
        
        // 逐个匹配路由段
        for (var i = 0; i < patternSegments.Length; i++)
        {
            var patternSeg = patternSegments[i];
            RouteSegment? routeSeg = null;
            
            if (i < routeSegments.Length)
            {
                routeSeg = routeSegments[i];
            }
            
            // 处理通配符段
            if (patternSeg.IsWildcard)
            {
                var wildcardValue = string.Join("/", 
                    routeSegments.Skip(i).Select(s => s.Name));
                parameters[patternSeg.Name] = wildcardValue;
                return true;
            }
            
            // 如果路由段不足，匹配失败
            if (routeSeg == null)
            {
                parameters = null;
                return false;
            }
            
            // 处理参数段
            if (patternSeg.IsParameter)
            {
                parameters[patternSeg.Name] = routeSeg.Name;
            }
            // 处理普通段
            else if (!string.Equals(patternSeg.Name, routeSeg.Name, 
                     StringComparison.OrdinalIgnoreCase))
            {
                parameters = null;
                return false;
            }
        }
        
        // 所有段匹配成功
        return true;
    }
}
    