namespace MFToolkit.AspNetCore.Authentication.JwtAuthorization.Configuration.BasicConfiguration;
public class BasicPayload
{
    /// <summary>
    /// 过期时间（单位：秒）/ 别名：expires
    /// <para>默认两个小时过期</para>
    /// </summary>
    public double ExpirationTime { get; set; } = 60 * 60 * 2;
    /// <summary>
    /// 刷新Token过期时间(默认一天)
    /// </summary>
    public double RefreshExpirationTime { get; set; } = (DateTime.Now.AddDays(1) - DateTime.UtcNow).TotalSeconds;
    /// <summary>
    /// 签发方
    /// </summary>
    public string Issuer { get; set; }
    /// <summary>
    /// 接收方
    /// </summary>
    public string Audience { get; set; }
    /// <summary>
    /// 是否启用jti？
    /// <para>
    /// <seealso cref="jti-tips.txt"/>
    /// </para>
    /// <para>暂未完成，或许你可以先注册</para>
    /// </summary>
    //public bool IsStartRegistered { get; set; }
}
