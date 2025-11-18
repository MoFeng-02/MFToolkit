using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;
using MFToolkit.Minecraft.Services.Java.Interfaces;

namespace MFToolkit.Minecraft.Services.Java;

/// <summary>
/// Java服务实现
/// 提供统一的Java查找和管理功能
/// </summary>
public class JavaService : IJavaService
{
    private readonly IJavaFinder _javaFinder;

    /// <summary>
    /// 使用Java查找器初始化服务
    /// </summary>
    /// <param name="javaFinder">Java查找器实例</param>
    public JavaService(IJavaFinder javaFinder)
    {
        _javaFinder = javaFinder ?? throw new ArgumentNullException(nameof(javaFinder));
    }

    /// <summary>
    /// 查找所有可用的Java安装
    /// </summary>
    /// <returns>Java安装列表</returns>
    public async Task<List<JavaInstallation>> FindAllJavaInstallationsAsync()
    {
        return await _javaFinder.FindJavaInstallationsAsync();
    }

    /// <summary>
    /// 通过进程查找Java安装
    /// </summary>
    /// <returns>从进程中找到的Java安装列表</returns>
    public async Task<List<JavaInstallation>> FindJavaFromProcessesAsync()
    {
        return await _javaFinder.FindJavaFromProcessesAsync();
    }

    /// <summary>
    /// 验证Java安装是否有效
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>是否有效</returns>
    public async Task<bool> ValidateJavaInstallationAsync(string javaPath)
    {
        return await _javaFinder.ValidateJavaInstallationAsync(javaPath);
    }

    /// <summary>
    /// 获取Java版本信息
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>版本信息字符串</returns>
    public async Task<string> GetJavaVersionAsync(string javaPath)
    {
        return await _javaFinder.GetJavaVersionAsync(javaPath);
    }

    /// <summary>
    /// 查找推荐的Java安装（按版本排序）
    /// </summary>
    /// <returns>推荐的Java安装，如果没有找到则返回null</returns>
    public async Task<JavaInstallation?> FindRecommendedJavaAsync()
    {
        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .OrderByDescending(j => TryParseVersion(j.Version))
            .FirstOrDefault();
    }

    /// <summary>
    /// 根据版本号查找Java安装
    /// </summary>
    /// <param name="version">版本号（如 "1.8", "11", "17"）</param>
    /// <returns>匹配的Java安装列表</returns>
    public async Task<List<JavaInstallation>> FindJavaByVersionAsync(string version)
    {
        if (string.IsNullOrEmpty(version))
            return [];

        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .Where(j => j.Version.Contains(version, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// 根据供应商查找Java安装
    /// </summary>
    /// <param name="vendor">Java供应商</param>
    /// <returns>匹配的Java安装列表</returns>
    public async Task<List<JavaInstallation>> FindJavaByVendorAsync(JavaVendor vendor)
    {
        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .Where(j => j.Vendor == vendor)
            .ToList();
    }

    /// <summary>
    /// 尝试解析版本字符串
    /// </summary>
    private static Version? TryParseVersion(string versionOutput)
    {
        if (string.IsNullOrEmpty(versionOutput))
            return null;

        // 简单的版本提取逻辑
        var lines = versionOutput.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("version", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split('"');
                if (parts.Length >= 2)
                {
                    var versionString = parts[1];
                    if (Version.TryParse(versionString.Split(' ')[0], out var version))
                        return version;
                }
            }
        }

        return null;
    }
    /// <summary>
    /// 根据架构查找Java安装
    /// </summary>
    public async Task<List<JavaInstallation>> FindJavaByArchitectureAsync(string architecture)
    {
        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .Where(j => j.Architecture.Equals(architecture, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// 查找64位Java安装
    /// </summary>
    public async Task<List<JavaInstallation>> Find64BitJavaAsync()
    {
        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .Where(j => j.Is64Bit)
            .ToList();
    }

    /// <summary>
    /// 查找32位Java安装
    /// </summary>
    public async Task<List<JavaInstallation>> Find32BitJavaAsync()
    {
        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .Where(j => !j.Is64Bit)
            .ToList();
    }

    /// <summary>
    /// 根据主版本号查找Java安装
    /// </summary>
    public async Task<List<JavaInstallation>> FindJavaByMajorVersionAsync(int majorVersion)
    {
        var installations = await FindAllJavaInstallationsAsync();
        return installations
            .Where(j => j.MajorVersion == majorVersion)
            .ToList();
    }
}