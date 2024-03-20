namespace MFToolkit.Authentication.JwtAuthentication.Configuration.BasicConfiguration;
public class BasicHeader
{
    /// <summary>
    /// 表示使用的签名算法。在这里，“HS256”代表的是 HMAC-SHA256 算法，这是一种基于密钥的哈希消息认证码（HMAC）算法，使用SHA-256作为其基础哈希函数。
    /// </summary>
    public string Algorithm { get; set; } = "HS256";
    /// <summary>
    /// 表示数据类型或者内容类型。在这里，“JWT”表示这是一个JSON Web Token，即遵循JWT规范的一种安全令牌。
    /// </summary>
    public string Type { get; set; } = "JWT";
}
