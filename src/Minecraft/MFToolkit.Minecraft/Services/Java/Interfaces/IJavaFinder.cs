using MFToolkit.Minecraft.Entities.Java;

namespace MFToolkit.Minecraft.Services.Java.Interfaces;

/// <summary>
/// Java查找器接口，用于在不同平台上查找Java安装
/// </summary>
public interface IJavaFinder
{
    /// <summary>
    /// 查找系统中所有可用的Java安装
    /// </summary>
    Task<List<JavaInstallation>> FindJavaInstallationsAsync();

    /// <summary>
    /// 通过后台进程查找Java安装
    /// </summary>
    Task<List<JavaInstallation>> FindJavaFromProcessesAsync();

    /// <summary>
    /// 验证Java安装是否有效
    /// </summary>
    Task<bool> ValidateJavaInstallationAsync(string javaPath);

    /// <summary>
    /// 获取Java版本信息
    /// </summary>
    Task<string> GetJavaVersionAsync(string javaPath);
}