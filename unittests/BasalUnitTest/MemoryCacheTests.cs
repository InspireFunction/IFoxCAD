namespace BasalUnitTests;

public class MemoryCacheTests : IDisposable
{
    private readonly MemoryCache<string, string> _cache;

    public MemoryCacheTests()
    {
        _cache = new MemoryCache<string, string>(TimeSpan.FromMilliseconds(500));
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Fact]
    public void Set_And_TryGet_ReturnsCorrectValue()
    {
        _cache.Set("key1", "value1", TimeSpan.FromSeconds(1));

        var result = _cache.TryGet("key1", out var value);

        Assert.True(result);
        Assert.Equal("value1", value);
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        var result = _cache.TryGet("nonexistent", out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Count_AfterSet_ReturnsCorrectCount()
    {
        _cache.Set("key1", "value1", TimeSpan.FromSeconds(1));
        _cache.Set("key2", "value2", TimeSpan.FromSeconds(1));

        Assert.Equal(2, _cache.Count);
    }

    [Fact]
    public void AbsoluteExpiration_ItemExpires_AfterTimeout()
    {
        _cache.Set("key1", "value1", TimeSpan.FromMilliseconds(200));

        Thread.Sleep(300);

        var result = _cache.TryGet("key1", out var value);
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void SlidingExpiration_ItemStaysAlive_WhenAccessed()
    {
        _cache.Set("key1", "value1", TimeSpan.FromMilliseconds(300), isSliding: true);

        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(150);
            var result = _cache.TryGet("key1", out var value);
            Assert.True(result);
            Assert.Equal("value1", value);
        }
    }

    [Fact]
    public void SlidingExpiration_ItemExpires_WhenNotAccessed()
    {
        _cache.Set("key1", "value1", TimeSpan.FromMilliseconds(200), isSliding: true);

        Thread.Sleep(400);

        var result = _cache.TryGet("key1", out var value);
        Assert.False(result);
    }

    [Fact]
    public void Remove_Item_RemovesFromCache()
    {
        _cache.Set("key1", "value1", TimeSpan.FromSeconds(1));
        Assert.Equal(1, _cache.Count);

        _cache.Remove("key1");

        Assert.Equal(0, _cache.Count);
        Assert.False(_cache.TryGet("key1", out _));
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        _cache.Set("key1", "value1", TimeSpan.FromSeconds(1));
        _cache.Set("key2", "value2", TimeSpan.FromSeconds(1));
        _cache.Set("key3", "value3", TimeSpan.FromSeconds(1));

        _cache.Clear();

        Assert.Equal(0, _cache.Count);
    }

    [Fact]
    public void ManualCleanup_RemovesExpiredItems()
    {
        _cache.Set("key1", "value1", TimeSpan.FromMilliseconds(100));
        _cache.Set("key2", "value2", TimeSpan.FromSeconds(10));

        Thread.Sleep(200);

        var removedCount = _cache.ManualCleanup();

        Assert.Equal(1, removedCount);
        Assert.Equal(1, _cache.Count);
        Assert.True(_cache.TryGet("key2", out _));
    }

    [Fact]
    public void OnItemRemoved_Fires_WhenItemExpires()
    {
        var removedKey = string.Empty;
        var removedValue = string.Empty;
        CacheRemovedReason removedReason = CacheRemovedReason.Removed;

        _cache.OnItemRemoved += (key, value, reason) =>
        {
            removedKey = key;
            removedValue = value;
            removedReason = reason;
        };

        _cache.Set("key1", "value1", TimeSpan.FromMilliseconds(100));

        Thread.Sleep(300);
        _cache.ManualCleanup();

        Assert.Equal("key1", removedKey);
        Assert.Equal("value1", removedValue);
        Assert.Equal(CacheRemovedReason.Expired, removedReason);
    }

    [Fact]
    public void Set_Overwrite_ExistingKey()
    {
        _cache.Set("key1", "value1", TimeSpan.FromSeconds(1));
        _cache.Set("key1", "value2", TimeSpan.FromSeconds(1));

        var result = _cache.TryGet("key1", out var value);

        Assert.True(result);
        Assert.Equal("value2", value);
        Assert.Equal(1, _cache.Count);
    }
}
