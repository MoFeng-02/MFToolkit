#nullable disable
namespace MFWeChatToolkit.Configurations.BasicConfiguration;
public class WeChatConfig
{
    #region 基本的两个重要配置
    /// <summary>
    /// 微信AppID
    /// </summary>
    public string AppId { get; internal set; }
    /// <summary>
    /// 微信密钥
    /// </summary>
    public string AppSecret { get; internal set; }
    #endregion

    #region 服务器模式的token和EncodingAESKey
    public string AppToken { get; internal set; }
    public string AppEncodingAESKey { get; internal set; }
    #endregion

    #region 微信支付配置
    /// <summary>
    /// 微信支付商户号
    /// </summary>
    public string PayAccount { get; internal set; }
    /// <summary>
    /// 微信支付密钥 V2版
    /// </summary>
    public string PaySecretApiV2 { get; internal set; }
    /// <summary>
    /// 微信支付密钥 V3版
    /// </summary>
    public string PaySecretApiV3 { get; internal set; }
    /// <summary>
    /// 统一下单接口 V2版
    /// </summary>
    public const string WX_PAY_INTERFACE_API_V2 = "https://api.mch.weixin.qq.com/pay/unifiedorder";

    #endregion
}
