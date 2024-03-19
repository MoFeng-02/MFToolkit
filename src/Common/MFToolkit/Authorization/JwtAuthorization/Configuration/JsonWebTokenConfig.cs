using MFToolkit.Authorization.JwtAuthorization.Configuration.BasicConfiguration;

namespace MFToolkit.Authorization.JwtAuthorization.Configuration;
/// <summary>
/// jwt 配置
/// </summary>
public class JsonWebTokenConfig
{
    private static JsonWebTokenConfig _jsonWebTokenConfig;
    private static Dictionary<string, JsonWebTokenConfig> _jsonWebTokenConfigDictionary;
    /// <summary>
    /// JWT的请求key
    /// <para>一般用于前端请求头里面的配置,例如：</para>
    /// <code>auth-key: 123456789，这代表key，根据这个获取相关Jwt的配置</code>
    /// </summary>
    public static string JwtConfigKey = "auth-key";
    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key">可能是多个配置，所以提供key来识别</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static JsonWebTokenConfig GetJsonWebTokenConfig(string? key = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return _jsonWebTokenConfig;
        }
        if (_jsonWebTokenConfigDictionary.TryGetValue(key, out JsonWebTokenConfig jsonWebTokenConfig)) { return jsonWebTokenConfig; }
        throw new Exception("您未设置JWT配置，请注册，注册方法：service.AddJwtAuthorization(new JsonWebTokenConfig...)");
    }
    /// <summary>
    /// 基本设置配置
    /// </summary>
    /// <param name="jsonWebTokenConfig"></param>
    public static void SetJsonWebTokenConfig(JsonWebTokenConfig jsonWebTokenConfig)
    {
        _jsonWebTokenConfig ??= jsonWebTokenConfig;
    }
    /// <summary>
    /// 多个配置注册
    /// </summary>
    /// <param name="jsonWebTokenConfigs"></param>
    public static void SetJsonWebTokenConfig(Dictionary<string, JsonWebTokenConfig> jsonWebTokenConfigs)
    {
        _jsonWebTokenConfigDictionary ??= jsonWebTokenConfigs;
    }


    /// <summary>
    /// 加密密钥
    /// </summary>
    public string EncryptionKey { get; set; }
    /// <summary>
    /// Header 头
    /// </summary>
    public BasicHeader Header { get; set; }
    /// <summary>
    /// Payload 载荷
    /// </summary>
    public BasicPayload Payload { get; set; }

}
