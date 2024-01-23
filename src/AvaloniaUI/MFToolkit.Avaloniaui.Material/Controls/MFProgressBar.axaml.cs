using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Helpers;

namespace MFToolkit.Avaloniaui.Material.Controls;

/// <summary>
/// 进度条控件
/// </summary>
public partial class MFProgressBar : UserControl
{
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<MFProgressBar, double>(
        nameof(Value));


    public double Value
    {
        get => GetValue(ValueProperty);
        set { SetValue(ValueProperty, value); }
    }

    public static readonly StyledProperty<double> ProgressBarValueProperty =
        AvaloniaProperty.Register<MFProgressBar, double>(
            nameof(ProgressBarValue));

    public double ProgressBarValue
    {
        get => GetValue(ProgressBarValueProperty);
        set => SetValue(ProgressBarValueProperty, value);
    }

    private bool CheckClassOk => Classes.Count == 0 || Classes[0] != ProgressBarClasses.CircleProgressBar.ToString() &&
        Classes[0]
        != ProgressBarClasses.PercentProgressBar.ToString();

    private double ValueMean()
    {
        if (CheckClassOk)
            return Value;
        if (ProgressBarClasses == ProgressBarClasses.CircleProgressBar)
        {
            var result = Value * 3.60;
            return result;
        }
        else
        {
            var reValue = Width * Value / 100;
            if (reValue > MaxValue) return MaxValue;
            if (reValue < MinValue) return MinValue;
            return reValue;
        }
    }

    public static readonly StyledProperty<double> MaxValueProperty = AvaloniaProperty.Register<MFProgressBar, double>(
        nameof(MaxValue), 100);

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double> MinValueProperty = AvaloniaProperty.Register<MFProgressBar, double>(
        nameof(MinValue));

    public double MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public static readonly StyledProperty<ProgressBarClasses> ProgressBarClassesProperty =
        AvaloniaProperty.Register<MFProgressBar, ProgressBarClasses>(
            nameof(ProgressBarClasses));

    public ProgressBarClasses ProgressBarClasses
    {
        get => GetValue(ProgressBarClassesProperty);
        set
        {
            SetValue(ProgressBarClassesProperty, value);

            Classes.Clear();
            Classes.Add(ProgressBarClasses.ToString());

            ProgressBarValue = ValueMean();
        }
    }

    public new static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<MFProgressBar, IBrush>(
            nameof(Background), ColorHelper.GenerateSolidColorBrush("#ebebeb"));

    /// <summary>
    /// 进度条底色
    /// </summary>
    public new IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public new static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<MFProgressBar, IBrush>(
            nameof(Foreground), ColorHelper.GenerateSolidColorBrush("#1989fa"));

    /// <summary>
    /// 进度条颜色
    /// </summary>
    public new IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<MFProgressBar, double>(
            nameof(StrokeWidth), 10);

    /// <summary>
    /// 进度条线条宽度
    /// </summary>
    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public MFProgressBar()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property.Name == nameof(Value))
        {
            ProgressBarValue = ValueMean();
            Console.WriteLine(ProgressBarValue);
        }

        base.OnPropertyChanged(change);
    }

    protected override void OnInitialized()
    {
        if (double.IsNaN(Width)) Width = 100;
        if (double.IsNaN(Height)) Height = ProgressBarClasses.PercentProgressBar == ProgressBarClasses ? 10 : 100;
        ProgressBarValue = ValueMean();
        base.OnInitialized();
    }
}

public enum ProgressBarClasses
{
    /// <summary>
    /// 条形百分比进度条
    /// </summary>
    PercentProgressBar,

    /// <summary>
    /// 圆进度条
    /// </summary>
    CircleProgressBar,
}