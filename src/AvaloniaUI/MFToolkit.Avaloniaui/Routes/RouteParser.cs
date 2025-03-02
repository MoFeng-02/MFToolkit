using System.Collections.Immutable;

namespace MFToolkit.Avaloniaui.Routes;

public interface IRouteParser
{
    bool TryParse(string route, out ImmutableArray<RouteSegment> segments);
}

public class DefaultRouteParser : IRouteParser
{
    public bool TryParse(string route, out ImmutableArray<RouteSegment> segments)
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