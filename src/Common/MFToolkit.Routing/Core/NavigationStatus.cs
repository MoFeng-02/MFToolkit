namespace MFToolkit.Routing;

/// <summary>
/// 导航状态枚举
/// </summary>
public enum NavigationStatus
{
    /// <summary>
    /// 导航成功
    /// </summary>
    Success,

    /// <summary>
    /// 导航被取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 导航被守卫阻止
    /// </summary>
    Blocked,

    /// <summary>
    /// 路由未找到
    /// </summary>
    NotFound,

    /// <summary>
    /// 导航过程中发生错误
    /// </summary>
    Error
}
