using System.Text.Json.Serialization;
using MFToolkit.Utils.HttpExtensions.Results;

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
[JsonSerializable(typeof(ApiResult))]
public partial class JsonContextDefaultAot : JsonSerializerContext
{
}

public class AT
{
    public int At { get; set; } = 1;
    public string Name { get; set; } = "2";
    public List<Re> Itmes { get; set; } = new();
}

public class Re
{
    public int Aa { get; set; }
}