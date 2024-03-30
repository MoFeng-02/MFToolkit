using System.Collections.Generic;
using System.Text;
using MFToolkit.App;
using MFToolkit.JsonExtensions;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;
using MFToolkit.Utils.VerifyUtils;
using MFToolkit.WeChat.Configurations;
using MFToolkit.WeChat.Configurations.BasicConfiguration;
using MFToolkit.WeChat.Json.AOT;
using MFToolkit.WeChat.Services;
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
    /// <summary>
    /// 所有服务默认提供类
    /// </summary>
    public static readonly AllService ServiceDefault = new();
    /// <summary>
    /// 注册WeChat配置持久化，不提供删除，删除需要自行去处理
    /// <para>Version: 0.0.1-beta</para>
    /// </summary>
    /// <param name="readPath">读取路径，只能是文件夹</param>
    /// <param name="encryptionKey">内容加密Key，如果是有加密的话</param>
    public static async Task WeChatConfigurationLasting(string? readPath = null, string? encryptionKey = null) => await WeChatConfigLastingUtil.WeChatConfigurationLasting(readPath, encryptionKey);
    /// <summary>
    /// 读取持久化里面的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static async Task<WeChatConfig?> GetWeChatConfigLasting(string key) => await WeChatConfigLastingUtil.GetWeChatConfigLasting(key);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static async Task<WeChatConfig?> GetWeChatConfigCache(string key) => await WeChatConfigLastingUtil.GetWeChatConfigCache(key);
}

public class AllService
{
    public IWeChatRequestService WeChatServiceDefault => AppUtil.GetService<IWeChatRequestService>() ?? throw new("无法获取IWeChatService");
}