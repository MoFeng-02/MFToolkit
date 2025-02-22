namespace MFToolkit.Utility;
/// <summary>
/// 雪花Id工具类
/// </summary>
public class SnowflakeId
{
    /// <summary>
    /// 当前机器的workerId
    /// <para>建议每个服务器配置一个唯一ID</para>
    /// </summary>
    private static long WorkerId = 1;
    /// <summary>
    /// 当前机器的数据中心ID
    /// <para>建议每个服务器配置一个唯一ID</para>
    /// </summary>
    private static long DatacenterId = 1;
    /// <summary>
    /// 自定义纪元时间戳（单位：毫秒），可以根据实际情况调整，默认起始时间戳（2023-01-01）
    /// </summary>
    private static long Twepoch = 1672531200000L;

    private static SnowflakeIdGenerator? _defaultSnowflakeIdGenerator;
    /// <summary>
    /// 生成器
    /// </summary>
    public static SnowflakeIdGenerator DefaultSnowflakeIdGenerator => _defaultSnowflakeIdGenerator ??= new(WorkerId, DatacenterId, Twepoch);

    /// <summary>
    /// 将给定的64位整数转换为Base62编码的字符串。
    /// </summary>
    /// <param name="num">要编码的64位整数。</param>
    /// <returns>转换后的Base62编码字符串。</returns>
    public static string EncodeToBase62(long num)
    {
        // 定义Base62字符集：数字、大写字母、小写字母
        const string baseChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        // 使用栈来存储结果字符，因为我们将从最低有效位开始构建字符串
        var result = new Stack<char>();

        // 当输入数字大于0时，继续进行编码
        while (num > 0)
        {
            // 取得当前数字除以62的余数，并用它作为索引在baseChars中取得相应的字符
            result.Push(baseChars[(int)(num % 62)]);

            // 更新num为原数值除以62的结果，准备处理下一位
            num /= 62;
        }

        // 如果原始数字是0，则直接返回"0"
        return result.Count == 0 ? "0" : new string([.. result]);
    }

    /// <summary>
    /// 获取下一个唯一ID。
    /// </summary>
    /// <returns>返回一个64位整数的唯一ID</returns>
    public static long NextId() => DefaultSnowflakeIdGenerator.NextId();

    /// <summary>
    /// 生成下一个雪花算法ID并返回其字符串形式。
    /// </summary>
    /// <returns>雪花算法生成的ID的字符串表示</returns>
    public static string GenerateSnowflakeStringId() => DefaultSnowflakeIdGenerator.NextId().ToString();

    /// <summary>
    /// 自定义前缀雪花ID，带时间格式
    /// <code>OrderNumberGenerator.GeneratePrefixedSnowflakeId("ORD"); 输出类似 "20250102123456789012345678"</code>
    /// <code>SnowflakeId.GeneratePrefixedSnowflakeId(dateFormat: "yyMMddHHmmss",format: "{prefix}{date:yyMMddHHmmss}{id}"); 输出类似 "250102153045123456789012345678"</code>
    /// <code>OrderNumberGenerator.GeneratePrefixedSnowflakeId(prefix: "INV", dateFormat: "yyMMdd", format: "{prefix}-{date:yyMMdd}-{id}"); 输出类似 "INV-250102-123456789012345678"</code>
    /// </summary>
    /// <param name="prefix">订单号前缀，默认为 "ORD" </param>
    /// <param name="dateFormat">日期格式字符串，默认为 "yyyyMMdd" </param>
    /// <param name="format">订单号格式模板，可以包含以下占位符：
    ///     {prefix} - 前缀
    ///     {date:yyyyMMdd} - 当前日期，格式可自定义
    ///     {id} - 雪花算法生成的ID
    /// </param>
    /// <param name="useUtc">是否使用UTC时间，默认为true</param>
    /// <returns>带有前缀和时间信息的雪花Id，适用于订单方面等场景</returns>
    public static string GeneratePrefixedSnowflakeId(string prefix = "", string dateFormat = "yyyyMMdd", string format = "{prefix}{date:yyyyMMdd}{id}", bool useUtc = true)
    {
        var snowflakeId = DefaultSnowflakeIdGenerator.NextId();
        var now = useUtc ? DateTime.UtcNow : DateTime.Now;

        // 构建订单号：根据提供的格式模板
        var orderNumber = format
            .Replace("{prefix}", prefix.ToUpper())
            .Replace("{date:" + dateFormat + "}", now.ToString(dateFormat))
            .Replace("{id}", snowflakeId.ToString());

        return orderNumber;
    }
    /// <summary>
    /// 生成一个优化后的雪花算法ID并格式化为订单号。
    /// </summary>
    /// <param name="prefix">订单号前缀，默认为空。</param>
    /// <param name="dateFormat">日期格式字符串，默认为 "yyyyMMdd"。</param>
    /// <param name="format">订单号格式模板，可以包含以下占位符：
    ///     {prefix} - 前缀
    ///     {date:yyyyMMdd} - 当前日期，格式可自定义
    ///     {id} - 雪花算法生成的ID，经过Base62编码
    /// </param>
    /// <param name="useUtc">是否使用UTC时间，默认为true。</param>
    /// <returns>带有前缀和时间信息的优化后雪花ID，适用于订单方面等场景。</returns>
    public static string GenerateOptimizedSnowflakeId(string prefix = "", string dateFormat = "yyyyMMdd", string format = "{prefix}{date:yyyyMMdd}{id}", bool useUtc = true)
    {
        // 获取下一个唯一的雪花ID
        var snowflakeId = DefaultSnowflakeIdGenerator.NextId();

        // 对雪花ID进行Base62编码以缩短长度
        var encodedId = EncodeToBase62(snowflakeId);

        // 根据是否使用UTC时间获取当前日期时间
        var now = useUtc ? DateTime.UtcNow : DateTime.Now;

        // 构建订单号：根据提供的格式模板替换占位符
        var orderNumber = format
            .Replace("{prefix}", prefix.ToUpper()) // 将前缀转为大写
            .Replace("{date:" + dateFormat + "}", now.ToString(dateFormat)) // 插入格式化后的日期
            .Replace("{id}", encodedId); // 插入Base62编码后的ID

        return orderNumber;
    }

    /// <summary>
    /// 构造函数，初始化SnowflakeIdWorker实例。
    /// </summary>
    /// <param name="workerId">工作节点ID</param>
    /// <param name="datacenterId">数据中心ID</param>
    /// <param name="twepoch">自定义纪元时间戳（单位：毫秒），可以根据实际情况调整，默认起始时间戳（2023-01-01）</param>
    public SnowflakeId(long workerId, long datacenterId, long? twepoch = null)
    {
        WorkerId = workerId;
        DatacenterId = datacenterId;
        Twepoch = twepoch ?? 0;
        _defaultSnowflakeIdGenerator = new(WorkerId, DatacenterId, Twepoch);
    }

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