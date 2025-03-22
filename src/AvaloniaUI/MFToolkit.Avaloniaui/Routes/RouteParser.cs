using System.Collections.Immutable;

namespace MFToolkit.Avaloniaui.Routes;

/// <summary>
/// 默认解析，支持/{param}
/// </summary>
public class RouteParser
{
    /// <summary>
    /// 解析路由
    /// </summary>
    /// <param name="route"></param>
    /// <param name="segments"></param>
    /// <returns></returns>
    public static bool TryParse(string route, out ImmutableArray<RouteSegment> segments)
    {
        var builder = ImmutableArray.CreateBuilder<RouteSegment>();
        var parts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (part.StartsWith('{') && part.EndsWith('}'))
            {
                var parameter = part[1..^1];
                builder.Add(new RouteSegment(SegmentType.Parameter, parameter));
            }
            else
            {
                builder.Add(new RouteSegment(SegmentType.Static, part));
            }
        }

        segments = builder.ToImmutable();
        return true;
    }
}