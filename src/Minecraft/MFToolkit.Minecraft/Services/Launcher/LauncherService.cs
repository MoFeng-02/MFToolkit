using System.Diagnostics;
using System.Runtime.InteropServices;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Extensions.VersionExtensions;
using MFToolkit.Minecraft.Helpers;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Java.Interfaces;
using MFToolkit.Minecraft.Services.Launcher.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Launcher;

public class LauncherService : ILauncherService
{
    private readonly ILogger<LauncherService> _logger;
    private readonly IOptionsMonitor<GameLauncherOptions> _launcherOptions;
    private readonly IJavaService _javaService;

    public LauncherService(ILogger<LauncherService> logger, IOptionsMonitor<GameLauncherOptions> launcherOptions, IJavaService javaService)
    {
        _logger = logger;
        _launcherOptions = launcherOptions;
        _javaService = javaService;
    }

    public Task<bool> CloseGameAsync(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            // 尝试优雅关闭
            process.CloseMainWindow();
            // 等待5秒，如果还没关闭则强制关闭
            if (!process.WaitForExit(5000))
            {
                process.Kill();
            }
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭游戏失败，进程ID: {ProcessId}", processId);
            return Task.FromResult(false);
        }
    }

    public Task<bool> IsGameRunningAsync(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            // 验证进程是否真的是Minecraft进程
            return Task.FromResult(!process.HasExited && 
                                  (process.ProcessName.Contains("java", StringComparison.OrdinalIgnoreCase) || 
                                   process.ProcessName.Contains("minecraft", StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
    }

    public async Task<LaunchResult> LaunchGameAsync(VersionInfoDetail versionInfo, Entities.Account.MinecraftAccount? account = null, LaunchCallbacks? callbacks = null, OutputCallbacks? outputCallbacks = null)
    {
        try
        {
            _logger.LogInformation("开始启动Minecraft，版本: {VersionName}", versionInfo.Name);
            
            // 1. 获取启动配置
            var options = _launcherOptions.CurrentValue;
            options.CalculateMemory(); // 确保内存配置有效
            
            // 2. 验证Java环境
            var javaPath = await ValidateJavaEnvironmentAsync(options, versionInfo);
            if (string.IsNullOrEmpty(javaPath))
            {
                return LaunchResult.FailureResult("Java环境验证失败，请检查Java安装");
            }
            
            // 3. 启动前准备工作
            await PrepareGameFilesAsync(versionInfo);
            
            // 4. 构建启动命令
            var processStartInfo = await BuildProcessStartInfoAsync(versionInfo, javaPath, options, account);
            
            // 5. 调用启动前回调
            callbacks?.OnGameStarting?.Invoke(processStartInfo);
            
            // 6. 启动游戏进程
            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return LaunchResult.FailureResult("无法启动游戏进程");
            }
            
            // 7. 配置输出重定向
            if (outputCallbacks != null)
            {
                ConfigureOutputRedirect(process, outputCallbacks);
            }
            
            // 8. 调用启动后回调
            callbacks?.OnGameStarted?.Invoke(process);
            
            // 9. 配置退出回调
            if (callbacks?.OnGameExited != null)
            {
                process.Exited += (sender, args) =>
                {
                    var exitedProcess = sender as Process;
                    if (exitedProcess != null)
                    {
                        callbacks.OnGameExited(exitedProcess.Id, DateTime.Now);
                    }
                };
                process.EnableRaisingEvents = true;
            }
            
            _logger.LogInformation("Minecraft启动成功，进程ID: {ProcessId}", process.Id);
            return LaunchResult.SuccessResult(process.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动Minecraft失败，版本: {VersionName}", versionInfo.Name);
            return LaunchResult.FailureResult(ex.Message, ex);
        }
    }
    
    #region 支持方法
    
    /// <summary>
    /// 验证Java环境
    /// </summary>
    private async Task<string> ValidateJavaEnvironmentAsync(GameLauncherOptions options, VersionInfoDetail versionInfo)
    {
        // 获取游戏所需的Java版本
        var requiredJavaVersion = versionInfo.JavaVersion?.MajorVersion ?? 8; // 默认需要Java 8
        _logger.LogInformation("游戏版本 {VersionName} 需要Java {RequiredJavaVersion}+", versionInfo.Name, requiredJavaVersion);
        
        // 如果用户指定了Java路径，验证其有效性和版本兼容性
        if (!string.IsNullOrEmpty(options.JavaPath))
        {
            _logger.LogInformation("正在验证用户指定的Java路径: {JavaPath}", options.JavaPath);
            
            if (await _javaService.ValidateJavaInstallationAsync(options.JavaPath))
            {
                var javaVersion = await _javaService.GetJavaVersionAsync(options.JavaPath);
                _logger.LogInformation("用户指定的Java版本: {JavaVersion}", javaVersion);
                
                // 验证Java版本是否符合要求
                if (IsJavaVersionCompatible(javaVersion, requiredJavaVersion))
                {
                    _logger.LogInformation("用户指定的Java版本符合要求");
                    return options.JavaPath;
                }
                else
                {
                    _logger.LogWarning("用户指定的Java版本 {JavaVersion} 不符合要求，需要Java {RequiredJavaVersion}+", javaVersion, requiredJavaVersion);
                }
            }
            else
            {
                _logger.LogWarning("用户指定的Java路径无效: {JavaPath}", options.JavaPath);
            }
        }
        
        // 否则自动查找合适的Java版本
        _logger.LogInformation("正在自动查找合适的Java环境...");
        
        // 1. 查找所有可用的Java安装
        var allJavaInstallations = await _javaService.FindAllJavaInstallationsAsync();
        _logger.LogInformation("找到 {JavaCount} 个Java安装", allJavaInstallations.Count);
        
        // 2. 按版本兼容性排序，优先选择64位版本
        var compatibleJavaInstallations = allJavaInstallations
            .Where(j => IsJavaVersionCompatible(j.Version, requiredJavaVersion))
            .OrderByDescending(j => j.Is64Bit)
            .ThenByDescending(j => j.MajorVersion)
            .ToList();
        
        if (compatibleJavaInstallations.Any())
        {
            var selectedJava = compatibleJavaInstallations.First();
            _logger.LogInformation("选择了Java安装: {JavaPath} (版本: {JavaVersion}, 架构: {Architecture})", 
                selectedJava.Path, selectedJava.Version, selectedJava.Is64Bit ? "64位" : "32位");
            
            return selectedJava.Path;
        }
        
        // 3. 如果没有找到合适的Java版本，尝试从进程中查找
        _logger.LogInformation("正在从运行中的进程查找Java...");
        var processJavaInstallations = await _javaService.FindJavaFromProcessesAsync();
        
        var compatibleProcessJava = processJavaInstallations
            .Where(j => IsJavaVersionCompatible(j.Version, requiredJavaVersion))
            .OrderByDescending(j => j.Is64Bit)
            .ThenByDescending(j => j.MajorVersion)
            .FirstOrDefault();
        
        if (compatibleProcessJava != null)
        {
            _logger.LogInformation("从进程中找到了合适的Java: {JavaPath} (版本: {JavaVersion}, 架构: {Architecture})", 
                compatibleProcessJava.Path, compatibleProcessJava.Version, compatibleProcessJava.Is64Bit ? "64位" : "32位");
            
            return compatibleProcessJava.Path;
        }
        
        // 4. 最后尝试在PATH中查找
        _logger.LogInformation("正在PATH中查找Java...");
        var javaExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
            Arguments = javaExecutable,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        try
        {
            using var process = Process.Start(processStartInfo);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var javaPaths = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var javaPath in javaPaths)
                    {
                        if (File.Exists(javaPath))
                        {
                            if (await _javaService.ValidateJavaInstallationAsync(javaPath))
                            {
                                var javaVersion = await _javaService.GetJavaVersionAsync(javaPath);
                                if (IsJavaVersionCompatible(javaVersion, requiredJavaVersion))
                                {
                                    _logger.LogInformation("在PATH中找到了合适的Java: {JavaPath} (版本: {JavaVersion})");
                                    return javaPath;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "在PATH中查找Java失败");
        }
        
        _logger.LogError("未找到合适的Java环境，需要Java {RequiredJavaVersion}+", requiredJavaVersion);
        return string.Empty;
    }
    
    /// <summary>
    /// 检查Java版本是否兼容
    /// </summary>
    /// <param name="javaVersion">Java版本字符串</param>
    /// <param name="requiredMajorVersion">所需的主版本号</param>
    /// <returns>是否兼容</returns>
    private bool IsJavaVersionCompatible(string javaVersion, int requiredMajorVersion)
    {
        try
        {
            // 解析Java版本字符串
            // 支持格式："1.8.0_301", "11.0.12", "17.0.1", "21"
            var versionParts = javaVersion.Split('.', '-', '+')[0];
            
            // 处理"1.x"格式
            if (versionParts.StartsWith("1."))
            {
                versionParts = versionParts.Substring(2);
            }
            
            if (int.TryParse(versionParts, out var majorVersion))
            {
                return majorVersion >= requiredMajorVersion;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析Java版本失败: {JavaVersion}", javaVersion);
        }
        
        return false;
    }
    
    /// <summary>
    /// 启动前准备工作
    /// </summary>
    private async Task PrepareGameFilesAsync(VersionInfoDetail versionInfo)
    {
        _logger.LogInformation("正在准备游戏文件...");
        
        // 获取游戏版本详情
        var gameVersionDetail = await versionInfo.GetGameVersionDetailAsync();
        if (gameVersionDetail == null)
        {
            throw new InvalidOperationException("游戏版本详情为空");
        }
        
        // 1. 验证游戏文件完整性
        await ValidateGameFilesAsync(versionInfo);
        
        // 2. 解压原生库
        await ExtractNativeLibrariesAsync(versionInfo, gameVersionDetail);
        
        // 3. 确保所有依赖库都存在
        await EnsureLibrariesExistAsync(versionInfo, gameVersionDetail);
        
        _logger.LogInformation("游戏文件准备完成");
    }
    
    /// <summary>
    /// 验证游戏文件完整性
    /// </summary>
    private async Task ValidateGameFilesAsync(VersionInfoDetail versionInfo)
    {
        _logger.LogInformation("正在验证游戏文件完整性...");
        
        // 验证版本JAR文件
        var jarPath = versionInfo.StorageOptions.GetVersionJarPath(versionInfo.Name);
        if (!File.Exists(jarPath))
        {
            throw new FileNotFoundException("游戏JAR文件不存在", jarPath);
        }
        
        // 验证版本JSON文件
        var jsonPath = versionInfo.StorageOptions.GetVersionJsonPath(versionInfo.Name);
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("游戏版本JSON文件不存在", jsonPath);
        }
        
        _logger.LogInformation("游戏文件完整性验证通过");
    }
    
    /// <summary>
    /// 解压原生库
    /// </summary>
    private async Task ExtractNativeLibrariesAsync(VersionInfoDetail versionInfo, GameVersionDetail gameVersionDetail)
    {
        _logger.LogInformation("正在解压原生库...");
        
        // 确保原生库目录存在
        var nativesFolder = versionInfo.NativesFolder;
        Directory.CreateDirectory(nativesFolder);
        
        // 清理旧的原生库文件
        NativeExtractorHelper.CleanNativeDirectory(nativesFolder);
        
        // 获取当前平台
        var platform = SystemHelper.GetPlatformDisplayName();
        
        // 获取所有需要的原生库
        var nativeLibraries = gameVersionDetail.Libraries.GetLibraryNativesSimples(platform, Path.Combine(versionInfo.StorageOptions.BasePath, "libraries"));
        
        // 解压每个原生库
        foreach (var nativeLib in nativeLibraries)
        {
            if (File.Exists(nativeLib.Path))
            {
                _logger.LogDebug("正在解压原生库: {NativeLibPath}", nativeLib.Path);
                await NativeExtractorHelper.ExtractNativesAsync(nativeLib.Path, nativesFolder, true);
            }
            else
            {
                _logger.LogWarning("原生库文件不存在: {NativeLibPath}", nativeLib.Path);
            }
        }
        
        _logger.LogInformation("原生库解压完成");
    }
    
    /// <summary>
    /// 确保所有依赖库都存在
    /// </summary>
    private async Task EnsureLibrariesExistAsync(VersionInfoDetail versionInfo, GameVersionDetail gameVersionDetail)
    {
        _logger.LogInformation("正在检查依赖库...");
        
        var platform = SystemHelper.GetPlatformDisplayName();
        var librariesRoot = Path.Combine(versionInfo.StorageOptions.BasePath, "libraries");
        
        // 获取所有需要的库
        var allLibraries = gameVersionDetail.Libraries.GetLibrarySimples(platform, librariesRoot);
        
        // 检查每个库是否存在
        var missingLibraries = new List<string>();
        foreach (var library in allLibraries)
        {
            if (!File.Exists(library.Path))
            {
                missingLibraries.Add(library.Name);
            }
        }
        
        if (missingLibraries.Any())
        {
            _logger.LogWarning("缺少以下依赖库: {MissingLibraries}", string.Join(", ", missingLibraries));
            // 这里可以添加自动下载缺失库的逻辑
        }
        else
        {
            _logger.LogInformation("所有依赖库都已存在");
        }
    }
    
    /// <summary>
    /// 构建进程启动信息
    /// </summary>
    private async Task<ProcessStartInfo> BuildProcessStartInfoAsync(VersionInfoDetail versionInfo, string javaPath, GameLauncherOptions options, Entities.Account.MinecraftAccount? account = null)
    {
        // 1. 获取游戏版本详情
        var gameVersionDetail = await versionInfo.GetGameVersionDetailAsync();
        if (gameVersionDetail == null)
        {
            throw new InvalidOperationException("游戏版本详情为空");
        }
        
        // 2. 构建启动参数
        var platform = SystemHelper.GetPlatformDisplayName();
        var replacements = BuildReplacementsDictionary(versionInfo, options, gameVersionDetail, account);
        
        // 3. 获取JVM参数
        var jvmArgs = new List<string>();
        
        // 添加基础JVM参数
        jvmArgs.AddRange(GetBaseJvmArguments(options, versionInfo, gameVersionDetail));
        
        // 添加版本特定的JVM参数
        jvmArgs.AddRange(gameVersionDetail.GetJvmArguments(platform, null, replacements));
        
        // 4. 获取游戏参数
        var gameArgs = new List<string>();
        gameArgs.AddRange(gameVersionDetail.GetGameArguments(platform, null, replacements));
        
        // 添加额外的游戏参数
        gameArgs.AddRange(options.ExtraGameArguments.Where(arg => !string.IsNullOrWhiteSpace(arg)));
        
        // 5. 构建完整的命令行参数
        var arguments = new List<string>();
        arguments.AddRange(jvmArgs);
        arguments.Add(gameVersionDetail.MainClass);
        arguments.AddRange(gameArgs);
        
        // 6. 创建进程启动信息
        var processStartInfo = new ProcessStartInfo
        {
            FileName = javaPath,
            Arguments = string.Join(" ", arguments.Select(EscapeArgument)),
            WorkingDirectory = versionInfo.StorageOptions.BasePath,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        return processStartInfo;
    }
    
    /// <summary>
    /// 获取基础JVM参数
    /// </summary>
    private List<string> GetBaseJvmArguments(GameLauncherOptions options, VersionInfoDetail versionInfo, GameVersionDetail gameVersionDetail)
    {
        var jvmArgs = new List<string>
        {
            // 内存参数
            $"-Xms{options.MinMemoryMB}M",
            $"-Xmx{options.MaxMemoryMB}M",
            
            // 原生库路径
            $"-Djava.library.path={versionInfo.NativesFolder}",
            
            // 游戏目录
            $"-Dminecraft.gameDir={versionInfo.StorageOptions.BasePath}",
            
            // 资源目录
            $"-Dminecraft.assetsDir={Path.Combine(versionInfo.StorageOptions.BasePath, "assets")}",
            
            // 启动器信息
            $"-Dminecraft.launcher.version=1.0.0",
            $"-Dminecraft.launcher.brand=MFToolkit"
        };
        
        // 添加GC参数
        if (options.UseZGC)
        {
            jvmArgs.Add("-XX:+UseZGC");
        }
        else if (options.UseG1GC)
        {
            jvmArgs.Add("-XX:+UseG1GC");
            jvmArgs.Add($"-XX:MaxGCPauseMillis={options.MaxGCPauseMillis}");
        }
        
        // 添加其他优化参数
        if (options.UseStringDeduplication)
        {
            jvmArgs.Add("-XX:+UseStringDeduplication");
        }
        
        if (options.OptimizeGCThreads)
        {
            jvmArgs.Add("-XX:ParallelGCThreads=0");
            jvmArgs.Add("-XX:ConcGCThreads=0");
        }
        
        if (options.UseClassDataSharing)
        {
            jvmArgs.Add("-Xshare:on");
        }
        
        // 添加通用优化参数
        jvmArgs.Add("-XX:+UnlockExperimentalVMOptions");
        jvmArgs.Add("-XX:+UnlockDiagnosticVMOptions");
        jvmArgs.Add("-XX:+AlwaysPreTouch");
        
        // 添加用户额外JVM参数
        jvmArgs.AddRange(options.ExtraJvmArguments.Where(arg => !string.IsNullOrWhiteSpace(arg)));
        
        return jvmArgs.Distinct().ToList();
    }
    
    /// <summary>
    /// 构建替换占位符的字典
    /// </summary>
    private Dictionary<string, string> BuildReplacementsDictionary(VersionInfoDetail versionInfo, GameLauncherOptions options, GameVersionDetail gameVersionDetail, Entities.Account.MinecraftAccount? account = null)
    {
        var platform = SystemHelper.GetPlatformDisplayName();
        
        // 构建类路径
        var classPath = gameVersionDetail.GetClassPath(
            platform,
            Path.Combine(versionInfo.StorageOptions.BasePath, "libraries"),
            versionInfo.StorageOptions.GetVersionJarPath(versionInfo.Name),
            null);
        
        // 获取当前时间戳
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        // 初始化替换字典，包含核心替换键
        var replacements = new Dictionary<string, string>
        {
            // 核心替换键
            { "natives_directory", versionInfo.NativesFolder },
            { "launcher_name", "MFToolkit" },
            { "launcher_version", "1.0.0" },
            { "classpath", classPath },
            { "version_name", versionInfo.Name },
            { "game_directory", versionInfo.StorageOptions.BasePath },
            { "assets_root", Path.Combine(versionInfo.StorageOptions.BasePath, "assets") },
            { "assets_index_name", gameVersionDetail.Assets },
            
            // 版本相关
            { "version_type", versionInfo.VersionType.ToString() },
            { "version_id", versionInfo.VersionId },
            
            // 窗口相关
            { "width", options.WindowWidth.ToString() },
            { "height", options.WindowHeight.ToString() },
            { "fullscreen", options.Fullscreen.ToString().ToLower() },
            
            // 其他
            { "language", options.Language },
            { "user_properties", "{}" },
            { "user_jvm_args", "[]" },
            { "launcher_brand", "MFToolkit" },
            { "timestamp", timestamp }
        };
        
        // 根据账号类型添加认证相关的替换键
        AddAuthReplacements(replacements, account);
        
        // 添加特定版本可能需要的替换键
        AddVersionSpecificReplacements(replacements, versionInfo, gameVersionDetail);
        
        return replacements;
    }
    
    /// <summary>
    /// 根据账号类型添加认证相关的替换键
    /// </summary>
    private void AddAuthReplacements(Dictionary<string, string> replacements, Entities.Account.MinecraftAccount? account)
    {
        if (account == null)
        {
            // 没有提供账号信息，使用默认的离线账号
            _logger.LogInformation("未提供账号信息，使用默认离线账号启动");
            
            // 生成默认的离线账号信息
            var playerName = "Player";
            var playerUuid = GenerateOfflineUuid(playerName);
            
            replacements.Add("auth_player_name", playerName);
            replacements.Add("auth_uuid", playerUuid.Replace("-", ""));
            replacements.Add("auth_access_token", "access_token");
            replacements.Add("user_type", "offline");
            replacements.Add("clientid", Guid.NewGuid().ToString());
            replacements.Add("auth_xuid", "");
            replacements.Add("auth_session", "token:access_token:" + playerUuid.Replace("-", ""));
            return;
        }
        
        // 根据账号类型添加不同的认证参数
        switch (account.Type)
        {
            case Entities.Account.AccountType.Offline:
                _logger.LogInformation("使用离线账号启动: {PlayerName}", account.PlayerName);
                
                replacements.Add("auth_player_name", account.PlayerName ?? "Player");
                replacements.Add("auth_uuid", account.PlayerUuid.ToString().Replace("-", ""));
                replacements.Add("auth_access_token", "access_token");
                replacements.Add("user_type", "offline");
                replacements.Add("clientid", Guid.NewGuid().ToString());
                replacements.Add("auth_xuid", "");
                replacements.Add("auth_session", "token:access_token:" + account.PlayerUuid.ToString().Replace("-", ""));
                break;
                
            case Entities.Account.AccountType.Microsoft:
            case Entities.Account.AccountType.Xbox:
                _logger.LogInformation("使用微软/Xbox账号启动: {PlayerName}", account.PlayerName);
                
                replacements.Add("auth_player_name", account.PlayerName ?? "Player");
                replacements.Add("auth_uuid", account.PlayerUuid.ToString().Replace("-", ""));
                replacements.Add("auth_access_token", account.MicrosoftAuthInfo?.AccessToken ?? account.XboxAuthInfo?.AccessToken ?? "access_token");
                replacements.Add("user_type", "microsoft");
                replacements.Add("clientid", Guid.NewGuid().ToString());
                
                // 获取Xbox ID
                var xboxId = string.Empty;
                if (account.XboxAuthInfo?.DisplayClaims?.UserInfo != null && account.XboxAuthInfo.DisplayClaims.UserInfo.Length > 0)
                {
                    xboxId = account.XboxAuthInfo.DisplayClaims.UserInfo[0].XboxId ?? string.Empty;
                }
                replacements.Add("auth_xuid", xboxId);
                
                replacements.Add("auth_session", "token:" + (account.MicrosoftAuthInfo?.AccessToken ?? account.XboxAuthInfo?.AccessToken ?? "access_token") + ":" + account.PlayerUuid.ToString().Replace("-", ""));
                break;
                
            case Entities.Account.AccountType.Mojang:
                _logger.LogInformation("使用Mojang账号启动: {PlayerName}", account.PlayerName);
                
                replacements.Add("auth_player_name", account.PlayerName ?? "Player");
                replacements.Add("auth_uuid", account.PlayerUuid.ToString().Replace("-", ""));
                replacements.Add("auth_access_token", account.MojangAuthInfo?.AccessToken ?? "access_token");
                replacements.Add("user_type", "mojang");
                replacements.Add("clientid", Guid.NewGuid().ToString());
                replacements.Add("auth_xuid", "");
                replacements.Add("auth_session", "token:" + (account.MojangAuthInfo?.AccessToken ?? "access_token") + ":" + account.PlayerUuid.ToString().Replace("-", ""));
                break;
                
            case Entities.Account.AccountType.ThirdParty:
                _logger.LogInformation("使用第三方账号启动: {PlayerName}", account.PlayerName);
                
                replacements.Add("auth_player_name", account.PlayerName ?? "Player");
                replacements.Add("auth_uuid", account.PlayerUuid.ToString().Replace("-", ""));
                replacements.Add("auth_access_token", "access_token");
                replacements.Add("user_type", "mojang");
                replacements.Add("clientid", Guid.NewGuid().ToString());
                replacements.Add("auth_xuid", "");
                replacements.Add("auth_session", "token:access_token:" + account.PlayerUuid.ToString().Replace("-", ""));
                break;
                
            default:
                _logger.LogWarning("使用未知账号类型启动: {AccountType}", account.Type);
                
                // 使用默认的离线账号信息
                replacements.Add("auth_player_name", account.PlayerName ?? "Player");
                replacements.Add("auth_uuid", account.PlayerUuid.ToString().Replace("-", ""));
                replacements.Add("auth_access_token", "access_token");
                replacements.Add("user_type", "offline");
                replacements.Add("clientid", Guid.NewGuid().ToString());
                replacements.Add("auth_xuid", "");
                replacements.Add("auth_session", "token:access_token:" + account.PlayerUuid.ToString().Replace("-", ""));
                break;
        }
    }
    
    /// <summary>
    /// 生成离线UUID
    /// </summary>
    private string GenerateOfflineUuid(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";
        
        // 使用MD5哈希生成离线UUID（符合Minecraft离线模式标准）
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes($"OfflinePlayer:{playerName}");
        var hashBytes = md5.ComputeHash(inputBytes);
        
        // 设置UUID版本和变体
        hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30); // 版本3
        hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80); // RFC4122变体
        
        return new Guid(hashBytes).ToString();
    }
    
    /// <summary>
    /// 添加版本特定的替换键
    /// </summary>
    private void AddVersionSpecificReplacements(Dictionary<string, string> replacements, VersionInfoDetail versionInfo, GameVersionDetail gameVersionDetail)
    {
        // 对于不同版本的Minecraft，可能需要不同的替换键
        // 这里可以根据版本号添加特定的替换键
        
        // 示例：为1.17+版本添加特定替换键
        if (IsVersionNewerOrEqual(versionInfo.Name, "1.17"))
        {
            replacements["auth_player_name"] = replacements["auth_player_name"];
            replacements["clientid"] = replacements["clientid"];
            replacements["auth_xuid"] = replacements["auth_xuid"];
        }
        
        // 示例：为旧版本添加特定替换键
        if (IsVersionOlder(versionInfo.Name, "1.13"))
        {
            // 旧版本可能不需要某些替换键
        }
    }
    
    /// <summary>
    /// 检查版本是否大于或等于目标版本
    /// </summary>
    private bool IsVersionNewerOrEqual(string currentVersion, string targetVersion)
    {
        try
        {
            var current = Version.Parse(currentVersion.Split('-')[0]);
            var target = Version.Parse(targetVersion);
            return current >= target;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析版本失败: {CurrentVersion} vs {TargetVersion}", currentVersion, targetVersion);
            return false;
        }
    }
    
    /// <summary>
    /// 检查版本是否小于目标版本
    /// </summary>
    private bool IsVersionOlder(string currentVersion, string targetVersion)
    {
        try
        {
            var current = Version.Parse(currentVersion.Split('-')[0]);
            var target = Version.Parse(targetVersion);
            return current < target;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析版本失败: {CurrentVersion} vs {TargetVersion}", currentVersion, targetVersion);
            return false;
        }
    }
    
    /// <summary>
    /// 配置输出重定向
    /// </summary>
    private void ConfigureOutputRedirect(Process process, OutputCallbacks outputCallbacks)
    {
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputCallbacks.OnGameOutputReceived?.Invoke(args.Data);
            }
        };
        
        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputCallbacks.OnGameErrorReceived?.Invoke(args.Data);
            }
        };
        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }
    
    /// <summary>
    /// 转义命令行参数
    /// </summary>
    private string EscapeArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
            return string.Empty;
        
        // 如果参数不包含空格或特殊字符，不需要转义
        if (!arg.Contains(' ') && !arg.Contains('"') && !arg.Contains('\'') && !arg.Contains('&'))
            return arg;
        
        // 转义双引号
        arg = arg.Replace("\"", "\\\"");
        
        // 用双引号包裹参数
        return $"\"{arg}\"";
    }
    
    #endregion
}
