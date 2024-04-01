using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MFToolkit.Authentication.JwtAuthorization.Configuration;
using MFToolkit.Authentication.JwtAuthorization.Utils;
using MFToolkit.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace MFToolkit.Authentication.JwtAuthorization.Handler;
/// <summary>
/// JWT 默认验证授权
/// </summary>
public class JwtAuthorizationHandler : IAuthorizationHandler, IAuthorizationRequirement
{
    /// <summary>
    /// 校验JWT是否正确合规
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual async Task HandleAsync(AuthorizationHandlerContext context) => await AuthorizationHandlerAsync(context);
    /// <summary>
    /// 自写验证逻辑
    /// <para>可用于权限验证等等</para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    protected virtual Task<bool> VerificationAsync(AuthorizationHandlerContext context, HttpContext httpContext) => Task.FromResult(true);
    /// <summary>
    /// 定义需要刷新的JwtToken处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    protected virtual Task RefreshAuthorizationAsync(AuthorizationHandlerContext context, HttpContext httpContext)
    {
        // 获取是否有需要刷新的Token，如果有的话
        var xauth = httpContext.Request.Headers["X-Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(xauth)) return Task.CompletedTask;
        // 获取所有的User值
        var users = httpContext.User.Claims.ToArray();
        var configKey = httpContext.Request.Headers[JsonWebTokenConfig.JwtConfigKey].ToString() ?? null;
        var rToken = JwtUtil.GenerateToken(users, out var _, configKey: configKey);
        httpContext.SetHttpHender("Authorization", rToken);
        return Task.CompletedTask;
    }
    /// <summary>
    /// 验证处理
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual async Task AuthorizationHandlerAsync(AuthorizationHandlerContext context)
    {
        // 获取当前请求的 HttpContext
        var httpContext = context.Resource as HttpContext ?? throw new("出现错误");
        // 获取请求头中的 Authorization 值
        var authorization = httpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            context.Fail();
            return;
        }
        // 获取jwt配置key
        var configKey = httpContext?.Request.Headers[JsonWebTokenConfig.JwtConfigKey].ToString() ?? null;
        var config = JsonWebTokenConfig.GetJsonWebTokenConfig(configKey);
        var hender = config.Header;
        var payload = config.Payload;

        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // 获取 token 的值
            var token = authorization["Bearer ".Length..].Trim();
            // 定义一个密钥，用于验证 token 的签名
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.EncryptionKey));
            // 定义一个令牌验证参数
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = payload.Issuer,
                ValidAudience = payload.Audience,
                IssuerSigningKey = key
            };
            // 定义一个令牌处理器
            var handler = new JwtSecurityTokenHandler();
            try
            {
                // 验证 token 的有效性，并获取用户的身份和声明
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                var claims = principal.Claims;
                // 判断是否满足 JWTHandler 的需求
                if (context.Requirements.Contains(this))
                {
                    // 创建一个基于声明的身份
                    var identity = new ClaimsIdentity(claims, "Bearer");
                    // 创建一个基于身份的主体
                    var user = new ClaimsPrincipal(identity);
                    // 将主体赋值给 httpContext.User
                    httpContext!.User = user;
                    var userId = httpContext.User.FindFirstValue(ClaimTypes.Sid);
                    await RefreshAuthorizationAsync(context, httpContext);
                    if (await VerificationAsync(context, httpContext))
                    {
                        // 标记需求为成功
                        context.Succeed(this);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
                else
                {
                    context.Fail();
                }

            }
            catch (Exception)
            {
                // 如果出现异常，标记需求为失败，并记录异常信息
                context.Fail();
                httpContext!.Response.StatusCode = 401;
                // await httpContext.Response.WriteAsync(ex.Message);
            }
        }
        else
        {
            // 如果不是 Bearer 类型的 token，标记需求为失败，并返回错误信息
            context.Fail();
            httpContext!.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("无效令牌类型");
        }
    }
}
