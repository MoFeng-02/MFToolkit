using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MFToolkit.Avaloniaui.Material.Controls.BaseExtensions;

namespace MFToolkit.Avaloniaui.Material.Controls.Dialogs;

public partial class MFDialogOne : Border
{
    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<MFDialogOne, object?>(
        nameof(Content));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<string> ButtonTextProperty = AvaloniaProperty.Register<MFDialogOne, string>(
        nameof(ButtonText));

    public string ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly StyledProperty<ButtonStylesEnum> ButtonClassProperty =
        AvaloniaProperty.Register<MFDialogOne, ButtonStylesEnum>(
            nameof(ButtonClass));

    public ButtonStylesEnum ButtonClass
    {
        get => GetValue(ButtonClassProperty);
        set => SetValue(ButtonClassProperty, value);
    }

    public MFDialogOne()
    {
        InitializeComponent();
    }

    public event EventHandler<EventArgs>? Clicked;

    protected override void OnInitialized()
    {
        //ButtonOne.ButtonClasses = ButtonClass;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Clicked?.Invoke(sender, e);
    }
}