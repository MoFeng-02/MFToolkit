namespace MFToolkit.Extensions;
/// <summary>
/// 时间的拓展
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 根据给定的时间来生成时间戳
    /// </summary>
    /// <param name="time">给定的时间</param>
    /// <returns></returns>
    public static long ToNowTimetamp(this DateTime time)
    {
        // 获取当前时间距离 Unix 时间戳起始点的时间间隔
        var span = time - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        // 将时间间隔转换为秒，并取整数部分
        var timestamp = (long)span.TotalSeconds;
        return timestamp;
    }
}
