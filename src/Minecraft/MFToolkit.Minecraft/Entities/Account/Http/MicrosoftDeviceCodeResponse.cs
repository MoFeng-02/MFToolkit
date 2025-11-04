using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account.Http;

/// <summary>
/// 微软设备代码响应
/// </summary>
public class MicrosoftDeviceCodeResponse
{
    /// <summary>
    /// 设备代码
    /// </summary>
    [JsonPropertyName("device_code")]
    public string? DeviceCode { get; set; }

    /// <summary>
    /// 用户代码
    /// </summary>
    [JsonPropertyName("user_code")]
    public string? UserCode { get; set; }

    /// <summary>
    /// 验证URL
    /// </summary>
    [JsonPropertyName("verification_uri")]
    public string? VerificationUri { get; set; }

    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 轮询间隔（秒）
    /// </summary>
    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    /// <summary>
    /// 用于指导用户登录的文本，默认为英文，可在查询参数中指定 ?mtk={语言区域性代码} 来将此内容本地化，但建议启动器自行生成文本指导
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// 微软设备代码认证结果
/// </summary>
public class MicrosoftDeviceCodeResult
{

    /// <summary>
    /// 设备代码 - 应用程序需要暂存此代码用于轮询用户授权状态
    /// </summary>
    public string? DeviceCode { get; set; }

    /// <summary>
    /// 用户代码
    /// </summary>
    public string? UserCode { get; set; }

    /// <summary>
    /// 验证URL
    /// </summary>
    public string? VerificationUri { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// 轮询间隔（秒）
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// 用于指导用户登录的文本，默认为英文，可在查询参数中指定 ?mtk={语言区域性代码} 来将此内容本地化，但建议启动器自行生成文本指导
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// 微软令牌错误响应
/// </summary>
public class MicrosoftTokenErrorResponse
{
    /// <summary>
    /// 错误代码
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// 错误描述
    /// </summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// 错误URI
    /// </summary>
    [JsonPropertyName("error_uri")]
    public string? ErrorUri { get; set; }
}