using System.Globalization;

namespace MFToolkit.Types;

/// <summary>
/// 表示金额和货币类型的不可变类。
/// </summary>
public class Money : IComparable, IEquatable<Money>, ICloneable
{
    private decimal amount; // 存储金额
    public string Currency { get; } // 货币类型

    /// <summary>
    /// 构造函数，初始化金额和货币类型
    /// </summary>
    /// <param name="amount">金额值</param>
    /// <param name="currency">货币代码（如 "CNY", "USD" 等）</param>
    /// <exception cref="ArgumentException">如果货币代码为空或仅包含空白字符时抛出</exception>
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("货币代码不能为空。", nameof(currency));

        this.amount = Math.Round(amount, 2); // 将金额四舍五入到两位小数。
        this.Currency = currency.ToUpperInvariant(); // 使用大写的货币代码
    }

    /// <summary>
    /// 创建一个新的 Money 对象，使用静态工厂方法
    /// </summary>
    /// <param name="amount">金额值</param>
    /// <param name="currency">货币代码</param>
    /// <returns>新的 Money 对象</returns>
    public static Money Create(decimal amount, string currency) => new Money(amount, currency);

    /// <summary>
    /// 获取此实例的哈希码
    /// </summary>
    /// <returns>一个适合用于哈希算法和哈希表（如字典或哈希集合）的哈希码。</returns>
    public override int GetHashCode() => HashCode.Combine(amount, Currency);

    /// <summary>
    /// 实现对象相等性检查
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>如果当前实例与指定对象相等，则为 true；否则为 false。</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Money money) return Equals(money);
        return false;
    }

    /// <summary>
    /// 检查两个 Money 对象是否相等
    /// </summary>
    /// <param name="other">要比较的另一个 Money 对象</param>
    /// <returns>如果两个 Money 对象相等，则为 true；否则为 false。</returns>
    public bool Equals(Money? other) => other! != null! && Currency == other.Currency && amount == other.amount;

    /// <summary>
    /// 实现等于运算符重载
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>如果 a 和 b 相等则返回 true；否则返回 false。</returns>
    public static bool operator ==(Money a, Money b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
        return a.Equals(b);
    }

    /// <summary>
    /// 实现不等于运算符重载
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>如果 a 和 b 不相等则返回 true；否则返回 false。</returns>
    public static bool operator !=(Money a, Money b) => !(a == b);

    /// <summary>
    /// 实现加法运算符重载，用于两个相同货币类型的金额相加
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>两个 Money 对象相加后的结果</returns>
    /// <exception cref="InvalidOperationException">如果两个 Money 对象的货币类型不同则抛出</exception>
    public static Money operator +(Money a, Money b)
    {
        CheckCurrencyMatch(a, b);
        return new Money(a.amount + b.amount, a.Currency);
    }

    /// <summary>
    /// 实现减法运算符重载，用于两个相同货币类型的金额相减
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>两个 Money 对象相减后的结果</returns>
    /// <exception cref="InvalidOperationException">如果两个 Money 对象的货币类型不同则抛出</exception>
    public static Money operator -(Money a, Money b)
    {
        CheckCurrencyMatch(a, b);
        return new Money(a.amount - b.amount, a.Currency);
    }

    /// <summary>
    /// 实现乘法运算符重载，允许金额与数字相乘
    /// </summary>
    /// <param name="a">Money 对象</param>
    /// <param name="factor">乘数</param>
    /// <returns>Money 对象乘以乘数后的结果</returns>
    public static Money operator *(Money a, decimal factor)
    {
        return new Money(a.amount * factor, a.Currency);
    }

    /// <summary>
    /// 实现除法运算符重载，允许金额被数字除
    /// </summary>
    /// <param name="a">Money 对象</param>
    /// <param name="divisor">除数</param>
    /// <returns>Money 对象除以除数后的结果</returns>
    /// <exception cref="DivideByZeroException">如果除数为零则抛出</exception>
    public static Money operator /(Money a, decimal divisor)
    {
        if (divisor == 0m)
            throw new DivideByZeroException("不能除以零。");
        return new Money(a.amount / divisor, a.Currency);
    }

    /// <summary>
    /// 检查货币是否匹配
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <exception cref="ArgumentNullException">如果任一参数为 null 则抛出</exception>
    /// <exception cref="InvalidOperationException">如果两个 Money 对象的货币类型不同则抛出</exception>
    private static void CheckCurrencyMatch(Money a, Money b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        if (a.Currency != b.Currency) throw new InvalidOperationException("必须使用相同的货币进行操作。");
    }

    /// <summary>
    /// 实现比较方法，用于比较两个金额的大小
    /// </summary>
    /// <param name="other">要比较的另一个 Money 对象</param>
    /// <returns>一个整数值，表示当前实例与另一个对象之间的相对顺序</returns>
    /// <exception cref="InvalidOperationException">如果两个 Money 对象的货币类型不同则抛出</exception>
    public int CompareTo(object? other)
    {
        if (other == null) return 1;
        if (!(other is Money money)) throw new ArgumentException("参数必须是 Money 类型。");
        CheckCurrencyMatch(this, money);
        return this.amount.CompareTo(money.amount);
    }

    /// <summary>
    /// 比较两个 Money 对象，判断是否大于
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>如果 a 大于 b 则返回 true；否则返回 false。</returns>
    public static bool GreaterThan(Money a, Money b) => a.CompareTo(b) > 0;

    /// <summary>
    /// 比较两个 Money 对象，判断是否小于
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>如果 a 小于 b 则返回 true；否则返回 false。</returns>
    public static bool LessThan(Money a, Money b) => a.CompareTo(b) < 0;

    /// <summary>
    /// 比较两个 Money 对象，判断是否大于等于
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>如果 a 大于等于 b 则返回 true；否则返回 false。</returns>
    public static bool GreaterThanOrEqual(Money a, Money b) => a.CompareTo(b) >= 0;

    /// <summary>
    /// 比较两个 Money 对象，判断是否小于等于
    /// </summary>
    /// <param name="a">第一个 Money 对象</param>
    /// <param name="b">第二个 Money 对象</param>
    /// <returns>如果 a 小于等于 b 则返回 true；否则返回 false。</returns>
    public static bool LessThanOrEqual(Money a, Money b) => a.CompareTo(b) <= 0;

    /// <summary>
    /// 重写 ToString 方法，提供格式化的金额输出
    /// </summary>
    /// <param name="format">格式字符串</param>
    /// <param name="provider">格式提供者</param>
    /// <returns>格式化后的金额字符串</returns>
    public string ToString(string format, IFormatProvider? provider = null)
    {
        provider ??= new CultureInfo(this.Currency.ToLowerInvariant());
        return amount.ToString(format ?? "C", provider); // 使用文化特定的格式化字符串来显示货币
    }

    /// <summary>
    /// 克隆方法，创建当前 Money 对象的副本
    /// </summary>
    /// <returns>当前 Money 对象的一个浅拷贝</returns>
    public object Clone()
    {
        return MemberwiseClone();
    }

    /// <summary>
    /// 从字符串解析出 Money 对象
    /// </summary>
    /// <param name="value">表示金额的字符串</param>
    /// <param name="currency">货币代码</param>
    /// <returns>新的 Money 对象</returns>
    /// <exception cref="FormatException">如果无法将提供的字符串解析为有效的金额则抛出</exception>
    public static Money Parse(string value, string currency)
    {
        if (!decimal.TryParse(value, out decimal amount))
            throw new FormatException("无法将提供的字符串解析为有效的金额。");

        return new Money(amount, currency);
    }

    /// <summary>
    /// 转换货币
    /// </summary>
    /// <param name="targetCurrency">目标货币代码</param>
    /// <param name="exchangeRateProvider">汇率提供者委托，根据源货币和目标货币返回当前汇率</param>
    /// <returns>转换后的新 Money 对象</returns>
    /// <exception cref="ArgumentNullException">如果汇率提供者为 null 则抛出</exception>
    /// <exception cref="InvalidOperationException">如果源货币和目标货币相同则直接返回原对象</exception>
    public Money ConvertTo(string targetCurrency, Func<string, string, decimal> exchangeRateProvider)
    {
        if (exchangeRateProvider == null)
            throw new ArgumentNullException(nameof(exchangeRateProvider));

        if (this.Currency == targetCurrency)
            return this;

        var rate = exchangeRateProvider(this.Currency, targetCurrency);
        var convertedAmount = Math.Round(this.amount * rate, 2);
        return new Money(convertedAmount, targetCurrency);
    }

    /// <summary>
    /// 自定义异常类，用于处理货币操作中的错误
    /// </summary>
    [Serializable]
    public class MoneyOperationException : Exception
    {
        /// <summary>
        /// 初始化 MoneyOperationException 的新实例
        /// </summary>
        public MoneyOperationException() { }

        /// <summary>
        /// 使用指定的错误消息初始化 MoneyOperationException 的新实例
        /// </summary>
        /// <param name="message">描述错误的消息</param>
        public MoneyOperationException(string message) : base(message) { }

        /// <summary>
        /// 使用指定的错误消息和导致此异常的内部异常初始化 MoneyOperationException 的新实例
        /// </summary>
        /// <param name="message">描述错误的消息</param>
        /// <param name="innerException">导致当前异常的异常；如果未指定内部异常，则是一个空引用</param>
        public MoneyOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}