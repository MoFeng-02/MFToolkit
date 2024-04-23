using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using MFToolkit.Avaloniaui.Material.Controls.MFBackHanderTemplate;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFBackHeader : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<MFBackHeader, string?>(
        nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<object?> CenterContentProperty = AvaloniaProperty.Register<MFBackHeader, object?>(
        nameof(CenterContent), default);

    public static readonly StyledProperty<object?> LeftContentProperty = AvaloniaProperty.Register<MFBackHeader, object?>(
        nameof(LeftContent), default);

    public static readonly StyledProperty<object?> RightContentProperty = AvaloniaProperty.Register<MFBackHeader, object?>(
        nameof(RightContent), default);

    /// <summary>
    /// 中间内容
    /// </summary>
    public object? CenterContent
    {
        get => GetValue(CenterContentProperty);
        set => SetValue(CenterContentProperty, value);
    }

    /// <summary>
    /// 左边内容
    /// </summary>
    public Object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    /// <summary>
    /// 右边内容
    /// </summary>
    public object RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    public MFBackHeader()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        LeftContent ??= new LeftTemplate();
        CenterContent ??= new TextBlock
        {
            Text = Title,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }
}