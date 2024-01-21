using Avalonia;
using Avalonia.Controls;

namespace MFToolkit.Avaloniaui.Material.Controls.Dialogs;

public partial class MFDialog : Border
{
    public static readonly StyledProperty<object> ContentProperty = AvaloniaProperty.Register<MFDialog, object>(
        nameof(Content));

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public MFDialog()
    {
        InitializeComponent();
    }
}