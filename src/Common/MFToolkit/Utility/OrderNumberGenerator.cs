//// -- MFToolkit
//// -- 说明：暂时不考虑这个，对于例如服务器断电后会造成重新回访之前的值，所以暂时不进行此类开发
//// -- 可以使用SnowflakeId类提供的方法
//namespace MFToolkit.Utility;
///// <summary>
///// 订单Id生成器
///// </summary>
//public class OrderNumberGenerator
//{
//    /// <summary>
//    /// 线程锁
//    /// </summary>
//#if NET8_0
//    private static readonly object _lock = new();
//#elif NET9_0_OR_GREATER
//    private static readonly Lock _lock = new();
//#endif
//    /// <summary>
//    /// 刷新日期前，例如，此日期是2025-01-01，而当前实际日期是2025-01-02，则刷新所有参数
//    /// </summary>
//    private DateTime _refreshDate = DateTime.UtcNow.Date;
//    /// <summary>
//    /// 连续订单号
//    /// </summary>
//    private int _orderNumber = 1;
//    /// <summary>
//    /// 基首参数<see cref="BasicFirstNumber"/>
//    /// </summary>
//    private int _firstOrderNumber = 0;
//    /// <summary>
//    /// <see cref="BasicFirstNumber"/>
//    /// </summary>
//    private int _basicFirstNumber = 0;
//    /// <summary>
//    /// 最大连续订单号，准刷新参数为默认10000，即可参考：
//    /// <code>Write：20250101000001，为了更好分割理解：20250101 0 000001为起点，当000001为10000的时候，下一次就执行，<see cref="_orderNumber"/>累加BasicNumber+1</code>
//    /// </summary>
//    public int MaxOrderNumber { get; set; } = 10000;
//    /// <summary>
//    /// 基参数，默认为0，位于日期后面的参数，例如下面这样
//    /// <code>Write：202501010...</code>
//    /// <code>Write：ORD202501010...</code>
//    /// <para>具体可以拆分：20250101 0，这个0就是代表它</para>
//    /// </summary>
//    public int BasicFirstNumber
//    {
//        get
//        {
//            return _basicFirstNumber;
//        }
//        set
//        {
//            _firstOrderNumber = value;
//            _basicFirstNumber = value;
//        }
//    }
//    /// <summary>
//    /// 获取<see cref="MaxOrderNumber"/>参数的所选填充数
//    /// </summary>
//    private int MaxOrderNumberLength => MaxOrderNumber.ToString().Length;
//    /// <summary>
//    /// 前缀参数的D格式化占位值
//    /// </summary>
//    public int MaxFirstFormatNumber { get; set; } = 2;
//    /// <summary>
//    /// 刷新参数方法
//    /// </summary>
//    /// <param name="useUtc">是否使用UTC时间，默认UTC时间</param>
//    protected void RefreshParameter(bool useUtc = false)
//    {
//        var date = useUtc ? DateTime.UtcNow.Date : DateTime.Now.Date;
//        // 避免日期倒退
//        if (date < _refreshDate)
//        {
//            throw new InvalidOperationException($"日期异常，日期倒退！请检查系统是否正常，时间是否正常，最后刷新日期为：{_refreshDate}，实际倒退日期为：{date}");
//        }
//        // 如果具体日期大于了昨天，则进行数据初始化
//        if (date > _refreshDate)
//        {
//            _orderNumber = 1;
//            _refreshDate = date;
//            _firstOrderNumber = _basicFirstNumber;
//            return;
//        }
//        // 如果当前订单确定比最大值大了，就设置为1，并且前数字累加1
//        if (_orderNumber > MaxOrderNumber)
//        {
//            _orderNumber = 1;
//            _firstOrderNumber += 1;
//            return;
//        }
//    }
//    /// <summary>
//    /// 生成方法
//    /// </summary>
//    /// <param name="useUtc">是否使用UTC时间，默认UTC时间</param>
//    /// <returns>返回<see cref="MaxOrderNumber"/>范围内的数值</returns>
//    protected virtual string Generate(bool useUtc = true)
//    {
//        lock (_lock)
//        {
//            RefreshParameter();
//            var nextId = _orderNumber.ToString($"D{MaxOrderNumberLength}");
//            _orderNumber++;
//            return $"{_firstOrderNumber.ToString($"D{MaxFirstFormatNumber}")}{nextId}";
//        }
//    }
//    /// <summary>
//    /// 获取下一个ID，根据指定的时间类型获取
//    /// </summary>
//    /// <param name="useUtc">是否使用UTC时间，默认UTC时间</param>
//    /// <returns>格式化的订单ID</returns>
//    public string GetNextOrderId(bool useUtc = true)
//    {
//        return Generate(useUtc);
//    }
//    /// <summary>
//    /// 自定义前缀订单ID，带时间格式
//    /// </summary>
//    /// <param name="prefix">订单号前缀，默认为 "ORD" </param>
//    /// <param name="dateFormat">日期格式字符串，默认为 "yyyyMMdd" </param>
//    /// <param name="format">订单号格式模板，可以包含以下占位符：
//    ///     {prefix} - 前缀
//    ///     {date:yyyyMMdd} - 当前日期，格式可自定义
//    ///     {id} - 订单ID
//    /// </param>
//    /// <param name="useUtc">是否使用UTC时间，默认为true</param>
//    /// <returns>带有前缀和时间信息的订单Id，适用于订单方面等场景</returns>
//    public string NextPrefixedOrderId(string prefix = "ORD", string dateFormat = "yyyyMMdd", string format = "{prefix}{date:yyyyMMdd}{id}", bool useUtc = true)
//    {
//        var nextId = Generate(useUtc);
//        var now = useUtc ? DateTime.UtcNow : DateTime.Now;

//        // 构建订单号：根据提供的格式模板
//        var orderNumber = format
//            .Replace("{prefix}", prefix.ToUpper())
//            .Replace("{date:" + dateFormat + "}", now.ToString(dateFormat))
//            .Replace("{id}", nextId);

//        return orderNumber;
//    }
//}
///// <summary>
///// 订单号工具类
///// </summary>
//public class OrderNumberUtils
//{
//    private static OrderNumberGenerator _default;
//    public static readonly OrderNumberGenerator Default = _default ??= new OrderNumberGenerator();
//    public OrderNumberUtils(OrderNumberGenerator generator)
//    {
//        if (_default != null) return;
//        _default = generator;
//    }

//    /// <summary>
//    /// 获取下一个ID，根据指定的时间类型获取
//    /// </summary>
//    /// <param name="useUtc">是否使用UTC时间，默认UTC时间</param>
//    /// <returns>格式化的订单ID</returns>
//    public static string GetNextOrderId(bool useUtc = true) => Default.GetNextOrderId(useUtc);
//    public static string NextPrefixedOrderId(string prefix = "ORD", string dateFormat = "yyyyMMdd", string format = "{prefix}{date:yyyyMMdd}{id}", bool useUtc = true) => Default.NextPrefixedOrderId(prefix, dateFormat, format, useUtc);
//}