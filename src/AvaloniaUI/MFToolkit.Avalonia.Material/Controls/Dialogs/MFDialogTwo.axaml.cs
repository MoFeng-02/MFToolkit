using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MFToolkit.Avaloniaui.Material.Controls.BaseExtensions;

namespace MFToolkit.Avaloniaui.Material.Controls.Dialogs;

/// <summary>
/// 通用的Dialog控件
/// </summary>
public partial class MFDialogTwo : Border
{
    public static readonly StyledProperty<object?> LeftButtonContentProperty =
        AvaloniaProperty.Register<MFDialogTwo, object?>(
            nameof(LeftButtonContent), "取消");

    public object? LeftButtonContent
    {
        get => GetValue(LeftButtonContentProperty);
        set => SetValue(LeftButtonContentProperty, value);
    }

    public static readonly StyledProperty<ButtonStylesEnum> LeftButtonClassProperty =
        AvaloniaProperty.Register<MFDialogTwo, ButtonStylesEnum>(
            nameof(LeftButtonClass));

    public ButtonStylesEnum LeftButtonClass
    {
        get => GetValue(LeftButtonClassProperty);
        set => SetValue(LeftButtonClassProperty, value);
    }

    public static readonly StyledProperty<object?> RightButtonContentProperty =
        AvaloniaProperty.Register<MFDialogTwo, object?>(
            nameof(RightButtonContent), "确认");

    public object? RightButtonContent
    {
        get => GetValue(RightButtonContentProperty);
        set => SetValue(RightButtonContentProperty, value);
    }

    public static readonly StyledProperty<ButtonStylesEnum> RightButtonClassProperty =
        AvaloniaProperty.Register<MFDialogTwo, ButtonStylesEnum>(
            nameof(RightButtonClass), ButtonStylesEnum.Primary);

    public ButtonStylesEnum RightButtonClass
    {
        get => GetValue(RightButtonClassProperty);
        set => SetValue(RightButtonClassProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<MFDialogTwo, object?>(
        nameof(Content));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public event EventHandler<EventArgs>? LeftButtonClicked;
    public event EventHandler<EventArgs>? RightButtonClicked;

    public MFDialogTwo()
    {
        InitializeComponent();
    }

    private void Left_OnClick(object sender, RoutedEventArgs e)
    {
        LeftButtonClicked?.Invoke(sender, e);
    }

    private void Right_OnClick(object sender, RoutedEventArgs e)
    {
        RightButtonClicked?.Invoke(sender, e);
    }

    protected override void OnInitialized()
    {
        //LeftButton.ButtonClasses = LeftButtonClass;
        //RightButton.ButtonClasses = RightButtonClass;
    }
}