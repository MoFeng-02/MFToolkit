using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Model;
using MFToolkit.AspNetCore.Extensions;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Configuration;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Utils;
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
    /// 定义需要刷新的JwtToken处理
    /// <para>暂不参与刷新Token</para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    protected virtual Task<ReponseToken?> RefreshAuthorizationAsync(AuthorizationHandlerContext context, HttpContext? httpContext)
    {
        if (httpContext == null) return Task.FromResult(default(ReponseToken));
        // 获取是否有需要刷新的Token，如果有的话
        var xauth = httpContext.Request.Headers["X-Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(xauth)) return Task.FromResult(default(ReponseToken));
        // 获取所有的User值
        var configKey = httpContext.Request.Headers[JsonWebTokenConfig.JwtConfigKey].ToString() ?? null;
        var principal = JwtUtil.GetPrincipalFromToken(xauth, configKey, true);
        var claims = principal.Claims.ToArray();

        var rToken = JwtUtil.GenerateToken(claims, configKey: configKey);

        httpContext.SetResponseHender("Access-Control-Expose-Headers", $"Authorization,X-Authorization,Token-Timetamp");
        httpContext.SetResponseHender("Authorization", rToken.Token);
        httpContext.SetResponseHender("X-Authorization", rToken.RefreshToken);
        httpContext.SetResponseHender("Token-Timetamp", rToken.Timetamp.ToString());

        return Task.FromResult(rToken)!;
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
        //var config = JsonWebTokenConfig.GetJsonWebTokenConfig(configKey) ?? throw new Exception($"未配置该JsonWebTokenConfig，key:{configKey}");
        //var hender = config.Header;
        //var payload = config.Payload;


        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                //var reponseToken = await RefreshAuthorizationAsync(context, httpContext);
                // 获取 token 的值
                var token = authorization["Bearer ".Length..].Trim();
                if (JwtUtil.VerifyRefreshToken(token, configKey))
                {
                    context.Fail(new(this, "无法使用刷新Token进行校验"));
                }
                // 定义一个密钥，用于验证 token 的签名
                //var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.EncryptionKey));
                //// 定义一个令牌验证参数
                //var validationParameters = new TokenValidationParameters
                //{
                //    ValidateIssuer = true,
                //    ValidateAudience = true,
                //    ValidateLifetime = true,
                //    ValidateIssuerSigningKey = true,
                //    ValidIssuer = payload.Issuer,
                //    ValidAudience = payload.Audience,
                //    IssuerSigningKey = key
                //};
                //// 定义一个令牌处理器
                //var handler = new JwtSecurityTokenHandler();
                // 验证 token 的有效性，并获取用户的身份和声明
                //var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                var principal = JwtUtil.GetPrincipalFromToken(token, configKey);
                var claims = principal.Claims;
                // 判断是否满足 JWTHandler 的需求
                if (context.Requirements.Contains(this))
                {
                    //    // 创建一个基于声明的身份
                    //    var identity = new ClaimsIdentity(claims, "Bearer");
                    //    // 创建一个基于身份的主体
                    //    var user = new ClaimsPrincipal(identity);
                    // 将主体赋值给 httpContext.User
                    //httpContext!.User = user;
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
