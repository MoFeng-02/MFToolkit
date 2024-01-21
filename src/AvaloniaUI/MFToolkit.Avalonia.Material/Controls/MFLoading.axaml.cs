using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Helpers;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFLoading : UserControl
{
    public static readonly StyledProperty<double> CircleAreaProperty =
        AvaloniaProperty.Register<MFLoading, double>(
            nameof(CircleArea), 52);

    /// <summary>
    /// 圆面积，代表宽和高
    /// </summary>
    public double CircleArea
    {
        get => GetValue(CircleAreaProperty);
        set => SetValue(CircleAreaProperty, value);
    }

    public static readonly StyledProperty<IBrush> RingColorProperty = AvaloniaProperty.Register<MFLoading,
        IBrush>(
        nameof(RingColor), ColorHelper.GenerateSolidColorBrush("#1989fa"));

    /// <summary>
    /// 圈颜色
    /// </summary>
    public IBrush RingColor
    {
        get => GetValue(RingColorProperty);
        set => SetValue(RingColorProperty, value);
    }

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<MFLoading, double>(
            nameof(StrokeThickness), 2);

    /// <summary>
    /// 圆圈线圈宽度
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly StyledProperty<LoadingClasses> LoadingClassesProperty =
        AvaloniaProperty.Register<MFLoading, LoadingClasses>(
            nameof(LoadingClasses));

    public LoadingClasses LoadingClasses
    {
        get => GetValue(LoadingClassesProperty);
        set => SetValue(LoadingClassesProperty, value);
    }

    public MFLoading()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        if (LoadingClasses != LoadingClasses.Default)
        {
            Classes.Add(LoadingClasses.ToString());
        }
    }
}

/// <summary>
/// 内部样式提示措施
/// </summary>
public enum LoadingClasses
{
    Default,
    White
}