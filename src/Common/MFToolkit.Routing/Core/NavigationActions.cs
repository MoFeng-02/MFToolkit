namespace MFToolkit.Routing.Core;

/// <summary>
/// 导航动作常量，提供预定义的导航动作和自定义扩展能力
/// </summary>
public static class NavigationActions
{
    /// <summary>
    /// 推入新页面
    /// </summary>
    public const string Push = "Push";

    /// <summary>
    /// 弹出当前页面
    /// </summary>
    public const string Pop = "Pop";

    /// <summary>
    /// 弹出到根页面
    /// </summary>
    public const string PopToRoot = "PopToRoot";

    /// <summary>
    /// 弹出到指定页面
    /// </summary>
    public const string PopToPage = "PopToPage";

    /// <summary>
    /// 替换当前页面
    /// </summary>
    public const string Replace = "Replace";

    /// <summary>
    /// 切换顶级路由
    /// </summary>
    public const string SwitchTop = "SwitchTop";
}
