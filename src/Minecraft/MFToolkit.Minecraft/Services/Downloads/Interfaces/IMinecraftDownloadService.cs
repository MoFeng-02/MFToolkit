using System.Collections.ObjectModel;
using MFToolkit.Minecraft.Entities.Downloads;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Downloads.Interfaces;

/// <summary>
/// Minecraft统一下载服务接口
/// </summary>
public interface IMinecraftDownloadService : IDisposable
{
    /// <summary>
    /// 主要用于加载器的，有的时候符合分配加载器处理上面需要用到这个属性
    /// </summary>
    ModLoaderType ModLoaderType { get; set; }

    /// <summary>
    /// 下载进度变化事件，如果有异常会在进度的第二个属性异常中显示
    /// </summary>
    event Action<MinecraftDownloadProgress, Exception?>? ProgressChanged;

    /// <summary>
    /// 下载完成事件 - 当所有文件下载完成后触发，包含所有下载文件的详细信息
    /// </summary>
    event Action<MinecraftDownloadCompletedResult>? DownloadCompleted;

    /// <summary>
    /// 只在正确下载完成后调用，传递各方位信息，和DownloadCompleted不同
    /// </summary>
    public Action<VersionInfoDetail>? CompletedInfoAction { get; set; }

    // /// <summary>
    // /// 下载错误事件 - 当下载过程中发生错误时触发
    // /// </summary>
    // event Action<MinecraftDownloadError>? DownloadError;

    // /// <summary>
    // /// UI线程执行事件 - 用于在UI线程上执行下载进度更新等操作
    // /// </summary>
    // event Action<Action>? ExecuteOnUiThread;

    /// <summary>
    /// 获取当前下载队列
    /// <remarks>
    /// 注意：此集合为线程安全实现，可在任意线程访问
    /// </remarks>
    /// </summary>
    ObservableCollection<MinecraftDownloadProgress> DownloadQueue { get; }

    /// <summary>
    /// 获取或设置存储选项
    /// </summary>
    IOptionsMonitor<StorageOptions> StorageOptions { get; set; }

    /// <summary>
    /// 获取或设置下载选项
    /// </summary>
    IOptionsMonitor<DownloadOptions> DownloadOptions { get; set; }

    /// <summary>
    /// 获取版本所有下载列表
    /// </summary>
    Dictionary<string, List<MinecraftVersionAllFilePath>> DictionaryVersionPaths { get; set; }

    /// <summary>
    /// 通过版本Id获取指定的下载表
    /// </summary>
    /// <param name="versionId"></param>
    /// <returns></returns>
    public List<MinecraftVersionAllFilePath> GetVersionPaths(string versionId);

    #region 下载控制方法

    /// <summary>
    /// 启动下载任务
    /// </summary>
    /// <param name="versionInfo">版本</param>
    /// <param name="customName">自定义名称(存储版本json和版本文件的名称，不需要后缀)</param>
    /// <param name="storageOptions">存储选项</param>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>是否成功启动</returns>
    Task<bool> StartDownloadAsync(VersionInfo versionInfo,
        string? customName = null,
        StorageOptions? storageOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量启动下载任务
    /// </summary>
    /// <param name="versionInfos">版本列表</param>
    /// <param name="customNames">自定义名称列表，需要和版本列表下标对应(存储版本json和版本文件的名称，不需要后缀)</param>
    /// <param name="storageOptions">存储选项</param>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>成功启动的版本数量</returns>
    Task<int> StartBatchDownloadAsync(IEnumerable<VersionInfo> versionInfos,
        IEnumerable<string>? customNames = null,
        StorageOptions? storageOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 暂停下载任务 - 统一暂停，暂停所有正在进行的下载任务和所有排队中的下载任务，暂停后可以通过StartDownloadAsync方法重新启动下载，继续下载未完成的部分
    /// </summary>
    /// <returns>是否成功暂停</returns>
    Task<bool> PauseDownloadAsync();

    /// <summary>
    /// 暂停指定版本的下载任务
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>是否成功暂停</returns>
    Task<bool> PauseDownloadAsync(string versionId);

    /// <summary>
    /// 取消下载任务，取消所有正在进行的下载任务和所有排队中的下载任务，并且删除已下载的文件
    /// </summary>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>是否成功取消</returns>
    Task<bool> CancelDownloadAsync();

    /// <summary>
    /// 取消指定版本的下载任务，并删除已下载的文件
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>是否成功取消</returns>
    Task<bool> CancelDownloadAsync(string versionId);

    #endregion

    #region 验证和检查方法

    /// <summary>
    /// 验证游戏文件完整性
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="storageOptions">存储选项</param>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>验证结果</returns>
    Task<MinecraftValidationFileResult> ValidateGameFilesAsync(string versionId, StorageOptions storageOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查游戏版本是否已安装
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="storageOptions">存储选项</param>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>是否已安装</returns>
    Task<bool> IsVersionInstalledAsync(string versionId, StorageOptions storageOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取下载进度信息
    /// </summary>
    /// <returns>当前的下载进度信息</returns>
    MinecraftDownloadProgress GetDownloadProgress();

    /// <summary>
    /// 获取指定版本的下载进度信息
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>指定版本的下载进度信息</returns>
    MinecraftDownloadProgress GetDownloadProgress(string versionId);

    /// <summary>
    /// 检查是否支持断点续传
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="storageOptions">存储选项</param>
    /// <param name="cancellationToken">任务令牌</param>
    /// <returns>是否支持断点续传</returns>
    Task<bool> CanResumeDownloadAsync(string versionId, StorageOptions storageOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置下载任务优先级
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="priority">新的优先级</param>
    /// <returns>是否成功设置</returns>
    Task<bool> SetDownloadPriorityAsync(string versionId, DownloadPriority priority);

    #endregion
}