namespace MFToolkit.Utils.HttpExtensions.Results;
/// <summary>
/// API通用返回参数
/// </summary>
public class ApiResult
{
    /// <summary>
    /// 响应码
    /// </summary>
    /// <value></value>
    public int Code { get; set; }
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// 数据
    /// </summary>
    public object? Data { get; set; }
    /// <summary>
    /// 错误信息
    /// </summary>
    public object? ErrorMsg { get; set; }
    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; }
}

/// <summary>
/// API通用返回参数
/// 采用多态序列化
/// </summary>
/// <typeparam name="T">返回参数Data的类型</typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// 响应码
    /// </summary>
    /// <value></value>
    public int Code { get; set; }
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
    /// <summary>
    /// 错误信息
    /// </summary>
    public object? ErrorMsg { get; set; }
    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; }
}
