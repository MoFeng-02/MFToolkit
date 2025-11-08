# Minecraft 下载服务文档

## 概述
这是一个完整的 Minecraft 原版游戏下载服务实现，支持多镜像源、并行下载、文件验证和进度跟踪等功能。
并且支持Mod Loader版本下载拓展，只需要继承

## 功能特性

### ✅ 核心功能
- 完整的下载流程：版本清单 → 版本JSON → 客户端JAR → 资源文件 → 库文件
- 多镜像源支持：官方源、BMCLAPI等
- 智能文件管理：文件存在性检查、哈希验证、自动跳过重复下载
- 并发下载控制：可配置的最大并发线程数
- 进度跟踪：实时下载进度和状态报告

### ✅ 下载管理
- 启动下载：StartDownloadAsync
- 暂停下载：保留已下载文件，可恢复
- 取消下载：删除所有已下载文件
- 文件验证：SHA1哈希和文件大小验证
- 安装状态检查：检查版本是否完整安装

### ✅ 事件系统
- 进度变化：实时下载进度更新
- 状态变化：下载状态变更通知
- 下载完成：包含完整文件信息的回调

## 快速开始

### 1. 服务注册
```csharp
// 在依赖注入容器中注册服务
services.AddKeySingleton<IMinecraftDownloadService, MinecraftDownloadService>();
```

### 2. 基本使用
```csharp
public class MinecraftManager
{
    private readonly IMinecraftDownloadService _downloadService;

    public MinecraftManager(IMinecraftDownloadService downloadService)
    {
        _downloadService = downloadService;

        // 订阅事件
        _downloadService.ProgressChanged += OnDownloadProgress;
        _downloadService.DownloadCompleted += OnDownloadCompleted;
        _downloadService.StatusChanged += OnStatusChanged;
    }

    // 开始下载
    public async Task DownloadVersionAsync(string versionId)
    {
        var success = await _downloadService.StartDownloadAsync(versionId);
        if (success)
        {
            Console.WriteLine($"版本 {versionId} 下载完成");
        }
    }

    // 暂停下载
    public async Task PauseDownloadAsync()
    {
        await _downloadService.PauseDownloadAsync();
    }

    // 取消下载
    public async Task CancelDownloadAsync()
    {
        await _downloadService.CancelDownloadAsync();
    }

    // 验证文件
    public async Task ValidateVersionAsync(string versionId, StorageOptions storageOptions)
    {
        var result = await _downloadService.ValidateGameFilesAsync(versionId, storageOptions);
        Console.WriteLine($"验证结果: {result.Message}");
    }

    // 事件处理
    private void OnDownloadProgress(DownloadProgress progress)
    {
        Console.WriteLine($"进度: {progress.OverallProgress:F1}% - {progress.CurrentFile}");
    }

    private void OnDownloadCompleted(DownloadCompletedResult result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"下载完成! 共下载 {result.FileCount} 个文件，总大小 {result.TotalSize} 字节");
        }
        else
        {
            Console.WriteLine($"下载失败: {result.ErrorMessage}");
        }
    }

    private void OnStatusChanged(DownloadStatus status)
    {
        Console.WriteLine($"状态变更: {status}");
    }
}
```

## 配置说明

### DownloadOptions 配置
```json
{
    "mirrors": {
        "currentMirror": "bmclapi",
        "mirrorConfigs": {
            "official": {
                "baseUrl": "",
                "urlMappings": {
                    "https://launcher.mojang.com": "https://launcher.mojang.com",
                    "https://libraries.minecraft.net": "https://libraries.minecraft.net",
                    "https://resources.download.minecraft.net": "https://resources.download.minecraft.net"
                }
            },
            "bmclapi": {
                "baseUrl": "https://bmclapi2.bangbang93.com",
                "urlMappings": {
                    "https://launcher.mojang.com": "",
                    "https://libraries.minecraft.net": "/libraries",
                    "https://resources.download.minecraft.net": "/assets"
                }
            }
        }
    },
    "settings": {
        "maxDownloadThreads": 5,
        "maxRetryCount": 3,
        "retryDelayMs": 1000,
        "downloadTimeoutSeconds": 60,
        "enableResume": true,
        "enableParallelDownload": true,
        "speedLimitKb": 0
    }
}
```

### StorageOptions 配置
```csharp
var storageOptions = new StorageOptions
{
    BasePath = ".minecraft",
    StorageMode = StorageMode.Global
};
```

## 高级用法

