using System.Diagnostics;
using MFToolkit.Minecraft.Entities.GameVersion;

namespace MFToolkit.Minecraft.Services.Launcher.Interfaces;

/// <summary>
/// Minecraft 启动器服务接口
/// </summary>
public interface ILauncherService
{
    /// <summary>
    /// 启动 Minecraft 游戏
    /// </summary>
    /// <param name="versionInfo">版本信息</param>
    /// <param name="account">Minecraft账号信息，用于获取认证参数</param>
    /// <param name="callbacks">基础回调配置</param>
    /// <param name="outputCallbacks">输出重定向回调配置（设置此参数将自动启用输出重定向）</param>
    /// <returns>启动结果</returns>
    /// <remarks>
    /// 以下示例展示如何处理启动结果：
    /// <code>
    /// var versionInfo = new VersionInfoDetail("1.20.1");
    /// var account = await authService.GetCurrentAccountAsync();
    /// var result = await launcherService.LaunchGameAsync(versionInfo, account);
    /// 
    /// if (result.Success)
    /// {
    ///     // 游戏启动成功，可以记录进程ID
    ///     logger.LogInformation("游戏已启动，进程ID: {ProcessId}", result.ProcessId);
    /// }
    /// else
    /// {
    ///     // 处理启动失败情况
    ///     logger.LogError("启动失败: {ErrorMessage}", result.ErrorMessage);
    ///     if (result.Exception != null)
    ///     {
    ///         logger.LogError(result.Exception, "启动异常");
    ///     }
    /// }
    /// </code>
    /// 回调使用示例：
    /// <code>
    /// // 只使用基础回调
    /// var result1 = await launcher.LaunchGameAsync(versionInfo, account,
    ///     new LaunchCallbacks(OnGameStarted: p => Console.WriteLine("启动")));
    /// 
    /// // 使用输出重定向回调
    /// var result2 = await launcher.LaunchGameAsync(versionInfo, account,
    ///     new LaunchCallbacks(OnGameStarted: p => Console.WriteLine("启动")),
    ///     new OutputCallbacks(
    ///         output => Console.WriteLine($"输出: {output}"),
    ///         error => Console.WriteLine($"错误: {error}")
    ///     ));
    /// </code>
    /// </remarks>
    Task<LaunchResult> LaunchGameAsync(
        VersionInfoDetail versionInfo,
        Entities.Account.MinecraftAccount? account = null,
        LaunchCallbacks? callbacks = null,
        OutputCallbacks? outputCallbacks = null);

    /// <summary>
    /// 关闭 Minecraft 游戏
    /// </summary>
    /// <param name="processId">游戏进程ID</param>
    /// <returns>是否成功关闭游戏</returns>
    Task<bool> CloseGameAsync(int processId);

    /// <summary>
    /// 检查游戏是否正在运行
    /// </summary>
    /// <param name="processId">游戏进程ID</param>
    /// <returns>游戏是否正在运行</returns>
    Task<bool> IsGameRunningAsync(int processId);
}

/// <summary>
/// Minecraft 游戏启动结果
/// </summary>
/// <param name="Success">启动是否成功</param>
/// <param name="ProcessId">游戏进程ID，启动失败时为0或-1</param>
/// <param name="ErrorMessage">错误信息，成功时为null</param>
/// <param name="Exception">异常信息，成功时为null</param>
public record LaunchResult(
    bool Success,
    int ProcessId,
    string? ErrorMessage = null,
    Exception? Exception = null
)
{
    /// <summary>
    /// 创建成功启动结果
    /// </summary>
    /// <param name="processId">游戏进程ID</param>
    /// <returns>成功启动结果</returns>
    public static LaunchResult SuccessResult(int processId)
        => new(true, processId);

    /// <summary>
    /// 创建失败启动结果
    /// </summary>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="exception">异常信息</param>
    /// <returns>失败启动结果</returns>
    public static LaunchResult FailureResult(string errorMessage, Exception? exception = null)
        => new(false, -1, errorMessage, exception);

    /// <summary>
    /// 创建失败启动结果（基于异常）
    /// </summary>
    /// <param name="exception">异常信息</param>
    /// <returns>失败启动结果</returns>
    public static LaunchResult FailureResult(Exception exception)
        => new(false, -1, exception.Message, exception);

    /// <summary>
    /// 检查游戏是否正在运行（仅对成功启动有效）
    /// </summary>
    /// <returns>游戏进程是否可能存在</returns>
    /// <remarks>
    /// 注意：此方法仅检查进程ID是否有效，不实际验证进程是否存在
    /// </remarks>
    public bool IsProcessLikelyRunning()
        => Success && ProcessId > 0;
}

/// <summary>
/// 基础启动回调配置
/// </summary>
/// <param name="OnGameStarting">游戏启动前回调</param>
/// <param name="OnGameStarted">游戏启动后回调</param>
/// <param name="OnGameExited">游戏退出回调</param>
public record LaunchCallbacks(
    Action<ProcessStartInfo>? OnGameStarting = null,
    Action<Process>? OnGameStarted = null,
    Action<int, DateTime>? OnGameExited = null
);

/// <summary>
/// 输出重定向回调配置
/// </summary>
/// <param name="OnGameOutputReceived">游戏输出回调（标准输出）</param>
/// <param name="OnGameErrorReceived">游戏错误回调（标准错误）</param>
/// <remarks>
/// 设置此回调配置将自动启用进程输出重定向。
/// 输出数据可能包含游戏日志、调试信息等。
/// </remarks>
public record OutputCallbacks(
    Action<string>? OnGameOutputReceived = null,
    Action<string>? OnGameErrorReceived = null
);