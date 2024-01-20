[返回](../Index.md)

```csharp
public class PageResult<T>
{
    /// <summary>
    /// 下标
    /// </summary>
    public int Index { get; set; } = 1;
    /// <summary>
    /// 返回量
    /// </summary>
    public int Size { get; set; } = 20;
    /// <summary>
    /// 总数据量
    /// </summary>
    public RefAsync<int> TotalCount { get; set; }
    /// <summary>
    /// 返回数据
    /// </summary>
    public List<T>? Items { get; set; }
}
public static class PageResultUtil
{
    /// <summary>
    /// 将查询模型转为结果模型
    /// </summary>
    /// <typeparam name="Source"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static PageResult<T> CreatePageResult<Source, T>(this Source source, List<T> t)
    {
        var result = source.Adapt<PageResult<T>>();
        result.Items = t;
        return result;
    }
}
```
