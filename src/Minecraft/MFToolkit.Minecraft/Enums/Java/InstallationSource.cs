namespace MFToolkit.Minecraft.Enums.Java;

/// <summary>
/// 安装来源枚举
/// </summary>
public enum InstallationSource
{
    /// <summary>PATH环境变量</summary>
    PathEnvironment,
    /// <summary>Windows注册表</summary>
    Registry,
    /// <summary>常见目录</summary>
    CommonDirectories,
    /// <summary>进程搜索</summary>
    ProcessSearch,
    /// <summary>自定义路径</summary>
    Custom
}