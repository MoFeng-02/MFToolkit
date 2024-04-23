using Avalonia.Controls;
using MFToolkit.Avaloniaui.Routings;

namespace MFToolkit.Avaloniaui.Material.Controls.MFHanderTemplate;

public partial class LeftTemplate : UserControl
{
    public LeftTemplate()
    {
        InitializeComponent();
        PointerPressed += async (_, _) =>
        {
            await Navigation.GoToAsync("..");
        };
    }
}