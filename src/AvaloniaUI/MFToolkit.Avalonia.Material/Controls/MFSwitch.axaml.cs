using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Helpers;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFSwitch : Border
{
    private Border? _circle;
    private Image? _icon;

    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<MFSwitch, bool>(
        nameof(IsLoading));

    /// <summary>
    /// 是否启用加载
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<MFSwitch, bool>(nameof(IsChecked), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// 是否选择
    /// </summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set
        {
            // 加载中不允许设置值
            if (IsLoading)
            {
                SetValue(IsCheckedProperty, !value);
                return;
            }

            SetValue(IsCheckedProperty, value);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsCheckedProperty)
        {
            // 处理对应改变状态
            CheckedUpdateState();
        }
    }

    public static readonly StyledProperty<double> InnerCircleMeanValueProperty =
        AvaloniaProperty.Register<MFSwitch, double>(
            nameof(InnerCircleMeanValue), 20);

    /// <summary>
    /// 内部圆圈统一宽高值
    /// </summary>
    public double InnerCircleMeanValue
    {
        get => GetValue(InnerCircleMeanValueProperty);
        set => SetValue(InnerCircleMeanValueProperty, value);
    }

    public static readonly StyledProperty<double> InnerCircleWidthProperty = AvaloniaProperty.Register<MFSwitch,
        double>(
        nameof(InnerCircleWidth), double.NaN);

    /// <summary>
    /// 内部圆圈宽度
    /// </summary>
    public double InnerCircleWidth
    {
        get => GetValue(InnerCircleWidthProperty);
        set => SetValue(InnerCircleWidthProperty, value);
    }

    public static readonly StyledProperty<double> InnerCircleHeightProperty = AvaloniaProperty.Register<MFSwitch,
        double>(
        nameof(InnerCircleHeight), double.NaN);

    /// <summary>
    /// 内部圆圈高度
    /// </summary>
    public double InnerCircleHeight
    {
        get => GetValue(InnerCircleHeightProperty);
        set => SetValue(InnerCircleHeightProperty, value);
    }
    //
    // public static readonly StyledProperty<string> CheckedTextProperty = AvaloniaProperty.Register<MFSwitch, string>(
    //     nameof(CheckedText));
    //
    // /// <summary>
    // /// 选中时的文本
    // /// </summary>
    // public string CheckedText
    // {
    //     get => GetValue(CheckedTextProperty);
    //     set => SetValue(CheckedTextProperty, value);
    // }
    //
    // public static readonly StyledProperty<string> UnCheckedTextProperty = AvaloniaProperty.Register<MFSwitch, string>(
    //     nameof(UnCheckedText));
    //
    // /// <summary>
    // /// 未选中时的文本
    // /// </summary>
    // public string UnCheckedText
    // {
    //     get => GetValue(UnCheckedTextProperty);
    //     set => SetValue(UnCheckedTextProperty, value);
    // }

    public static readonly StyledProperty<IBrush> CheckedBrushProperty = AvaloniaProperty.Register<MFSwitch, IBrush>(
        nameof(CheckedBrush), ColorHelper.GenerateSolidColorBrush("#1989fa"));

    /// <summary>
    /// 选中时的颜色
    /// </summary>
    public IBrush CheckedBrush
    {
        get => GetValue(CheckedBrushProperty);
        set => SetValue(CheckedBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush> UnCheckedBrushProperty = AvaloniaProperty.Register<MFSwitch,
        IBrush>(
        nameof(UnCheckedBrush), ColorHelper.GenerateSolidColorBrush("#e2e3e7"));


    /// <summary>
    /// 未选中时的颜色
    /// </summary>
    public IBrush UnCheckedBrush
    {
        get => GetValue(UnCheckedBrushProperty);
        set => SetValue(UnCheckedBrushProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CheckedCommandProperty =
        AvaloniaProperty.Register<MFSwitch, ICommand?>(
            nameof(CheckedCommand));

    /// <summary>
    /// 选中时候的命令
    /// </summary>
    public ICommand? CheckedCommand
    {
        get => GetValue(CheckedCommandProperty);
        set => SetValue(CheckedCommandProperty, value);
    }

    public static readonly StyledProperty<object?> CheckedCommandParameterProperty =
        AvaloniaProperty.Register<MFSwitch, object?>(
            nameof(CheckedCommandParameter));

    /// <summary>
    /// 需要传输给CheckedCommand的值
    /// </summary>
    public object? CheckedCommandParameter
    {
        get => GetValue(CheckedCommandParameterProperty);
        set => SetValue(CheckedCommandParameterProperty, value);
    }

    public static readonly StyledProperty<IImage?> CheckedIconProperty = AvaloniaProperty.Register<MFSwitch, IImage?>(
        nameof(CheckedIcon));

    /// <summary>
    /// 选中时的按钮圆圈内部Icon
    /// </summary>
    public IImage? CheckedIcon
    {
        get => GetValue(CheckedIconProperty);
        set => SetValue(CheckedIconProperty, value);
    }

    public static readonly StyledProperty<IImage?> UnCheckedIconProperty =
        AvaloniaProperty.Register<MFSwitch, IImage?>(
            nameof(UnCheckedIcon));

    /// <summary>
    /// 未选中时的按钮圆圈内部Icon
    /// </summary>
    public IImage? UnCheckedIcon
    {
        get => GetValue(UnCheckedIconProperty);
        set => SetValue(UnCheckedIconProperty, value);
    }

    /// <summary>
    /// 切换选择事件
    /// </summary>
    public event EventHandler<EventArgs>? Checked;

    public MFSwitch()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 选中动画过渡效果
    /// </summary>
    /// <returns></returns>
    private static void CheckedTransitions(Animatable element)
    {
        element.Transitions = new()
        {
            new ThicknessTransition()
            {
                Property = MarginProperty,
                Duration = TimeSpan.FromSeconds(0.1)
            }
        };
    }

    private void CheckedUpdateState()
    {
        if (_icon == null && _circle == null) return;
        Background = IsChecked ? CheckedBrush : UnCheckedBrush;
        _icon!.Source = IsChecked ? CheckedIcon : UnCheckedIcon;
        // circle.HorizontalAlignment = IsChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        _circle!.Margin = new Thickness(IsChecked ? Width - (_circle.Width + Padding.Left + Padding.Right) : 0, 0, 0,
            0);
    }

    protected override void OnInitialized()
    {
        // 使用代码进行控件初始化
        Background = IsChecked ? CheckedBrush : UnCheckedBrush; // 设为透明
        Width = double.IsNaN(Width) ? 50 : Width; // 默认宽度为60
        if (Padding is { Left: 0, Top: 0, Right: 0, Bottom: 0 })
            Padding = Thickness.Parse("2");
        CornerRadius = new CornerRadius(50);
        // 定义内部子控件
        _circle = new();
        _icon = new();
        _icon.Source = IsChecked ? CheckedIcon : UnCheckedIcon;
        _circle.Child = _icon;
        _circle.Width = double.IsNaN(InnerCircleWidth) ? InnerCircleMeanValue : InnerCircleWidth;
        _circle.Height = double.IsNaN(InnerCircleHeight) ? InnerCircleMeanValue : InnerCircleHeight;
        _circle.Background = Brushes.White;
        _circle.CornerRadius = new(50);
        _circle.HorizontalAlignment = HorizontalAlignment.Left;
        _circle.VerticalAlignment = VerticalAlignment.Center;
        _circle.Margin = new Thickness(IsChecked ? Width - (_circle.Width + Padding.Left + Padding.Right) : 0, 0, 0,
            0);
        CheckedTransitions(_circle);
        Child = _circle;


        Tapped += (o, e) =>
        {
            // 加载中不允许执行事物
            if (IsLoading) return;
            IsChecked = !IsChecked;
            if (CheckedCommand != null)
            {
                CheckedCommand?.Execute(CheckedCommandParameter);
                return;
            }

            Checked?.Invoke(o, e);
        };
    }
}