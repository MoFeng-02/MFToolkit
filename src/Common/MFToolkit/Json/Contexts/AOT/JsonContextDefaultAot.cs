using System.Text.Json.Serialization;
using MFToolkit.Http.Models;

namespace MFToolkit.Json.Contexts.AOT;

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
[JsonSerializable(typeof(Ulid))]
[JsonSerializable(typeof(ApiResult))]
public partial class JsonContextDefaultAot : JsonSerializerContext;