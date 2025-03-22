using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MFToolkit.Avaloniaui.Routes;

namespace DemoDesktop;

public partial class DemoPage : UserControl,IPageLifecycle
{
    public DemoPage()
    {
        InitializeComponent();
    }

    public async Task OnActivatedAsync(Dictionary<string, object?> parameters)
    {
    }

    public async Task OnDeactivatedAsync()
    {
    }

    public async Task OnReactivatedAsync(Dictionary<string, object?> parameters)
    {
    }
}