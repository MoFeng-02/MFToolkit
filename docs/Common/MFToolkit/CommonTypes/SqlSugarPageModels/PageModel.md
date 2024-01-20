[返回](../Index.md)

```csharp
public class PageModel<Condition> where Condition : class
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
    public RefAsync<int> TotalCount { get; set; } = new();
    public Condition SearchCondition { get; set; }
}

```
