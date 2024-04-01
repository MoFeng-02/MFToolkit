using MFToolkit.Json.Extensions;
using MFToolkit.SqlSugarExtensions.Configuration;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;
using SqlSugar;

namespace MFToolkit.SqlSugarExtensions.Storage;

/// <summary>
/// 本类为，SqlSugar 的 SQLite 版偏好设置类
/// </summary>
public class Preferences : IPreferences
{
    public static IPreferences Default = null!;

    /// <summary>
    /// 加密密钥
    /// </summary>
    private static string _encryptionKey = "7R9pXcE2qFbY5vA1sT3uZ8wI4oP6lK0j";

    public static void SetEncryptionKey(string encryptionKey) => _encryptionKey = encryptionKey;
    private static ConnectionConfig? ConnectionConfig { get; set; }

    private static SqlSugarClient CreateClient(bool isSingleConfig = true, Action<SqlSugarClient>? configAction = null)
    {
        if (ConnectionConfig == null)
            throw new(
                "数据库连接配置未初始化，它应该如此初始化：在应用程序启动目录等前文件中调用 Preference.InitPreference 方法来初始化它");

        var db = new SqlSugarClient(ConnectionConfig, it => configAction?.Invoke(it));
        return db;
    }


    /// <summary>
    /// 初始化偏好类
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="encryptionKey">自定义加密key</param>
    public static void InitPreference(ConnectionConfig config, string? encryptionKey = null)
    {
        if (Default != null!) return;
        if (ConnectionConfig != null) return;
        if (!string.IsNullOrEmpty(encryptionKey)) SetEncryptionKey(encryptionKey);
        StaticConfig.EnableAot = true;
        ConnectionConfig = config;
        using var aotClient = CreateClient();
        aotClient.CurrentConnectionConfig.ConfigureExternalServices =
            SqlSugarConfiguration.ConfigureExternalServices;
        //建库：如果不存在创建数据库存在不会重复创建 
        aotClient.DbMaintenance.CreateDatabase();
        // 建表
        aotClient.CodeFirst.InitTables(typeof(PreferenceModel));
        Default = new Preferences();
    }

    public bool ContainsKey(string key, string? sharedName = null)
    {
        using var aotClient = CreateClient();
        var isExist = aotClient.Queryable<PreferenceModel>().Any(q => q.Key == key && q.SharedName ==
            sharedName);
        return isExist;
    }

    public async Task<bool> ContainsKeyAsync(string key, string? sharedName = null)
    {
        using var aotClient = CreateClient();
        var isExist = await aotClient.Queryable<PreferenceModel>().AnyAsync(q => q.Key == key && q.SharedName ==
            sharedName);
        return isExist;
    }

    public void Remove(string key, string? sharedName = null)
    {
        using var aotClient = CreateClient();
        var query = aotClient.Queryable<PreferenceModel>().First(q => q.Key == key && q.SharedName ==
            sharedName);
        if (query == null) return;
        aotClient.Deleteable(query).ExecuteCommand();
    }

    public async Task RemoveAsync(string key, string? sharedName = null)
    {
        using var aotClient = CreateClient();
        var query = await aotClient.Queryable<PreferenceModel>().FirstAsync(q => q.Key == key && q.SharedName ==
            sharedName);
        if (query == null) return;
        await aotClient.Deleteable(query).ExecuteCommandAsync();
    }

    public void Clear(string? sharedName = null)
    {
        using var aotClient = CreateClient();
        var querys = aotClient.Queryable<PreferenceModel>().Where(q => q.SharedName == sharedName).ToList();
        if (querys == null || querys.Count == 0) return;
        aotClient.Deleteable(querys).ExecuteCommand();
    }

    public async Task ClearAsync(string? sharedName = null)
    {
        using var aotClient = CreateClient();
        var querys = await aotClient.Queryable<PreferenceModel>().Where(q => q.SharedName == sharedName).ToListAsync();
        if (querys == null || querys.Count == 0) return;
        await aotClient.Deleteable(querys).ExecuteCommandAsync();
    }

    public void Set<T>(string key, T value, string? sharedName = null)
    {
        CheckIsSupportedType<T>();
        using var aotClient = CreateClient();
        var query = aotClient.Queryable<PreferenceModel>().First(q => q.Key == key && q.SharedName == sharedName);
        var encryptionData = AesUtil.Encrypt(value?.ValueToJson()!, _encryptionKey);
        if (query != null)
        {
            query.Value = encryptionData;
            aotClient.Updateable(query).ExecuteCommand();
            return;
        }

        var newModel = new PreferenceModel()
        {
            Key = key,
            Value = encryptionData,
            SharedName = sharedName
        };
        aotClient.Insertable(newModel).ExecuteCommand();
    }

    public async Task SetAsync<T>(string key, T value, string? sharedName = null)
    {
        CheckIsSupportedType<T>();
        using var aotClient = CreateClient();
        var query = await aotClient.Queryable<PreferenceModel>().FirstAsync(q => q.Key == key && q.SharedName ==
            sharedName);
        var encryptionData = AesUtil.Encrypt(value?.ValueToJson()!, _encryptionKey);
        if (query != null)
        {
            query.Value = encryptionData;
            await aotClient.Updateable(query).ExecuteCommandAsync();
            return;
        }

        var newModel = new PreferenceModel()
        {
            Key = key,
            Value = encryptionData,
            SharedName = sharedName
        };
        await aotClient.Insertable(newModel).ExecuteCommandAsync();
    }

    public T Get<T>(string key, T defaultValue, string? sharedName = null)
    {
        CheckIsSupportedType<T>();
        using var aotClient = CreateClient();
        var query = aotClient.Queryable<PreferenceModel>().First(q => q.Key == key && q.SharedName == sharedName);
        var d = AesUtil.Decrypt(query?.Value!, _encryptionKey);
        var result = d.JsonToDeserialize(defaultValue: defaultValue);
        return result!;
    }

    public async Task<T> GetAsync<T>(string key, T defaultValue, string? sharedName = null)
    {
        CheckIsSupportedType<T>();
        using var aotClient = CreateClient();
        var query = await aotClient.Queryable<PreferenceModel>().FirstAsync(q => q.Key == key && q.SharedName ==
            sharedName);
        var d = AesUtil.Decrypt(query?.Value!, _encryptionKey);
        var result = d.JsonToDeserialize(defaultValue: defaultValue);
        return result!;
    }

    private static readonly Type[] SupportedTypes =
    {
        typeof(string),
        typeof(int),
        typeof(bool),
        typeof(long),
        typeof(double),
        typeof(float),
        typeof(DateTime)
    };

    /// <summary>
    /// 检查类型是否符合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="NotSupportedException"></exception>
    private static void CheckIsSupportedType<T>()
    {
        var type = typeof(T);
        if (!SupportedTypes.Contains(type))
        {
            throw new NotSupportedException($"Preferences using '{type}' type is not supported");
        }
    }
}

public class PreferenceModel
{
    [SugarColumn(IsPrimaryKey = true)] public string Key { get; init; } = null!;
    [SugarColumn(IsNullable = true)] public string Value { get; set; }
    [SugarColumn(IsNullable = true)] public string SharedName { get; init; }
}