using System.Collections.Concurrent;
using MFToolkit.JsonExtensions;
using MFToolkit.Utils.EncryptionExtensions.AESEncryption;
using Mapster;

namespace MFToolkit.Storage;

/// <summary>
/// 偏好存储
/// </summary>
public class Preferences : IPreferences
{
    private static string _encryptionKey = "7R9pXcE2qFbY5vA1sT3uZ8wI4oP6lK0j";

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
    /// 设置自定义加密Key
    /// </summary>
    /// <param name="key"></param>
    public static void SetEncryptionKey(string key)
    {
        _encryptionKey = key;
    }

    private static IPreferences? _current;

    private static void CheckIsSupportedType<T>()
    {
        var type = typeof(T);
        if (!SupportedTypes.Contains(type))
        {
            throw new NotSupportedException($"Preferences using '{type}' type is not supported");
        }
    }

    /// <summary>
    /// 提供此API静态使用的默认实现。
    /// </summary>
    public static IPreferences Default => _current ??= new PreferencesImplementation();

    public bool ContainsKey(string key, string? sharedName = null)
    {
        sharedName ??= defaultFileName;
        return preferencesData.ContainsKey(key);
    }

    public void Remove(string key, string? sharedName = null)
    {
        sharedName ??= defaultFileName;
        if (!preferencesData.TryGetValue(sharedName, out var value)) return;
        var result = value.FirstOrDefault(q => q.Key == key);
        if (result == null) return;
        value.Remove(result);
        SetPreferences(sharedName);
    }

    public void Clear(string? sharedName = null)
    {
        sharedName ??= defaultFileName;
        if (!preferencesData.TryGetValue(sharedName, out var value)) return;
        value.Clear();
        SetPreferences(sharedName);
    }

    public void Set<T>(string key, T value, string? sharedName = null)
    {
        CheckIsSupportedType<T>();
        sharedName ??= defaultFileName;
        if (!preferencesData.TryGetValue(sharedName, out var queryValue))
        {
            // 如果没有这个共享空间
            preferencesData.TryAdd(sharedName, new()
            {
                new()
                {
                    Key = key, Value = value, SharedName = sharedName
                }
            });
        }
        else
        {
            var resultData = queryValue.FirstOrDefault(q => q.Key == key);
            if (resultData != null)
            {
                queryValue.Remove(resultData);
                queryValue.Add(new()
                {
                    Key = key,
                    Value = value,
                    SharedName = sharedName
                });
            }
            else
            {
                queryValue.Add(new()
                {
                    Key = key,
                    Value = value,
                    SharedName = sharedName
                });
            }
        }

        SetPreferences(sharedName);
    }

    public T Get<T>(string key, T defaultValue, string? sharedName = null)
    {
        CheckIsSupportedType<T>();
        sharedName ??= defaultFileName;
        if (!preferencesData.TryGetValue(sharedName, out var value)) return defaultValue;
        var resultValue = value.FirstOrDefault(q => q.Key == key)?.Value;
        if (resultValue == null) return defaultValue;
        var jsonEl = resultValue.ToString();

        return jsonEl.Adapt<T>();
    }

    #region 提供存储和查找功能

    private ConcurrentDictionary<string, List<PreferencesModel>> preferencesData;

    /// <summary>
    /// 保存文件夹
    /// </summary>
    private readonly string directoryPath;

    private const string defaultFileName = "default";
    private string suffix = ".dat";

    private class PreferencesModel
    {
        public string Key { get; init; } = null!;
        public object? Value { get; init; }
        public string? SharedName { get; init; }
    }

    private Preferences(string directoryPath = null!)
    {
        this.directoryPath = directoryPath;
    }

    public static IPreferences CreateSingletonPreferences(string directoryPath = null!, string encryptionKey = null!)
    {
        if (_current != null) return _current;
        if (!string.IsNullOrWhiteSpace(encryptionKey)) _encryptionKey = encryptionKey;
        if (string.IsNullOrWhiteSpace(directoryPath))
            directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storages");
        var current = new Preferences(directoryPath);
        current.GetPreferences();
        _current = current;
        return current;
    }

    private void GetPreferences()
    {
        preferencesData = [];
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            return;
        }

        var files = new DirectoryInfo(directoryPath);
        // 获取文件内容
        foreach (var file in files.GetFiles())
        {
            if (!file.Name.Contains(suffix) || file.Extension != suffix) return;
            string encryptedData = File.ReadAllText(Path.Combine(directoryPath, file.Name.Contains(suffix)
                ? file.Name
                : file.Name + suffix));
            if (string.IsNullOrWhiteSpace(encryptedData)) continue;
            string decryptedData = AesUtil.Decrypt(encryptedData, _encryptionKey);
            var value = decryptedData.JsonToDeserialize<List<PreferencesModel>>();
            var sharedName = value?.FirstOrDefault()?.SharedName;
            if (string.IsNullOrWhiteSpace(sharedName)) return;
            preferencesData.TryAdd(sharedName, value);
        }
    }

    private void SetPreferences(string sharedName)
    {
        try
        {
            // 如果没找到的话直接退出
            if (!preferencesData.TryGetValue(sharedName, out var value)) return;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var path = Path.Combine(directoryPath, sharedName + suffix);
            var jsonData = value.ValueToJson();
            var encryptedData = AesUtil.Encrypt(jsonData, _encryptionKey);
            File.WriteAllText(path, encryptedData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    #endregion
}