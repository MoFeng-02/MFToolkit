using System.Net.Mime;
using System.Text.Json.Serialization;
using MFToolkit.App;
using MFToolkit.AspNetCore.Extensions.ResultExttensions.Models;
using MFToolkit.Json.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace MFToolkit.AspNetCore.Extensions.ResultExtemsions;
/// <summary>
/// 通用结果返回
/// <para>支持AOT</para>
/// </summary>
public class CommonResult<T> : IResult
{
    /// <summary>
    /// 获取或设置操作的结果。
    /// </summary>
    public T Result { get; set; }
    /// <summary>
    /// 获取或设置与响应相关联的 HTTP 状态码。
    /// </summary>
    public int StatusCode { get; set; } = 200;
    JsonSerializerContext? Context { get; set; }

    /// <summary>
    /// 初始化 <see cref="CommonResult{T}"/> 类的新实例，表示具有关联状态码和可选 JSON 序列化上下文的结果。
    /// </summary>
    /// <param name="result">要封装的类型为 <typeparamref name="T"/> 的结果对象。</param>
    /// <param name="statusCode">与结果关联的 HTTP 状态码。默认为 200。</param>
    /// <param name="context">用于结果 JSON 序列化的可选 <see cref="JsonSerializerContext"/>。可以为 <see langword="null"/>。</param>
    public CommonResult(T result, int statusCode = 200, JsonSerializerContext? context = null)
    {
        Result = result;
        StatusCode = statusCode;
        Context = context;
    }
    /// <summary>
    /// 成功状态码
    /// </summary>
    static readonly int[] SuccessCodes =
    [
        200,    //200 OK：这是最标准的成功状态码，表示请求已成功处理，并且服务器已经返回了请求的数据。
        201,    //201 Created：这个状态码表示请求已经被执行，而且有一个新的资源已经被创建，通常在POST请求后返回。
        202,    //202 Accepted：表示请求已经被接收，但是还没有被处理，最终的处理结果可能需要等待一段时间才能完成。
        203,    //Non-Authoritative Information（非权威信息）
        204,    //204 No Content：表示请求已经成功处理，但是没有返回任何内容。当不需要返回数据给客户端时使用。
        205,    //Reset Content（重置内容）
        206,    //Partial Content（部分内容）
    ];
    /// <summary>
    /// 执行操作并将结果写入 HTTP 响应中。/// </summary>
    /// <remarks>此方法将响应内容类型设置为 JSON，并将操作的结果写入 HTTP 响应中。响应状态码由 <c>StatusCode</c> 属性决定。如果结果无法序列化为 JSON 格式，则会写入默认模型。</remarks>
    /// <param name="httpContext">代表当前 HTTP 请求和响应的 <see cref="HttpContext"/> 对象。</param>
    /// <returns>表示异步操作的 <see cref="Task"/> 对象。</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        // 获取状态码
        // 错误返回
        var model = new CommonModel<T>(Result)
        {
            StatusCode = StatusCode,
            IsSuccess = SuccessCodes.Contains(StatusCode)
        };
        var result = await model.ValueToJsonAsync(options: Context?.Options);
        httpContext.Response.StatusCode = StatusCode;
        if (result == null)
        {
            await httpContext.Response.WriteAsJsonAsync(model);
        }
        else
        {
            await httpContext.Response.WriteAsync(result);
        }
    }

}
