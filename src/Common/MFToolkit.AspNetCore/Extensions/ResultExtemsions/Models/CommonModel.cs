#nullable disable

using System.Text.Json.Serialization;
using MFToolkit.Extensions;

namespace MFToolkit.AspNetCore.Extensions.ResultExttensions.Models;
/// <summary>
/// 基本返回模型
/// </summary>
public class CommonModel
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; }
    /// <summary>
    /// 请求是否成功
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// 返回时间戳
    /// </summary>
    public long Timestamp { get; } = DateTime.UtcNow.ToNowTimetampInSeconds();
}
/// <summary>
/// 返回值模型
/// </summary>
/// <typeparam name="T"></typeparam>
public class CommonModel<T> : CommonModel
{
    /// <inheritdoc/>
    public CommonModel()
    {
    }
    /// <inheritdoc/>
    public CommonModel(T data)
    {
        Data = data;
    }

    /// <summary>
    /// 返回值
    /// </summary>
    public T Data { get; set; }
}
/// <summary>
/// 错误模型
/// </summary>
public class ErrorModel
{

    /// <summary>
    /// 错误码
    /// </summary>
    public int ErrorCode { get; set; }
    /// <summary>
    /// 错误原因
    /// </summary>
    public string Message { get; set; }
}
/// <summary>
/// 错误返回模型
/// </summary>
public class CommonErrorModel<T> : CommonModel
{
    /// <inheritdoc/>
    public CommonErrorModel() { }
    /// <inheritdoc/>
    public CommonErrorModel(T error)
    {
        Error = error;
    }
    /// <summary>
    /// 错误信息实体泛型
    /// </summary>
    public T Error { get; set; }
}

/// <summary>
/// 通用返回 JSON 序列化上下文
/// </summary>
[JsonSerializable(typeof(CommonModel))]
[JsonSerializable(typeof(CommonModel<ErrorModel>))]
[JsonSerializable(typeof(CommonErrorModel<ErrorModel>))]
[JsonSerializable(typeof(ErrorModel))]
public partial class JsonCommonResultAotContext : JsonSerializerContext
{

}