using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MFToolkit.Authentication.JwtAuthentication.Configuration;
using MFToolkit.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace MFToolkit.Authentication.JwtAuthentication.Utils;
public class JwtUtil
{
    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <param name="claims">要生成的值</param>
    /// <param name="timetamp">有效期截止的时间戳</param>
    /// <param name="seconds">有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="configKey">JsonWebTokenConfig配置key</param>
    /// <returns></returns>
    public static string GenerateToken(Claim[] claims, out long timetamp, int seconds = -1, string? configKey = null)
    {
        var config = JsonWebTokenConfig.GetJsonWebTokenConfig(configKey);
        if (config == null) throw new Exception("未配置JsonWebTokenConfig");
        var hender = config.Header;
        var payload = config.Payload;
        var now = DateTime.UtcNow;
        var token = new JwtSecurityTokenHandler().WriteToken(
                    new JwtSecurityToken(
                        issuer: payload.Issuer,
                        audience: payload.Audience,
                        claims: claims,
                        notBefore: now,
                        expires: now.AddSeconds(seconds == -1 ? payload.ExpirationTime : seconds),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.EncryptionKey)),
                            config.Header.Algorithm)
                        )
                    );
        timetamp = now.AddSeconds(seconds).ToNowTimetamp();
        return token;
    }
}
