using StackExchange.Redis;

namespace MFToolkit.Garnet.Interfaces;

/// <summary>
/// 提供与 Garnet 服务器交互的一组操作。
/// </summary>
public interface IGarnetService : IDisposable
{
    // 基础键值操作

    /// <summary>
    /// 设置指定键的字符串值。
    /// </summary>
    Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null);

    /// <summary>
    /// 获取指定键的字符串值。
    /// </summary>
    Task<string> StringGetAsync(string key);

    /// <summary>
    /// 删除一个或多个键。
    /// </summary>
    Task<long> KeyDeleteAsync(params string[] keys);

    /// <summary>
    /// 判断给定键是否存在。
    /// </summary>
    Task<bool> KeyExistsAsync(string key);

    /// <summary>
    /// 为给定键设置过期时间。
    /// </summary>
    Task<bool> KeyExpireAsync(string key, TimeSpan expiry);

    /// <summary>
    /// 获取给定键的剩余生存时间。
    /// </summary>
    Task<TimeSpan?> KeyTimeToLiveAsync(string key);

    // 列表操作

    /// <summary>
    /// 向列表的左侧添加一个元素。
    /// </summary>
    Task<long> ListLeftPushAsync(string key, string value);

    /// <summary>
    /// 从列表的右侧弹出一个元素。
    /// </summary>
    Task<string> ListRightPopAsync(string key);

    /// <summary>
    /// 获取列表中指定范围内的元素。
    /// </summary>
    Task<List<string>> ListRangeAsync(string key, long start, long stop);

    // 集合操作

    /// <summary>
    /// 向集合中添加一个成员。
    /// </summary>
    Task<bool> SetAddAsync(string key, string member);

    /// <summary>
    /// 判断成员是否存在于集合中。
    /// </summary>
    Task<bool> SetContainsAsync(string key, string member);

    /// <summary>
    /// 获取集合中的所有成员。
    /// </summary>
    Task<HashSet<string>> SetMembersAsync(string key);

    // 有序集合操作

    /// <summary>
    /// 向有序集合中添加一个成员，或者更新其分数。
    /// </summary>
    Task<bool> SortedSetAddAsync(string key, string member, double score);

    /// <summary>
    /// 获取有序集合中指定排名范围内的成员。
    /// </summary>
    Task<List<string>> SortedSetRangeByRankAsync(string key, long start, long stop);

    /// <summary>
    /// 获取有序集合中成员的排名。
    /// </summary>
    Task<long?> SortedSetRankAsync(string key, string member);

    // 发布/订阅操作

    /// <summary>
    /// 发布消息到指定频道。
    /// </summary>
    Task<long> PublishAsync(RedisChannel channel, string message);

    /// <summary>
    /// 订阅指定的频道，并在接收到消息时调用回调函数。
    /// </summary>
    void SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> messageReceivedHandler);

    // 事务处理

    /// <summary>
    /// 开始一个新的事务。
    /// </summary>
    ITransaction BeginTransaction();

    // 管道操作

    /// <summary>
    /// 使用管道发送多个命令并批量接收结果。
    /// </summary>
    /// <param name="commands">命令集合</param>
    /// <returns></returns>
    Task<IEnumerable<RedisResult>> PipelineExecuteAsync(IEnumerable<Func<Task<RedisResult>>> commands);

    // 哈希表操作

    /// <summary>
    /// 设置哈希表中的字段。
    /// </summary>
    Task<bool> HashSetAsync(string key, RedisValue field, string value);

    /// <summary>
    /// 获取哈希表中的字段值。
    /// </summary>
    Task<RedisValue> HashGetAsync(string key, string field);

    /// <summary>
    /// 获取哈希表中所有字段和值。
    /// </summary>
    Task<Dictionary<string, string>> HashGetAllAsync(string key);

    /// <summary>
    /// 检查哈希表中字段是否存在。
    /// </summary>
    Task<bool> HashExistsAsync(string key, string field);

    /// <summary>
    /// 删除哈希表中的字段。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    Task<bool> HashDeleteAsync(string key, RedisValue field);

    // 锁机制

    /// <summary>
    /// 尝试获取一个分布式锁。
    /// </summary>
    Task<bool> TryAcquireLockAsync(string key, string value, TimeSpan expiry);

    /// <summary>
    /// 释放一个分布式锁。
    /// </summary>
    Task<bool> ReleaseLockAsync(string key, string value);

    // 地理位置操作

    /// <summary>
    /// 向有序集合中添加一个带有地理坐标的成员。
    /// </summary>
    Task<bool> GeoAddAsync(string key, double longitude, double latitude, string member);

    /// <summary>
    /// 根据给定的半径查询附近的成员。
    /// </summary>
    Task<IEnumerable<(string Member, double Distance)>> GeoRadiusAsync(string key, string member, double radius, GeoUnit unit);

    Task<RedisResult> ExecuteAsync(string command, params object[] args);
}

