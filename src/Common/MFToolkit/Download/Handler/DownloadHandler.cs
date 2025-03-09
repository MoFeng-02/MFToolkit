using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using MFToolkit.Download.Constant;
using MFToolkit.Download.Models;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;

namespace MFToolkit.Download.Handler;

/// <summary>
/// 下载管理器
/// 负责批量下载任务的状态管理，支持按日期归档
/// </summary>
public class DownloadHandler
{
    /// <summary>
    /// 暂停信息存储目录路径
    /// </summary>
    protected virtual string SaveFolderPauseinfoPath { get; set; } = Path.Combine(DownloadConstant.SaveFolderPath, "Pauseinfo");

    /// <summary>
    /// 加密密钥（默认与DownloadPauseInfoHandler保持一致）
    /// </summary>
    private readonly string _encryptionKey = "otEwyom+/bgjiUzyMio8aJfqfL44h5Xu";

    /// <summary>
    /// 文件缓存（提升读取性能）
    /// </summary>
    private static readonly ConcurrentDictionary<string, List<DownloadModel>> _fileCache =
        new ConcurrentDictionary<string, List<DownloadModel>>();

    /// <summary>
    /// 根据下载URL查询暂停信息
    /// </summary>
    /// <param name="downloadUrl">文件下载地址</param>
    /// <param name="saveTime">指定查询日期（null时查询全部）</param>
    /// <returns>匹配的下载信息或null</returns>
    public virtual async Task<DownloadModel?> GetPauseInfoAsync(string downloadUrl, DateTime? saveTime = null)
    {
        if (!Directory.Exists(SaveFolderPauseinfoPath)) return null;

        // 构建目标文件路径
        string? queryTimeName = saveTime != null ? $"UTC_{saveTime:yyyyMMdd}.bin" : null;
        var filePath = queryTimeName == null ? SaveFolderPauseinfoPath : Path.Combine(SaveFolderPauseinfoPath, queryTimeName);

        if (queryTimeName != null)
        {
            // 指定日期的精确查询
            return await FindInFile(filePath, downloadUrl);
        }
        else
        {
            // 遍历所有日期文件查询
            var files = Directory.GetFiles(SaveFolderPauseinfoPath).Where(q => q.EndsWith(".bin"));
            foreach (var file in files)
            {
                var result = await FindInFile(file, downloadUrl);
                if (result != null) return result;
            }
            return null;
        }
    }

    /// <summary>
    /// 保存下载任务状态（按UTC日期归档）
    /// </summary>
    /// <param name="model">下载任务模型</param>
    /// <exception cref="Exception">序列化或加密失败时抛出</exception>
    public virtual async ValueTask<bool> SetPauseInfoAsync(DownloadModel? model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.DownloadUrl))
            return false;

        const int maxRetries = 3;
        int attempt = 0;

        // 确保存储目录存在
        if (!Directory.Exists(SaveFolderPauseinfoPath))
            Directory.CreateDirectory(SaveFolderPauseinfoPath);

        // 生成当日文件名（UTC时间）
        var dateNow = DateTime.UtcNow;
        var fileName = $"UTC_{dateNow:yyyyMMdd}.bin";
        var filePath = Path.Combine(SaveFolderPauseinfoPath, fileName);

        while (attempt < maxRetries)
        {
            try
            {
                // 读取现有数据或创建新集合
                var models = await ReadFileAsync(filePath);

                // 更新数据集合
                var existing = models.FirstOrDefault(q => q.DownloadUrl == model.DownloadUrl);
                if (existing != null) models.Remove(existing);
                models.Add(model);

                // 写入文件
                return await WriteFileAsync(filePath, models);
            }
            catch (IOException ex) when (ex.HResult == -2147024864) // 文件被锁定
            {
                await Task.Delay(100);
                attempt++;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"加密失败: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未知错误: {ex.Message}");
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 从指定文件解析下载信息集合
    /// </summary>
    private async Task<List<DownloadModel>> ReadFileAsync(string path)
    {
        // 缓存检查
        if (_fileCache.TryGetValue(path, out var cachedModels))
            return cachedModels;

        if (!File.Exists(path)) return new List<DownloadModel>();

        try
        {
            // 流式读取（性能优化）
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,  // 允许并发读取
                bufferSize: 4096,
                useAsync: true);

            var encryptedData = await JsonSerializer.DeserializeAsync<string>(
                stream,
                DownloadHandlerContext.Default.String);

            var json = AesEncryptionUtil.Decrypt(encryptedData!, _encryptionKey);
            var models = JsonSerializer.Deserialize(
                json,
                DownloadHandlerContext.Default.ListDownloadModel)
                ?? [];

            // 更新缓存
            _fileCache.TryAdd(path, models);
            return models;
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// 带互斥锁的文件写入（跨进程并发控制）
    /// </summary>
    private async Task<bool> WriteFileAsync(string path, List<DownloadModel> models)
    {
        var mutexName = $"Global\\DownloadHandler_{Path.GetFileName(path)}";
        using var mutex = new Mutex(false, mutexName);

        try
        {
            bool mutexAcquired = mutex.WaitOne(1000); // 等待1秒获取锁
            if (!mutexAcquired) throw new TimeoutException("获取文件锁超时");

            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,  // 禁止并发写入
                bufferSize: 4096,
                useAsync: true);

            // 序列化并加密存储
            var valueJson = JsonSerializer.Serialize(
                models,
                DownloadHandlerContext.Default.ListDownloadModel);

            if (string.IsNullOrWhiteSpace(valueJson))
                throw new Exception("JSON序列化失败");

            var encrypted = AesEncryptionUtil.Encrypt(valueJson, _encryptionKey);
            if (string.IsNullOrEmpty(encrypted))
                throw new Exception("AES加密失败");

            // 写入文件
            await JsonSerializer.SerializeAsync(
                stream,
                encrypted,
                DownloadHandlerContext.Default.String);

            // 更新缓存
            _fileCache.AddOrUpdate(path, models, (k, v) => models);
            return true;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"并发写入冲突: {ex.Message}");
            return false;
        }
        finally
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    /// <summary>
    /// 在单个文件中查找匹配的下载记录
    /// </summary>
    private async Task<DownloadModel?> FindInFile(string filePath, string downloadUrl)
    {
        if (!File.Exists(filePath)) return null;

        try
        {
            var models = await ReadFileAsync(filePath);
            return models.FirstOrDefault(q => q.DownloadUrl == downloadUrl);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// AOT序列化上下文（自动生成序列化代码）
/// </summary>
[JsonSerializable(typeof(List<DownloadModel>))]
[JsonSerializable(typeof(string))]
public partial class DownloadHandlerContext : JsonSerializerContext
{
}