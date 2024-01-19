using System.Drawing;
using MFToolkit.Utils.QRCodeExtensions.QRCodeServices;
using QRCoder;
using static QRCoder.QRCodeGenerator;

namespace MFToolkit.Utils.QRCodeExtensions.QRCodes;

internal class QRCodeMultiPlatform
{
    /// <summary>
    /// 生成QR码
    /// </summary>
    /// <param name="content">实际处理内容</param>
    /// <param name="level">
    /// <code>
    /// ECC Level L（低）：
    /// 低级别提供最低的错误校正能力。
    /// QRCode可以容忍少量错误，适用于QRCode包含的数据较少，或者在一个相对低噪声环境下扫描QRCode的情况。
    /// ECC Level M（中等）：

    /// 中等级别提供适度的错误校正能力。
    /// 这是QRCode的默认ECC级别，适用于大多数情况，可以容忍一定数量的错误。
    /// ECC Level Q（高）：

    /// 高级别提供更高的错误校正能力。
    /// 当QRCode可能在高噪声环境下扫描，或者QRCode包含的数据非常重要时，可以选择这个级别。
    /// ECC Level H（最高）：

    /// 最高级别提供最高的错误校正能力。
    /// 这个级别用于QRCode包含的数据非常关键，且QRCode可能在非常高噪声或受损的情况下扫描。
    /// </code>
    /// </param>
    /// <returns></returns>
    internal static byte[] GenerateQRCode(GenerateModel content, ECCLevel level = ECCLevel.Q)
    {
        // 创建 QRCodeGenerator 实例
        using QRCodeGenerator qrGenerator = new();

        // 创建 QRCode 数据
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(content.Value, level);
        // 创建 QRCode
        using PngByteQRCode qrCode = new(qrCodeData);

        // 获取 QRCode 的数据
        var qrCodeImage = qrCode.GetGraphic(content.Pixel); // 20 是像素大小
        return qrCodeImage;
    }
}