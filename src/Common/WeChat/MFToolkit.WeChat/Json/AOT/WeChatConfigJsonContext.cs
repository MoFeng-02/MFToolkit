using System.Text.Json.Serialization;
using MFToolkit.WeChat.Configurations.BasicConfiguration;
using MFToolkit.WeChat.Model;

namespace MFToolkit.WeChat.Json.AOT;
/// <summary>
/// WeChat相关处理的
/// </summary>
[JsonSerializable(typeof(AccessData))]
[JsonSerializable(typeof(WeChatConfig))]
[JsonSerializable(typeof(Dictionary<string, WeChatConfig>))]
[JsonSerializable(typeof(KeyValuePair<string, WeChatConfig>))]
public partial class WeChatConfigJsonContext : JsonSerializerContext;
