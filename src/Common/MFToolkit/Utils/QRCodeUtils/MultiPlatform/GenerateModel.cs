using System.Drawing;

namespace MFToolkit.Utils.QRCodeUtils.QRCodeServices;

public class GenerateModel
{
    /// <summary>
    /// 生成内容
    /// </summary>
    public string Value { get; set; }
    /// <summary>
    /// 生成图像宽度
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// 生成图像高度
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// 像素大小
    /// </summary>
    public int Pixel { get; set; } = 20;
    /// <summary>
    /// 深色
    /// </summary>
    public Color DarkColor { get; set; }
    /// <summary>
    /// 亮色
    /// </summary>
    public Color LightColor { get; set; }
}