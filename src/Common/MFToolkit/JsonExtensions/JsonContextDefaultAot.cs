using System.Text.Json.Serialization;

namespace MFToolkit.JsonExtensions;

/// <summary>
/// AOT JSON 序列化配置器
/// </summary>

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(object))]
public partial class JsonContextDefaultAot : JsonSerializerContext
{
}