// using MFToolkit.Minecraft.Entities.Java;
// using MFToolkit.Minecraft.Enums.Java;
// using MFToolkit.Minecraft.Services.Java.Interfaces;
// using Microsoft.Extensions.Logging;
//
// namespace MFToolkit.Minecraft.Services.Java.Internals;
//
// /// <summary>
//     /// 安全的Java服务包装器，防止异常传播
//     /// </summary>
//     internal class SafeJavaService : IJavaService
//     {
//         private readonly IJavaService _innerService;
//         private readonly ILogger<SafeJavaService>? _logger;
//
//         public SafeJavaService(IJavaService innerService, ILogger<SafeJavaService>? logger = null)
//         {
//             _innerService = innerService;
//             _logger = logger;
//         }
//
//         public async Task<List<JavaInstallation>> FindAllJavaInstallationsAsync()
//         {
//             try
//             {
//                 _logger?.LogInformation("开始安全查找Java安装...");
//                 return await _innerService.FindAllJavaInstallationsAsync();
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "查找Java安装时发生异常");
//                 return new List<JavaInstallation>();
//             }
//         }
//
//         public async Task<List<JavaInstallation>> FindJavaFromProcessesAsync()
//         {
//             try
//             {
//                 _logger?.LogInformation("开始安全从进程查找Java...");
//                 return await _innerService.FindJavaFromProcessesAsync();
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "从进程查找Java时发生异常");
//                 return new List<JavaInstallation>();
//             }
//         }
//
//         public async Task<bool> ValidateJavaInstallationAsync(string javaPath)
//         {
//             try
//             {
//                 _logger?.LogDebug("安全验证Java安装: {JavaPath}", javaPath);
//                 return await _innerService.ValidateJavaInstallationAsync(javaPath);
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "验证Java安装时发生异常: {JavaPath}", javaPath);
//                 return false;
//             }
//         }
//
//         public async Task<string> GetJavaVersionAsync(string javaPath)
//         {
//             try
//             {
//                 _logger?.LogDebug("安全获取Java版本: {JavaPath}", javaPath);
//                 return await _innerService.GetJavaVersionAsync(javaPath);
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "获取Java版本时发生异常: {JavaPath}", javaPath);
//                 return string.Empty;
//             }
//         }
//
//         public async Task<JavaInstallation?> FindRecommendedJavaAsync()
//         {
//             try
//             {
//                 _logger?.LogInformation("安全查找推荐的Java安装...");
//                 return await _innerService.FindRecommendedJavaAsync();
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "查找推荐的Java安装时发生异常");
//                 return null;
//             }
//         }
//
//         public async Task<List<JavaInstallation>> FindJavaByVersionAsync(string version)
//         {
//             try
//             {
//                 _logger?.LogInformation("安全根据版本查找Java: {Version}", version);
//                 return await _innerService.FindJavaByVersionAsync(version);
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "根据版本查找Java时发生异常: {Version}", version);
//                 return new List<JavaInstallation>();
//             }
//         }
//
//         public async Task<List<JavaInstallation>> FindJavaByVendorAsync(JavaVendor vendor)
//         {
//             try
//             {
//                 _logger?.LogInformation("安全根据供应商查找Java: {Vendor}", vendor);
//                 return await _innerService.FindJavaByVendorAsync(vendor);
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError(ex, "根据供应商查找Java时发生异常: {Vendor}", vendor);
//                 return new List<JavaInstallation>();
//             }
//         }
//     }