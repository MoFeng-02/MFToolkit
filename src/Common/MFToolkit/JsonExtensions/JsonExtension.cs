using System.Text.Json;

namespace MFToolkit.JsonExtensions;

/// <summary>
/// json 拓展类
/// </summary>
public static class JsonExtension
{
    /// <summary>
    /// 默认json配置
    /// </summary>
    private static JsonSerializerOptions? _defaultJsonSerializerOptions;

    /// <summary>
    /// 设置默认Json配置，如果调用本拓展类中的转换方法并且设置了Json配置，那默认配置不生效
    /// </summary>
    /// <param name="options"></param>
    public static void SetDefaultJsonSerializerOptions(JsonSerializerOptions options)
    {
        _defaultJsonSerializerOptions = options;
    }

    /// <summary>
    /// 设置默认Json配置，如果调用本拓展类中的转换方法并且设置了Json配置，那默认配置不生效
    /// </summary>
    /// <param name="_"></param>
    /// <param name="options"></param>
    public static void SetDefaultJsonSerializerOptions(this object _, JsonSerializerOptions options)
    {
        _defaultJsonSerializerOptions = options;
    }

    /// <summary>
    /// 转为Json字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="options">配置</param>
    /// <returns></returns>
    public static string ValueToJson<T>(this T t, JsonSerializerOptions? options = null)
    {
        options ??= _defaultJsonSerializerOptions;
        try
        {
            return JsonSerializer.Serialize(t, options);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 序列化为 UTF-8 字节数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="options">配置</param>
    /// <returns></returns>
    public static byte[]? TypeToUtf8ByteArray<T>(this T t, JsonSerializerOptions? options = null)
    {
        options ??= _defaultJsonSerializerOptions;
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(t, options ?? _defaultJsonSerializerOptions);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// 反序列化为类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <param name="options">配置</param>
    /// <param name="defaultValue">如果反序列化失败或者什么的就返回默认值</param>
    /// <returns></returns>
    public static T? JsonToDeserialize<T>(this string str, JsonSerializerOptions? options = null,
        T? defaultValue = default)
    {
        options ??= _defaultJsonSerializerOptions;
        try
        {
            var resutl = JsonSerializer.Deserialize<T>(str, options);
            return resutl;
        }
        catch (Exception)
        {
            return defaultValue ?? default;
        }
    }

    /// <summary>
    /// 从 UTF-8 进行反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <param name="options"></param>
    /// <param name="defaultValue">如果反序列化失败或者什么的就返回默认值</param>
    /// <returns></returns>
    public static T? Utf8ByteArrayToDeserialize<T>(this byte[] bytes, JsonSerializerOptions? options = null,
        T? defaultValue = default)
    {
        options ??= _defaultJsonSerializerOptions;
        try
        {
            var readOnlySpan = new ReadOnlySpan<byte>(bytes);
            var t = JsonSerializer.Deserialize<T>(readOnlySpan, options)!;
            return t;
        }
        catch (Exception)
        {
            return defaultValue ?? default;
        }
    }
}