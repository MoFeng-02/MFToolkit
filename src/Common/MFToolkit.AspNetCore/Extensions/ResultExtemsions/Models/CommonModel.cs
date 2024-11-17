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
    public long Timetamp { get; } = DateTime.UtcNow.ToNowTimetampInSeconds();
}
/// <summary>
/// 返回值模型
/// </summary>
/// <typeparam name="T"></typeparam>
public class CommonModel<T> : CommonModel
{
    public CommonModel()
    {
    }
    public CommonModel(T data)
    {
        Data = data;
    }

    /// <summary>
    /// 返回值
    /// </summary>
    public T Data { get; set; }
}
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
    public CommonErrorModel() { }
    public CommonErrorModel(T error)
    {
        Error = error;
    }
    public T Error { get; set; }
}
//[JsonSerializable(typeof(CommonModel))]
//[JsonSerializable(typeof(CommonModel<>))]
//public partial class JsonCommonResultAotContext : JsonSerializerContext;