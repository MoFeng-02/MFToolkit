using Avalonia.Controls;
using MFToolkit.Avaloniaui.BaseExtensions;

namespace MFToolkit.Avaloniaui.Routings;


public sealed class Navigation
{
    private static Action<object?, RouteCurrentInfo>? NavigationTo { get; set; }

    /// <summary>
    /// 设置页面返回处理
    /// </summary>
    /// <param name="action"></param>
    public static void SetPageAction(Action<object?, RouteCurrentInfo> action)
    {
        NavigationTo = action;
    }

    /// <summary>
    /// 跳转页面
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    public static async Task<object?> GoToAsync(string route)
    {
        var page = await Routing.GoToAsync(route);
        if (page == null) return null;
        if (page.CurrentPage is Control control)
        {
            var vm = control.DataContext as ViewModelBase;
            // 调用子类的接收参数方法
#if NET8_0_OR_GREATER
            vm?.ApplyQueryAttributes(page.Parameters ?? []);
#else
            vm?.ApplyQueryAttributes(page.Parameters ?? new());
#endif
        }

        NavigationTo?.Invoke(page?.CurrentPage, page);
        return page?.CurrentPage;
    }

    /// <summary>
    /// 获取是否还有上一页
    /// </summary>
    /// <returns></returns>
    public static bool GetPrevRouting() => Routing.GetPrevRouting();
}