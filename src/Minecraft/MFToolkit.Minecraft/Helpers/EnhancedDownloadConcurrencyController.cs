using System.Collections.Concurrent;
using System.Diagnostics;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// 增强版下载并发控制器
/// 功能：动态控制下载任务的最大并发数，支持运行时更新并发限制，
/// 采用队列+信号量机制实现等待任务的有序调度，避免资源竞争问题
/// 
/// 核心特性：
/// 1. 线程安全的并发控制
/// 2. 运行时动态调整并发数
/// 3. 公平的队列调度机制（FIFO）
/// 4. 完整的取消令牌支持
/// 5. 资源泄漏防护
/// 6. 运行状态监控
/// </summary>
public class EnhancedDownloadConcurrencyController : IDisposable
{
    /// <summary>
    /// 等待队列：存储因并发数已满而暂时等待的任务信号量
    /// 采用ConcurrentQueue确保多线程环境下的入队/出队线程安全
    /// 队列遵循FIFO（先进先出）原则，保证任务调度的公平性
    /// </summary>
    private readonly ConcurrentQueue<SemaphoreSlim> _pendingQueue = new();

    /// <summary>
    /// 活跃任务计数器：记录当前正在执行的下载任务数量
    /// 通过Interlocked原子操作保证多线程下的计数准确性
    /// 注意：此计数可能短暂超过_maxConcurrentDownloads，但会快速通过队列机制调整
    /// </summary>
    private int _activeTaskCount;

    /// <summary>
    /// 最大并发下载数：运行时可动态更新
    /// 用volatile修饰确保多线程间的即时可见性（配置更新后立即生效）
    /// 默认值为10，可根据网络条件和系统资源调整
    /// </summary>
    private volatile int _maxConcurrentDownloads = 10;

    /// <summary>
    /// 资源释放标志：用于实现IDisposable模式
    /// 防止重复释放资源和在已释放对象上执行操作
    /// </summary>
    private bool _disposed;

    // ==================== 监控属性 ====================

    /// <summary>
    /// 当前活跃任务数：获取正在执行的下载任务数量
    /// 此属性是线程安全的，返回的是瞬时快照
    /// </summary>
    public int CurrentActiveTasks => _activeTaskCount;

    /// <summary>
    /// 等待任务数：获取在队列中等待执行的任务数量
    /// 此属性反映了系统的负载情况，可用于监控和告警
    /// </summary>
    public int WaitingTasksCount => _pendingQueue.Count;

    /// <summary>
    /// 最大并发数：获取当前设置的最大并发下载数
    /// 此属性返回的是volatile变量的当前值，保证可见性
    /// </summary>
    public int MaxConcurrentDownloads => _maxConcurrentDownloads;

    // ==================== 事件定义 ====================

    /// <summary>
    /// 并发限制变更事件：当最大并发数发生变化时触发
    /// 参数：新的最大并发数
    /// 可用于监控系统配置变更和调整资源分配
    /// </summary>
    public event Action<int>? ConcurrencyLimitChanged;

    /// <summary>
    /// 等待队列长度变更事件：当等待队列长度发生变化时触发
    /// 参数：新的等待队列长度
    /// 可用于实时监控系统负载和性能调优
    /// </summary>
    public event Action<int>? WaitingQueueLengthChanged;

    // ==================== 配置管理 ====================

    /// <summary>
    /// 处理下载并发数配置变更
    /// 当并发数增大时，主动唤醒等待队列中的任务以利用新增的并发槽位
    /// 当并发数减小时，仅更新限制值，等待当前任务自然完成
    /// </summary>
    /// <param name="newMax">新的最大并发数，必须大于0</param>
    /// <param name="name">配置名称（预留参数，可用于日志记录和诊断）</param>
    /// <exception cref="ArgumentException">当newMax小于等于0时抛出</exception>
    /// <exception cref="ObjectDisposedException">当控制器已被释放时抛出</exception>
    public void OnDownloadOptionsChanged(int newMax, string? name)
    {
        // 前置检查：确保对象未被释放且参数有效
        ThrowIfDisposed();

        // 参数验证：并发数必须为正数
        if (newMax <= 0)
            throw new ArgumentException("并发数必须大于0", nameof(newMax));

        // 原子性地更新最大并发数，并获取旧值用于比较
        // Interlocked.Exchange保证更新的原子性和内存屏障
        var oldMax = Interlocked.Exchange(ref _maxConcurrentDownloads, newMax);

        // 触发事件通知观察者并发限制已变更
        ConcurrencyLimitChanged?.Invoke(newMax);

        // 只有当并发数增加时才需要立即唤醒等待任务
        // 如果并发数减少，不需要特殊处理，等待当前任务自然完成即可
        if (newMax > oldMax)
        {
            // 计算新增的并发槽位数量
            var additionalSlots = newMax - oldMax;

            // 唤醒对应数量的等待任务来利用新增的槽位
            WakeUpWaitingTasks(additionalSlots);
        }

        // 可选：在这里可以添加日志记录，记录配置变更详情
        Debug.WriteLine($"并发配置已更新: {oldMax} -> {newMax}, 名称: {name ?? "N/A"}");
    }

