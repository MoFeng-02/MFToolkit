using MFToolkit.Download.Constant;
using MFToolkit.Download.Models;
using MFToolkit.SqlSugarExtensions.Configuration;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace MFToolkit.Download.DownloadHandlers;
/// <summary>
/// 下载暂停信息
/// </summary>
public class DownloadPauseInfoHandler
{
    /// <summary>
    /// 是否启动加密
    /// </summary>
    private static bool IsStartEncryption { get; set; } = false;
    /// <summary>
    /// 加密密钥，启动加密才生效
    /// </summary>
    private static string EncryptionKey { get; set; } = "otEwyom+/bgjiUzyMio8aJfqfL44h5Xu";
    private static ConnectionConfig ConnectionConfig { get; set; } = new ConnectionConfig()
    {
        ConnectionString = $"Data Source={Path.Combine(DownloadConstant.SaveFolderPath, "PauseInfo", "DownloadPauseInfo.db")}",
        DbType = DbType.Sqlite,
        IsAutoCloseConnection = true,
    };
    /// <summary>
    /// 保存所在文件夹路径
    /// </summary>
    protected virtual string SaveFolderPauseInfoPath { get; set; } = Path.Combine(DownloadConstant.SaveFolderPath, "pauseinfo");
    private static SqlSugarClient CreateClient(Action<SqlSugarClient>? configAction = null)
    {
        if (ConnectionConfig == null)
            throw new(
                "数据库连接配置未初始化，它应该如此初始化：在应用程序启动目录等前文件中调用 Preference.InitPreference 方法来初始化它");

        var db = new SqlSugarClient(ConnectionConfig, it => configAction?.Invoke(it));
        return db;
    }
    /// <summary>
    /// 本类内部实现GRUD功能
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="isStartEncryption">启动加密</param>
    /// <param name="encryptionKey">自定义加密key</param>
    internal static void DownloadInitPauseInfoHandler(Func<ConnectionConfig>? config = null, bool isStartEncryption = false, string? encryptionKey = null)
    {
        if (!string.IsNullOrEmpty(encryptionKey) && isStartEncryption)
        {
            IsStartEncryption = isStartEncryption;
            EncryptionKey = encryptionKey;
        }
        StaticConfig.EnableAot = true;
        if (config != null)
        {
            ConnectionConfig = config.Invoke();
        }
        using var aotClient = CreateClient();
        aotClient.CurrentConnectionConfig.ConfigureExternalServices =
            SqlSugarConfiguration.ConfigureExternalServices;
        //建库：如果不存在创建数据库存在不会重复创建 
        aotClient.DbMaintenance.CreateDatabase();
        // 建表
        aotClient.CodeFirst.InitTables(typeof(DownloadModel));
    }
    /// <summary>
    /// 获取暂停信息
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual async Task<DownloadModel?> GetPauseInfoAsync(string key)
    {
        using var client = CreateClient();
        var query = await client.Queryable<DownloadModel>().FirstAsync(q => q.Key == key);
        if (query == null || !IsStartEncryption) return query;
        // 解密
        query.DownloadUrl = AesUtil.Decrypt(query.DownloadUrl, EncryptionKey);
        return query;
    }
    /// <summary>
    /// 保存暂停信息
    /// </summary>
    /// <param name="model"></param>
    /// <returns>是否保存成功</returns>
    public virtual async Task<bool> SavePauseInfoAsync(DownloadModel? model)
    {
        if(model == null) return false;
        using var client = CreateClient();
        var query = await client.Queryable<DownloadModel>().FirstAsync(q => q.Key == model.Key);
        bool isEdit = query != null;
        if (isEdit)
        {
            query!.WirteSize = model.WirteSize;
            query.YetDownloadSize = model.YetDownloadSize;
            return await client.Updateable(query).ExecuteCommandAsync() > 0;
        }
        // 如果是新增
        if (IsStartEncryption)
        {
            model.DownloadUrl = AesUtil.Encrypt(model.DownloadUrl, EncryptionKey) ?? model.DownloadUrl;
        }
        return await client.Insertable(model).ExecuteCommandAsync() > 0;
    }
    /// <summary>
    /// 删除暂停信息
    /// </summary>
    /// <param name="key">要删除的key</param>
    /// <returns></returns>
    public virtual async Task<bool> DeletePauseInfoAsyn(string key)
    {
        using var client = CreateClient();
        var query = await client.Queryable<DownloadModel>().FirstAsync(q => q.Key == key);
        if (query == null) return false;
        return await client.Deleteable(query).ExecuteCommandAsync() >= 0;
    }

}