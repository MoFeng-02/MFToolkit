using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 资源索引信息
/// </summary>
public class AssetIndex
{
    /// <summary>
    /// 资源索引ID
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// 资源索引下载URL
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    
    /// <summary>
    /// SHA1哈希值
    /// </summary>
    [JsonPropertyName("sha1")]
    public required string Sha1 { get; set; }
    
    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
    /// <summary>
    /// 总大小（字节）
    /// </summary>
    [JsonPropertyName("totalSize")]
    public long TotalSize { get; set; }
}

/// <summary>
/// 资源索引内容
/// </summary>
public class AssetIndexContent
{
    [JsonPropertyName("objects")]
    public Dictionary<string, AssetObject>? Objects { get; set; }
}

/// <summary>
/// 资源对象
/// </summary>
public class AssetObject
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;
    [JsonPropertyName("size")]
    public long Size { get; set; }
}