using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using MFToolkit.Download.Constant;
using MFToolkit.Download.Models;
using MFToolkit.Json.Extensions;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;

namespace MFToolkit.Download.Handler;

/// <summary>
/// 下载暂停信息处理器
/// 负责管理单个下载任务的暂停状态信息存储和恢复
/// </summary>
public class DownloadPauseInfoHandler
{
    /// <summary>
    /// 是否启用下载信息加密存储（静态配置）
    /// </summary>
    private static bool IsStartEncryption { get; set; } = false;

    /// <summary>
    /// AES加密密钥（默认提供开发环境测试密钥）
    /// 注意：生产环境应通过安全方式注入密钥
    /// </summary>
    private static string EncryptionKey { get; set; } = "otEwyom+/bgjiUzyMio8aJfqfL44h5Xu";

    /// <summary>
    /// 暂停信息存储目录路径
    /// 默认路径：DownloadConstant.SaveFolderPath/Pauseinfo
    /// </summary>
    protected virtual string SaveFolderPauseInfoPath { get; set; } = Path.Combine(DownloadConstant.SaveFolderPath, "Pauseinfo");

    /// <summary>
    /// 生成具体文件存储路径
    /// </summary>
    /// <param name="key">下载任务唯一标识</param>
    /// <returns>完整文件路径（格式：存储目录/key.bin）</returns>
    private string GetFilePath(string key) =>
        Path.Combine(SaveFolderPauseInfoPath, $"{key}.bin");

    /// <summary>
    /// 初始化暂停信息处理器配置
    /// </summary>
    /// <param name="isStartEncryption">是否启用加密存储</param>
    /// <param name="encryptionKey">自定义加密密钥（需16/24/32字节长度）</param>
    internal static void DownloadInitPauseInfoHandler(bool isStartEncryption = false, string? encryptionKey = null)
    {
        if (!string.IsNullOrEmpty(encryptionKey) && isStartEncryption)
        {
            IsStartEncryption = isStartEncryption;
            EncryptionKey = encryptionKey;
        }
    }

    /// <summary>
    /// 获取指定下载任务的暂停信息
    /// </summary>
    /// <param name="key">下载任务唯一标识</param>
    /// <returns>
    /// 成功：返回DownloadModel实例
    /// 失败：文件不存在或解析失败返回null
    /// </returns>
    public virtual async Task<DownloadModel?> GetPauseInfoAsync(string key)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath)) return null;

        try
        {
            // 异步读取文件内容
            var json = await File.ReadAllTextAsync(filePath);

            // 使用AOT上下文反序列化
            var model = await json.JsonToDeserializeAsync<DownloadModel>(context: DownloadPauseInfoHandlerContext.Default);

            // 启用加密时进行解密处理
            if (model != null && IsStartEncryption)
            {
                try
                {
                    model.DownloadUrl = AesEncryptionUtil.Decrypt(model.DownloadUrl, EncryptionKey);
                }
                catch (Exception ex)
                {
                    // 记录解密失败日志（实际项目应添加日志记录）
                    Console.WriteLine(ex.Message);
                }
            }
            return model;
        }
        catch (Exception ex)
        {
            // 捕获文件读写或反序列化异常
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 保存下载暂停信息
    /// </summary>
    /// <param name="model">下载任务模型</param>
    /// <returns>
    /// true: 保存成功
    /// false: 参数无效或保存失败
    /// </returns>
    public virtual async Task<bool> SavePauseInfoAsync(DownloadModel? model)
    {
        if (model == null) return false;

        const int maxRetries = 3;
        const int retryDelayMs = 100;
        int attempt = 0;

        Directory.CreateDirectory(SaveFolderPauseInfoPath);
        var tempModel = new DownloadModel()
        {
            Key = model.Key,
            FileSavePath = model.FileSavePath,
            // 加密处理下载URL
            DownloadUrl = IsStartEncryption
                ? AesEncryptionUtil.Encrypt(model.DownloadUrl, EncryptionKey) ?? model.DownloadUrl
                : model.DownloadUrl,
            WriteSize = model.WriteSize,
            YetSize = model.YetSize,
            SumSize = model.SumSize
        };

        while (attempt < maxRetries)
        {
            try
            {
                // 使用FileShare.None实现文件锁
                await using var stream = new FileStream(
                    GetFilePath(model.Key),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,  // 禁止其他进程访问
                    bufferSize: 4096,
                    useAsync: true);

                // 流式序列化（性能优化）
                await JsonSerializer.SerializeAsync(
                    stream,
                    tempModel,
                    DownloadPauseInfoHandlerContext.Default.DownloadModel);

                return true;
            }
            catch (IOException ex) when (ex.HResult == -2147024864) // 文件被锁定
            {
                await Task.Delay(retryDelayMs);
                attempt++;
            }
            catch (CryptographicException ex)
            {
                // 记录加密失败日志
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                // 处理权限问题
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // 通用异常处理
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        return false;
    }
    //public virtual async Task<bool> SavePauseInfoAsync(DownloadModel? model)
    //{
    //    if (model == null) return false;

    //    // 确保存储目录存在
    //    Directory.CreateDirectory(SaveFolderPauseInfoPath);

    //    // 创建副本避免修改原始对象
    //    var tempModel = new DownloadModel
    //    {
    //        Key = model.Key,
    //        FileSavePath = model.FileSavePath,
    //        // 加密处理下载URL
    //        DownloadUrl = IsStartEncryption
    //            ? AesEncryptionUtil.Encrypt(model.DownloadUrl, EncryptionKey) ?? model.DownloadUrl
    //            : model.DownloadUrl,
    //        WriteSize = model.WriteSize,
    //        YetSize = model.YetSize,
    //        SumSize = model.SumSize
    //    };

    //    try
    //    {
    //        // 序列化为JSON并写入文件
    //        var json = tempModel.ValueToJson(context: DownloadPauseInfoHandlerContext.Default);
    //        await File.WriteAllTextAsync(GetFilePath(model.Key), json);
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        // 处理文件写入异常
    //        Console.WriteLine(ex.Message);
    //        return false;
    //    }
    //}

    /// <summary>
    /// 删除指定下载任务的暂停信息
    /// </summary>
    /// <param name="key">下载任务唯一标识</param>
    /// <returns>
    /// true: 删除成功或文件不存在
    /// false: 删除过程中发生异常
    /// </returns>
    public virtual bool DeletePauseInfo(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        var filePath = GetFilePath(key);
        if (!File.Exists(filePath)) return false;

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // 处理文件删除异常
            return false;
        }
    }
}

/// <summary>
/// AOT序列化上下文（自动生成序列化代码）
/// </summary>
[JsonSerializable(typeof(DownloadModel))]
public partial class DownloadPauseInfoHandlerContext : JsonSerializerContext
{
}