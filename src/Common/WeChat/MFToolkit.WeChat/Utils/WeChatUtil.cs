using System.Text;
using MFToolkit.Utils.VerifyUtils;
using MFToolkit.WeChat.Configurations;
using Microsoft.AspNetCore.Http;

namespace MFToolkit.WeChat.Utils;
/// <summary>
/// 微信开发工具库
/// </summary>
public class WeChatUtil
{
    /// <summary>
    /// 校验、验证消息的确来自微信服务器
    /// </summary>
    /// <param name="signature">微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数。</param>
    /// <param name="timestamp">时间戳</param>
    /// <param name="nonce">随机数</param>
    /// <param name="configKey">配置key，如果你在启动的时候配置了多个小程序或公众号的配置的话</param>
    /// <returns></returns>
    public static bool VerifySignature(string signature, string timestamp, string nonce, string? configKey = null)
    {
        var config = WeChatConfigUtil.GetBasicConfiguration(configKey);
        string verify = VerifySHA1Util.VerifySHA1(encoding: Encoding.UTF8, config.AppToken, timestamp, nonce);
        return verify == signature;
    }
    public class VerifySignatureResult
    {
        public string Signature { get; set; }
        public string Echostr { get; set; }
        public bool Verify { get; set; }
    }
    /// <summary>
    /// 校验、验证消息的确来自微信服务器
    /// </summary>
    /// <param name="configKey">配置key，如果你在启动的时候配置了多个小程序或公众号的配置的话</param>
    /// <returns>返回验证后的参数</returns>
    public static VerifySignatureResult VerifySignature(HttpContext httpContext, string? configKey = null)
    {
        var config = WeChatConfigUtil.GetBasicConfiguration(configKey);
        string signature = httpContext.Request.Query["signature"];
        string timestamp = httpContext.Request.Query["timestamp"];
        string nonce = httpContext.Request.Query["nonce"];
        string echostr = httpContext.Request.Query["echostr"];
        string verify = VerifySHA1Util.VerifySHA1(encoding: Encoding.UTF8, config.AppToken, timestamp, nonce);
        return new()
        {
            Signature = signature,
            Echostr = echostr,
            Verify = verify == signature
        };
    }
}
