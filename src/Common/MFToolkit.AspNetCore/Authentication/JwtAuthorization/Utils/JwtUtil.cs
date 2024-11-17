using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Model;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Configuration;
using MFToolkit.Exceptions;
using MFToolkit.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace MFToolkit.AspNetCore.Authentication.JwtAuthorization.Utils;
/// <summary>
/// JWT 工具类
/// </summary>
public class JwtUtil
{
    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <param name="claims">要生成的值</param>
    /// <param name="seconds">有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="refreshSeconds">刷新token有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="configKey">JsonWebTokenConfig配置key</param>
    /// <returns></returns>
    public static ReponseToken GenerateToken(Claim[] claims, double seconds = -1, double refreshSeconds = -1, string? configKey = null)
    {
        var config = JsonWebTokenConfig.GetJsonWebTokenConfig(configKey) ?? throw OhException.ApplicationError($"未配置该JsonWebTokenConfig，key:{configKey}");
        var hender = config.Header;
        var payload = config.Payload;
        var tokenTime = seconds == -1 ? payload.ExpirationTime : seconds;
        var now = DateTime.UtcNow;
        var token = new JwtSecurityTokenHandler().WriteToken(
                    new JwtSecurityToken(
                        issuer: payload.Issuer,
                        audience: payload.Audience,
                        claims: claims,
                        notBefore: now,
                        expires: now.AddSeconds(tokenTime),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.EncryptionKey)),
                            hender.Algorithm)
                        )
                    );
        var timetamp = now.AddSeconds(tokenTime).ToNowTimetampInSeconds();
        var refreshToken = GenerateRefreshToken(claims, out var refreshTimetamp, refreshSeconds, configKey);
        var result = new ReponseToken
        {
            Token = token,
            RefreshToken = refreshToken,
            Timetamp = timetamp,
            RefreshTimetamp = refreshTimetamp
        };
        return result;
    }
    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <param name="claims">要生成的值</param>
    /// <param name="seconds">有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="refreshSeconds">刷新token有效期（秒），如果为-1则采用JsonWebTokenConfig配置，否则传输过来的值</param>
    /// <param name="configKey">JsonWebTokenConfig配置key</param>
    /// <returns></returns>
    public static ReponseToken GenerateToken(Dictionary<string, string> claims, double seconds = -1, double refreshSeconds = -1, string? configKey = null)
    {
        List<Claim> cls = [];
        foreach (var claim in claims)
        {
            cls.Add(new(claim.Key, claim.Value));
        }
        return GenerateToken(cls.ToArray(), seconds, refreshSeconds, configKey);
    }
    /// <summary>
    /// 从 token 中获取 ClaimsPrincipal
    /// </summary>
    /// <param name="token"></param>
    /// <param name="configKey"></param>
    /// <param name="isRefresh">是否是刷新Token</param>
    /// <returns></returns>
    public static ClaimsPrincipal GetPrincipalFromToken(string token, string? configKey = null, bool isRefresh = false)
    {
        var config = JsonWebTokenConfig.GetJsonWebTokenConfig(configKey) ?? throw OhException.ApplicationError($"未配置该JsonWebTokenConfig，key:{configKey}");
        var hender = config.Header;
        var payload = config.Payload;
        var tokenValidationParameters = new TokenValidationParameters
        {

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = payload.Issuer,
            ValidAudience = payload.Audience,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(!isRefresh ? config.EncryptionKey : config.RefreshEncryptionKey)),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(hender.Algorithm, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
    /// <summary>
    /// 验证是否是刷新Token
    /// </summary>
    /// <param name="token"></param>
    /// <param name="configKey"></param>
    /// <returns></returns>
    public static bool VerifyRefreshToken(string token, string? configKey = null)
    {
        try
        {
            var c = GetPrincipalFromToken(token, configKey, true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    /// <summary>
    /// 创建刷新Token
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="refreshSeconds"></param>
    /// <param name="configKey"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static string GenerateRefreshToken(Claim[] claims, out long timetamp, double refreshSeconds = -1, string? configKey = null)
    {
        var config = JsonWebTokenConfig.GetJsonWebTokenConfig(configKey) ?? throw new Exception($"未配置该JsonWebTokenConfig，key:{configKey}");
        var hender = config.Header;
        var payload = config.Payload;
        var refresTokenTime = refreshSeconds == -1 ? payload.RefreshExpirationTime : refreshSeconds;
        var now = DateTime.UtcNow;
        var token = new JwtSecurityTokenHandler().WriteToken(
                    new JwtSecurityToken(
                        issuer: payload.Issuer,
                        audience: payload.Audience,
                        claims: claims,
                        notBefore: now,
                        expires: now.AddSeconds(refresTokenTime),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.RefreshEncryptionKey)),
                            hender.Algorithm)
                        )
                    );
        timetamp = now.AddSeconds(refresTokenTime).ToNowTimetampInSeconds();
        return token;
    }
}
