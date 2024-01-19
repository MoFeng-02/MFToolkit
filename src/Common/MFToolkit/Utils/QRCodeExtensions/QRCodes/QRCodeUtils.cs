using MFToolkit.Utils.QRCodeExtensions.QRCodeServices;
using static QRCoder.QRCodeGenerator;

namespace MFToolkit.Utils.QRCodeExtensions.QRCodes;

public class QRCodeUtils
{
    /// <summary>
    /// 生成QR码
    /// </summary>
    /// <param name="content">实际处理内容</param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static byte[] GenerateQRCode(GenerateModel content, ECCLevel level = ECCLevel.Q) => QRCodeMultiPlatform.GenerateQRCode(content, level);
}