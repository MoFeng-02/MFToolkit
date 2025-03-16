using Avalonia.Data;

namespace MFToolkit.Avaloniaui.Helpers;

/// <summary>
/// Binding帮助类
/// </summary>
public class BindingHelper
{
    /// <summary>
    /// 创建一个Binding
    /// </summary>
    /// <param name="path">可以这样使用：nameof(Param)</param>
    /// <param name="soruce">源，可以这样：this</param>
    /// <param name="mode">绑定方式</param>
    /// <returns></returns>
    public static Binding CreateToBinding(string path, object soruce, BindingMode mode = BindingMode.Default)
    {
        return new(path, mode)
        {
            Source = soruce
        };
    }
}