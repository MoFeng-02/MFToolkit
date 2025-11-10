namespace MFToolkit.Avaloniaui.Routes.Core.Abstractions;

/// <summary>
/// 查询参数接口，实现此接口的视图模型可以接收路由参数
/// </summary>
public interface IQueryAttributable
{
    /// <summary>
    /// 路由传递参数
    /// </summary>
    /// <param name="parameters">查询参数字典</param>
    void ApplyQueryAttributes(IDictionary<string, object?> parameters);
}
    