### 1. 自定义镜像源
```csharp
// 添加自定义镜像源
_downloadOptions.Mirrors.AddCustomMirror(
    "custom-mirror",
    "https://mirror.example.com",
    new Dictionary<string, string>
    {
        ["https://launcher.mojang.com"] = "",
        ["https://libraries.minecraft.net"] = "/libraries"
    }
);

// 切换镜像源
_downloadOptions.SetCurrentMirror("custom-mirror");
```

### 2. 监控下载状态
```csharp
// 获取当前活动下载任务
var activeDownloads = ((VanillaDownloadService)_downloadService).GetActiveDownloads();
foreach (var task in activeDownloads)
{
    Console.WriteLine($"{task.FileName} - {task.Status} - {task.DownloadedBytes}/{task.Size}");
}

// 获取当前下载版本
var currentVersion = ((VanillaDownloadService)_downloadService).GetCurrentDownloadVersion();
```

### 3. 自定义存储路径
```csharp
var customStorage = new StorageOptions
{
    BasePath = "D:\Games\Minecraft",
    StorageMode = StorageMode.Isolated
};

await _downloadService.StartDownloadAsync("1.20.1", customStorage);
```

## 错误处理

### 常见异常处理
```csharp
try
{
    await _downloadService.StartDownloadAsync("1.20.1");
}
catch (OperationCanceledException)
{
    // 下载被用户取消或暂停
    Console.WriteLine("下载已取消");
}
catch (HttpRequestException ex)
{
    // 网络错误
    Console.WriteLine($"网络错误: {ex.Message}");
}
catch (IOException ex)
{
    // 文件操作错误
    Console.WriteLine($"文件操作错误: {ex.Message}");
}
catch (Exception ex)
{
    // 其他错误
    Console.WriteLine($"下载失败: {ex.Message}");
}
```

### 验证失败处理
```csharp
var validationResult = await _downloadService.ValidateGameFilesAsync("1.20.1", storageOptions);
if (!validationResult.IsValid)
{
    foreach (var invalidFile in validationResult.InvalidFiles)
    {
        Console.WriteLine($"无效文件: {invalidFile.FilePath}");
        Console.WriteLine($"错误: {invalidFile.Error}");

        // 可以选择重新下载无效文件
        if (invalidFile.Error.Contains("文件不存在"))
        {
            // 重新下载缺失文件
        }
    }
}
```

## 性能优化建议

### 1. 调整并发设置
```csharp
_downloadOptions.Settings.MaxDownloadThreads = Environment.ProcessorCount * 2;
_downloadOptions.Settings.EnableParallelDownload = true;
```

### 2. 网络优化
```csharp
// 配置 HttpClient 超时和重试策略
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(_downloadOptions.Settings.DownloadTimeoutSeconds)
};
```

### 3. 存储优化
```csharp
// 使用 SSD 存储提高IO性能
var storageOptions = new StorageOptions
{
    BasePath = "D:\SSD\Minecraft",  // SSD 路径
    StorageMode = StorageMode.Global
};
```

## 扩展开发

### 实现自定义下载服务
```csharp
public class CustomDownloadService : IMinecraftDownloadService
{
    public ModLoaderType ModLoaderType { get; set; }

    public event Action<DownloadProgress>? ProgressChanged;
    public event Action<DownloadStatus>? StatusChanged;
    public event Action<DownloadCompletedResult>? DownloadCompleted;

    public Task<bool> StartDownloadAsync(string versionId, StorageOptions? storageOptions = null, CancellationToken cancellationToken = default)
    {
        // 实现自定义下载逻辑
    }

    // 实现其他接口方法...
}
```

## 注意事项

### 1. 文件权限
- 确保应用程序有写入存储目录的权限
- 在 Linux/macOS 上注意文件权限设置

### 2. 网络环境
- 在企业网络环境中可能需要配置代理
- 某些网络可能阻止对 Mojang 服务器的访问

### 3. 磁盘空间
- 下载前检查可用磁盘空间
- 完整版本可能需要 1-2GB 空间

### 4. 并发限制
- 避免过多的并发下载，以免被服务器限制
- 根据网络状况调整并发线程数

## 故障排除

### 常见问题

#### 下载速度慢
- 尝试切换镜像源
- 检查网络连接
- 调整并发线程数

#### 文件验证失败
- 检查磁盘空间
- 验证网络稳定性
- 重新下载失败的文件

#### 内存使用过高
- 减少并发线程数
- 增加下载超时时间
- 监控大文件下载

## 技术支持

如果遇到问题，请检查：
- 网络连接状态
- 磁盘空间和权限
- 配置文件格式
- 日志输出信息

这个下载服务提供了完整的 Minecraft 版本管理功能，可以轻松集成到各种启动器和管理工具中。
