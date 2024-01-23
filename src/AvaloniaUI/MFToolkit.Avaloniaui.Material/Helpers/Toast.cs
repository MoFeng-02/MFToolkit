using MFToolkit.Avaloniaui.Material.Controls;

namespace MFToolkit.Avaloniaui.Material.Helpers;

public class Toast
{
    /// <summary>
    /// 显示土司弹窗
    /// </summary>
    /// <param name="content">显示内容</param>
    /// <param name="seconds">持续时间</param>
    public static void ShowToast(object content, int seconds = 2) => MFContainer.ShowToast(content, seconds);

    /// <summary>
    /// 显示土司弹窗
    /// </summary>
    /// <param name="content">显示内容</param>
    /// <param name="seconds">持续时间</param>
    public static Task ShowToastAsync(object content, int seconds = 2) => MFContainer.ShowToastAsync(content,
        seconds);

    public static Action<string>? ShowToastPlatformAction;
    /// <summary>
    /// 本机平台托管显示本地通知
    /// </summary>
    public static void ShowToastPlatform(string text)
    {
        ShowToastPlatformAction?.Invoke(text);
    }
}