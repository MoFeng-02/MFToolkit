using Mapster;

namespace MFToolkit.SqlSugarExtensions.Extensions;
public static class MapperExtension
{
    public static TResult Adapt<TSource, TResult>(this TSource source)
    {
        return source.Adapt<TResult>();
    }
}
