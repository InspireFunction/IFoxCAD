#if NET35
#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Linq
{
    public class ParallelQuery<TSource> : IEnumerable<TSource>
    {
        private readonly IEnumerable<TSource> _source;
        private int _degreeOfParallelism = Environment.ProcessorCount;

        public ParallelQuery(IEnumerable<TSource> source)
        {
            _source = source;
        }

        public ParallelQuery<TSource> WithDegreeOfParallelism(int degree)
        {
            if (degree <= 0)
                throw new ArgumentOutOfRangeException(nameof(degree), "并行度必须大于0");

            _degreeOfParallelism = Math.Min(degree, 512); // 限制最大线程数
            return this;
        }

        public IEnumerable<TSource> Where(Func<TSource, bool> predicate)
        {
            return new ParallelWhereQuery<TSource>(_source, predicate, _degreeOfParallelism);
        }

        public IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
        {
            return new ParallelSelectQuery<TSource, TResult>(_source, selector, _degreeOfParallelism);
        }

        public List<TSource> ToList()
        {
            var sourceList = new List<TSource>(_source);
            if (sourceList.Count == 0)
                return new List<TSource>();

            int degree = Math.Min(_degreeOfParallelism, sourceList.Count);
            int batchSize = (int)Math.Ceiling((double)sourceList.Count / degree);

            var results = new List<TSource>[degree];
            var resetEvents = new ManualResetEvent[degree];
            int completedCount = 0;
            var allDoneEvent = new ManualResetEvent(false);

            for (int i = 0; i < degree; i++)
            {
                results[i] = new List<TSource>();
                resetEvents[i] = new ManualResetEvent(false);
                int threadIndex = i;

                ThreadPool.QueueUserWorkItem(state => {
                    try
                    {
                        int startIndex = threadIndex * batchSize;
                        int endIndex = (threadIndex == degree - 1)
                            ? sourceList.Count
                            : Math.Min(startIndex + batchSize, sourceList.Count);
                        var batchResults = new List<TSource>();

                        for (int j = startIndex; j < endIndex; j++)
                        {
                            batchResults.Add(sourceList[j]);
                        }

                        results[threadIndex] = batchResults;
                    }
                    finally
                    {
                        resetEvents[threadIndex].Set();

                        if (Interlocked.Increment(ref completedCount) == degree)
                        {
                            allDoneEvent.Set();
                        }
                    }
                });
            }

            // 优化的等待策略
            WaitForCompletion(resetEvents, allDoneEvent);

            // 合并结果
            var finalResult = new List<TSource>();
            for (int i = 0; i < degree; i++)
            {
                if (results[i] != null)
                {
                    finalResult.AddRange(results[i]);
                }
            }

            return finalResult;
        }

        public TSource[] ToArray()
        {
            return ToList().ToArray();
        }

        public int GetDegreeOfParallelism()
        {
            return _degreeOfParallelism;
        }

        private static void WaitForCompletion(ManualResetEvent[] resetEvents, ManualResetEvent allDoneEvent)
        {
            // 优先等待allDoneEvent（最快完成时）
            allDoneEvent.WaitOne();

            // 确保所有线程都完成
            for (int i = 0; i < resetEvents.Length; i++)
            {
                if (!resetEvents[i].WaitOne(0))
                {
                    resetEvents[i].WaitOne();
                }
            }
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class ParallelWhereQuery<TSource> : IEnumerable<TSource>
    {
        private readonly IEnumerable<TSource> _source;
        private readonly Func<TSource, bool> _predicate;
        private readonly int _degreeOfParallelism;

        public ParallelWhereQuery(IEnumerable<TSource> source, Func<TSource, bool> predicate, int degreeOfParallelism)
        {
            _source = source;
            _predicate = predicate;
            _degreeOfParallelism = degreeOfParallelism;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            var sourceList = new List<TSource>(_source);
            if (sourceList.Count == 0)
                return (new List<TSource>()).GetEnumerator();

            int degree = Math.Min(_degreeOfParallelism, sourceList.Count);
            int batchSize = (int)Math.Ceiling((double)sourceList.Count / degree);

            var results = new List<TSource>[degree];
            var resetEvents = new ManualResetEvent[degree];
            int completedCount = 0;
            var allDoneEvent = new ManualResetEvent(false);

            for (int i = 0; i < degree; i++)
            {
                results[i] = new List<TSource>();
                resetEvents[i] = new ManualResetEvent(false);
                int threadIndex = i;

                ThreadPool.QueueUserWorkItem(state => {
                    try
                    {
                        int startIndex = threadIndex * batchSize;
                        int endIndex = (threadIndex == degree - 1)
                            ? sourceList.Count
                            : Math.Min(startIndex + batchSize, sourceList.Count);
                        var batchResults = new List<TSource>();

                        for (int j = startIndex; j < endIndex; j++)
                        {
                            var item = sourceList[j];
                            if (_predicate(item))
                            {
                                batchResults.Add(item);
                            }
                        }

                        results[threadIndex] = batchResults;
                    }
                    finally
                    {
                        resetEvents[threadIndex].Set();

                        if (Interlocked.Increment(ref completedCount) == degree)
                        {
                            allDoneEvent.Set();
                        }
                    }
                });
            }

            // 等待所有线程完成
            WaitForCompletion(resetEvents, allDoneEvent);

            // 合并结果
            var finalResult = new List<TSource>();
            for (int i = 0; i < degree; i++)
            {
                if (results[i] != null)
                {
                    finalResult.AddRange(results[i]);
                }
            }

            return finalResult.GetEnumerator();
        }

        private static void WaitForCompletion(ManualResetEvent[] resetEvents, ManualResetEvent allDoneEvent)
        {
            allDoneEvent.WaitOne();

            foreach (var resetEvent in resetEvents)
            {
                if (!resetEvent.WaitOne(0))
                {
                    resetEvent.WaitOne();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class ParallelSelectQuery<TSource, TResult> : IEnumerable<TResult>
    {
        private readonly IEnumerable<TSource> _source;
        private readonly Func<TSource, TResult> _selector;
        private readonly int _degreeOfParallelism;

        public ParallelSelectQuery(IEnumerable<TSource> source, Func<TSource, TResult> selector, int degreeOfParallelism)
        {
            _source = source;
            _selector = selector;
            _degreeOfParallelism = degreeOfParallelism;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            var sourceList = new List<TSource>(_source);
            if (sourceList.Count == 0)
                return (new List<TResult>()).GetEnumerator();

            int degree = Math.Min(_degreeOfParallelism, sourceList.Count);
            int batchSize = (int)Math.Ceiling((double)sourceList.Count / degree);

            var results = new List<TResult>[degree];
            var resetEvents = new ManualResetEvent[degree];
            int completedCount = 0;
            var allDoneEvent = new ManualResetEvent(false);

            for (int i = 0; i < degree; i++)
            {
                results[i] = new List<TResult>();
                resetEvents[i] = new ManualResetEvent(false);
                int threadIndex = i;

                ThreadPool.QueueUserWorkItem(state => {
                    try
                    {
                        int startIndex = threadIndex * batchSize;
                        int endIndex = (threadIndex == degree - 1)
                            ? sourceList.Count
                            : Math.Min(startIndex + batchSize, sourceList.Count);
                        var batchResults = new List<TResult>();

                        for (int j = startIndex; j < endIndex; j++)
                        {
                            batchResults.Add(_selector(sourceList[j]));
                        }

                        results[threadIndex] = batchResults;
                    }
                    finally
                    {
                        resetEvents[threadIndex].Set();

                        if (Interlocked.Increment(ref completedCount) == degree)
                        {
                            allDoneEvent.Set();
                        }
                    }
                });
            }

            // 等待所有线程完成
            WaitForCompletion(resetEvents, allDoneEvent);

            // 合并结果
            var finalResult = new List<TResult>();
            for (int i = 0; i < degree; i++)
            {
                if (results[i] != null)
                {
                    finalResult.AddRange(results[i]);
                }
            }

            return finalResult.GetEnumerator();
        }

        private static void WaitForCompletion(ManualResetEvent[] resetEvents, ManualResetEvent allDoneEvent)
        {
            allDoneEvent.WaitOne();

            foreach (var resetEvent in resetEvents)
            {
                if (!resetEvent.WaitOne(0))
                {
                    resetEvent.WaitOne();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}



public static class ParallelExtensions
{
    public static void ForAll<T>(this ParallelQuery<T> source, Action<T> action, int maxDegreeOfParallelism = -1)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (action == null) throw new ArgumentNullException("action");

        var sourceList = new List<T>(source);
        if (sourceList.Count == 0) return;

        // 确定并行度：如果未指定，使用ParallelQuery的并行度；如果指定为-1，使用默认核心数
        int degree;
        if (maxDegreeOfParallelism == -1)
        {
            degree = Math.Min(source.GetDegreeOfParallelism(), sourceList.Count);
        }
        else
        {
            degree = Math.Min(maxDegreeOfParallelism, sourceList.Count);
        }

        int batchSize = (int)Math.Ceiling((double)sourceList.Count / degree);

        var resetEvents = new ManualResetEvent[degree];
        int completedCount = 0;
        var allDoneEvent = new ManualResetEvent(false);

        for (int i = 0; i < degree; i++)
        {
            resetEvents[i] = new ManualResetEvent(false);
            int threadIndex = i;

            ThreadPool.QueueUserWorkItem(state => {
                try
                {
                    int startIndex = threadIndex * batchSize;
                    int endIndex = (threadIndex == degree - 1)
                        ? sourceList.Count
                        : Math.Min(startIndex + batchSize, sourceList.Count);

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        try
                        {
                            action(sourceList[j]);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Parallel execution error: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    resetEvents[threadIndex].Set();

                    if (Interlocked.Increment(ref completedCount) == degree)
                    {
                        allDoneEvent.Set();
                    }
                }
            });
        }

        // 等待完成
        ParallelEnumerableExtensions.WaitForCompletion(resetEvents, allDoneEvent);
    }

    public static void ForAll<T>(this IEnumerable<T> source, Action<T> action, int maxDegreeOfParallelism = -1)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (action == null) throw new ArgumentNullException("action");

        // 默认并行度为核心数
        int degree = maxDegreeOfParallelism == -1 ? Environment.ProcessorCount : maxDegreeOfParallelism;

        var sourceList = new List<T>(source);
        if (sourceList.Count == 0) return;

        degree = Math.Min(degree, sourceList.Count);
        int batchSize = (int)Math.Ceiling((double)sourceList.Count / degree);

        var resetEvents = new ManualResetEvent[degree];
        int completedCount = 0;
        var allDoneEvent = new ManualResetEvent(false);

        for (int i = 0; i < degree; i++)
        {
            resetEvents[i] = new ManualResetEvent(false);
            int threadIndex = i;

            ThreadPool.QueueUserWorkItem(state => {
                try
                {
                    int startIndex = threadIndex * batchSize;
                    int endIndex = (threadIndex == degree - 1)
                        ? sourceList.Count
                        : Math.Min(startIndex + batchSize, sourceList.Count);

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        try
                        {
                            action(sourceList[j]);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Parallel execution error: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    resetEvents[threadIndex].Set();

                    if (Interlocked.Increment(ref completedCount) == degree)
                    {
                        allDoneEvent.Set();
                    }
                }
            });
        }

        // 等待完成
        ParallelEnumerableExtensions.WaitForCompletion(resetEvents, allDoneEvent);
    }
}





namespace System.Linq
{
    public static class ParallelEnumerableExtensions
    {
        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return new ParallelQuery<TSource>(source);
        }

        public static ParallelQuery<TSource> WithDegreeOfParallelism<TSource>(
            this ParallelQuery<TSource> source, int degreeOfParallelism)
        {
            return source.WithDegreeOfParallelism(degreeOfParallelism);
        }

        public static IEnumerable<TSource> Where<TSource>(
            this ParallelQuery<TSource> source,
            Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this ParallelQuery<TSource> source,
            Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }

        public static List<TSource> ToList<TSource>(this ParallelQuery<TSource> source)
        {
            return source.ToList();
        }

        public static TSource[] ToArray<TSource>(this ParallelQuery<TSource> source)
        {
            return source.ToArray();
        }

        public static int Count<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            var sourceList = new List<TSource>(source);
            if (sourceList.Count == 0)
                return 0;

            int degreeOfParallelism = Math.Min(Environment.ProcessorCount, sourceList.Count);
            int batchSize = (int)Math.Ceiling((double)sourceList.Count / degreeOfParallelism);

            var counts = new int[degreeOfParallelism];
            var resetEvents = new ManualResetEvent[degreeOfParallelism];
            int completedCount = 0;
            var allDoneEvent = new ManualResetEvent(false);

            for (int i = 0; i < degreeOfParallelism; i++)
            {
                counts[i] = 0;
                resetEvents[i] = new ManualResetEvent(false);
                int threadIndex = i;

                ThreadPool.QueueUserWorkItem(state => {
                    try
                    {
                        int startIndex = threadIndex * batchSize;
                        int endIndex = Math.Min(startIndex + batchSize, sourceList.Count);
                        int localCount = 0;

                        for (int j = startIndex; j < endIndex; j++)
                        {
                            if (predicate(sourceList[j]))
                            {
                                localCount++;
                            }
                        }

                        counts[threadIndex] = localCount;
                    }
                    finally
                    {
                        resetEvents[threadIndex].Set();

                        if (Interlocked.Increment(ref completedCount) == degreeOfParallelism)
                        {
                            allDoneEvent.Set();
                        }
                    }
                });
            }

            // 等待完成
            WaitForCompletion(resetEvents, allDoneEvent);

            // 汇总结果
            int totalCount = 0;
            for (int i = 0; i < degreeOfParallelism; i++)
            {
                totalCount += counts[i];
            }

            return totalCount;
        }

        public static TSource? FirstOrDefault<TSource>(
            this ParallelQuery<TSource> source,
            Func<TSource, bool> predicate)
        {
            var sourceList = new List<TSource>(source);
            if (sourceList.Count == 0)
                return default(TSource);

            TSource? result = default(TSource);
            bool found = false;
            object lockObj = new object();
            int degreeOfParallelism = Math.Min(Environment.ProcessorCount, sourceList.Count);
            int batchSize = (int)Math.Ceiling((double)sourceList.Count / degreeOfParallelism);
            var resetEvents = new ManualResetEvent[degreeOfParallelism];
            int completedCount = 0;
            var allDoneEvent = new ManualResetEvent(false);

            for (int i = 0; i < degreeOfParallelism; i++)
            {
                resetEvents[i] = new ManualResetEvent(false);
                int threadIndex = i;

                ThreadPool.QueueUserWorkItem(state => {
                    try
                    {
                        int startIndex = threadIndex * batchSize;
                        int endIndex = Math.Min(startIndex + batchSize, sourceList.Count);

                        for (int j = startIndex; j < endIndex; j++)
                        {
                            lock (lockObj)
                            {
                                if (found) return;
                            }

                            var item = sourceList[j];
                            if (predicate(item))
                            {
                                lock (lockObj)
                                {
                                    if (!found)
                                    {
                                        result = item;
                                        found = true;
                                    }
                                }
                                return;
                            }
                        }
                    }
                    finally
                    {
                        resetEvents[threadIndex].Set();

                        if (Interlocked.Increment(ref completedCount) == degreeOfParallelism)
                        {
                            allDoneEvent.Set();
                        }
                    }
                });
            }

            // 等待完成
            WaitForCompletion(resetEvents, allDoneEvent);

            return result;
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <param name="resetEvents"></param>
        /// <param name="allDoneEvent"></param>
        public static void WaitForCompletion(ManualResetEvent[] resetEvents, ManualResetEvent allDoneEvent)
        {
            allDoneEvent.WaitOne();

            foreach (var resetEvent in resetEvents)
            {
                if (!resetEvent.WaitOne(0))
                {
                    resetEvent.WaitOne();
                }
            }
        }
    }
}
#endif