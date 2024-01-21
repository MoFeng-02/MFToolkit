using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Material.Controls.Models;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFTabbarItem : Button
{
#if NET8_0_OR_GREATER
    public static List<TabbarItem> Items = [];
#else
    public static List<TabbarItem> Items = new();
#endif
    public static Grid grid = new();
    private TabbarItem _tabbarItem = new();

    public TabbarItem GetTabbarItem() => _tabbarItem;
    public bool IsActive { get; set; }
    public int ActiveIndex { get; set; }

    public static readonly StyledProperty<object?> TabContentProperty =
        AvaloniaProperty.Register<MFTabbarItem, object?>(
            nameof(IconControl));

    public object? IconControl
    {
        get => GetValue(TabContentProperty);
        set => SetValue(TabContentProperty, value);
    }

    public static readonly StyledProperty<string> RouteProperty = AvaloniaProperty.Register<MFTabbarItem, string>(
        nameof(Route));

    public string Route
    {
        get => GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<MFTabbarItem, string>(
        nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<IImage?> SelectedImageProperty =
        AvaloniaProperty.Register<MFTabbarItem, IImage?>(
            nameof(SelectedImage));

    public IImage? SelectedImage
    {
        get => GetValue(SelectedImageProperty);
        set => SetValue(SelectedImageProperty, value);
    }

    public static readonly StyledProperty<IImage?> UnSelectedImageProperty =
        AvaloniaProperty.Register<MFTabbarItem, IImage?>(
            nameof(UnSelectedImage));

    public IImage? UnSelectedImage
    {
        get => GetValue(UnSelectedImageProperty);
        set => SetValue(UnSelectedImageProperty, value);
    }

    public static readonly StyledProperty<IBrush> SelectedTextColorProperty =
        AvaloniaProperty.Register<MFTabbarItem, IBrush>(
            nameof(SelectedTextColor));

    public IBrush SelectedTextColor
    {
        get => GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    public static readonly StyledProperty<IBrush> UnSelectedTextColorProperty =
        AvaloniaProperty.Register<MFTabbarItem, IBrush>(
            nameof(UnSelectedTextColor));

    public IBrush UnSelectedTextColor
    {
        get => GetValue(UnSelectedTextColorProperty);
        set => SetValue(UnSelectedTextColorProperty, value);
    }

    #region Events

    public event EventHandler<EventArgs>? SelectedChange;

    #endregion

    private void Init()
    {
        grid.ColumnDefinitions.Add(new()
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        StackPanel stackPanel = new();
        Image? img = null;
        if (SelectedImage != null && IsActive)
        {
            img = new();
            img.Source = SelectedImage;
            _tabbarItem.SelectedImage = SelectedImage;
        }

        if (UnSelectedImage != null && !IsActive)
        {
            img = new();
            img.Source = UnSelectedImage;
            img.HorizontalAlignment = HorizontalAlignment.Center;
            img.VerticalAlignment = VerticalAlignment.Top;
            _tabbarItem.UnSelectedImage = UnSelectedImage;
        }

        if (img != null)
        {
            img.MaxWidth = 35;
            stackPanel.Children.Add(img);
        }

        if (IconControl != null)
        {
            ContentPresenter contentPresenter = new();
            contentPresenter.Content = IconControl;
            stackPanel.Children.Add(contentPresenter);
            _tabbarItem.IconControl = IconControl;
        }

        TextBlock text = new();
        text.Text = Label;
        text.Foreground = IsActive ? SelectedTextColor : UnSelectedTextColor;
        text.HorizontalAlignment = HorizontalAlignment.Center;
        text.VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
        stackPanel.Children.Add(text);
        _tabbarItem.Label = Label;
        _tabbarItem.SelectedTextColor = SelectedTextColor;
        _tabbarItem.UnSelectedTextColor = UnSelectedTextColor;
        _tabbarItem.Route = Route;
        _tabbarItem.Children = stackPanel.Children;
        Content = stackPanel;
        Items.Add(_tabbarItem);
        Click += (o, e) => SelectedChange?.Invoke(o, e);
    }

    protected override void OnInitialized()
    {
        Init();
    }

    public MFTabbarItem()
    {
        InitializeComponent();
    }
}