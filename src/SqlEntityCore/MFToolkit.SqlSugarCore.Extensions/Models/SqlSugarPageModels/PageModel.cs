using SqlSugar;

namespace MFToolkit.SqlSugarCore.Extensions.Models.SqlSugarPageModels;
public class PageModel
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
}
