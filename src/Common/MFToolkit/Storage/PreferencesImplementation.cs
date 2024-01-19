namespace MFToolkit.Storage;

class PreferencesImplementation : IPreferences
{
    public bool ContainsKey(string key, string sharedName) =>
        throw new("不支持或未实现的异常");

    public void Remove(string key, string sharedName) =>
        throw new("不支持或未实现的异常");

    public void Clear(string sharedName) =>
        throw new("不支持或未实现的异常");

    public void Set<T>(string key, T value, string sharedName) =>
        throw new("不支持或未实现的异常");

    public T Get<T>(string key, T defaultValue, string sharedName) =>
        throw new("不支持或未实现的异常");
}