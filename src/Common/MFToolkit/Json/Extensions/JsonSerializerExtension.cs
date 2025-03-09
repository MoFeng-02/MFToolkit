using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MFToolkit.Json.Contexts.AOT;
using Org.BouncyCastle.Utilities.IO;

namespace MFToolkit.Json.Extensions;

/// <summary>
/// json 拓展类
/// <para>本类支持进行处理是AOT模式还是框架依赖模式</para>
/// </summary>
public static class JsonSerializerExtension
{
    /// <summary>
    /// 默认json配置
    /// </summary>
    private static JsonSerializerOptions? _defaultJsonSerializerOptions;
    /// <summary>
    /// 全局Json配置
    /// </summary>
    public static JsonSerializerOptions? DefaultJsonSerializerOptions => _defaultJsonSerializerOptions;
    /// <summary>
    /// 设置默认Json配置，如果调用本拓展类中的转换方法并且设置了Json配置，那默认配置不生效，AOT模式不适用
    /// </summary>
    /// <param name="options"></param>
    public static void SetDefaultJsonSerializerOptions(JsonSerializerOptions options)
    {
        _defaultJsonSerializerOptions = options;
    }

    /// <summary>
    /// 设置默认Json配置，如果调用本拓展类中的转换方法并且设置了Json配置，那默认配置不生效，AOT模式不适用
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
    /// <param name="options">配置，这是反射也就是不是AOT模式的时候有效</param>
    /// <param name="context">AOT模式下兼容的处理，自行参考<see cref="JsonContextDefaultAot"/></param>
    /// <returns></returns>
    public static string? ValueToJson<T>(this T t, JsonSerializerOptions? options = null, JsonSerializerContext?
        context = null)
    {
        try
        {
            // 判断是否禁用反射
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                options ??= _defaultJsonSerializerOptions;
                return JsonSerializer.Serialize(t, options);
            }

            context ??= JsonContextDefaultAot.Default;
            var re = JsonSerializer.Serialize(t, typeof(T), context);
            return re;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 异步流序列化版本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="options"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<string> ValueToJsonAsync<T>(this T t, JsonSerializerOptions? options = null, JsonSerializerContext? context = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();

        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            options ??= _defaultJsonSerializerOptions;
            await JsonSerializer.SerializeAsync(stream, t, options, cancellationToken);
        }
        else
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "禁用反射时必须提供上下文。");

            await JsonSerializer.SerializeAsync(stream, t, typeof(T), context, cancellationToken);
        }

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// 序列化为 UTF-8 字节数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="options">配置，这是反射也就是不是AOT模式的时候有效</param>
    /// <param name="context">AOT模式下兼容的处理，自行参考<see cref="JsonContextDefaultAot"/></param>
    /// <returns></returns>
    public static byte[]? TypeToUtf8ByteArray<T>(this T t, JsonSerializerOptions? options = null, JsonSerializerContext?
        context = null)
    {
        try
        {
            // 判断是否禁用反射
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                options ??= _defaultJsonSerializerOptions;
                return JsonSerializer.SerializeToUtf8Bytes(t, options);
            }

            context ??= JsonContextDefaultAot.Default;
            var re = JsonSerializer.SerializeToUtf8Bytes(t, typeof(T), context);
            return re;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 反序列化为类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <param name="options">配置，这是反射也就是不是AOT模式的时候有效</param>
    /// <param name="defaultValue">如果反序列化失败或者什么的就返回默认值</param>
    /// <param name="context">AOT模式下兼容的处理，自行参考<see cref="JsonContextDefaultAot"/></param>
    /// <returns></returns>
    public static T? JsonToDeserialize<T>(this string str, JsonSerializerOptions? options = null,
        T? defaultValue = default, JsonSerializerContext?
            context = null)
    {
        try
        {
            // 判断是否禁用反射
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                options ??= _defaultJsonSerializerOptions;
                return JsonSerializer.Deserialize<T>(str, options);
            }

            context ??= JsonContextDefaultAot.Default;
            var re = JsonSerializer.Deserialize(str, typeof(T), context);
            if (re is not T result) return defaultValue ?? default;
            return result;
        }
        catch
        {
            return defaultValue ?? default;
        }
    }

    /// <summary>
    /// 异步反序列化版本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <param name="options"></param>
    /// <param name="defaultValue"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<T?> JsonToDeserializeAsync<T>(
     this string str,
     JsonSerializerOptions? options = null,
     T? defaultValue = default,
     JsonSerializerContext? context = null,
     CancellationToken cancellationToken = default)
    {
        // 空值快速返回
        if (string.IsNullOrWhiteSpace(str))
            return defaultValue;

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(str));

            // 反射模式
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                options ??= _defaultJsonSerializerOptions;
                var result = await JsonSerializer.DeserializeAsync<T>(
                    stream,
                    options,
                    cancellationToken
                ).ConfigureAwait(false);

                return result ?? defaultValue;
            }

            // 源生成模式
            if (context == null)
                throw new ArgumentNullException(nameof(context), "JSON context is required when reflection is disabled");

            var typeInfo = context.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
                ?? throw new InvalidOperationException($"No JSON type info found for {typeof(T)} in context");

            return await JsonSerializer.DeserializeAsync(
                stream,
                typeInfo,
                cancellationToken
            ).ConfigureAwait(false) ?? defaultValue;
        }
        catch (JsonException ex)
        {
            // 结构化异常处理
            throw new JsonOperationException("JSON deserialization failed", ex)
            {
                SourceJson = str,
                TargetType = typeof(T)
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return defaultValue; // 非JSON相关异常返回默认值
        }
    }



    /// <summary>
    /// 从 UTF-8 进行反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes">原字节数组</param>
    /// <param name="options">配置，这是反射也就是不是AOT模式的时候有效</param>
    /// <param name="defaultValue">如果反序列化失败 返回默认值</param>
    /// <param name="context">AOT模式下兼容的处理，自行参考<see cref="JsonContextDefaultAot"/></param>
    /// <returns></returns>
    public static T? Utf8ByteArrayToDeserialize<T>(this byte[] bytes, JsonSerializerOptions? options = null,
        T? defaultValue = default, JsonSerializerContext?
            context = null)
    {
        try
        {
            var readOnlySpan = new ReadOnlySpan<byte>(bytes);
            // 判断是否禁用反射
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                options ??= _defaultJsonSerializerOptions;
                var t = JsonSerializer.Deserialize<T>(readOnlySpan, options);
                return t;
            }

            context ??= JsonContextDefaultAot.Default;
            var re = JsonSerializer.Deserialize(readOnlySpan, typeof(T), context);
            if (re is not T result) return defaultValue ?? default;
            return result;
        }
        catch
        {
            return defaultValue ?? default;
        }
    }
    
    /// <summary>
    /// 自定义异常类型
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

        /// <summary>
        /// 默认构造
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public JsonOperationException(string message, Exception inner)
            : base(message, inner) { }
    }
}