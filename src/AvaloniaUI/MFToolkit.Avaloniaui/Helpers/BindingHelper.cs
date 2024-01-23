using Avalonia.Data;

namespace MFToolkit.Avaloniaui.Helpers;

public class BindingHelper
{
    /// <summary>
    /// 创建一个Binding
    /// </summary>
    /// <param name="path">可以这样使用：nameof(Param)</param>
    /// <param name="soruce">源，可以这样：this</param>
    /// <returns></returns>
    public static Binding CreateToBinding(string path, object soruce)
    {
        return new(path)
        {
            Source = soruce
        };
    }
}