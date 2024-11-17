using System.Security.Claims;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Model;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Utils;

namespace MFToolkit.AspNetCore.Authentication.JwtAuthorization.Extensions;
public static class JwtUtilExtension
{
    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <param name="claims">要生成的值</param>
    /// <param name="seconds">有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="refreshSeconds">刷新token有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="configKey">JsonWebTokenConfig配置key</param>
    /// <returns></returns>
    public static ReponseToken GenerateToken(this Claim[] claims, double seconds = -1, double refreshSeconds = -1, string? configKey = null)
    {
        return JwtUtil.GenerateToken(claims, seconds, refreshSeconds, configKey);
    }
}
