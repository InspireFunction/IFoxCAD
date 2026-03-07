#if NET35
using System;

namespace NET35VolatileUnitTests;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Volatile功能测试 (NET3.5) ===");
        Console.WriteLine();

        RunTest("1. int 类型", VolatileTestRunner.TestIntVolatile);
        RunTest("2. long 类型", VolatileTestRunner.TestLongVolatile);
        RunTest("3. float 类型", VolatileTestRunner.TestFloatVolatile);
        RunTest("4. double 类型", VolatileTestRunner.TestDoubleVolatile);
        RunTest("5. bool true", VolatileTestRunner.TestBoolTrueVolatile);
        RunTest("6. bool false", VolatileTestRunner.TestBoolFalseVolatile);
        RunTest("7. 多线程安全", VolatileTestRunner.TestThreadSafety);

        Console.WriteLine("所有测试完成！");
    }

    static void RunTest(string name, Func<(bool Success, string Message)> test)
    {
        Console.WriteLine($"{name}:");
        var (success, message) = test();
        Console.WriteLine($"   {message}");
        Console.WriteLine($"   结果: {(success ? "通过" : "失败")}");
        Console.WriteLine();
    }
}
#endif
