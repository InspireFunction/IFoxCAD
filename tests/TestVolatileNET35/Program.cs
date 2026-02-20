using System;
using System.Threading;

namespace TestVolatileNET35
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Volatile功能测试 (NET3.5) ===");
            Console.WriteLine();

            // 测试各种数据类型的Volatile读写
            TestIntVolatile();
            TestLongVolatile();
            TestFloatVolatile();
            TestDoubleVolatile();
            TestBoolVolatile();
            
            // 多线程安全性测试
            TestThreadSafety();

            Console.WriteLine("所有测试完成！");
          
        }

        static void TestIntVolatile()
        {
            Console.WriteLine("1. 测试 int 类型 Volatile 读写:");
            int value = 0;
            int readValue;

            // 写入测试
            Volatile.Write(ref value, 42);
            Console.WriteLine($"   写入值: 42");

            // 读取测试
            readValue = Volatile.Read(ref value);
            Console.WriteLine($"   读取值: {readValue}");
            Console.WriteLine($"   测试结果: {(readValue == 42 ? "通过" : "失败")}");
            Console.WriteLine();
        }

        static void TestLongVolatile()
        {
            Console.WriteLine("2. 测试 long 类型 Volatile 读写:");
            long value = 0L;
            long readValue;

            // 写入测试
            Volatile.Write(ref value, 123456789012345L);
            Console.WriteLine($"   写入值: 123456789012345");

            // 读取测试
            readValue = Volatile.Read(ref value);
            Console.WriteLine($"   读取值: {readValue}");
            Console.WriteLine($"   测试结果: {(readValue == 123456789012345L ? "通过" : "失败")}");
            Console.WriteLine();
        }

        static void TestFloatVolatile()
        {
            Console.WriteLine("3. 测试 float 类型 Volatile 读写:");
            float value = 0.0f;
            float readValue;

            // 写入测试
            Volatile.Write(ref value, 3.14159f);
            Console.WriteLine($"   写入值: 3.14159");

            // 读取测试
            readValue = Volatile.Read(ref value);
            Console.WriteLine($"   读取值: {readValue}");
            Console.WriteLine($"   测试结果: {(Math.Abs(readValue - 3.14159f) < 0.0001f ? "通过" : "失败")}");
            Console.WriteLine();
        }

        static void TestDoubleVolatile()
        {
            Console.WriteLine("4. 测试 double 类型 Volatile 读写:");
            double value = 0.0;
            double readValue;

            // 写入测试
            Volatile.Write(ref value, 2.718281828459045);
            Console.WriteLine($"   写入值: 2.718281828459045");

            // 读取测试
            readValue = Volatile.Read(ref value);
            Console.WriteLine($"   读取值: {readValue}");
            Console.WriteLine($"   测试结果: {(Math.Abs(readValue - 2.718281828459045) < 0.0000000001 ? "通过" : "失败")}");
            Console.WriteLine();
        }

        static void TestBoolVolatile()
        {
            Console.WriteLine("5. 测试 bool 类型 Volatile 读写:");
            
            // 测试true值
            bool trueValue = false;
            Volatile.Write(ref trueValue, true);
            bool readTrue = Volatile.Read(ref trueValue);
            Console.WriteLine($"   true值测试 - 写入: true, 读取: {readTrue}, 结果: {(readTrue ? "通过" : "失败")}");

            // 测试false值
            bool falseValue = true;
            Volatile.Write(ref falseValue, false);
            bool readFalse = Volatile.Read(ref falseValue);
            Console.WriteLine($"   false值测试 - 写入: false, 读取: {readFalse}, 结果: {(!readFalse ? "通过" : "失败")}");
            
            Console.WriteLine();
        }

        // 多线程测试示例
        static void TestThreadSafety()
        {
            Console.WriteLine("6. 多线程安全性测试:");
            int sharedValue = 0;
            bool stopFlag = false;

            // 启动写入线程
            Thread writerThread = new Thread(() => {
                for (int i = 0; i < 1000; i++)
                {
                    Volatile.Write(ref sharedValue, i);
                    Thread.Sleep(1);
                }
                Volatile.Write(ref stopFlag, true);
            });

            // 启动读取线程
            Thread readerThread = new Thread(() => {
                int lastValue = -1;
                int readCount = 0;
                while (!Volatile.Read(ref stopFlag))
                {
                    int currentValue = Volatile.Read(ref sharedValue);
                    if (currentValue != lastValue)
                    {
                        Console.WriteLine($"   读取到新值: {currentValue}");
                        lastValue = currentValue;
                        readCount++;
                    }
                }
                Console.WriteLine($"   总共读取到 {readCount} 个不同值");
            });

            writerThread.Start();
            readerThread.Start();

            writerThread.Join();
            readerThread.Join();
            
            Console.WriteLine("   多线程测试完成");
            Console.WriteLine();
        }
    }
}