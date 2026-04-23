using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Extensions.VersionExtensions;
using MFToolkit.Minecraft.Helpers;
using MFToolkit.Minecraft.Options;

namespace MFToolkit.Minecraft.Services.Launcher.Internals;

/// <summary>
/// 版本处理器
/// </summary>
internal class VersionHandler
{
    private readonly VersionInfoDetail _versionInfoDetail;
    private readonly GameVersionDetail _gameVersionDetail;
    private readonly GameLauncherOptions _launcherOptions;

    /// <param name="versionInfoDetail">版本信息详情</param>
    /// <param name="gameVersionDetail">游戏版本详情</param>
    /// <param name="launcherOptions">启动配置选项</param>
    public VersionHandler(VersionInfoDetail versionInfoDetail, GameVersionDetail gameVersionDetail, GameLauncherOptions launcherOptions)
    {
        _versionInfoDetail = versionInfoDetail;
        _gameVersionDetail = gameVersionDetail;
        _launcherOptions = launcherOptions;
    }

    /// <summary>
    /// 构建Java启动参数
    /// </summary>
    /// <returns>完整的Java启动参数字符串</returns>
    internal string BuildJavaArguments()
    {
        var args = new List<string>
        {
            // 内存设置
            $"-Xms{_launcherOptions.MinMemoryMB}M",
            $"-Xmx{_launcherOptions.MaxMemoryMB}M",
            
            // 类路径
            $"-cp {BuildClassPath()}",
            
            // 原生库路径
            $"-Djava.library.path={_versionInfoDetail.NativesFolder}",
            
            // JNA临时文件目录
            $"-Djna.tmpdir={_versionInfoDetail.NativesFolder}",
            
            // 游戏目录
            $"-Dminecraft.gameDir={_versionInfoDetail.StorageOptions.BasePath}",
            
            // 资源目录
            $"-Dminecraft.assetsDir={Path.Combine(_versionInfoDetail.StorageOptions.BasePath, "assets")}",
            
            // 基础优化参数
            "-XX:+UseG1GC",
            "-XX:MaxGCPauseMillis=200",
            "-XX:+UnlockExperimentalVMOptions",
            "-XX:+UseStringDeduplication",
            
            // 启动器信息
            $"-Dminecraft.launcher.version=1.0.0",
            $"-Dminecraft.launcher.brand=MFToolkit"
        };
        
        // 添加额外的JVM参数
        if (_launcherOptions.ExtraJvmArguments != null && _launcherOptions.ExtraJvmArguments.Any())
        {
            args.AddRange(_launcherOptions.ExtraJvmArguments);
        }
        
        // 根据操作系统添加特定参数
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            args.Add("-Dorg.lwjgl.librarypath=" + _versionInfoDetail.NativesFolder);
        }
        
        return string.Join(" ", args);
    }

    /// <summary>
    /// 构建完整的启动命令
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>完整的启动命令</returns>
    internal string BuildLaunchCommand(string javaPath)
    {
        var jvmArgs = BuildJavaArguments();
        var gameArgs = BuildGameArguments();
        
        return $"{javaPath} {jvmArgs} {_gameVersionDetail.MainClass} {gameArgs}";
    }
    
    /// <summary>
    /// 构建游戏启动参数
    /// </summary>
    /// <returns>游戏启动参数字符串</returns>
    internal string BuildGameArguments()
    {
        var platform = SystemHelper.GetPlatformDisplayName();
        var replacements = BuildReplacementsDictionary();
        
        // 获取游戏参数
        var gameArgs = _gameVersionDetail.GetGameArguments(platform, null, replacements);
        
        // 添加额外的游戏参数
        if (_launcherOptions.ExtraGameArguments != null && _launcherOptions.ExtraGameArguments.Any())
        {
            gameArgs = gameArgs.Concat(_launcherOptions.ExtraGameArguments);
        }
        
        return string.Join(" ", gameArgs.Select(EscapeArgument));
    }

    #region 支持方法

    /// <summary>
    /// 构建类路径
    /// </summary>
    /// <returns>类路径字符串</returns>
    private string BuildClassPath()
    {
        var paths = new List<string>
        {
            // 主JAR路径：base/versions/<版本>/<版本>.jar
            _versionInfoDetail.StorageOptions.GetVersionJarPath(_versionInfoDetail.Name)
        };
        
        var libraries = _gameVersionDetail.Libraries.GetLibrarySimples(SystemHelper.GetPlatformDisplayName(), _versionInfoDetail.BasePath);
        paths.AddRange(libraries.Select(lib => lib.Path));
        
        // 根据操作系统使用不同的路径分隔符
        return string.Join(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":",
            paths
        );
    }
    
    /// <summary>
    /// 构建替换占位符的字典
    /// </summary>
    /// <returns>替换字典</returns>
    private Dictionary<string, string> BuildReplacementsDictionary()
    {
        var platform = SystemHelper.GetPlatformDisplayName();
        
        // 构建替换字典
        var replacements = new Dictionary<string, string>
        {
            { "natives_directory", _versionInfoDetail.NativesFolder },
            { "launcher_name", "MFToolkit" },
            { "launcher_version", "1.0.0" },
            { "classpath", BuildClassPath() },
            { "version_name", _versionInfoDetail.Name },
            { "game_directory", _versionInfoDetail.StorageOptions.BasePath },
            { "assets_root", Path.Combine(_versionInfoDetail.StorageOptions.BasePath, "assets") },
            { "assets_index_name", _gameVersionDetail.Assets },
            { "auth_player_name", "Player" }, // 实际应该从账号服务获取
            { "auth_uuid", Guid.NewGuid().ToString() }, // 实际应该从账号服务获取
            { "auth_access_token", "access_token" }, // 实际应该从账号服务获取
            { "user_type", "mojang" },
            { "version_type", _versionInfoDetail.VersionType.ToString() },
            { "width", _launcherOptions.WindowWidth.ToString() },
            { "height", _launcherOptions.WindowHeight.ToString() },
            { "fullscreen", _launcherOptions.Fullscreen.ToString().ToLower() },
            { "language", _launcherOptions.Language }
        };
        
        return replacements;
    }
    
    /// <summary>
    /// 转义命令行参数
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>转义后的参数</returns>
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
