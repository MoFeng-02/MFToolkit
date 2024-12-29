namespace MFToolkit.Extensions;
/// <summary>
/// 时间的拓展
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 根据给定的时间来生成时间戳\秒时间戳
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static long ToNowTimetampInSeconds(this DateTime time)
    {
        // 计算从1970年1月1日00:00:00 UTC到现在的时间差
        long timestampInSeconds = (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        return timestampInSeconds;
    }
    /// <summary>
    /// 根据给定的时间来生成时间戳\毫秒时间戳
    /// </summary>
    /// <param name="time">给定的时间</param>
    /// <returns></returns>
    public static long ToNowTimetampInMilliseconds(this DateTime time)
    {
        // 转换为 UTC 时间
        DateTime utcTime = time.ToUniversalTime();

        // 获取自 Unix 纪元以来的总毫秒数（时间戳）
        long timestampInMilliseconds = (long)(utcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        return timestampInMilliseconds;
    }
}
