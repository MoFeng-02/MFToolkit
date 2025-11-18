using MFToolkit.Minecraft.Entities.GameVersion;

namespace MFToolkit.Minecraft.Services.Launcher.Interfaces;

/// <summary>
/// 启动器服务接口
/// </summary>
public interface ILauncherService
{
    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="detailSimple">简略信息</param>
    /// <returns>进程ID</returns>
    Task<int> LauncherAsync(VersionInfoDetail detailSimple);
}