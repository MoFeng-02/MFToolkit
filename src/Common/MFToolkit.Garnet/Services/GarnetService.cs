using MFToolkit.Garnet.Interfaces;
using StackExchange.Redis;

namespace MFToolkit.Garnet.Services;
/// <summary>
/// 使用StackExchange.Redisd的Garnet提供类
/// </summary>
public class GarnetService(IConnectionMultiplexer redis, IDatabase db, ISubscriber subscriber) : IGarnetService
{

    // 基础键值操作

    public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await db.StringSetAsync(key, value, expiry: expiry);
    }

    public async Task<string> StringGetAsync(string key)
    {
        return (await db.StringGetAsync(key)).ToString();
    }

    public async Task<long> KeyDeleteAsync(params string[] keys)
    {
        return await db.KeyDeleteAsync([.. keys.Select(k => (RedisKey)k)]);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await db.KeyExistsAsync(key);
    }

    public async Task<bool> KeyExpireAsync(string key, TimeSpan expiry)
    {
        return await db.KeyExpireAsync(key, expiry);
    }

    public async Task<TimeSpan?> KeyTimeToLiveAsync(string key)
    {
        return await db.KeyTimeToLiveAsync(key);
    }

    // 列表操作

    public async Task<long> ListLeftPushAsync(string key, string value)
    {
        return await db.ListLeftPushAsync(key, value);
    }

    public async Task<string> ListRightPopAsync(string key)
    {
        return (await db.ListRightPopAsync(key)).ToString();
    }

    public async Task<List<string>> ListRangeAsync(string key, long start, long stop)
    {
        var redisValues = await db.ListRangeAsync(key, start, stop);
        return [.. redisValues.Select(rv => rv.ToString())];
    }

    // 集合操作

    public async Task<bool> SetAddAsync(string key, string member)
    {
        return await db.SetAddAsync(key, member);
    }

    public async Task<bool> SetContainsAsync(string key, string member)
    {
        return await db.SetContainsAsync(key, member);
    }

    public async Task<HashSet<string>> SetMembersAsync(string key)
    {
        var redisValues = await db.SetMembersAsync(key);
        return [.. redisValues.Select(rv => rv.ToString())];
    }

    // 有序集合操作

    public async Task<bool> SortedSetAddAsync(string key, string member, double score)
    {
        return await db.SortedSetAddAsync(key, member, score);
    }

    public async Task<List<string>> SortedSetRangeByRankAsync(string key, long start, long stop)
    {
        var redisValues = await db.SortedSetRangeByRankAsync(key, start, stop);
        return [.. redisValues.Select(rv => rv.ToString())];
    }

    public async Task<long?> SortedSetRankAsync(string key, string member)
    {
        var rank = await db.SortedSetRankAsync(key, member);
        return rank.HasValue ? rank.Value : null;
    }

    // 发布/订阅操作

    public async Task<long> PublishAsync(RedisChannel channel, string message)
    {
        return await subscriber.PublishAsync(channel, message);
    }

    public void SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> messageReceivedHandler)
    {
        subscriber.Subscribe(channel, messageReceivedHandler);
    }

    // 事务处理

    public ITransaction BeginTransaction()
    {
        return db.CreateTransaction();
    }

    // 管道操作

    public async Task<IEnumerable<RedisResult>> PipelineExecuteAsync(IEnumerable<Func<Task<RedisResult>>> commands)
    {
        // 创建一个列表来存储所有的任务
        var tasks = new List<Task<RedisResult>>();

        // 将所有命令添加到任务列表中
        foreach (var command in commands)
        {
            tasks.Add(command());
        }

        // 并行等待所有任务完成，并获取结果
        var results = await Task.WhenAll(tasks);

        return results;
    }

    // 哈希表操作

    public async Task<bool> HashSetAsync(string key, RedisValue field, string value)
    {
        return await db.HashSetAsync(key, field, value);
    }

    public async Task<RedisValue> HashGetAsync(string key, string field)
    {
        return await db.HashGetAsync(key, field);
    }

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        var entries = await db.HashGetAllAsync(key);
        return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
    }

    public async Task<bool> HashExistsAsync(string key, string field)
    {
        return await db.HashExistsAsync(key, field);
    }

    public Task<bool> HashDeleteAsync(string key, RedisValue field)
    {
        return db.HashDeleteAsync(key, field);
    }

    // 锁机制

    public async Task<bool> TryAcquireLockAsync(string key, string value, TimeSpan expiry)
    {
        return await db.LockTakeAsync(key, value, expiry);
    }

    public async Task<bool> ReleaseLockAsync(string key, string value)
    {
        return await db.LockReleaseAsync(key, value);
    }

    // 地理位置操作

    public async Task<bool> GeoAddAsync(string key, double longitude, double latitude, string member)
    {
        return await db.GeoAddAsync(key, new GeoEntry(longitude, latitude, member));
    }

    public async Task<IEnumerable<(string Member, double Distance)>> GeoRadiusAsync(string key, string member, double radius, GeoUnit unit)
    {
        var geoResults = await db.GeoRadiusAsync(key, member, radius, unit);
        return (IEnumerable<(string Member, double Distance)>)geoResults.Select(gr => (gr.Member.ToString(), gr.Distance));
    }

    // 实现 IDisposable

    public void Dispose()
    {
        redis?.Dispose();
    }

    public Task<RedisResult> ExecuteAsync(string command, params object[] args)
    {
        return db.ExecuteAsync(command, args);
    }
}