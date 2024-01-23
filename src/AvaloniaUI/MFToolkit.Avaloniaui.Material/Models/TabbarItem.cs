using Avalonia.Media;
using MFToolkit.Avaloniaui.Helpers;

namespace MFToolkit.Avaloniaui.Material.Controls.Models;

/// <summary>
/// Tabbar 模型
/// </summary>
public class TabbarItem
{
    public bool Active { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Label { get; set; }

    // = "#1989fa";
    /// <summary>
    /// 文字选中颜色
    /// </summary>
    public IBrush? SelectedTextColor { get; set; }

    // = "#8C8C8C";
    /// <summary>
    /// 文字未选择颜色
    /// </summary>
    public IBrush? UnSelectedTextColor { get; set; }

    /// <summary>
    /// 绑定路由
    /// </summary>
    public string Route { get; set; } = null!;

    /// <summary>
    /// Icon路径，如果有自定义IconControl控件的话这个不生效
    /// </summary>
    public IImage? SelectedImage { get; set; }

    /// <summary>
    /// Icon路径，如果有自定义IconControl控件的话这个不生效
    /// </summary>
    public IImage? UnSelectedImage { get; set; }

    /// <summary>
    /// 自定义Icon的控件
    /// </summary>
    public object? IconControl { get; set; }

    public bool IsIconVisible => SelectedImage != null && IconControl == null;
    public bool IsIconControlVisbile => IconControl != null;
    public Avalonia.Controls.Controls? Children { get; set; }

    public TabbarItem()
    {
    }

    public TabbarItem(string label, string? icon = null, string? unIcon = null)
    {
        Label = label;
        if (!string.IsNullOrEmpty(icon)) SelectedImage = ImageHelper.LoadFromResource(new(icon));
        if (!string.IsNullOrEmpty(unIcon)) UnSelectedImage = ImageHelper.LoadFromResource(new(unIcon));
    }
}