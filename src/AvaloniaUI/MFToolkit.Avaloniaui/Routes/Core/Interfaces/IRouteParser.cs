using System.Collections.Immutable;
using MFToolkit.Avaloniaui.Routes.Core.Entities;

namespace MFToolkit.Avaloniaui.Routes.Core.Interfaces;

/// <summary>
/// 路由解析器接口，定义路由解析的基本功能
/// </summary>
public interface IRouteParser
{
    /// <summary>
    /// 尝试解析路由字符串为路由段集合
    /// </summary>
    /// <param name="route">路由字符串</param>
    /// <param name="segments">解析后的路由段集合</param>
    /// <returns>是否解析成功</returns>
    bool TryParse(string route, out ImmutableArray<RouteSegment> segments);
    
    /// <summary>
    /// 匹配路由
    /// </summary>
    /// <param name="route">要匹配的路由</param>
    /// <param name="pattern">路由模式</param>
    /// <param name="parameters">匹配到的参数</param>
    /// <returns>是否匹配成功</returns>
    bool Match(string route, string pattern, out Dictionary<string, string>? parameters);
}
    