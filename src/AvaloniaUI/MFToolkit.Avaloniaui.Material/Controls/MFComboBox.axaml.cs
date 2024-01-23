using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFComboBox : ComboBox
{
    public static readonly StyledProperty<ICommand?> SelectedCommandProperty =
        AvaloniaProperty.Register<MFComboBox, ICommand?>(
            nameof(SelectedCommand));

    public ICommand? SelectedCommand
    {
        get => GetValue(SelectedCommandProperty);
        set => SetValue(SelectedCommandProperty, value);
    }

    public MFComboBox()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        var a = this;
        SelectionChanged += (o, e) => { SelectedCommand?.Execute(o); };
    }
}