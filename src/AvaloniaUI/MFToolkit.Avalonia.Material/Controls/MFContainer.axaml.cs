using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFContainer : UserControl
{
    public MFContainer()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MFContainer, bool>(
        nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// 是否处于打开状态
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }


    public static readonly StyledProperty<bool> IsToastOpenProperty = AvaloniaProperty.Register<MFContainer, bool>(
        nameof(IsToastOpen));

    /// <summary>
    /// 是否是toast弹出
    /// </summary>
    public bool IsToastOpen
    {
        get => GetValue(IsToastOpenProperty);
        set => SetValue(IsToastOpenProperty, value);
    }

    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<MFContainer, object?>(
        nameof(Title));

    /// <summary>
    /// Title区域的内容
    /// </summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<object?> DialogContentProperty =
        AvaloniaProperty.Register<MFContainer, object?>(
            nameof(DialogContent));

    public object? DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    public static readonly StyledProperty<object?> ToastContentProperty =
        AvaloniaProperty.Register<MFContainer, object?>(
            nameof(ToastContent));

    public object? ToastContent
    {
        get => GetValue(ToastContentProperty);
        set => SetValue(ToastContentProperty, value);
    }

    public static readonly StyledProperty<bool> ShowAtBottomProperty = AvaloniaProperty.Register<MFContainer, bool>(
        nameof(ShowAtBottom));

    public bool ShowAtBottom
    {
        get => GetValue(ShowAtBottomProperty);
        set => SetValue(ShowAtBottomProperty, value);
    }

    public static readonly StyledProperty<bool> ClickOutsideTheElementToHideProperty =
        AvaloniaProperty.Register<MFContainer, bool>(
            nameof(ClickOutsideTheElementToHide), true);

    /// <summary>
    /// 点击元素外隐藏
    /// </summary>
    public bool ClickOutsideTheElementToHide
    {
        get => GetValue(ClickOutsideTheElementToHideProperty);
        set => SetValue(ClickOutsideTheElementToHideProperty, value);
    }

    /// <summary>
    /// 获取自身实例
    /// </summary>
    /// <returns></returns>
    private static MFContainer GetMFContainerInstance()
    {
        MFContainer container = null!;
        try
        {
            container = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView
                .GetVisualDescendants().OfType<MFContainer>().First();
        }
        catch (Exception e)
        {
            try
            {
                container = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime)
                    .MainWindow.GetVisualDescendants().OfType<MFContainer>().First();
            }
            catch (Exception exception)
            {
                throw new("请设置MFContainer为最顶层控件");
            }
        }

        return container;
    }

    /// <summary>
    /// 获取顶级容器
    /// </summary>
    /// <returns></returns>
    public static MFContainer GetContainer() => GetMFContainerInstance();

    /// <summary>
    /// 显示土司弹窗
    /// </summary>
    /// <param name="content">显示内容</param>
    /// <param name="seconds">持续时间</param>
    public static void ShowToast(object content, int seconds = 2)
    {
        var container = GetMFContainerInstance();
        container.ToastContent = content;
        container.IsToastOpen = true;
        Task.Run(async () =>
        {
            await Task.Delay(seconds * 1000);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                container.IsToastOpen = false;
                container.ToastContent = null;
            });
        });
    }

    /// <summary>
    /// 显示土司弹窗
    /// </summary>
    /// <param name="content">显示内容</param>
    /// <param name="seconds">持续时间</param>
    public static async Task ShowToastAsync(object content, int seconds = 2)
    {
        var container = GetMFContainerInstance();
        container.ToastContent = content;
        container.IsToastOpen = true;
        await Task.Delay(seconds * 1000);
        // await Dispatcher.UIThread.InvokeAsync(() => container.IsToast = false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            container.IsToastOpen = false;
            container.ToastContent = null;
        });
    }

    /// <summary>
    /// 显示Dialog对话
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    public static void ShowDialog(object content, bool showAtBottom = false, bool clickOutsideTheElementToHide = true)
    {
        var container = GetMFContainerInstance();
        container.IsOpen = true;
        container.DialogContent = content;
        container.ShowAtBottom = showAtBottom;
        container.ClickOutsideTheElementToHide = clickOutsideTheElementToHide;
    }

    /// <summary>
    /// 显示Dialog对话
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    public static async Task ShowDialogAsync(object content, bool showAtBottom = false,
        bool clickOutsideTheElementToHide = true)
    {
        var container = GetMFContainerInstance();
        container.IsOpen = true;
        container.DialogContent = content;
        container.ShowAtBottom = showAtBottom;
        container.ClickOutsideTheElementToHide = clickOutsideTheElementToHide;
    }

    /// <summary>
    /// 关闭Dialog
    /// </summary>
    public static void CloasDialog()
    {
        var container = GetMFContainerInstance();
        container.IsOpen = false;
        container.DialogContent = null;
        container.ShowAtBottom = false;
        container.ClickOutsideTheElementToHide = false;
    }

    protected override void OnInitialized()
    {
        var container = GetMFContainerInstance();
        PointerPressed += (o, e) =>
        {
            if (!container.ClickOutsideTheElementToHide) return;
            if (e.Source is not Control control) return;
            var or = GetIsParentDialogContent(control, container);
            if (or) return;
            CloasDialog();
        };
    }

    // private static bool GetIsParentDialogContent(Control control, MFContainer MFContainer)
    // {
    //     if (control.Parent == null) return false;
    //     if (control == MFContainer.DialogContent) return true;
    //     if (control.Parent is not Control reControl) return false;
    //     return GetIsParentDialogContent(reControl, MFContainer);
    // }
    /// <summary>
    /// 获取它的上一级是不是DialogContent
    /// </summary>
    /// <param name="control"></param>
    /// <param name="MFContainer"></param>
    /// <returns></returns>
    private static bool GetIsParentDialogContent(Control control, MFContainer MFContainer)
    {
        while (true)
        {
            if (control.Parent is not Control reControl) return false;
            if (control == MFContainer.DialogContent) return true;
            control = reControl;
        }
    }
}