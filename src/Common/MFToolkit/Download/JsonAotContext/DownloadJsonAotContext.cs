using System.Text.Json.Serialization;
using MFToolkit.Download.Models;

namespace MFToolkit.Download.JsonAotContext;
[JsonSerializable(typeof(DownloadModel))]
[JsonSerializable(typeof(List<DownloadModel>))]
public partial class DownloadJsonAotContext : JsonSerializerContext;