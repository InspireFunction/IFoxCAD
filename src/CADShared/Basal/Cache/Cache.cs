namespace IFoxCAD.Basal;

using ConcurrentCollections;

/// <summary>
/// 缓存移除原因
/// </summary>
public enum CacheRemovedReason
{
    /// <summary>
    /// 过期
    /// </summary>
    Expired,

    /// <summary>
    /// 被移除
    /// </summary>
    Removed,

    /// <summary>
    /// 被替换
    /// </summary>
    Replaced
}


/// <summary>
/// 完整的内存缓存实现，带定时清理过期项
/// </summary>
/// <typeparam name="TKey">键类型</typeparam>
/// <typeparam name="TValue">值类型</typeparam>
public class MemoryCache<TKey, TValue> : IDisposable
{
    private class CacheItem(TValue value, DateTime expiryTime, bool isSliding, TimeSpan slidingExpiration)
    {
        public TValue Value { get; set; } = value;
        public DateTime ExpiryTime { get; set; } = expiryTime;
        public bool IsSliding { get; set; } = isSliding;
        public TimeSpan SlidingExpiration { get; set; } = slidingExpiration;

        public bool IsExpired => DateTime.Now > ExpiryTime;
    }

    private bool _disposed = false;

    // 使用 ConcurrentDictionary 保证线程安全
    private readonly ConcurrentDictionary<TKey, CacheItem> _cache = [];

    // 清理定时器
    private readonly System.Threading.Timer _cleanupTimer;

    // 默认清理间隔（1分钟）
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 缓存项数量
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// 构造缓存实例
    /// </summary>
    /// <param name="cleanupInterval">清理间隔,默认1分钟</param>
    public MemoryCache(TimeSpan? cleanupInterval = null)
    {
        if (cleanupInterval.HasValue)
        {
            _cleanupInterval = cleanupInterval.Value;
        }

        // 启动清理定时器
        _cleanupTimer = new System.Threading.Timer(
            CleanupCallback,
            null,
            (int)_cleanupInterval.TotalMilliseconds,  // 第一次执行时间
            (int)_cleanupInterval.TotalMilliseconds   // 执行间隔
        );
    }

    /// <summary>
    /// 添加或更新缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">要缓存的值</param>
    /// <param name="slidingExpiration">续费的时间</param>
    /// <param name="isSliding">true是滑动过期,false是绝对过期</param>
    public void Set(TKey key, TValue value, TimeSpan slidingExpiration, bool isSliding = true)
    {
        var item = new CacheItem(
            value,
            DateTime.Now.Add(slidingExpiration),
            isSliding,
            slidingExpiration
        );
        _cache[key] = item;
    }

    /// <summary>
    /// 获取缓存值
    /// </summary>
    public bool TryGet(TKey key, out TValue? value)
    {
        CacheItem item;
        if (_cache.TryGetValue(key, out item))
        {
            if (item.IsExpired)
            {
                // 过期则移除
                _cache.TryRemove(key, out item);
                value = default(TValue);
                return false;
            }

            // 如果是滑动过期，更新过期时间
            if (item.IsSliding)
            {
                // 重新设置缓存项以更新过期时间
                var newItem = new CacheItem(
                    item.Value,
                    DateTime.Now.Add(item.SlidingExpiration),
                    true,
                    item.SlidingExpiration
                );
                _cache.TryUpdate(key, newItem, item);
            }

            value = item.Value;
            return true;
        }

        value = default(TValue);
        return false;
    }

    /// <summary>
    /// 移除指定键的缓存
    /// </summary>
    public bool Remove(TKey key)
    {
        CacheItem removed;
        return _cache.TryRemove(key, out removed);
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// 清理过期项的定时器回调
    /// </summary>
    private void CleanupCallback(object state)
    {
        try
        {
            CleanupExpiredItems();
        }
        catch (Exception ex)
        {
            // 记录日志，但不要抛出异常影响定时器
            System.Diagnostics.Debug.WriteLine($"缓存清理出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 清理所有过期缓存项
    /// </summary>
    public int CleanupExpiredItems()
    {
        int removedCount = 0;
        var now = DateTime.Now;

        // 遍历所有缓存项
        foreach (var kvp in _cache)
        {
            if (kvp.Value.IsExpired)
            {
                // 尝试移除过期项
                CacheItem removed;
                if (_cache.TryRemove(kvp.Key, out removed))
                {
                    removedCount++;

                    // 可选的：触发移除事件
                    OnItemRemoved?.Invoke(kvp.Key, removed.Value, CacheRemovedReason.Expired);
                }
            }
        }

        return removedCount;
    }

    /// <summary>
    /// 缓存项被移除的事件
    /// </summary>
    public event Action<TKey, TValue, CacheRemovedReason> OnItemRemoved = delegate { };

    /// <summary>
    /// 获取所有缓存键
    /// </summary>
    public IEnumerable<TKey> Keys => _cache.Keys;

    /// <summary>
    /// 获取所有有效缓存项
    /// </summary>
    public IEnumerable<KeyValuePair<TKey, TValue>> GetAllValidItems()
    {
        var now = DateTime.Now;
        foreach (var kvp in _cache)
        {
            if (!kvp.Value.IsExpired)
            {
                yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value);
            }
        }
    }

    /// <summary>
    /// 手动触发一次清理
    /// </summary>
    /// <returns>清理的过期项数量</returns>
    public int ManualCleanup()
    {
        return CleanupExpiredItems();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 停止定时器
                if (_cleanupTimer != null)
                {
                    _cleanupTimer.Dispose();
                }

                // 清空缓存
                Clear();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// 析构
    /// </summary>
    ~MemoryCache()
    {
        Dispose(false);
    }
}

