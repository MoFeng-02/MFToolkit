using System.Text;
using MFToolkit.Minecraft.Entities.GameVersion;

namespace MFToolkit.Minecraft.Extensions.VersionExtensions;

/// <summary>
/// 启动参数相关的扩展方法
/// </summary>
public static class LaunchArgumentsExtensions
{
    /// <summary>
    /// 获取完整的JVM启动参数
    /// </summary>
    public static IEnumerable<string> GetJvmArguments(this GameVersionDetail version, string os, 
        Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        // 新版本格式
        if (version.Arguments?.Jvm != null && version.Arguments.Jvm.Count > 0)
        {
            foreach (var arg in version.Arguments.Jvm)
            {
                foreach (var value in ProcessArgument(arg, os, features, replacements))
                {
                    yield return value;
                }
            }
            yield break;
        }

        // 旧版本默认参数
        yield return ReplacePlaceholders("-Djava.library.path=${natives_directory}", replacements);
        yield return "-cp";
        yield return ReplacePlaceholders("${classpath}", replacements);
    }

    /// <summary>
    /// 获取完整的游戏启动参数
    /// </summary>
    public static IEnumerable<string> GetGameArguments(this GameVersionDetail version, string os,
        Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        // 旧版本格式
        if (!string.IsNullOrEmpty(version.MinecraftArguments))
        {
            foreach (var arg in version.MinecraftArguments.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                yield return ReplacePlaceholders(arg, replacements);
            }
            yield break;
        }

        // 新版本格式
        if (version.Arguments?.Game != null)
        {
            foreach (var arg in version.Arguments.Game)
            {
                foreach (var value in ProcessArgument(arg, os, features, replacements))
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// 获取完整的启动命令（JVM + 主类 + 游戏参数）
    /// </summary>
    public static string GetLaunchCommand(this GameVersionDetail version, string os,
        Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        var args = new List<string>();

        // JVM 参数
        args.AddRange(version.GetJvmArguments(os, features, replacements));
        
        // 主类
        args.Add(version.MainClass);
        
        // 游戏参数
        args.AddRange(version.GetGameArguments(os, features, replacements));

        return string.Join(" ", args.Select(arg => EscapeArgument(arg)));
    }

    /// <summary>
    /// 获取类路径字符串
    /// </summary>
    public static string GetClassPath(this GameVersionDetail version, string os, 
        string librariesDir, string versionJarPath, Dictionary<string, bool>? features = null)
    {
        var paths = new List<string> { versionJarPath };

        // 添加所有适用的库
        foreach (var library in version.GetNormalLibraries(os, features))
        {
            if (library.Downloads?.Artifact != null)
            {
                var libraryPath = Path.Combine(librariesDir, library.GetLibraryPath());
                paths.Add(libraryPath);
            }
        }

        return string.Join(Path.PathSeparator.ToString(), paths);
    }

    private static IEnumerable<string> ProcessArgument(object arg, string os, 
        Dictionary<string, bool>? features, Dictionary<string, string>? replacements)
    {
        switch (arg)
        {
            case string strArg:
                yield return ReplacePlaceholders(strArg, replacements);
                break;

            case RuleBasedArgument ruleArg:
                if (ruleArg.Matches(os, features))
                {
                    foreach (var value in ruleArg.GetValues())
                    {
                        yield return ReplacePlaceholders(value, replacements);
                    }
                }
                break;
        }
    }

    private static string ReplacePlaceholders(string input, Dictionary<string, string>? replacements)
    {
        if (replacements == null || string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder(input);
        foreach (var replacement in replacements)
        {
            result.Replace("${" + replacement.Key + "}", replacement.Value);
        }
        return result.ToString();
    }

    private static string EscapeArgument(string arg)
    {
        return arg.Contains(' ') ? $"\"{arg}\"" : arg;
    }
}