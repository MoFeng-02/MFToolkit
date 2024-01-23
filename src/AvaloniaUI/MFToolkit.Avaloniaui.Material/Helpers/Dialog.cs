using MFToolkit.Avaloniaui.Material.BaseExtensions;
using MFToolkit.Avaloniaui.Material.Controls;
using MFToolkit.Avaloniaui.Material.Controls.Dialogs;

namespace MFToolkit.Avaloniaui.Material.Helpers;

public class Dialog
{
    /// <summary>
    /// 显示Dialog对话
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    public static void
        ShowDialog(object content, bool showAtBottom = false, bool clickOutsideTheElementToHide = true) => MFContainer
        .ShowDialog(content, showAtBottom, clickOutsideTheElementToHide);

    /// <summary>
    /// 显示Dialog对话
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    public static Task ShowDialogAsync(object content, bool showAtBottom = false,
        bool clickOutsideTheElementToHide = true) => MFContainer
        .ShowDialogAsync(content, showAtBottom, clickOutsideTheElementToHide);

    /// <summary>
    /// 关闭Dialog
    /// </summary>
    public static void CloasDialog() => MFContainer.CloasDialog();


    /// <summary>
    /// 模板 - 自定义模板
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="width">总宽</param>
    /// <param name="height">总高</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    public static void ShowDialogAtCustom(object content, bool showAtBottom = false, double width = 300,
        double height = 200, bool clickOutsideTheElementToHide = true)
    {
        var MFDialogOne = new MFDialog
        {
            Content = content,
            Width = width,
            Height = height
        };
        ShowDialog(MFDialogOne, showAtBottom, clickOutsideTheElementToHide);
    }

    /// <summary>
    /// 模板1 - 单按钮
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="buttonText">按钮文案</param>
    /// <param name="action">点击按钮的操作</param>
    /// <param name="width">总宽</param>
    /// <param name="height">总高</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    public static void ShowDialogAtOneButton(object content, bool showAtBottom = false, string buttonText = "关闭",
        Action? action = null, double width = 300, double height = 200, bool clickOutsideTheElementToHide = true,
        ButtonStylesEnum buttonClass = ButtonStylesEnum.Default)
    {
        var MFDialogOne = new MFDialogOne
        {
            Content = content,
            ButtonText = buttonText,
            Width = width,
            Height = height,
            ButtonClass = buttonClass
        };
        MFDialogOne.Clicked += (_, _) =>
        {
            if (action != null)
            {
                action.Invoke();
                return;
            }

            CloasDialog();
        };
        ShowDialog(MFDialogOne, showAtBottom, clickOutsideTheElementToHide);
    }

    /// <summary>
    /// 模板2 - 双按钮
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="showAtBottom">是否显示在底部</param>
    /// <param name="leftButtonText">左边按钮文案</param>
    /// <param name="rightButtonText">右边按钮文案</param>
    /// <param name="leftAction">点击左边按钮的操作</param>
    /// <param name="rightAction">点击右边按钮的操作</param>
    /// <param name="width">总宽</param>
    /// <param name="height">总高</param>
    /// <param name="clickOutsideTheElementToHide">点击元素外隐藏</param>
    /// <param name="leftButtonClass"></param>
    /// <param name="rightButtonClass"></param>
    public static void ShowDialogAtTwoButton(object content, bool showAtBottom = false, string leftButtonText = "取消",
        string rightButtonText = "确认",
        Action? leftAction = null, Action? rightAction = null, double width = 300, double height = 200,
        bool clickOutsideTheElementToHide = true, ButtonStylesEnum leftButtonClass = ButtonStylesEnum.Default,
        ButtonStylesEnum rightButtonClass = ButtonStylesEnum.Info)
    {
        var MFDialog = new MFDialogTwo
        {
            Content = content,
            LeftButtonText = leftButtonText,
            RightButtonText = rightButtonText,
            Width = width,
            Height = height,
            LeftButtonClass = leftButtonClass,
            RightButtonClass = rightButtonClass
        };
        MFDialog.LeftButtonClicked += (_, _) =>
        {
            if (leftAction != null)
            {
                leftAction.Invoke();
                return;
            }

            CloasDialog();
        };
        MFDialog.RightButtonClicked += (_, _) =>
        {
            if (rightAction != null)
            {
                rightAction.Invoke();
                return;
            }

            CloasDialog();
        };
        ShowDialog(MFDialog, showAtBottom, clickOutsideTheElementToHide);
    }
}