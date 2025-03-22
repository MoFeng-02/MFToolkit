using System.Text.Json.Serialization;

namespace MFToolkit.Storage.PreferencesUtils;

/// <summary>
/// PreferencesJsonAotContext
/// </summary>
[JsonSerializable(typeof(Preferences))]
[JsonSerializable(typeof(List<Preferences>))]
public partial class PreferencesJsonAotContext : JsonSerializerContext
{
}
