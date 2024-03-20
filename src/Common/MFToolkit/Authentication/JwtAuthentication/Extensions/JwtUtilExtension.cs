using System.Security.Claims;
using MFToolkit.Authentication.JwtAuthentication.Utils;

namespace MFToolkit.Authentication.JwtAuthentication.Extensions;
public static class JwtUtilExtension
{
    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <param name="claims">要生成的值</param>
    /// <param name="timetamp">有效期截止的时间戳</param>
    /// <param name="seconds">有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="configKey">JsonWebTokenConfig配置key</param>
    /// <returns></returns>
    public static string GenerateToken(this Claim[] claims, out long timetamp, int seconds = -1, string? configKey = null)
    {
        return JwtUtil.GenerateToken(claims, out timetamp, seconds, configKey);
    }
}
