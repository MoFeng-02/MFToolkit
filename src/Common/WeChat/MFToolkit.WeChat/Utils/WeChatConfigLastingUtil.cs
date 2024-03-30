using MFToolkit.JsonExtensions;
using MFToolkit.Utils.EncryptionUtils.AESEncryption;
using MFToolkit.WeChat.Configurations.BasicConfiguration;
using MFToolkit.WeChat.Configurations;
using MFToolkit.WeChat.Json.AOT;
using System.Text.Json;

namespace MFToolkit.WeChat.Utils;
public class WeChatConfigLastingUtil
{
    /// <summary>
    /// 保存文件夹
    /// </summary>
    private static string _saveFolder;
    /// <summary>
    /// 缓存文件夹
    /// </summary>
    private static string _saveCacheFolder;
    /// <summary>
    /// 加密密钥
    /// </summary>
    private static string _encryptionKey;
    /// <summary>
    /// 注册WeChat配置持久化，不提供删除，删除需要自行去处理
    /// <para>Version: 0.0.1-beta</para>
    /// </summary>
    /// <param name="readPath">读取路径，只能是文件夹</param>
    /// <param name="encryptionKey">内容加密Key，如果是有加密的话</param>
    public static async Task WeChatConfigurationLasting(string? readPath = null, string? encryptionKey = null)
    {
        // 先读取本地
        _saveFolder = readPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "WeApp");
        _saveCacheFolder = Path.Combine(_saveFolder, "cache");
        _encryptionKey = encryptionKey;
        // 是否存在文件夹目录
        bool isExistData = true;
        if (!Directory.Exists(_saveFolder))
        {
            isExistData = false;
            Directory.CreateDirectory(_saveFolder);
        }
        List<string> filePaths = [];
        List<Dictionary<string, WeChatConfig>> configs = [];
        var isJiaMi = !string.IsNullOrWhiteSpace(encryptionKey);
        if (isExistData)
        {
            foreach (var filePath in Directory.GetFiles(_saveFolder))
            {
                if (!filePath.EndsWith(isJiaMi ? ".conf" : ".json")) continue;
                var oldStr = File.ReadAllText(filePath);
                if (oldStr == null) continue;
                var rstr = isJiaMi ? AesUtil.Decrypt(oldStr, encryptionKey) : oldStr;
                if (rstr == null) continue;
                configs.Add(rstr.JsonToDeserialize<Dictionary<string, WeChatConfig>>(context: WeChatConfigJsonContext.Default) ?? []);
                filePaths.Add(filePath);
            }
        }
        // 存入
        if (!Directory.Exists(_saveCacheFolder)) Directory.CreateDirectory(_saveCacheFolder);
        Dictionary<string, WeChatConfig> caches = [];
        int maxCacheCount = 100000;
        // 获取代码中写好的，然后根据日期判断是否需要覆盖已经持久化的
        var codeValues = WeChatConfigUtil.GetBasicConfigs();
        if (codeValues == null || codeValues.Count == 0) return;
        // 100个在一个文件里面
        int maxSaveCount = 1000;
        // 保存到本地的也就是已经有的
        List<string> saveKeys = [];
        for (int i = 0; configs.Count > i; i++)
        {
            var config = configs[i];
            foreach (var item in config)
            {
                var codeValue = codeValues[item.Key];
                if (codeValue == null) continue;
                saveKeys.Add(item.Key);
                if (codeValue.LastUpdateTime > item.Value.LastUpdateTime)
                {
                    config[item.Key] = codeValue;
                    caches.TryAdd(item.Key, codeValue);
                    continue;
                }
                caches.TryAdd(item.Key, item.Value);
            }
        }
        var notSaveKeys = codeValues.Keys.Where(q => !saveKeys.Contains(q));
        // 重新保存原文件
        if (filePaths.Count == 0)
        {
            // 如果没有就新建
            var c = notSaveKeys.Count();
            var paging1 = c % maxSaveCount == 0 ? c / maxSaveCount : c / maxSaveCount + 1;
            for (int i = 0; i < paging1; i++)
            {
                var newPath = Path.Combine(_saveFolder, Guid.NewGuid() + (isJiaMi ? ".conf" : ".json"));
                var keys = notSaveKeys.Skip(i * maxSaveCount).Take(maxSaveCount);
                Dictionary<string, WeChatConfig> cc = [];
                foreach (var key in keys)
                {
                    cc.TryAdd(key, codeValues[key]);
                    caches.TryAdd(key, codeValues[key]);
                }
                string content;
                if (isJiaMi)
                {
                    content = AesUtil.Encrypt(cc.ValueToJson(context: WeChatConfigJsonContext.Default), encryptionKey);
                }
                else
                {
                    content = cc.ValueToJson(context: WeChatConfigJsonContext.Default);
                }
                await File.WriteAllTextAsync(newPath, content);
            }
            notSaveKeys = [];
        }
        else
            for (int i = 0; i < filePaths.Count; i++)
            {
                var kv = configs[i];
                if (kv.Count < maxSaveCount)
                {
                    var plusValueCount = maxSaveCount - kv.Count;
                    // 取出相应的可以合并入的值
                    var keys = notSaveKeys.Take(plusValueCount);
                    foreach (var key in keys)
                    {
                        kv.TryAdd(key, codeValues[key]);
                        codeValues.Remove(key);
                    }
                }
                var jsonStr = kv.ValueToJson(context: WeChatConfigJsonContext.Default);
                var path = filePaths[i];
                if (string.IsNullOrWhiteSpace(jsonStr))
                {
                    File.Delete(path);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(encryptionKey))
                {
                    await File.WriteAllTextAsync(path, jsonStr);
                }
                else
                {
                    var etxt = AesUtil.Encrypt(jsonStr, encryptionKey);
                    await File.WriteAllTextAsync(path, etxt);
                }
            }
        // 代表还有未存完的
        if (notSaveKeys.Any())
        {
            var c = notSaveKeys.Count();
            var paging1 = c % maxSaveCount == 0 ? c / maxSaveCount : c / maxSaveCount + 1;
            for (int i = 0; i < paging1; i++)
            {
                var newPath = Path.Combine(_saveFolder, Guid.NewGuid() + (isJiaMi ? ".conf" : ".json"));
                var keys = notSaveKeys.Skip(i * maxSaveCount).Take(maxSaveCount);
                Dictionary<string, WeChatConfig> cc = [];
                foreach (var key in keys)
                {
                    cc.TryAdd(key, codeValues[key]);
                    caches.TryAdd(key, codeValues[key]);
                }
                string content;
                if (isJiaMi)
                {
                    content = AesUtil.Encrypt(cc.ValueToJson(context: WeChatConfigJsonContext.Default), encryptionKey);
                }
                else
                {
                    content = cc.ValueToJson(context: WeChatConfigJsonContext.Default);
                }
                await File.WriteAllTextAsync(newPath, content);
            }
        }
        // 写入到缓存文件夹（缓存文件夹单个缓存文件最大只可存放10万条数据）
        var count = caches.Count;
        var paging = count % maxCacheCount == 0 ? count / maxCacheCount : count / maxCacheCount + 1;
        for (int i = 0; i < paging; i++)
        {
            var fileName = (i + 1) + "-cache.json";
            var savePath = Path.Combine(_saveCacheFolder, fileName);
            var data = caches.Skip(i * maxCacheCount).Take(maxCacheCount).ToDictionary();
            await File.WriteAllTextAsync(savePath, data.ValueToJson(context: WeChatConfigJsonContext.Default));
        }
    }
    /// <summary>
    /// 读取持久化里面的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static async Task<WeChatConfig?> GetWeChatConfigLasting(string key)
    {
        if (string.IsNullOrWhiteSpace(_saveFolder)) throw new("未注册AddWeChatConfigurationLasting");
        var isJiaMi = !string.IsNullOrWhiteSpace(_encryptionKey);
        // 获取下面所有文件路径
        var filePaths = Directory.GetFiles(_saveFolder).Where(q => q.EndsWith(isJiaMi ? ".conf" : ".json"));
        if (!filePaths.Any()) throw new("可读取配置为空");
        foreach (var filePath in filePaths)
        {
            var content = isJiaMi ? AesUtil.Decrypt(await File.ReadAllTextAsync(filePath), _encryptionKey) : await File.ReadAllTextAsync(filePath);
            var json = content.JsonToDeserialize<Dictionary<string, WeChatConfig>>(context: WeChatConfigJsonContext.Default);
            //var jsonNode = JsonSerializer.SerializeToNode(content, inputType: typeof(string), context: WeChatConfigJsonContext.Default);
            var value = json?[key];
            if (value == null) continue;
            return value;
        }
        return null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static async Task<WeChatConfig?> GetWeChatConfigCache(string key)
    {
        if (string.IsNullOrWhiteSpace(_saveCacheFolder)) throw new("未注册AddWeChatConfigurationLasting");// 获取下面所有文件路径
        var filePaths = Directory.GetFiles(_saveCacheFolder).Where(q => q.EndsWith(".json"));
        if (!filePaths.Any()) throw new("可读取配置为空");
        foreach (var filePath in filePaths)
        {
            var content = await File.ReadAllTextAsync(filePath);
            //var jsonNode = content.JsonToDeserialize<Dictionary<string, WeChatConfig>>(context: WeChatConfigJsonContext.Default);
            var json = content.JsonToDeserialize<Dictionary<string, WeChatConfig>>(context: WeChatConfigJsonContext.Default);
            //var jsonNode = JsonSerializer.SerializeToNode(content, inputType: typeof(string), context: WeChatConfigJsonContext.Default);
            var value = json?[key];
            if (value == null) continue;
            return value;
        }
        return null;
    }
}
