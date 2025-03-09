namespace MFToolkit.SqlSugarCore.Extensions.Storage;

/// <summary>
/// Preferences API 用于在键/值存储中存储应用程序首选项
/// </summary>
public interface IPreferences
{
    /// <summary>
    /// 检查给定键是否存在。
    /// </summary>
    /// <param name="key">要检查的键</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <returns>如果首选项中存在键，则为 true，否则为 false。</returns>
    bool ContainsKey(string key, string? sharedName = null);

    /// <summary>
    /// 检查给定键是否存在。
    /// </summary>
    /// <param name="key">要检查的键</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <returns>如果首选项中存在键，则为 true，否则为 false。</returns>
    Task<bool> ContainsKeyAsync(string key, string? sharedName = null);

    /// <summary>
    /// 如果存在，移除键及其关联值。
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <param name="sharedName">共享容器名称</param>
    void Remove(string key, string? sharedName = null);

    /// <summary>
    /// 如果存在，移除键及其关联值。
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <param name="sharedName">共享容器名称</param>
    Task RemoveAsync(string key, string? sharedName = null);

    /// <summary>
    /// 清除所有键和值
    /// </summary>
    /// <param name="sharedName">共享容器名称</param>
    void Clear(string? sharedName = null);

    /// <summary>
    /// 清除所有键和值
    /// </summary>
    /// <param name="sharedName">共享容器名称</param>
    Task ClearAsync(string? sharedName = null);

    /// <summary>
    /// 为给定键设置值
    /// </summary>
    /// <param name="key">要设置值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <typeparam name="T">存储在此首选项中的对象类型</typeparam>
    void Set<T>(string key, T value, string? sharedName = null);

    /// <summary>
    /// 为给定键设置值
    /// </summary>
    /// <param name="key">要设置值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <typeparam name="T">存储在此首选项中的对象类型</typeparam>
    Task SetAsync<T>(string key, T value, string? sharedName = null);

    /// <summary>
    /// 获取给定键的值，如果键不存在，则返回指定的默认值
    /// </summary>
    /// <param name="key">要检索值的键</param>
    /// <param name="defaultValue">当键的现有值不存在时要返回的默认值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <typeparam name="T">存储在此首选项中的对象类型</typeparam>
    /// <returns>给定键的值，如果不存在则为 defaultValue 中的值</returns>
    T Get<T>(string key, T defaultValue, string? sharedName = null);

    /// <summary>
    /// 获取给定键的值，如果键不存在，则返回指定的默认值
    /// </summary>
    /// <param name="key">要检索值的键</param>
    /// <param name="defaultValue">当键的现有值不存在时要返回的默认值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <typeparam name="T">存储在此首选项中的对象类型</typeparam>
    /// <returns>给定键的值，如果不存在则为 defaultValue 中的值</returns>
    Task<T> GetAsync<T>(string key, T defaultValue, string? sharedName = null);
}