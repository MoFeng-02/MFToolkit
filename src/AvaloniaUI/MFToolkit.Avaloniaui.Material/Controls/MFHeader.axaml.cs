using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using MFToolkit.Avaloniaui.Material.Controls.MFHanderTemplate;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFHeader : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<MFHeader, string?>(
        nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<object?> CenterContentProperty = AvaloniaProperty.Register<MFHeader, object?>(
        nameof(CenterContent), default);

    public static readonly StyledProperty<object?> LeftContentProperty = AvaloniaProperty.Register<MFHeader, object?>(
        nameof(LeftContent), default);

    public static readonly StyledProperty<object?> RightContentProperty = AvaloniaProperty.Register<MFHeader, object?>(
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

    public MFHeader()
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