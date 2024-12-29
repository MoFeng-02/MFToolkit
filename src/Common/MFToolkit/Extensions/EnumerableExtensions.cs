namespace MFToolkit.Extensions;
public static class EnumerableExtensions
{
    /// <summary>
    /// 将输入的序列按照指定大小分批处理，并逐个返回批次。
    /// <para>每个批次都是一个 List{T} 类型，其中包含至多 size 个元素。</para>
    /// <para>如果最后一个批次的元素数量少于 size，则会返回一个包含剩余元素的批次。</para>
    /// </summary>
    /// <typeparam name="T">源序列中元素的类型。</typeparam>
    /// <param name="source">要分批处理的源序列。</param>
    /// <param name="size">每个批次的最大元素数量。必须是正数。</param>
    /// <returns>
    /// 返回一个 IEnumerable{List{T}}，它在迭代时会逐个返回分批后的列表，
    /// 每个列表包含至多 size 个元素。
    /// </returns>
    /// <exception cref="ArgumentNullException">如果 source 是 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">如果 size 小于或等于零。</exception>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        // 参数检查：确保 source 不为 null 和 size 大于零。
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size, nameof(size));

        // 创建一个新的 List<T> 实例，并预分配 size 大小的空间以提高性能。
        var list = new List<T>(size);

        // 遍历源集合中的每一个元素。
        foreach (var item in source)
        {
            // 将当前元素添加到 list 中。
            list.Add(item);

            // 如果 list 中的元素数量达到了指定的大小，则返回当前 batch。
            if (list.Count >= size)
            {
                // 使用 yield return 返回当前 batch 的列表，并暂停方法的执行。
                // 下次迭代时，将从这里继续执行。
                yield return list;

                // 创建一个新的 List<T> 用于收集下一个 batch 的元素。
                list = new List<T>(size);
            }
        }

        // 如果在遍历结束后，list 中仍然有未处理的元素（即最后一批元素的数量小于 size），
        // 则返回剩余的元素。
        if (list.Count != 0)
        {
            // 返回最后一个 batch，即使它包含的元素少于 size。
            yield return list;
        }
    }
}
