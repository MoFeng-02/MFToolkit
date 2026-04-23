using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;

namespace MFToolkit.Minecraft.Options;

public class JavaOptions : BaseOptions
{
    public static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config", "java_config.json");

    /// <summary>
    /// 下载使用的Java供应商，默认Microsoft Build of OpenJDK
    /// </summary>
    public JavaVendor Vendor { get; set; } = JavaVendor.Microsoft;

    /// <summary>
    /// 是否自动选择Java安装，在启用时将忽略用户指定的Java安装
    /// </summary>
    public bool IsAutoSelect { get; set; } = true;

    /// <summary>
    /// 选择的Java安装，用户指定选择
    /// </summary>
    public JavaInstallation? JavaInstallation { get; set; }

    /// <summary>
    /// 当前已配置的Java安装列表
    /// </summary>
    [JsonPropertyName("javaInstallations")]
    public List<JavaInstallation> JavaInstallations { get; set; } = [];
}
