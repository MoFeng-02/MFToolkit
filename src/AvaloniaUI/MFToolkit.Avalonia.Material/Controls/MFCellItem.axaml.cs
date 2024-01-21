using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using MFToolkit.Avaloniaui.Material.Controls.Models.Enumerations;

namespace MFToolkit.Avaloniaui.Material.Controls;

public partial class MFCellItem : Grid
{
    /// <summary>
    /// 事件或命令操作是否通过
    /// </summary>
    private bool ActionIsPass = true;

    public static readonly StyledProperty<bool> AntiShakeProperty = AvaloniaProperty.Register<MFCellItem, bool>(
        nameof(AntiShake), true);

    /// <summary>
    /// 是否启动防抖
    /// </summary>
    public bool AntiShake
    {
        get => GetValue(AntiShakeProperty);
        set => SetValue(AntiShakeProperty, value);
    }

    public static readonly StyledProperty<MFCellItemClassEnum?> CellItemClassesProperty =
        AvaloniaProperty.Register<MFCellItem, MFCellItemClassEnum?>(
            nameof(CellItemClasses));

    public MFCellItemClassEnum? CellItemClasses
    {
        get => GetValue(CellItemClassesProperty);
        set => SetValue(CellItemClassesProperty, value);
    }

    public static readonly StyledProperty<object> ValueProperty = AvaloniaProperty.Register<MFCellItem, object>(
        nameof(Value));

    /// <summary>
    /// 传输给Clicked事件的Value
    /// </summary>
    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<Thickness?> LeftContentPaddingProperty =
        AvaloniaProperty.Register<MFCellItem, Thickness?>(
            nameof(LeftContentPadding));

    /// <summary>
    /// 左方内容内边距
    /// </summary>
    public Thickness? LeftContentPadding
    {
        get => GetValue(LeftContentPaddingProperty);
        set => SetValue(LeftContentPaddingProperty, value);
    }

    public static readonly StyledProperty<Thickness?> RightContentPaddingProperty =
        AvaloniaProperty.Register<MFCellItem, Thickness?>(
            nameof(RightContentPadding));

    /// <summary>
    /// 右方内容内边距
    /// </summary>
    public Thickness? RightContentPadding
    {
        get => GetValue(RightContentPaddingProperty);
        set => SetValue(RightContentPaddingProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<MFCellItem, ICommand?>(
            nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<MFCellItem, object?>(
            nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly StyledProperty<object> LeftContentProperty = AvaloniaProperty.Register<MFCellItem, object>(
        nameof(LeftContent));

    /// <summary>
    /// 左边内容
    /// </summary>
    public object LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    /// <summary>
    /// 右边内容
    /// </summary>
    public static readonly StyledProperty<object> RightContentProperty =
        AvaloniaProperty.Register<MFCellItem, object>(
            nameof(RightContent));

    public object RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    public event EventHandler<EventArgs>? Clicked;

    protected override void OnInitialized()
    {
        if (Classes.Count == 0 && CellItemClasses != null)
        {
            Classes.Add(CellItemClasses.ToString());
        }

        Tapped += (o, e) =>
        {
            if (AntiShake && !ActionIsPass) return;
            ActionIsPass = false;
            try
            {
                if (Command != null)
                {
                    Command?.Execute(CommandParameter);
                    return;
                }

                Clicked?.Invoke(o, e);
            }
            finally
            {
                ActionIsPass = true;
            }
        };
    }

    public MFCellItem()
    {
        InitializeComponent();
    }
}