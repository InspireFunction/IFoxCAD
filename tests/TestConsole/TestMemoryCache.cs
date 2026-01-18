using System;
using IFoxCAD.Basal;

namespace TestConsole;

public class TestMemoryCache
{
    public static void RunTest()
    {
        Console.WriteLine("=== MemoryCache 测试 ===");

        // 创建缓存实例，设置清理间隔为500毫秒
        var cache = new MemoryCache<string, string>(TimeSpan.FromMilliseconds(500));

        // 注册缓存项移除事件
        cache.OnItemRemoved += (key, value, reason) => {
            Console.WriteLine($"缓存项被移除: 键={key}, 值={value}, 原因={reason}");
        };

        // 测试1: 添加和获取缓存项（绝对过期）
        Console.WriteLine("\n1. 测试添加和获取缓存项（绝对过期）:");
        cache.Set("key1", "value1", TimeSpan.FromSeconds(1));
        Console.WriteLine($"缓存项数量: {cache.Count}");

        if (cache.TryGet("key1", out var value))
        {
            Console.WriteLine($"获取到缓存项: key1={value}");
        }

        // 测试2: 添加和获取缓存项（滑动过期）
        Console.WriteLine("\n2. 测试添加和获取缓存项（滑动过期）:");
        cache.Set("key2", "value2", TimeSpan.FromSeconds(1), true);
        Console.WriteLine($"缓存项数量: {cache.Count}");

        if (cache.TryGet("key2", out value))
        {
            Console.WriteLine($"获取到缓存项: key2={value}");
        }

        // 测试3: 测试过期功能
        Console.WriteLine("\n3. 测试过期功能:");
        Console.WriteLine("等待1.5秒让缓存项过期...");
        System.Threading.Thread.Sleep(1500);

        if (!cache.TryGet("key1", out value))
        {
            Console.WriteLine("key1已过期，无法获取");
        }

        Console.WriteLine($"当前缓存项数量: {cache.Count}");

        // 测试4: 测试滑动过期更新
        Console.WriteLine("\n4. 测试滑动过期更新:");
        cache.Set("key3", "value3", TimeSpan.FromSeconds(1), true);

        for (int i = 0; i < 3; i++)
        {
            if (cache.TryGet("key3", out value))
            {
                Console.WriteLine($"第{i + 1}次获取到缓存项: key3={value}");
            }
            System.Threading.Thread.Sleep(500);
        }

        // 测试5: 手动清理过期项
        Console.WriteLine("\n5. 测试手动清理过期项:");
        cache.Set("key4", "value4", TimeSpan.FromMilliseconds(200));
        cache.Set("key5", "value5", TimeSpan.FromSeconds(5));
        Console.WriteLine($"清理前缓存项数量: {cache.Count}");

        System.Threading.Thread.Sleep(300);
        int removedCount = cache.ManualCleanup();
        Console.WriteLine($"手动清理后缓存项数量: {cache.Count}, 清理了{removedCount}个过期项");

        // 测试6: 测试移除和清空功能
        Console.WriteLine("\n6. 测试移除和清空功能:");
        cache.Remove("key5");
        Console.WriteLine($"移除key5后缓存项数量: {cache.Count}");

        cache.Clear();
        Console.WriteLine($"清空后缓存项数量: {cache.Count}");

        // 释放资源
        cache.Dispose();

        Console.WriteLine("\n=== MemoryCache 测试完成 ===");
    }
}