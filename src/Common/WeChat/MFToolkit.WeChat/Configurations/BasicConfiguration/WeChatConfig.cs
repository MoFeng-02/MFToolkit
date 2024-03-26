#nullable disable
namespace MFToolkit.WeChat.Configurations.BasicConfiguration;
public class WeChatConfig
{
    #region 基本的两个重要配置
    /// <summary>
    /// 微信AppID
    /// </summary>
    public string AppId { get; init; }
    /// <summary>
    /// 微信密钥
    /// </summary>
    public string AppSecret { get; init; }
    #endregion

    #region 服务器模式的token和EncodingAESKey
    public string AppToken { get; init; }
    public string AppEncodingAESKey { get; init; }
    #endregion

    #region 微信支付配置
    /// <summary>
    /// 微信支付商户号
    /// </summary>
    public string PayAccount { get; init; }
    /// <summary>
    /// 微信支付密钥 V2版
    /// </summary>
    public string PaySecretApiV2 { get; init; }
    /// <summary>
    /// 微信支付密钥 V3版
    /// </summary>
    public string PaySecretApiV3 { get; init; }

    #endregion

    #region Lasting Field（持久化字段）
    /// <summary>
    /// 最后保存日期
    /// </summary>
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disabled { get; set; } = false;
    #endregion
}
public sealed class WeChatConfigConstant
{
    /// <summary>
    /// 统一下单接口 V2版
    /// </summary>
    public const string WX_PAY_INTERFACE_API_V2 = "https://api.mch.weixin.qq.com/pay/unifiedorder";
}