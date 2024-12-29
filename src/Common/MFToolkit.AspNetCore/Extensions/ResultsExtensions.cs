using System.Text.Json.Serialization;
using MFToolkit.AspNetCore.Extensions.ResultExttensions;
using Microsoft.AspNetCore.Http;

namespace MFToolkit.AspNetCore.Extensions;
public static class ResultsExtensions
{
    /// <summary>
    /// 返回HTML结果类型
    /// </summary>
    /// <param name="resultExtensions"></param>
    /// <param name="html"></param>
    /// <returns></returns>
    public static IResult Html(this IResultExtensions resultExtensions, string html)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new HtmlResult(html);
    }
    /// <summary>
    /// 通用结果返回
    /// <para>支持AOT</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resultExtensions"></param>
    /// <param name="common"></param>
    /// <param name="statusCode"></param>
    /// <param name="context">AOT JSON 序列化上下文</param>
    /// <returns></returns>
    public static IResult Common<T>(this IResultExtensions resultExtensions, T common, int statusCode = 200, JsonSerializerContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);
        return new CommonResult<T>(common, statusCode, context);
    }
}
