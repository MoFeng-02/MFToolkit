using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using MFToolkit.Avaloniaui.Routings;

namespace MFToolkit.Avaloniaui.Material.Controls;

/// <summary>
/// 路由显示页
/// </summary>
public partial class MFRoutePage : UserControl
{
    public static readonly StyledProperty<Action<object?, RouteCurrentInfo>?> NavigationToProperty =
        AvaloniaProperty.Register<MFRoutePage, Action<object?, RouteCurrentInfo>?>(
            nameof(NavigationTo), defaultBindingMode: BindingMode.OneWayToSource);

    public Action<object?, RouteCurrentInfo>? NavigationTo;

    public static readonly StyledProperty<object?> CurrentPageProperty =
        AvaloniaProperty.Register<MFRoutePage, object?>(
            nameof(CurrentPage), defaultBindingMode: BindingMode.OneWayToSource);

    /// <summary>
    /// 当前页面
    /// </summary>
    public object? CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public static readonly StyledProperty<bool> IsPrevPageProperty = AvaloniaProperty.Register<MFRoutePage, bool>(
        nameof(IsPrevPage), defaultBindingMode: BindingMode.OneWayToSource);

    /// <summary>
    /// 是否还有上一页
    /// </summary>
    public bool IsPrevPage
    {
        get => GetValue(IsPrevPageProperty);
        set => SetValue(IsPrevPageProperty, value);
    }

    public MFRoutePage()
    {
        InitializeComponent();

        NavigationTo = (page, info) =>
        {
            // 注册切换页面时候响应事件
            CurrentPage = page;
            IsPrevPage = Navigation.GetPrevRouting();
        };
        Navigation.SetRoutePageAction(NavigationTo);
    }


    protected override void OnInitialized()
    {
        CurrentPage ??= Routing.GetCurrentInfo()?.CurrentPage;
    }
}