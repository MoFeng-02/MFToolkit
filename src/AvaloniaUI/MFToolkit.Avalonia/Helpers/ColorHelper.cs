using Avalonia.Media;

namespace MFToolkit.Avaloniaui.Helpers;

public class ColorHelper
{
    /// <summary>
    /// 使用简单的哈希算法将字符串转换为颜色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Color GenerateColor(string input)
    {
        // 使用简单的哈希算法将字符串转换为颜色
        Color c = Color.Parse(input);
        return c;
    }
    /// <summary>
    /// 生成纯色画笔
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static SolidColorBrush GenerateSolidColorBrush(string input) => new(GenerateColor(input));
}