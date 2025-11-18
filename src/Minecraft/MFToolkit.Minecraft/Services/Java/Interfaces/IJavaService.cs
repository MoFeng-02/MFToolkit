using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;

namespace MFToolkit.Minecraft.Services.Java.Interfaces;

/// <summary>
/// Java服务接口
/// 提供Java安装查找和管理功能
/// </summary>
public interface IJavaService
{
    /// <summary>
    /// 查找所有可用的Java安装
    /// </summary>
    /// <returns>Java安装列表</returns>
    Task<List<JavaInstallation>> FindAllJavaInstallationsAsync();
        
    /// <summary>
    /// 通过进程查找Java安装
    /// </summary>
    /// <returns>从进程中找到的Java安装列表</returns>
    Task<List<JavaInstallation>> FindJavaFromProcessesAsync();
        
    /// <summary>
    /// 验证Java安装是否有效
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>是否有效</returns>
    Task<bool> ValidateJavaInstallationAsync(string javaPath);
        
    /// <summary>
    /// 获取Java版本信息
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>版本信息字符串</returns>
    Task<string> GetJavaVersionAsync(string javaPath);
        
    /// <summary>
    /// 查找推荐的Java安装（按版本排序）
    /// </summary>
    /// <returns>推荐的Java安装，如果没有找到则返回null</returns>
    Task<JavaInstallation?> FindRecommendedJavaAsync();
        
    /// <summary>
    /// 根据版本号查找Java安装
    /// </summary>
    /// <param name="version">版本号（如 "1.8", "11", "17"）</param>
    /// <returns>匹配的Java安装列表</returns>
    Task<List<JavaInstallation>> FindJavaByVersionAsync(string version);
        
    /// <summary>
    /// 根据供应商查找Java安装
    /// </summary>
    /// <param name="vendor">Java供应商</param>
    /// <returns>匹配的Java安装列表</returns>
    Task<List<JavaInstallation>> FindJavaByVendorAsync(JavaVendor vendor);
    
    /// <summary>
    /// 根据架构查找Java安装
    /// </summary>
    Task<List<JavaInstallation>> FindJavaByArchitectureAsync(string architecture);
    
    /// <summary>
    /// 查找64位Java安装
    /// </summary>
    Task<List<JavaInstallation>> Find64BitJavaAsync();
    
    /// <summary>
    /// 查找32位Java安装
    /// </summary>
    Task<List<JavaInstallation>> Find32BitJavaAsync();
    
    /// <summary>
    /// 根据主版本号查找Java安装
    /// </summary>
    Task<List<JavaInstallation>> FindJavaByMajorVersionAsync(int majorVersion);
}