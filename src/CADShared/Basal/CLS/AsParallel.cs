#if NET35
#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace System.Linq
{
    public class ParallelQuery<TSource> : IEnumerable<TSource>
    {
        private readonly IEnumerable<TSource> _source;

        public ParallelQuery(IEnumerable<TSource> source)
        {
            _source = source;
        }

        public IEnumerable<TSource> Where(Func<TSource, bool> predicate)
        {
            return new ParallelWhereQuery<TSource>(_source, predicate);
        }

        public IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
        {
            return new ParallelSelectQuery<TSource, TResult>(_source, selector);
        }

        public List<TSource> ToList()
        {
            var result = new List<TSource>();

            // 使用ManualResetEvent等待所有线程完成
            var resetEvents = new List<ManualResetEvent>();
            var locker = new object();

            foreach (var item in _source)
            {
                var resetEvent = new ManualResetEvent(false);
                resetEvents.Add(resetEvent);

                ThreadPool.QueueUserWorkItem(state => {
                    var tuple = (ValueTuple<TSource, ManualResetEvent>)state;
                    lock (locker)
                    {
                        result.Add(tuple.Item1);
                    }
                    tuple.Item2.Set();
                }, new ValueTuple<TSource, ManualResetEvent>(item, resetEvent));
            }

            try
            {
                // SAT上面不能用WaitAll，会报异常
                // WaitHandle.WaitAll(resetEvents.ToArray());

                // 改用WaitOne逐个等待
                foreach (var resetEvent in resetEvents)
                {
                    resetEvent.WaitOne();
                }
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }

            return result;
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

        public ParallelWhereQuery(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            var results = new List<TSource>();
            var resetEvents = new List<ManualResetEvent>();
            var locker = new object();

            foreach (var item in _source)
            {
                var resetEvent = new ManualResetEvent(false);
                resetEvents.Add(resetEvent);

                ThreadPool.QueueUserWorkItem(state => {
                    var tuple = (ValueTuple<TSource, ManualResetEvent>)state;
                    if (_predicate(tuple.Item1))
                    {
                        lock (locker)
                        {
                            results.Add(tuple.Item1);
                        }
                    }
                    tuple.Item2.Set();
                }, new ValueTuple<TSource, ManualResetEvent>(item, resetEvent));
            }


            try
            {
                // SAT上面不能用WaitAll，会报异常
                // WaitHandle.WaitAll(resetEvents.ToArray());

                // 改用WaitOne逐个等待
                foreach (var resetEvent in resetEvents)
                {
                    resetEvent.WaitOne();
                }
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }


            return results.GetEnumerator();
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

        public ParallelSelectQuery(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            _source = source;
            _selector = selector;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            var results = new List<TResult>();
            var resetEvents = new List<ManualResetEvent>();
            var locker = new object();

            foreach (var item in _source)
            {
                var resetEvent = new ManualResetEvent(false);
                resetEvents.Add(resetEvent);

                ThreadPool.QueueUserWorkItem(state => {
                    var tuple = (ValueTuple<TSource, ManualResetEvent>)state;
                    var transformed = _selector(tuple.Item1);

                    lock (locker)
                    {
                        results.Add(transformed);
                    }
                    tuple.Item2.Set();
                }, new ValueTuple<TSource, ManualResetEvent>(item, resetEvent));
            }

            try
            {
                // SAT上面不能用WaitAll，会报异常
                // WaitHandle.WaitAll(resetEvents.ToArray());

                // 改用WaitOne逐个等待
                foreach (var resetEvent in resetEvents)
                {
                    resetEvent.WaitOne();
                }
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }

            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
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
            // 在 .NET 3.5 中，我们可以通过设置线程池大小来模拟这个行为
            // 但这里我们简单地返回原对象，因为在自定义实现中线程数由系统决定
            return source;
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
            return source.ToList().ToArray();
        }

        public static int Count<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            int count = 0;
            var resetEvents = new List<ManualResetEvent>();
            var locker = new object();

            foreach (var item in source)
            {
                var resetEvent = new ManualResetEvent(false);
                resetEvents.Add(resetEvent);

                ThreadPool.QueueUserWorkItem(state => {
                    var tuple = (ValueTuple<TSource, ManualResetEvent>)state;
                    if (predicate(tuple.Item1))
                    {
                        lock (locker)
                        {
                            count++;
                        }
                    }
                    tuple.Item2.Set();
                }, new ValueTuple<TSource, ManualResetEvent>(item, resetEvent));
            }

            try
            {
                // SAT上面不能用WaitAll，会报异常
                // WaitHandle.WaitAll(resetEvents.ToArray());

                // 改用WaitOne逐个等待
                foreach (var resetEvent in resetEvents)
                {
                    resetEvent.WaitOne();
                }
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }
            return count;
        }

        public static TSource? FirstOrDefault<TSource>(
            this ParallelQuery<TSource> source,
            Func<TSource, bool> predicate)
        {
            TSource? result = default(TSource);
            var found = false;
            var resetEvent = new ManualResetEvent(false);
            var locker = new object();

            foreach (var item in source)
            {
                if (found) break;

                ThreadPool.QueueUserWorkItem(state => {
                    var current = (TSource)state;
                    if (predicate(current))
                    {
                        lock (locker)
                        {
                            if (!found)
                            {
                                result = current;
                                found = true;
                            }
                        }
                    }
                }, item);
            }

            resetEvent.WaitOne(1000); // 超时等待
            return result;
        }
    }
}
#endif