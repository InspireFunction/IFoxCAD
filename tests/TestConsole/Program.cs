// See https://aka.ms/new-console-template for more information
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TestConsole;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using IFoxCAD.Cad;


#if true
namespace CalculatorDemo
{
    class Program
    {
        static void Main(string[] args)
        {
#if true
            //int nResult = AddTwoNumbers(10, 20);
            //Console.WriteLine(nResult);

            //AddTwoNumbers22((a, b) => {
            //    Console.WriteLine(a + b);
            //});

            //var a = new int[] { 1, 2, 3, 4, 5, 6, 78, 9, 92, };
            //a.ForEach(a => {
            //    Console.WriteLine(a);
            //});

            // 运行MemoryCache测试
            //Console.WriteLine();
            //TestMemoryCache.RunTest();  
#endif

            // 运行JSON序列化测试
            Console.WriteLine();
            TestJson.RunTests();

            // 运行Newtonsoft.Json对比测试
            Console.WriteLine();
            TestNewtonsoftJsonComparison.RunTests();

            // 运行JSON对比测试
            Console.WriteLine();
            TestJsonComparison.RunTests();

            // 运行简化JSON比较测试
            Console.WriteLine();
            SimpleJsonComparison.RunSimpleTest();

            // 运行 PreserveReferencesHandling 专项测试
            Console.WriteLine();
            PreserveRefTest.RunTest();

            // 运行循环引用测试
            Console.WriteLine();
            TestCircularReference.RunTests();

#if true2
            // 运行栈溢出修复测试
            Console.WriteLine();
            StackOverflowTestHelper.TestStackOverflowFix();

            // 运行HashtableEnumerator测试
            Console.WriteLine();
            TestHashtableEnumerator.RunTests(); 
#endif


        }

        [DebuggerHidden]
        private static int AddTwoNumbers(int nNum1, int nNum2)
        {
            return Add(nNum1, nNum2);
        }
        private static int Add(int op1, int op2)
        {
            return op1 + op2;
        }

        [DebuggerHidden]
        private static void AddTwoNumbers22(Action<int, int> action)
        {
            action(10, 20);
        }
    }

    public static class Fors
    {
        /// <summary>
        /// 遍历集合,执行委托
        /// </summary>
        /// <typeparam name="T">集合值的类型</typeparam>
        /// <param name="source">集合</param>
        /// <param name="action">委托</param>
        [DebuggerHidden]
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action.Invoke(element);
            }
        }
    }
}
#endif

#if true2

Console.WriteLine("***************************************************");


List<int> list = new List<int>();
list.Add(1);
list.Add(2);
list.Add(3);
list.Add(4);
list.Add(5);
list.ForEach((x, loop) => {
    if (x == 3)
        loop.Break();
    Console.WriteLine(x);
});



// 乱序
Console.WriteLine(PlugIn.JoinBox.PrintNote());
Console.WriteLine(PlugIn.Lisp.PrintNote());// 这里先交换顺序来试试能不能成功
Console.WriteLine(PlugIn.IMinCad.PrintNote());
Console.WriteLine(PlugIn.YuanQuan.PrintNote());
Console.WriteLine(PlugIn.All.PrintNote());
Console.WriteLine(PlugIn.DOCBAR.PrintNote());
Console.WriteLine(PlugIn.DUOTAB.PrintNote());
Console.WriteLine("***************************************************");
// 乱序2
Console.WriteLine(PlugIn2.JoinBox.PrintNote());
Console.WriteLine(PlugIn2.Lisp.PrintNote());// 这里先交换顺序来试试能不能成功
Console.WriteLine(PlugIn2.IMinCad.PrintNote());
Console.WriteLine(PlugIn2.YuanQuan.PrintNote());
Console.WriteLine(PlugIn2.All.PrintNote());
Console.WriteLine(PlugIn2.DOCBAR.PrintNote());
Console.WriteLine(PlugIn2.DUOTAB.PrintNote());

EnumEx.CleanCache();

// 表达式树例子
TestConsole.Test_Expression.Demo3();
// TestConsole.Test_Expression.Demo1();

#region 元组测试
var valuetuple = (1, 2);

Console.WriteLine(valuetuple.ToString());

int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
int lastElement = someArray[^1]; // lastElement = 5
Console.WriteLine(lastElement);
int midElement = someArray[^3];
Console.WriteLine(midElement);
var range = someArray[1..3];
foreach (var item in range)
    Console.WriteLine(item);
#endregion

Console.ReadLine();


#region 测试遍历枚举
// Season a = Season.Autumn;
// Console.WriteLine($"Integral value of {a} is {(int)a}");  // output: Integral value of Autumn is 2
// foreach (var enumItem in Enum.GetValues(typeof(Season)))
//    Console.WriteLine((byte)enumItem);

var sb = new StringBuilder();
/*因为 net framework 没写好的原因,导致直接使用迭代器反而更慢,到了NET60就迭代器比foreach更快*/
var enums = Enum.GetValues(typeof(Season)).GetEnumerator();
while (enums.MoveNext())
{
    sb.Append(((byte)enums.Current).ToString());
    sb.Append(",");
}
Console.WriteLine(sb);

sb.Remove(sb.Length - 1, 1);// 剔除末尾,
// 因为有返回值所以容易理解成 sb = sb.Remove(sb.Length - 1, 1);
Console.WriteLine(sb);

public enum Season : byte
{
    Spring,
    Summer,
    Autumn,
    Winter
}
#endregion
#endif

public class StackOverflowTestHelper
{
    public class Node
    {
        public string Name { get; set; } = string.Empty;
        public Node? Parent { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
    }

    public static void TestStackOverflowFix()
    {
        Console.WriteLine("=== 栈溢出修复测试 ===");

        // 创建循环引用
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child" };

        parent.Children.Add(child);
        child.Parent = parent; // 创建循环引用

        Console.WriteLine("1. 测试 ReferenceLoopHandling.Error（应该抛出异常）:");
        try
        {
            var errorSettings = new MyJsonSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };
            var errorResult = MyJson.SerializeObject(parent, errorSettings);
            Console.WriteLine($"意外成功: {errorResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"预期异常: {ex.Message}");
        }

        Console.WriteLine("\n2. 测试 ReferenceLoopHandling.Ignore（应该忽略循环引用）:");
        try
        {
            var ignoreSettings = new MyJsonSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var ignoreResult = MyJson.SerializeObject(parent, ignoreSettings);
            Console.WriteLine($"成功，输出: {ignoreResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"意外异常: {ex.Message}");
        }

        Console.WriteLine("\n3. 测试 ReferenceLoopHandling.Serialize（应该保留引用）:");
        try
        {
            var serializeSettings = new MyJsonSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            var serializeResult = MyJson.SerializeObject(parent, serializeSettings);
            Console.WriteLine($"成功，输出: {serializeResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"意外异常: {ex.Message}");
        }

        Console.WriteLine("\n栈溢出修复测试完成！");
    }
}