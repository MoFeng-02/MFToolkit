using System.Linq.Expressions;
using SqlSugar;

namespace MFToolkit.SqlSugarExtensions.Extensions;
public static class SelectExtension
{
    public static ISugarQueryable<T> SelectNav<T, TResult>(Expression<Func<T, TResult>> expression)
    {
        return null;
    }
}
