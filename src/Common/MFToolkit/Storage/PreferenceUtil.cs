namespace MFToolkit.Storage;

/// <summary>
/// 首选项工具类
/// 在应用程序入口调用一次CreateSingletonPreferences，如下所示，即可以使用本类，不建议一次存储过多数据，建议用于存储设置之类的即首选项之类的
/// <para>
/// <code>PreferenceUtil.CreateSingletonPreferences();</code>
/// </para>
/// </summary>
public class PreferenceUtil
{
    /// <summary>
    /// 创建全局唯一的首选项，不设置用不了这个类
    /// </summary>
    /// <param name="directoryPath">存储目录</param>
    /// <param name="encryptionKey"></param>
    /// <returns></returns>
    public static IPreferences CreateSingletonPreferences(string directoryPath = null!, string encryptionKey = null!) =>
        Preferences.CreateSingletonPreferences(directoryPath, encryptionKey);

    /// <summary>
    /// 检查给定键是否存在。
    /// </summary>
    /// <param name="key">要检查的键</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <returns>如果首选项中存在键，则为 true，否则为 false。</returns>
    public static bool ContainsKey(string key, string? sharedName = null) =>
        Preferences.Default.ContainsKey(key, sharedName);

    /// <summary>
    /// 如果存在，移除键及其关联值。
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <param name="sharedName">共享容器名称</param>
    public static void Remove(string key, string? sharedName = null) => Preferences.Default.Remove(key, sharedName);

    /// <summary>
    /// 清除所有键和值
    /// </summary>
    /// <param name="sharedName">共享容器名称</param>
    public static void Clear(string? sharedName = null) => Preferences.Default.Clear(sharedName);

    /// <summary>
    /// 为给定键设置值
    /// </summary>
    /// <param name="key">要设置值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <typeparam name="T">存储在此首选项中的对象类型</typeparam>
    public static void Set<T>(string key, T value, string? sharedName = null) =>
        Preferences.Default.Set(key, value, sharedName);

    /// <summary>
    /// 获取给定键的值，如果键不存在，则返回指定的默认值
    /// </summary>
    /// <param name="key">要检索值的键</param>
    /// <param name="defaultValue">当键的现有值不存在时要返回的默认值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <typeparam name="T">存储在此首选项中的对象类型</typeparam>
    /// <returns>给定键的值，如果不存在则为 defaultValue 中的值</returns>
    public static T Get<T>(string key, T defaultValue, string? sharedName = null) =>
        Preferences.Default.Get(key, defaultValue, sharedName);
}