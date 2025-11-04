using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Enums;

/// <summary>
/// 存储模式
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StorageMode
{
    /// <summary>
    /// 全局共享模式
    /// <para>所有版本共享同一套资源文件和配置</para>
    /// </summary>
    [JsonPropertyName("global")]
    Global,

    /// <summary>
    /// 版本隔离模式
    /// <para>每个版本拥有独立的资源文件和配置</para>
    /// </summary>
    [JsonPropertyName("isolated")]
    Isolated,

    /// <summary>
    /// 智能混合模式
    /// <para>核心资源共享，版本特有资源隔离</para>
    /// </summary>
    [JsonPropertyName("hybrid")]
    Hybrid,

    /// <summary>
    /// 完全自定义模式
    /// <para>用户自定义资源和配置的存储路径</para>
    /// </summary>
    [JsonPropertyName("custom")]
    Custom
}
