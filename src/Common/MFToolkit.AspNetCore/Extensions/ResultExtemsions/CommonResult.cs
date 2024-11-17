using System.Net.Mime;
using System.Text.Json.Serialization;
using MFToolkit.AspNetCore.Extensions.ResultExttensions.Models;
using MFToolkit.Json.Extensions;
using Microsoft.AspNetCore.Http;

namespace MFToolkit.AspNetCore.Extensions.ResultExttensions;
/// <summary>
/// 通用结果返回
/// <para>支持AOT</para>
/// </summary>
public class CommonResult<T> : IResult
{
    public T Result { get; set; }
    public int StatusCode { get; set; } = 200;
    JsonSerializerContext? Context { get; set; }
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
        var result = model.ValueToJson(context: Context);
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
