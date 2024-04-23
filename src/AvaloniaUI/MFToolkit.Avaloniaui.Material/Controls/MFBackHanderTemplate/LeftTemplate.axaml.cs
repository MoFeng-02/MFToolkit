using Avalonia.Controls;
using MFToolkit.Avaloniaui.Routings;

namespace MFToolkit.Avaloniaui.Material.Controls.MFBackHanderTemplate;

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