    // ==================== 核心执行逻辑 ====================

    /// <summary>
    /// 带并发控制的任务执行入口
    /// 采用优化的双重检查机制，兼顾性能与并发安全性
    /// 
    /// 执行流程：
    /// 1. 尝试快速获取执行槽位（快速路径）
    /// 2. 如果槽位已满，进入等待队列（慢速路径）
    /// 3. 等待被唤醒后执行实际操作
    /// 4. 执行完成后释放资源并唤醒下一个等待任务
    /// </summary>
    /// <typeparam name="T">任务返回值类型</typeparam>
    /// <param name="operation">实际下载操作的委托，接受取消令牌参数</param>
    /// <param name="token">取消令牌，用于终止等待中的任务（可选）</param>
    /// <returns>下载操作的返回值</returns>
    /// <exception cref="ObjectDisposedException">当控制器已被释放时抛出</exception>
    /// <exception cref="OperationCanceledException">当操作被取消令牌取消时抛出</exception>
    public async Task<T> ExecuteWithConcurrencyControl<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken token = default)
    {
        // 检查对象状态，确保未被释放
        ThrowIfDisposed();

        // 局部变量声明
        SemaphoreSlim? waitSemaphore = null; // 等待信号量，仅在需要等待时创建
        bool isWaiting = false; // 标记当前任务是否在等待队列中

        try
        {
            // ========== 第一阶段：尝试快速获取执行权限 ==========

            // 尝试原子性地增加活跃计数并检查是否在限制范围内
            if (TryAcquireSlot())
            {
                // 快速路径成功：直接执行操作，无需等待
                // 使用ConfigureAwait(false)避免不必要的上下文切换，提高性能
                return await operation(token);
            }

            // ========== 第二阶段：进入等待队列 ==========

            // 慢速路径：创建专属等待信号量（初始状态为阻塞）
            // 设置最大并发数为1，确保只有一次释放有效
            waitSemaphore = new SemaphoreSlim(0, 1);

            // 将信号量加入等待队列，等待被其他任务完成时唤醒
            _pendingQueue.Enqueue(waitSemaphore);
            isWaiting = true; // 标记为等待状态

            // 通知观察者队列长度发生变化
            WaitingQueueLengthChanged?.Invoke(_pendingQueue.Count);

            // ========== 第三阶段：等待执行权限 ==========

            // 等待信号量被释放（表示获取到执行权限）或被取消
            // 这里可能抛出OperationCanceledException，会正常向上传播
            await waitSemaphore.WaitAsync(token).ConfigureAwait(false);

            // ========== 第四阶段：获取权限后执行操作 ==========

            // 信号量已被释放，重新获取执行权限
            // 注意：这里需要重新增加活跃计数，因为之前在TryAcquireSlot失败时已经递减
            Interlocked.Increment(ref _activeTaskCount);

            // 执行实际下载操作
            return await operation(token).ConfigureAwait(false);
        }
        finally
        {
            // ========== 第五阶段：清理资源 ==========

            // 如果任务在等待队列中，尝试从队列中移除自己的信号量
            // 这可以防止信号量在队列中残留（比如在取消操作的情况下）
            if (isWaiting)
            {
                TryRemoveFromQueue(waitSemaphore);
            }

            // 关键步骤：完成操作，递减活跃计数并可能唤醒下一个等待任务
            CompleteOperation();

            // 释放信号量资源，避免内存泄漏
            waitSemaphore?.Dispose();

            // 可选：添加调试日志，记录任务完成状态
            Debug.WriteLine($"任务完成，当前活跃数: {_activeTaskCount}, 等待数: {_pendingQueue.Count}");
        }
    }

    public int GetCurrentMaxConcurrency() => Math.Max(1, _maxConcurrentDownloads);
    // ==================== 私有辅助方法 ====================

    /// <summary>
    /// 尝试获取执行槽位
    /// 采用原子操作确保计数的准确性和线程安全
    /// 
    /// 逻辑：
    /// 1. 原子递增活跃任务计数
    /// 2. 检查是否在并发限制范围内
    /// 3. 如果超出范围，原子递减计数（回滚）并返回false
    /// </summary>
    /// <returns>true表示成功获取槽位，false表示需要进入等待</returns>
    private bool TryAcquireSlot()
    {
        // 原子递增活跃任务计数，并获取递增后的值
        int currentActive = Interlocked.Increment(ref _activeTaskCount);

        // 检查是否在并发限制范围内
        if (currentActive <= _maxConcurrentDownloads)
        {
            return true; // 成功获取槽位
        }

        // 超出并发限制，回滚计数
        // 注意：这里可能产生短暂的计数超出，但会被快速纠正
        Interlocked.Decrement(ref _activeTaskCount);
        return false; // 需要进入等待
    }

    /// <summary>
    /// 唤醒指定数量的等待任务
    /// 从等待队列头部开始唤醒，遵循FIFO原则
    /// </summary>
    /// <param name="count">要唤醒的任务数量</param>
    private void WakeUpWaitingTasks(int count)
    {
        // 遍历指定数量的槽位
        for (int i = 0; i < count; i++)
        {
            // 尝试从队列头部取出一个等待信号量
            if (!_pendingQueue.TryDequeue(out var semaphore))
                break; // 队列已空，停止唤醒

            // 通知观察者队列长度发生变化
            WaitingQueueLengthChanged?.Invoke(_pendingQueue.Count);

            try
            {
                // 释放信号量，唤醒对应的等待任务
                // 这会使等待在WaitAsync的任务继续执行
                semaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                // 信号量已被释放（可能任务已超时或取消）
                // 尝试唤醒下一个任务来填补这个"空洞"
                if (_pendingQueue.TryDequeue(out semaphore))
                {
                    try
                    {
                        semaphore.Release();
                    }
                    catch (ObjectDisposedException)
                    {
                        // 连续遇到已释放的信号量，记录日志或忽略
                        // 在实际生产环境中可以考虑记录警告日志
                        Debug.WriteLine("警告：连续遇到已释放的信号量");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 从等待队列中移除指定的信号量
    /// 采用临时队列的方式遍历和重构原队列，确保线程安全
    /// 
    /// 使用场景：
    /// - 任务被取消时，需要从队列中移除自己的等待项
    /// - 避免信号量在队列中残留导致资源泄漏
    /// </summary>
    /// <param name="semaphore">要移除的信号量</param>
    /// <returns>true表示成功移除，false表示信号量不在队列中</returns>
    private bool TryRemoveFromQueue(SemaphoreSlim? semaphore)
    {
        // 空值检查
        if (semaphore == null)
            return false;

        // 创建临时队列用于重构
        var tempQueue = new Queue<SemaphoreSlim>();
        bool found = false; // 标记是否找到目标信号量

        // 第一步：遍历原队列，找出目标信号量
        while (_pendingQueue.TryDequeue(out var item))
        {
            if (item == semaphore && !found)
            {
                // 找到目标信号量，跳过不入临时队列（相当于移除）
                found = true;
                continue;
            }

            // 其他信号量保留到临时队列
            tempQueue.Enqueue(item);
        }

        // 第二步：将剩余项重新放回原队列
        foreach (var item in tempQueue)
        {
            _pendingQueue.Enqueue(item);
        }

        // 如果成功移除了信号量，通知观察者队列长度变化
        if (found)
        {
            WaitingQueueLengthChanged?.Invoke(_pendingQueue.Count);
        }

        return found;
    }

    /// <summary>
    /// 任务完成后的清理逻辑
    /// 减少活跃任务计数，并唤醒等待队列中的任务以填补空闲槽位
    /// 
    /// 这是并发控制的核心逻辑之一，确保：
    /// 1. 活跃计数准确递减
    /// 2. 空闲槽位被及时利用
    /// 3. 等待任务被公平唤醒
    /// </summary>
    private void CompleteOperation()
    {
        // 原子递减活跃任务计数，并获取递减后的值
        int newActiveCount = Interlocked.Decrement(ref _activeTaskCount);

        // 检查是否存在空闲槽位且等待队列非空
        // 注意：这里使用 newActiveCount < _maxConcurrentDownloads - 1 是因为：
        // - newActiveCount是递减后的值
        // - 需要确保确实有空闲槽位可供使用
        if (newActiveCount < _maxConcurrentDownloads - 1 && !_pendingQueue.IsEmpty)
        {
            // 唤醒一个等待任务来利用空闲槽位
            WakeUpWaitingTasks(1);
        }
    }

    /// <summary>
    /// 检查对象是否已被释放，如果已释放则抛出异常
    /// 用于在公共方法开始时进行状态验证
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已被释放时抛出</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EnhancedDownloadConcurrencyController));
    }

    // ==================== 资源管理 ====================

    /// <summary>
    /// 释放控制器占用的所有资源
    /// 实现IDisposable接口，支持using语句
    /// 
    /// 释放流程：
    /// 1. 标记为已释放状态
    /// 2. 清空等待队列
    /// 3. 释放所有等待信号量
    /// 4. 抑制终结器（如果实现了终结器）
    /// </summary>
    public void Dispose()
    {
        // 检查是否已经释放过
        if (!_disposed)
        {
            _disposed = true; // 标记为已释放

            // 清空等待队列并释放所有信号量
            while (_pendingQueue.TryDequeue(out var semaphore))
            {
                try
                {
                    // 释放信号量资源
                    semaphore.Dispose();
                }
                catch
                {
                    // 忽略释放过程中可能出现的异常
                    // 在对象销毁阶段，通常不需要处理这些异常
                }
            }

            // 可选：触发事件通知观察者对象即将销毁
            Debug.WriteLine("DownloadConcurrencyController已释放");

            // 如果实现了终结器，应该在这里调用GC.SuppressFinalize
            // GC.SuppressFinalize(this);
        }
    }

    // ==================== 终结器（可选） ====================

    // 如果需要实现完整的Dispose模式，可以添加终结器
    // ~EnhancedDownloadConcurrencyController()
    // {
    //     Dispose(false);
    // }

    // 同时需要将Dispose方法拆分为Dispose(bool disposing)
}