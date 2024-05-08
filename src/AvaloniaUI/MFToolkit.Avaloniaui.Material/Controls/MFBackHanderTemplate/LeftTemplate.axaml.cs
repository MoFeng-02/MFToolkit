using Avalonia;
using Avalonia.Controls;
using MFToolkit.Avaloniaui.Helpers;
using MFToolkit.Avaloniaui.Routings;

namespace MFToolkit.Avaloniaui.Material.Controls.MFBackHanderTemplate;

public partial class LeftTemplate : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<LeftTemplate, string?>(
        nameof(Title));

    /// <summary>
    /// 返回标题
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public LeftTemplate()
    {
        InitializeComponent();
        PointerPressed += async (_, _) => { await Navigation.GoToAsync(".."); };
    }
}