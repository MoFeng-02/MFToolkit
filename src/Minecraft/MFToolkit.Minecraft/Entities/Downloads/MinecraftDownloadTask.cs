using System.ComponentModel;
using System.Runtime.CompilerServices;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Entities.Downloads;

/// <summary>
/// Minecraft下载任务
/// </summary>
public class MinecraftDownloadTask : INotifyPropertyChanged
{
    private Guid _id = Guid.NewGuid();
    private string _versionId = string.Empty;
    private long _size;
    private string _sha1 = string.Empty;
    private string _originUrl = string.Empty;
    private string _downloadUrl = string.Empty;
    private string _name = string.Empty;
    private string _savePath = string.Empty;
    private MinecraftFileType _minecraftFileType;
    private DownloadStatus _downloadStatus;
    private DownloadPriority _priority = DownloadPriority.Normal;
    private int _retryCount;
    private int _maxRetries = 3;
    private DateTime _createdTime = DateTime.Now;
    private DateTime? _lastUpdatedTime;
    private string? _localFilePath;
    private bool _isVerified;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// 下载任务ID
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    /// <summary>
    /// 版本所属ID
    /// </summary>
    public required string VersionId
    {
        get => _versionId;
        set => SetField(ref _versionId, value);
    }

    /// <summary>
    /// 当前文件大小
    /// </summary>
    public long Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    /// <summary>
    /// 当前文件sha1值
    /// </summary>
    public string Sha1
    {
        get => _sha1;
        set => SetField(ref _sha1, value);
    }

    /// <summary>
    /// 源下载Url
    /// </summary>
    public required string OriginUrl
    {
        get => _originUrl;
        set => SetField(ref _originUrl, value);
    }

    /// <summary>
    /// 实际下载Url（即可能是镜像源下载所以可能替换）
    /// </summary>
    public required string DownloadUrl
    {
        get => _downloadUrl;
        set => SetField(ref _downloadUrl, value);
    }

    /// <summary>
    /// 文件名称
    /// </summary>
    public required string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// 本地保存路径
    /// </summary>
    public required string SavePath
    {
        get => _savePath;
        set => SetField(ref _savePath, value);
    }

    /// <summary>
    /// 所属文件类型
    /// </summary>
    public MinecraftFileType MinecraftFileType
    {
        get => _minecraftFileType;
        set => SetField(ref _minecraftFileType, value);
    }

    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadStatus DownloadStatus
    {
        get => _downloadStatus;
        set => SetField(ref _downloadStatus, value);
    }

    /// <summary>
    /// 优先级
    /// </summary>
    public DownloadPriority Priority
    {
        get => _priority;
        set => SetField(ref _priority, value);
    }

    /// <summary>
    /// 已重试次数
    /// </summary>
    public int RetryCount
    {
        get => _retryCount;
        set => SetField(ref _retryCount, value);
    }

    /// <summary>
    /// 最大重试次数（实际按DownloadOptions里面的来提供）
    /// </summary>
    public int MaxRetries
    {
        get => _maxRetries;
        set => SetField(ref _maxRetries, value);
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime
    {
        get => _createdTime;
        set => SetField(ref _createdTime, value);
    }

    /// <summary>
    /// 最新更新时间
    /// </summary>
    public DateTime? LastUpdatedTime
    {
        get => _lastUpdatedTime;
        set => SetField(ref _lastUpdatedTime, value);
    }

    /// <summary>
    /// 实际保存地址
    /// </summary>
    public string? LocalFilePath
    {
        get => _localFilePath;
        set => SetField(ref _localFilePath, value);
    }

    /// <summary>
    /// 验证是否通过
    /// </summary>
    public bool IsVerified
    {
        get => _isVerified;
        set => SetField(ref _isVerified, value);
    }

    /// <summary>
    /// 设置返回进度表
    /// </summary>
    /// <returns></returns>
    public MinecraftDownloadProgress SetDownloadProgress()
    {
        return new MinecraftDownloadProgress()
        {
            Name = Name,
            DownloadStatus = DownloadStatus,
            RetryCount = RetryCount,
            MaxRetries = MaxRetries,
            CreatedTime = CreatedTime,
            LastUpdatedTime = LastUpdatedTime,
            LocalFilePath = LocalFilePath,
            IsVerified = IsVerified,
            Priority = Priority,
            SavePath = SavePath,
            MinecraftFileType = MinecraftFileType,
            DownloadUrl = DownloadUrl,
            OriginUrl = OriginUrl,
            VersionId = VersionId,
            Size = Size,
            Sha1 = Sha1,
            Id = Id,
        };
    }

    public MinecraftVersionAllFilePath SetDownloadVersionAllFilePath(Library? library = null)
    {
        return new MinecraftVersionAllFilePath()
        {
            Name = Name,
            DownloadStatus = DownloadStatus,
            RetryCount = RetryCount,
            MaxRetries = MaxRetries,
            CreatedTime = CreatedTime,
            LastUpdatedTime = LastUpdatedTime,
            LocalFilePath = LocalFilePath,
            IsVerified = IsVerified,
            Priority = Priority,
            SavePath = SavePath,
            MinecraftFileType = MinecraftFileType,
            DownloadUrl = DownloadUrl,
            OriginUrl = OriginUrl,
            VersionId = VersionId,
            Size = Size,
            Sha1 = Sha1,
            Id = Id
        };
    }
}