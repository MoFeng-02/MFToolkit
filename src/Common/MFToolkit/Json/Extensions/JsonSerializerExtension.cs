using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using MFToolkit.Loggers.MFLogger.Utils;

namespace MFToolkit.Json.Extensions;

/// <summary>
/// JSON 序列化/反序列化扩展方法
/// <para>支持反射模式和源生成模式(AOT)</para>
/// </summary>
public static class JsonSerializerExtension
{
    private static JsonSerializerOptions? _defaultJsonSerializerOptions;

    /// <summary>
    /// 全局默认 JSON 序列化配置
    /// </summary>
    public static JsonSerializerOptions? DefaultJsonSerializerOptions => _defaultJsonSerializerOptions;

    /// <summary>
    /// 设置全局默认 JSON 序列化配置
    /// </summary>
    /// <param name="options">JSON 序列化配置</param>
    public static void SetDefaultJsonSerializerOptions(this JsonSerializerOptions options)
    {
        _defaultJsonSerializerOptions = options;
    }

    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="value">要序列化的对象</param>
    /// <param name="options">序列化选项</param>
    /// <returns>JSON 字符串或 null</returns>
    public static string? ValueToJson<T>(this T value, JsonSerializerOptions? options = null)
    {
        if (value is null) return null;

        try
        {
            ValidateOptionsForAot(options ??= _defaultJsonSerializerOptions);
            return JsonSerializer.Serialize(value, options);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            LoggerUtil.LogError($"Json格式化序列错误：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 异步将对象序列化为 JSON 字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="value">要序列化的对象</param>
    /// <param name="options">序列化选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>JSON 字符串或 null</returns>
    public static async Task<string?> ValueToJsonAsync<T>(
        this T value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (value is null) return null;

        ValidateOptionsForAot(options ??= _defaultJsonSerializerOptions);

        try
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);

            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            LoggerUtil.LogError($"Json格式化序列错误：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 将对象序列化为 UTF-8 字节数组
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="value">要序列化的对象</param>
    /// <param name="options">序列化选项</param>
    /// <returns>UTF-8 字节数组或 null</returns>
    public static byte[]? ValueToUtf8Bytes<T>(this T value, JsonSerializerOptions? options = null)
    {
        if (value is null) return null;

        try
        {
            ValidateOptionsForAot(options ??= _defaultJsonSerializerOptions);
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            LoggerUtil.LogError($"Json格式化序列错误：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为对象
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <param name="defaultValue">反序列化失败时返回的默认值</param>
    /// <param name="options">反序列化选项</param>
    /// <returns>反序列化后的对象或默认值</returns>
    public static T? JsonToValue<T>(
        [StringSyntax("Json")] this string? json,
        T? defaultValue = default,
        JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return defaultValue;

        try
        {
            options ??= _defaultJsonSerializerOptions;
            return GetDeserializedValue<T>(json, options) ?? defaultValue;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            LoggerUtil.LogError($"Json格式化序列错误：{ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// 异步将 JSON 字符串反序列化为对象
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <param name="defaultValue">反序列化失败时返回的默认值</param>
    /// <param name="options">反序列化选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>反序列化后的对象或默认值</returns>
    public static async ValueTask<T?> JsonToValueAsync<T>(
        [StringSyntax("Json")] this string? json,
        T? defaultValue = default,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(json))
            return defaultValue;

        try
        {
            options ??= _defaultJsonSerializerOptions;
            return await GetDeserializedValueAsync<T>(json, options, cancellationToken) ?? defaultValue;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 将 UTF-8 字节数组反序列化为对象
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="bytes">UTF-8 字节数组</param>
    /// <param name="defaultValue">反序列化失败时返回的默认值</param>
    /// <param name="options">反序列化选项</param>
    /// <returns>反序列化后的对象或默认值</returns>
    public static T? BytesToValue<T>(
        this byte[]? bytes,
        T? defaultValue = default,
        JsonSerializerOptions? options = null)
    {
        if (bytes is null || bytes.Length == 0)
            return defaultValue;

        try
        {
            options ??= _defaultJsonSerializerOptions;
            return GetDeserializedValue<T>(bytes, options) ?? defaultValue;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            LoggerUtil.LogError($"Json格式化序列错误：{ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// 从 JSON 字符串反序列化对象（支持反射和源生成模式）
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <param name="options">反序列化选项</param>
    /// <returns>反序列化后的对象</returns>
    /// <remarks>
    /// 根据当前运行模式自动选择反射或源生成方式：
    /// 1. 反射模式：直接使用 JsonSerializer.Deserialize
    /// 2. 源生成模式：从选项中获取类型信息进行反序列化
    /// </remarks>
    private static T? GetDeserializedValue<T>(string json, JsonSerializerOptions? options)
    {
        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }

        ValidateOptionsForAot(options ??= _defaultJsonSerializerOptions);
        var typeInfo = GetTypeInfo<T>(options);
        return JsonSerializer.Deserialize(json, typeInfo);
    }

    /// <summary>
    /// 从字节数组反序列化对象（支持反射和源生成模式）
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="bytes">UTF-8 字节数组</param>
    /// <param name="options">反序列化选项</param>
    /// <returns>反序列化后的对象</returns>
    /// <remarks>
    /// 使用 ReadOnlySpan 避免不必要的内存分配，特别适合字节数组操作
    /// </remarks>
    private static T? GetDeserializedValue<T>(ReadOnlySpan<byte> bytes, JsonSerializerOptions? options)
    {
        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            return JsonSerializer.Deserialize<T>(bytes, options);
        }

        ValidateOptionsForAot(options ??= _defaultJsonSerializerOptions);
        var typeInfo = GetTypeInfo<T>(options);
        return JsonSerializer.Deserialize(bytes, typeInfo);
    }

    /// <summary>
    /// 异步从 JSON 字符串反序列化对象（支持反射和源生成模式）
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <param name="options">反序列化选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>反序列化后的对象</returns>
    /// <remarks>
    /// 使用 ArrayPool 重用缓冲区减少内存分配，特别适合大字符串处理
    /// </remarks>
    private static async ValueTask<T?> GetDeserializedValueAsync<T>(
        string json,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        // 从共享池租用缓冲区，避免大字符串的内存分配
        var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(json.Length));
        try
        {
            // 将 JSON 字符串编码为字节流
            var count = Encoding.UTF8.GetBytes(json, buffer);
            using var stream = new MemoryStream(buffer, 0, count);

            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
            }

            ValidateOptionsForAot(options ??= _defaultJsonSerializerOptions);
            var typeInfo = GetTypeInfo<T>(options);
            return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken);
        }
        catch (Exception ex)
        {
            LoggerUtil.LogError($"Json格式化序列错误：{ex.Message}");
            return default;
        }
        finally
        {
            // 确保归还缓冲区到共享池
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// 验证 AOT 模式下的选项配置
    /// </summary>
    /// <param name="options">反序列化选项</param>
    /// <exception cref="InvalidOperationException">
    /// 当选项为 null 时抛出，AOT 模式必须提供包含类型信息的选项
    /// </exception>
    private static void ValidateOptionsForAot(JsonSerializerOptions? options)
    {
        if (!JsonSerializer.IsReflectionEnabledByDefault && options is null)
        {
            throw new InvalidOperationException(
                "在 AOT 模式下必须提供 JsonSerializerOptions 并配置 JSON 类型信息");
        }
    }

    /// <summary>
    /// 从选项中获取指定类型的 JSON 类型信息
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="options">反序列化选项</param>
    /// <returns>JSON 类型信息</returns>
    /// <exception cref="InvalidOperationException">
    /// 当无法从选项中获取指定类型的类型信息时抛出
    /// </exception>
    private static JsonTypeInfo<T> GetTypeInfo<T>(JsonSerializerOptions? options)
    {
        return options?.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new InvalidOperationException(
                $"在提供的选项中找不到 {typeof(T)} 的 JSON 类型信息");
    }

    /// <summary>
    /// JSON 操作异常
    /// </summary>
    public class JsonOperationException : Exception
    {
        /// <summary>
        /// 源 JSON 字符串
        /// </summary>
        public string? SourceJson { get; init; }

        /// <summary>
        /// 目标类型
        /// </summary>
        public Type? TargetType { get; init; }
        /// <inheritdoc/>
        public JsonOperationException(string message, Exception inner)
            : base(message, inner) { }
    }
}