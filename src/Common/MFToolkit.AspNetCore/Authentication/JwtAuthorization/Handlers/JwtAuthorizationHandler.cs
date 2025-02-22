using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Configuration;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Utils;
using MFToolkit.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace MFToolkit.AspNetCore.Authentication.JwtAuthorization.Handlers;
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

        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // 获取 token 的值
                var token = authorization["Bearer ".Length..].Trim();
                // 验证 token 的有效性，并获取用户的身份和声明
                var principal = JwtUtil.GetPrincipalFromToken(token, configKey);
                var claims = principal.Claims;
                // 判断是否满足 JWTHandler 的需求
                if (context.Requirements.Contains(this))
                {
                    httpContext!.User = principal;
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
            catch (SecurityTokenExpiredException)
            {
                context.Fail(new(this, "Token 已过期"));
                httpContext!.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Token 已过期");
            }
            catch (SecurityTokenValidationException ex)
            {
                context.Fail(new(this, "Token 验证失败"));
                httpContext!.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync($"Token 验证失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 如果出现异常，标记需求为失败，并记录异常信息
                context.Fail();
                httpContext!.Response.StatusCode = 401;
                throw OhException.ApplicationError(ex.Message, ex, 401);
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
