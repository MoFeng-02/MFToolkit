//using System.Drawing;
//using System.Drawing.Imaging;
//using MFToolkit.Utils.QRCodeExtensions.QRCodeServices;
//using ZXing;
//using ZXing.Common;
//using ZXing.QrCode;
//using ZXing.Rendering;

//namespace MFToolkit.Utils.QRCodeExtensions.ZXingNetQRCodes;
//internal class ZXQRCodeMultiPlatform
//{
//    /// <summary>
//    /// 产生图像
//    /// </summary>
//    /// <param name="content"></param>
//    /// <param name="barcodeFormat"></param>
//    /// <returns></returns>
//    //internal static byte[] Generate(GenerateModel content, BarcodeFormat barcodeFormat)
//    //{
//    //    // 创建一个BarcodeWriter实例
//    //    var barcodeWriter = new BarcodeWriter<PixelData>
//    //    {
//    //        // 设置生成的格式
//    //        Format = barcodeFormat,

//    //        // 配置QR码的选项
//    //        Options = new QrCodeEncodingOptions
//    //        {
//    //            DisableECI = true,
//    //            CharacterSet = "UTF-8",
//    //            Width = content.Width, // 图像宽度
//    //            Height = content.Height // 图像高度
//    //        }
//    //    };

//    //    // 生成格式码图像或数据
//    //    var result = barcodeWriter.Write(content.Value);
//    //    return result.Pixels;
//    //}
//    /// <summary>
//    /// 生成QR码
//    /// </summary>
//    /// <param name="content">实际处理内容</param>
//    /// <param name="barcodeFormat">设置生成的格式</param>
//    /// <returns></returns>
//    internal static byte[] GenerateQRCode(GenerateModel content, BarcodeFormat barcodeFormat = BarcodeFormat.QR_CODE)
//    {
//        if (content.Width == 0 && content.Height == 0)
//        {
//            content.Width = 300;
//            content.Height = 300;
//        }
//        if (string.IsNullOrWhiteSpace(content.Value))
//        {
//            return null;
//        }
//        var w = new QRCodeWriter();
//        BitMatrix b = w.encode(content.Value, BarcodeFormat.QR_CODE, content.Width, content.Height);
//        //var zzb = new BarcodeWriter();
//        zzb.Options = new EncodingOptions()
//        {
//            Margin = 0,
//        };
//        Bitmap b2 = zzb.Write(b);
//        byte[] bytes = BitmapToArray(b2);
//        return bytes;
//        //// 创建一个BarcodeWriter实例用于生成QR码
//        //var barcodeWriter = new BarcodeWriter<Bitmap>
//        //{
//        //    // 设置生成的格式为QR_CODE
//        //    Format = barcodeFormat,
//        //    // 配置QR码的选项
//        //    Options = new QrCodeEncodingOptions
//        //    {
//        //        DisableECI = true,
//        //        CharacterSet = "UTF-8",
//        //        Width = content.Width, // 图像宽度
//        //        Height = content.Height // 图像高度
//        //    }
//        //};
//        //// 生成QR码数据
//        //using var barcodeBitmap = barcodeWriter.Write(content.Value);
//        //using var stream = new MemoryStream();
//        //barcodeBitmap.Save(stream, ImageFormat.Png);
//        //return stream.ToArray();
//    }
//    /// <summary>
//    /// 生成BAR码
//    /// </summary>
//    /// <param name="content">实际处理内容</param>
//    /// <param name="barcodeFormat">设置生成的格式</param>
//    /// <returns></returns>
//    internal static byte[] GenerateBarcode(GenerateModel content, BarcodeFormat barcodeFormat = BarcodeFormat.CODE_128)
//    {
//        if (content.Width == 0 && content.Height == 0)
//        {
//            content.Width = 300;
//            content.Height = 100;
//        }
//        // 创建一个BarcodeWriter实例用于生成BAR码
//        var barcodeWriter = new BarcodeWriter<Bitmap>
//        {
//            // 设置生成的格式
//            Format = barcodeFormat,

//            // 配置BAR码的选项
//            Options = new EncodingOptions
//            {
//                Width = content.Width, // 图像宽度
//                Height = content.Height // 图像高度
//            }
//        };

//        // 生成BAR码图像
//        using var barcodeBitmap = barcodeWriter.Write(content.Value);
//        using var stream = new MemoryStream();
//        barcodeBitmap.Save(stream, ImageFormat.Png);
//        return stream.ToArray();
//    }
  
//}
