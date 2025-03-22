using Avalonia.Controls;
using MFToolkit.Avaloniaui.Routes;

namespace DemoDesktop.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = Navigation.GoToAsync("download/1.0.0/alpha");
    }
}
