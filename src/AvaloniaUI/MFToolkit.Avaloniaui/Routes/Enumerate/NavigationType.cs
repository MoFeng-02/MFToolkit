namespace MFToolkit.Avaloniaui.Routes.Enumerate;

/// <summary>
/// 定义路由跳转的类型，用于明确导航操作的行为。
/// </summary>
public enum NavigationType
{
    /// <summary>
    /// 默认的路由跳转操作，将新页面追加到导航堆栈中。
    /// </summary>
    Push,

    /// <summary>
    /// 替换当前页面，而不是追加到导航堆栈中，适用于重定向场景。
    /// </summary>
    Replace,

    /// <summary>
    /// 返回上一页，从导航堆栈中弹出当前页面。
    /// </summary>
    Pop,

    /// <summary>
    /// 返回到导航堆栈的根页面，清空当前堆栈。
    /// </summary>
    PopToRoot,

    #region 暂未实现

    /// <summary>
    /// 以模态方式打开一个新页面，通常用于临时界面或对话框。
    /// </summary>
    ModalPush,

    /// <summary>
    /// 关闭当前模态页面，返回到之前的页面。
    /// </summary>
    ModalPop

    #endregion
}