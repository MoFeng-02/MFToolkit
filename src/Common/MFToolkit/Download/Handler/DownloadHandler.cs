using System.Data;
using System.Text;
using MFToolkit.App;
using MFToolkit.Download.Constant;
using MFToolkit.Download.JsonAotContext;
using MFToolkit.Download.Models;
using MFToolkit.Json.Extensions;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;

namespace MFToolkit.Download.Handler;
/// <summary>
/// 下载处理
/// </summary>
public class DownloadHandler
{
    //private readonly string saveFolderPath = DownloadConstant.SaveFolderPath;
    private readonly string saveFolderPauseinfoPath = Path.Combine(DownloadConstant.SaveFolderPath, "pauseinfo");
    private readonly string eKey = "otEwyom+/bgjiUzyMio8aJfqfL44h5Xu";
    /// <summary>
    /// 获取暂停下载列表的信息
    /// </summary>
    /// <param name="downloadUrl">根据下载地址进行匹配</param>
    /// <param name="saveTime">该暂停信息保存的日期</param>
    /// <returns></returns>
    public virtual async Task<DownloadModel?> GetPauseInfoAsync(string downloadUrl, DateTime? saveTime = null)
    {
        if (!Directory.Exists(saveFolderPauseinfoPath)) return null;
        string? queryTimeName = saveTime != null ? $"UTC_{saveTime:yyyyMMdd}.bin" : null;
        var filePath = queryTimeName == null ? saveFolderPauseinfoPath : Path.Combine(saveFolderPauseinfoPath, queryTimeName);
        List<DownloadModel> models;
        if (queryTimeName != null)
        {
            var jstr = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrEmpty(jstr)) return null;
            var reValue = AesUtil.Decrypt(jstr, eKey);
            models = reValue.JsonToDeserialize<List<DownloadModel>>(context: DownloadJsonAotContext.Default) ?? new List<DownloadModel>();
            var existingModel = models.FirstOrDefault(q => q.DownloadUrl == downloadUrl);
            return existingModel;
        }
        // 如果没有指定哪个日期
        var files = Directory.GetFiles(saveFolderPauseinfoPath).Where(q => q.EndsWith(".bin"));
        foreach (var file in files)
        {
            var jstr = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrEmpty(jstr)) continue;
            var reValue = AesUtil.Decrypt(jstr, eKey);
            models = reValue.JsonToDeserialize<List<DownloadModel>>(context: DownloadJsonAotContext.Default) ?? new List<DownloadModel>();
            var existingModel = models.FirstOrDefault(q => q.DownloadUrl == downloadUrl);
            return existingModel;
        }
        return null;
    }
    /// <summary>
    /// 获取暂停下载列表的信息
    /// </summary>
    /// <param name="model">主要根据下载地址进行匹配</param>
    /// <param name="saveTime">该暂停信息保存的日期</param>
    /// <returns></returns>
    public virtual async Task<DownloadModel?> GetPauseInfoAsync(DownloadModel model, DateTime? saveTime = null)
    {
        if (!Directory.Exists(saveFolderPauseinfoPath)) return null;
        string? queryTimeName = saveTime != null ? $"UTC_{saveTime:yyyyMMdd}.bin" : null;
        var filePath = queryTimeName == null ? saveFolderPauseinfoPath : Path.Combine(saveFolderPauseinfoPath, queryTimeName);
        List<DownloadModel> models;
        if (queryTimeName != null)
        {
            var jstr = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrEmpty(jstr)) return null;
            var reValue = AesUtil.Decrypt(jstr, eKey);
            models = reValue.JsonToDeserialize<List<DownloadModel>>(context: DownloadJsonAotContext.Default) ?? new List<DownloadModel>();
            var existingModel = models.FirstOrDefault(q => q.DownloadUrl == model.DownloadUrl);
            if (existingModel == null) return null;
            return existingModel;
        }
        // 如果没有指定哪个日期
        var files = Directory.GetFiles(saveFolderPauseinfoPath).Where(q => q.EndsWith(".bin"));
        foreach (var file in files)
        {
            var jstr = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrEmpty(jstr)) continue;
            var reValue = AesUtil.Decrypt(jstr, eKey);
            models = reValue.JsonToDeserialize<List<DownloadModel>>(context: DownloadJsonAotContext.Default) ?? new List<DownloadModel>();
            var existingModel = models.FirstOrDefault(q => q.DownloadUrl == model.DownloadUrl);
            if (existingModel == null) return null;
            return existingModel;
        }
        return null;
    }
    public virtual async Task<bool> SetPauseInfoAsync(DownloadModel? model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.DownloadUrl))
            return false;

        if (!Directory.Exists(saveFolderPauseinfoPath))
            Directory.CreateDirectory(saveFolderPauseinfoPath);

        var dateNow = DateTime.UtcNow;
        var fileName = $"UTC_{dateNow:yyyyMMdd}.bin";
        var filePath = Path.Combine(saveFolderPauseinfoPath, fileName);

        List<DownloadModel> models;

        // 尝试读取现有文件中的信息
        if (File.Exists(filePath))
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            var jstr = Encoding.UTF8.GetString(bytes);
            if (!string.IsNullOrEmpty(jstr))
            {
                models = jstr.JsonToDeserialize<List<DownloadModel>>(context: DownloadJsonAotContext.Default) ?? new List<DownloadModel>();
            }
            else
            {
                models = new List<DownloadModel>();
            }
        }
        else
        {
            models = new List<DownloadModel>();
        }

        // 查找是否存在相同的下载URL，如果存在则更新，否则添加新的
        var existingModel = models.FirstOrDefault(q => q.DownloadUrl == model.DownloadUrl);
        if (existingModel != null)
        {
            models.Remove(existingModel);
        }
        models.Add(model);

        // 将更新后的列表加密并保存到文件
        var valueJson = models.ValueToJson(context: DownloadJsonAotContext.Default);
        if (string.IsNullOrWhiteSpace(valueJson))
            throw new Exception("转换失败，将类型DownloadModel转换为字符串json的时候失败");

        // 加密
        var eStr = AesUtil.Encrypt(valueJson, eKey);
        if (string.IsNullOrEmpty(eStr))
            throw new Exception("加密失败");

        byte[] modelBytes = Encoding.UTF8.GetBytes(eStr);
        using FileStream fileStream = new(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        await fileStream.WriteAsync(modelBytes);
        //await File.WriteAllBytesAsync(filePath, modelBytes);
        return true;
    }
}
