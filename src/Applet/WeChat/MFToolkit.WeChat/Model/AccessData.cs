#nullable disable
using System.Text.Json.Serialization;

namespace MFToolkit.WeChat.Model;
/// <summary>
/// 微信小程序数据
/// </summary>
public class AccessData
{
    [JsonPropertyName("access_token")]
    public string Access_token { get; set; }
    [JsonPropertyName("expires_in")]
    public string Expires_in { get; set; }
    [JsonPropertyName("refresh_token")]
    public string Refresh_token { get; set; }
    [JsonPropertyName("openid")]
    public string Openid { get; set; }
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
    [JsonPropertyName("is_snapshotuser")]
    public string Is_snapshotuser { get; set; }
    [JsonPropertyName("unionid")]
    public string Unionid { get; set; }
    [JsonPropertyName("session_key")]
    public string Session_key { get; set; }
}
