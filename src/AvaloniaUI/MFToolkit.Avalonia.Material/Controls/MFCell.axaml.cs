using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MFToolkit.Avaloniaui.Helpers;
using MFToolkit.Avaloniaui.Material.Controls.Models.Enumerations;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFCell : StackPanel
{
    public static readonly StyledProperty<object> ContentProperty = AvaloniaProperty.Register<MFCell, object>(
        nameof(Content));

    public static readonly StyledProperty<IBrush> BorderBrushProperty = AvaloniaProperty.Register<MFCell, IBrush>(
        nameof(BorderBrush));

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<MFCell, Thickness>(
            nameof(BorderThickness));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<MFCell, CornerRadius>(
            nameof(CornerRadius));

    public static readonly StyledProperty<double> ShowLineMarginProperty = AvaloniaProperty.Register<MFCell, double>(
        nameof(ShowLineMargin));

    public static readonly StyledProperty<bool> IsShowInnerLineProperty = AvaloniaProperty.Register<MFCell, bool>(
        nameof(IsShowInnerLine));

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<MFCell, BoxShadows>(
            nameof(BoxShadow));

    public static readonly StyledProperty<Thickness> TheContentPaddingProperty =
        AvaloniaProperty.Register<MFCell, Thickness>(
            nameof(TheContentPadding), new Thickness(6));

    public static readonly StyledProperty<MFCellItemClassEnum> CellItemClassesProperty =
        AvaloniaProperty.Register<MFCellItem, MFCellItemClassEnum>(
            nameof(CellItemClasses));

    public static readonly StyledProperty<MFCellClassEnum> CellClassesProperty =
        AvaloniaProperty.Register<MFCell, MFCellClassEnum>(
            nameof(CellClasses));

    /// <summary>
    /// 自身样式
    /// </summary>
    public MFCellClassEnum CellClasses
    {
        get => GetValue(CellClassesProperty);
        set => SetValue(CellClassesProperty, value);
    }

    /// <summary>
    /// Cell Item样式
    /// </summary>
    public MFCellItemClassEnum CellItemClasses
    {
        get => GetValue(CellItemClassesProperty);
        set => SetValue(CellItemClassesProperty, value);
    }

    /// <summary>
    /// 内容内边距，左右统一内容内边距
    /// </summary>
    public Thickness TheContentPadding
    {
        get => GetValue(TheContentPaddingProperty);
        set => SetValue(TheContentPaddingProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// 是否显示内部分割线
    /// </summary>
    public bool IsShowInnerLine
    {
        get => GetValue(IsShowInnerLineProperty);
        set => SetValue(IsShowInnerLineProperty, value);
    }

    /// <summary>
    /// 显示线的左右边距
    /// </summary>
    public double ShowLineMargin
    {
        get => GetValue(ShowLineMarginProperty);
        set => SetValue(ShowLineMarginProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public IBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<IBrush> ShowInnerLineBrushProperty =
        AvaloniaProperty.Register<MFCell, IBrush>(
            nameof(ShowInnerLineBrush));

    /// <summary>
    /// 设置内联线（屈服线）显示的颜色
    /// </summary>
    public IBrush ShowInnerLineBrush
    {
        get => GetValue(ShowInnerLineBrushProperty);
        set => SetValue(ShowInnerLineBrushProperty, value);
    }

    protected override void OnInitialized()
    {
        Classes.Add(CellClasses.ToString());
        Border border = new()
        {
            ClipToBounds = true,
            BorderThickness = BorderThickness,
            CornerRadius = CornerRadius
        };
        border.Bind(Border.BoxShadowProperty, BindingHelper.CreateToBinding(nameof(BoxShadow), this));
        border.Bind(Border.BackgroundProperty, BindingHelper.CreateToBinding(nameof(Background), this));
#if NET8_0_OR_GREATER
        List<Control> items = [];
#else
        List<Control> items = new();
#endif
        foreach (var child in Children)
        {
            if (child is not MFCellItem item)
            {
                items.Add(child);
                continue;
            }

            item.LeftContentPadding ??= TheContentPadding;
            item.RightContentPadding ??= TheContentPadding;
            // if (item.Classes.Count == 0)
            // {
            //     item.Classes.Add(CellItemClasses.ToString());
            // }

            items.Add(item);
        }


        Children.Clear();
        var index = 0;

        StackPanel sk = new();
        // bview.Margin = new(10, 5);
        var count = items.Count - 1;
        foreach (var item in items)
        {
            if (index > 0 && IsShowInnerLine)
            {
                Border bview = new()
                {
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new(ShowLineMargin, 0),
                };
                bview.Bind(Border.BorderBrushProperty, BindingHelper.CreateToBinding(nameof(ShowInnerLineBrush), this));
                sk.Children.Add(bview);
            }

            if (item.Classes.Count == 0)
            {
                item.Classes.Add(CellItemClasses.ToString());
            }

            sk.Children.Add(item);
            index++;
        }

        border.Child = sk;
        Children.Add(border);
        Background = Brushes.Transparent;
    }

    public MFCell()
    {
        InitializeComponent();
    }
}