namespace NET35VolatileUnitTests;

public static class VolatileTestRunner
{
    public static (bool Success, string Message) TestIntVolatile()
    {
        int value = 0;
        Volatile.Write(ref value, 42);
        int readValue = Volatile.Read(ref value);
        bool success = readValue == 42;
        return (success, $"int: 写入 42, 读取 {readValue}");
    }

    public static (bool Success, string Message) TestLongVolatile()
    {
        long value = 0L;
        Volatile.Write(ref value, 123456789012345L);
        long readValue = Volatile.Read(ref value);
        bool success = readValue == 123456789012345L;
        return (success, $"long: 写入 123456789012345, 读取 {readValue}");
    }

    public static (bool Success, string Message) TestFloatVolatile()
    {
        float value = 0.0f;
        Volatile.Write(ref value, 3.14159f);
        float readValue = Volatile.Read(ref value);
        bool success = Math.Abs(readValue - 3.14159f) < 0.0001f;
        return (success, $"float: 写入 3.14159, 读取 {readValue}");
    }

    public static (bool Success, string Message) TestDoubleVolatile()
    {
        double value = 0.0;
        Volatile.Write(ref value, 2.718281828459045);
        double readValue = Volatile.Read(ref value);
        bool success = Math.Abs(readValue - 2.718281828459045) < 0.0000000001;
        return (success, $"double: 写入 2.718281828459045, 读取 {readValue}");
    }

    public static (bool Success, string Message) TestBoolTrueVolatile()
    {
        bool value = false;
        Volatile.Write(ref value, true);
        bool readValue = Volatile.Read(ref value);
        return (readValue, $"bool true: 写入 true, 读取 {readValue}");
    }

    public static (bool Success, string Message) TestBoolFalseVolatile()
    {
        bool value = true;
        Volatile.Write(ref value, false);
        bool readValue = Volatile.Read(ref value);
        return (!readValue, $"bool false: 写入 false, 读取 {readValue}");
    }

    public static (bool Success, string Message) TestThreadSafety()
    {
        int sharedValue = 0;
        bool startFlag = false;
        bool stopFlag = false;
        int readCount = 0;

        var writerThread = new Thread(() => {
            while (!Volatile.Read(ref startFlag)) { }
            for (int i = 1; i <= 10; i++)
            {
                Volatile.Write(ref sharedValue, i);
                Thread.Sleep(1);
            }
            Volatile.Write(ref stopFlag, true);
        });

        var readerThread = new Thread(() => {
            Volatile.Write(ref startFlag, true);
            int lastValue = 0;
            while (!Volatile.Read(ref stopFlag))
            {
                int currentValue = Volatile.Read(ref sharedValue);
                if (currentValue > lastValue)
                {
                    lastValue = currentValue;
                    readCount++;
                }
            }
        });

        writerThread.Start();
        readerThread.Start();

        bool completed = writerThread.Join(500) && readerThread.Join(500);
        bool success = completed && readCount > 0;
        return (success, $"线程完成: {completed}, 读取次数: {readCount}");
    }
}
