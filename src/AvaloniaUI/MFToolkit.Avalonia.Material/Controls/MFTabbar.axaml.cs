using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Material.Controls.Models;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFTabbar : Grid
{
    public static readonly StyledProperty<int> ActiveIndexProperty = AvaloniaProperty.Register<MFTabbar, int>(
        nameof(ActiveIndex));

    public int ActiveIndex
    {
        get => GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }

    public static readonly StyledProperty<double> ItemHeightProperty = AvaloniaProperty.Register<MFTabbar,
        double>(
        nameof(ItemHeight), 60);

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<TabbarItem>?> ItemsProperty = AvaloniaProperty
        .Register<MFTabbar, ObservableCollection<TabbarItem>?>(
            nameof(Items));

    public ObservableCollection<TabbarItem>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public int ItemLength => Items?.Count ?? 0;

    public static readonly StyledProperty<ICommand?> SelectedCommandProperty =
        AvaloniaProperty.Register<MFTabbar, ICommand?>(
            nameof(SelectedCommand));

    public ICommand? SelectedCommand
    {
        get => GetValue(SelectedCommandProperty);
        set => SetValue(SelectedCommandProperty, value);
    }

    public static readonly StyledProperty<IBrush?> SelectedTextColorProperty =
        AvaloniaProperty.Register<MFTabbar, IBrush?>(
            nameof(SelectedTextColor));

    /// <summary>
    /// 选中时候的颜色
    /// </summary>
    public IBrush? SelectedTextColor
    {
        get => GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    public static readonly StyledProperty<IBrush?> UnSelectedTextColorProperty =
        AvaloniaProperty.Register<MFTabbar, IBrush?>(
            nameof(UnSelectedTextColor));

    /// <summary>
    /// 未选中时候的颜色
    /// </summary>
    public IBrush? UnSelectedTextColor
    {
        get => GetValue(UnSelectedTextColorProperty);
        set => SetValue(UnSelectedTextColorProperty, value);
    }

    public MFTabbar()
    {
        InitializeComponent();
    }

    public event EventHandler<EventArgs>? Selected;

    protected override void OnInitialized()
    {
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        SetActiveIndex(ActiveIndex);
        SetColumnDefinitions();
        SetItems();
    }

    /// <summary>
    /// 设置列宽
    /// </summary>
    private void SetColumnDefinitions()
    {
        var count = Children.Count != 0 ? Children.Count : Items?.Count;
        for (int i = 0; i < count; i++)
        {
            ColumnDefinitions.Add(new()
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
        }
    }

    /// <summary>
    /// 获取是否存在MFTabbarItem
    /// </summary>
    /// <returns></returns>
    private bool GetTabbarItemExist()
    {
        bool isExist = false;
        int index = -1;
        for (int i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (child is not Button btn) continue;
            index++;
            var item = btn as MFTabbarItem;
            if (!isExist || Items == null)
            {
#if NET8_0_OR_GREATER
                Items = [];
#else
                Items = new();
#endif
            }

            item!.Height = ItemHeight;
            isExist = true;
            item.ActiveIndex = i;
            item.IsActive = i == ActiveIndex;
            var tabItem = item.GetTabbarItem();
            Items.Add(tabItem);
            SetRow(item, 0);
            SetColumn(item, index);
            item.SelectedChange += (o, e) => { SelectedChange(tabItem, e); };
        }

        return isExist;
    }

    /// <summary>
    /// 设置可用项
    /// </summary>
    private void SetItems()
    {
        if (GetTabbarItemExist())
        {
            SetActiveIndex(ActiveIndex);
            return;
        }

        if (Items == null) return;
        var count = Items.Count;
        for (var i = 0; i < count; i++)
        {
            var item = Items![i];
            Button btn = new()
            {
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Classes =
                {
                    "None"
                }
            };
            StackPanel stackPanel = new();
            if (item.IsIconVisible)
            {
                Image img = new();
                img.MaxWidth = 35;
                img.Source = i == ActiveIndex ? item.SelectedImage : item.UnSelectedImage;
                img.HorizontalAlignment = HorizontalAlignment.Center;
                img.VerticalAlignment = VerticalAlignment.Top;
                stackPanel.Children.Add(img);
            }

            if (item.IsIconControlVisbile)
            {
                ContentPresenter contentPresenter = new();
                contentPresenter.Content = item.IconControl;
                stackPanel.Children.Add(contentPresenter);
            }

            TextBlock textBlock = new();
            textBlock.Text = item.Label;
            textBlock.Foreground = item.Active
                ? item.SelectedTextColor ?? SelectedTextColor
                : item.UnSelectedTextColor ?? UnSelectedTextColor;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            stackPanel.Height = ItemHeight;
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(textBlock);

            item.Children = stackPanel.Children;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.Content = stackPanel;
            SetRow(btn, 0);
            SetColumn(btn, i);
            Children.Add(btn);
            btn.Click += (o, e) => { SelectedChange(item, e); };
        }
    }

    /// <summary>
    /// 手动设置下标
    /// </summary>
    /// <param name="i"></param>
    public void SetActiveIndex(int i = 0)
    {
        if (Items == null || Items.Count == 0) return;
        ActiveIndex = i;
        var item = Items[i];
        UpdateState(item);
    }

    /// <summary>
    /// 设置tab状态
    /// </summary>
    /// <param name="item"></param>
    private void UpdateState(TabbarItem item)
    {
        for (int i = 0; i < Items?.Count; i++)
        {
            var query = Items[i];
            if (item == query)
            {
                ActiveIndex = i;
                query.Active = true;
            }
            else
            {
                query.Active = false;
            }

            UpdateChildrenState(query, query.Active);
        }
    }

    private void UpdateChildrenState(TabbarItem item, bool active)
    {
        if (item.Children == null) return;
        for (var i = 0; i < item.Children.Count; i++)
        {
            var element = item.Children[i];
            switch (element)
            {
                case Image image:
                    image.Source = active ? item.SelectedImage : item.UnSelectedImage;
                    break;
                case TextBlock text:
                    text.Foreground = active
                        ? item.SelectedTextColor ?? SelectedTextColor
                        : item.UnSelectedTextColor ?? UnSelectedTextColor;
                    break;
            }
        }
    }

    private void SelectedChange(TabbarItem sender, EventArgs e)
    {
        var item = sender;
        UpdateState(item);
        if (SelectedCommand != null)
        {
            SelectedCommand?.Execute(item);
            return;
        }

        Selected?.Invoke(item, e);
    }
}