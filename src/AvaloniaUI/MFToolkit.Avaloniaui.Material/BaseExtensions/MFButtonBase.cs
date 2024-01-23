using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Helpers;
using MFToolkit.Avaloniaui.Material.Controls;

namespace MFToolkit.Avaloniaui.Material.BaseExtensions;

/// <summary>
/// 拓展Button自定义基类
/// </summary>
public class MFButtonBase : Button
{
    public static readonly StyledProperty<ButtonStylesEnum> ButtonClassProperty =
        AvaloniaProperty.Register<MFButtonBase, ButtonStylesEnum>(
            nameof(ButtonClasses));

    public static readonly StyledProperty<bool> LoadingProperty = AvaloniaProperty.Register<MFButtonBase, bool>(
        nameof(Loading));

    /// <summary>
    /// 加载,如果处于加载状态下，再次点击无法进行操作
    /// </summary>
    public bool Loading
    {
        get => GetValue(LoadingProperty);
        set
        {
            SetValue(LoadingProperty, value);
            IsEnabled = !value;
        }
    }

    public static readonly StyledProperty<double> LoadingAreaProperty =
        AvaloniaProperty.Register<MFButtonBase, double>(
            nameof(LoadingArea), 32);

    /// <summary>
    /// loading的大小面积
    /// </summary>
    public double LoadingArea
    {
        get => GetValue(LoadingAreaProperty);
        set => SetValue(LoadingAreaProperty, value);
    }

    public static readonly StyledProperty<IBrush> LoadingRingColorProperty = AvaloniaProperty.Register<MFLoading,
        IBrush>(
        nameof(LoadingRingColor), ColorHelper.GenerateSolidColorBrush("#1989fa"));

    /// <summary>
    /// 圈颜色
    /// </summary>
    public IBrush LoadingRingColor
    {
        get => GetValue(LoadingRingColorProperty);
        set => SetValue(LoadingRingColorProperty, value);
    }

    public static readonly StyledProperty<double> LoadingStrokeThicknessProperty =
        AvaloniaProperty.Register<MFLoading, double>(
            nameof(LoadingStrokeThickness), 2);

    /// <summary>
    /// 圆圈线圈宽度
    /// </summary>
    public double LoadingStrokeThickness
    {
        get => GetValue(LoadingStrokeThicknessProperty);
        set => SetValue(LoadingStrokeThicknessProperty, value);
    }

    public static readonly StyledProperty<LoadingClasses> LoadingClassesProperty =
        AvaloniaProperty.Register<MFLoading, LoadingClasses>(
            nameof(LoadingClasses));

    public LoadingClasses LoadingClasses
    {
        get => GetValue(LoadingClassesProperty);
        set => SetValue(LoadingClassesProperty, value);
    }


    /// <summary>
    /// Button 的样式枚举
    /// </summary>
    public ButtonStylesEnum ButtonClasses
    {
        get => GetValue(ButtonClassProperty);
        set
        {
            // 如果为Default，就不设置了，默认的毕竟就那样
            if (value == ButtonStylesEnum.Default)
            {
                SetValue(ButtonClassProperty, value);
                return;
            }

            var classes = value.ToString();
            Classes.Add(classes);
            SetValue(ButtonClassProperty, value);
        }
    }
}

public class ButtonStylesEnumSelectClassToClassesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ButtonStylesEnum buttonStylesEnum)
        {
            var el = buttonStylesEnum.ToString();
            return el;
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public enum ButtonStylesEnum
{
    Default,
    Primary,
    Info,
    Success,
    Warning,
    Danger
}