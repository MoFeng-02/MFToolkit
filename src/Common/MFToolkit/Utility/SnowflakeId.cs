namespace MFToolkit.Utility;

public class SnowflakeId
{
    /// <summary>
    /// 当前机器的workerId
    /// <para>建议每个服务器配置一个唯一ID</para>
    /// </summary>
    public static long WorkId = 1;
    /// <summary>
    /// 当前机器的数据中心ID
    /// <para>建议每个服务器配置一个唯一ID</para>
    /// </summary>
    public static long DatacenterId = 1;
    /// <summary>
    /// 自定义纪元时间戳（单位：毫秒），可以根据实际情况调整，默认起始时间戳（2023-01-01）
    /// </summary>
    public static long Twepoch = 1672531200000L;

    private static SnowflakeIdGenerator? _defaultSnowflakeIdGenerator;
    public static SnowflakeIdGenerator DefaultSnowflakeIdGenerator => _defaultSnowflakeIdGenerator ??= new(WorkId, DatacenterId, Twepoch);

    /// <summary>
    /// 获取下一个唯一ID。
    /// </summary>
    /// <returns>返回一个64位整数的唯一ID</returns>
    public static long NextId() => DefaultSnowflakeIdGenerator.NextId();

}
public class SnowflakeIdGenerator
{
#if NET8_0
    private static readonly object _lock = new();
#elif NET9_0_OR_GREATER
    private static readonly Lock _lock = new();
#endif
    // 自定义纪元时间戳（单位：毫秒），可以根据实际情况调整、起始时间戳（2023-01-01）
    private readonly long Twepoch = 1672531200000L;

    // workerId占用的位数
    private const int WorkerIdBits = 5;
    // datacenterId占用的位数
    private const int DatacenterIdBits = 5;
    // 序列号占用的位数
    private const int SequenceBits = 12;

    // 最大workerId值
    private const int MaxWorkerId = -1 ^ (-1 << WorkerIdBits);
    // 最大数据中心ID值
    private const int MaxDatacenterId = -1 ^ (-1 << DatacenterIdBits);

    // workerId左移位数（12位）
    private const int WorkerIdShift = SequenceBits;
    // datacenterId左移位数（12+5=17位）
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
    // 时间戳左移位数（12+5+5=22位）
    private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
    // 序列号掩码，用来确保序列号在12位内循环
    private const int SequenceMask = -1 ^ (-1 << SequenceBits);

    // 当前机器的workerId
    private long _workerId;
    // 当前机器的数据中心ID
    private long _datacenterId;
    // 当前毫秒内的序列号
    private long _sequence = 0L;

    // 上次生成ID的时间戳
    private long _lastTimestamp = -1L;

    /// <summary>
    /// 构造函数，初始化SnowflakeIdWorker实例。
    /// </summary>
    /// <param name="workerId">工作节点ID</param>
    /// <param name="datacenterId">数据中心ID</param>
    /// <param name="twepoch">自定义纪元时间戳（单位：毫秒），可以根据实际情况调整，默认起始时间戳（2023-01-01）</param>
    public SnowflakeIdGenerator(long workerId, long datacenterId, long? twepoch = null)
    {
        if (workerId > MaxWorkerId || workerId < 0)
        {
            throw new ArgumentException($"worker Id can't be greater than {MaxWorkerId} or less than 0");
        }
        if (datacenterId > MaxDatacenterId || datacenterId < 0)
        {
            throw new ArgumentException($"datacenter Id can't be greater than {MaxDatacenterId} or less than 0");
        }

        _workerId = workerId;
        _datacenterId = datacenterId;
        if (twepoch != null) Twepoch = twepoch.Value;
    }

    /// <summary>
    /// 获取下一个唯一ID。
    /// </summary>
    /// <returns>返回一个64位整数的唯一ID</returns>
    public long NextId()
    {
        lock (_lock) // 确保线程安全
        {
            var timestamp = TimeGen();

            // 如果当前时间小于上次生成ID的时间，说明时钟回退，抛出异常
            if (timestamp < _lastTimestamp)
            {
                throw new Exception("Clock moved backwards. Refusing to generate id for " + (_lastTimestamp - timestamp) + " milliseconds");
            }

            // 如果在同一毫秒内，则对序列号进行+1操作 
            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                // 如果同一毫秒内的序列号溢出，则等待下一毫秒
                if (_sequence == 0)
                {
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                // 不是同一毫秒，重置序列号为0
                _sequence = 0L;
            }

            // 更新上次生成ID的时间戳
            _lastTimestamp = timestamp;

            // 按照位运算组合成最终的ID
            return ((timestamp - Twepoch) << TimestampLeftShift) |
                   (_datacenterId << DatacenterIdShift) |
                   (_workerId << WorkerIdShift) |
                   _sequence;
        }
    }

    /// <summary>
    /// 等待直到获取到一个比上一次更大的时间戳。
    /// </summary>
    /// <param name="lastTimestamp">上次的时间戳</param>
    /// <returns>新的时间戳</returns>
    protected virtual long TilNextMillis(long lastTimestamp)
    {
        var timestamp = TimeGen();
        while (timestamp <= lastTimestamp)
        {
            timestamp = TimeGen();
        }
        return timestamp;
    }

    /// <summary>
    /// 获取当前时间戳（单位：毫秒）。
    /// </summary>
    /// <returns>当前时间的时间戳</returns>
    protected virtual long TimeGen()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}