using System.Net;
using System.Net.Mime;
using System.Text.Json;
using MFToolkit.AspNetCore.Extensions.ResultExttensions.Models;
using MFToolkit.Exceptions;
using MFToolkit.Json.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MFToolkit.AspNetCore.Middlewares;
/// <summary>
/// 全局异常处理中间件，用于捕获并处理未处理的异常。
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    /// <summary>
    /// 构造函数，初始化下一个中间件。
    /// </summary>
    /// <param name="next">下一个中间件的委托。</param>
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 调用中间件，处理请求。
    /// </summary>
    /// <param name="httpContext">HTTP 上下文。</param>
    /// <returns>一个 Task 表示异步操作。</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            // 调用下一个中间件
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            // 捕获并处理未处理的异常
            await HandleExceptionAsync(httpContext, ex);
            _logger.LogError(ex, ex.Message);
        }
    }

    /// <summary>
    /// 处理异常，返回格式化的错误信息。
    /// </summary>
    /// <param name="httpContext">HTTP 上下文。</param>
    /// <param name="exception">捕获到的异常。</param>
    /// <returns>一个 Task 表示异步操作。</returns>
    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        // 设置响应的内容类型为 JSON
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        // 根据异常类型设置响应的状态码和错误信息
        var (statusCode, errorMessage) = GetExceptionDetails(exception);
        // 创建错误模型
        var error = new ErrorModel
        {
            ErrorCode = statusCode,
            Message = errorMessage
        };

        // 创建 CommonErrorModel 包装错误
        var commonErrorModel = new CommonErrorModel<ErrorModel>
        {
            StatusCode = httpContext.Response.StatusCode,
            IsSuccess = false,
            Error = error
        };


        // 将 CommonErrorModel 序列化为 JSON 字符串
        var errorJson = JsonSerializer.Serialize(commonErrorModel, JsonSerializerExtension.DefaultJsonSerializerOptions);

        // 将 JSON 字符串写入响应体
        await httpContext.Response.WriteAsync(errorJson);
    }
    private static (int, string) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not Found"),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Invalid Operation"),
            DbUpdateException => ((int)HttpStatusCode.Conflict, "Database Update Conflict"),
            HttpRequestException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            MFCommonException => (((MFCommonException)exception).Code, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, exception.Message ?? "Internal Server Error"),
        };
    }
}
/// <summary>
/// 全局异常处理中间件的扩展方法。
/// <para>参考：https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/extensibility?view=aspnetcore-9.0#imiddleware</para>
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    /// <summary>
    /// 注册全局异常处理中间件。
    /// </summary>
    /// <param name="builder">应用构建器。</param>
    /// <returns>应用构建器。</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}