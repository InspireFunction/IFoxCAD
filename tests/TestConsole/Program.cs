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

#if true
namespace CalculatorDemo
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            int nResult = AddTwoNumbers(10, 20);
            Console.WriteLine(nResult);

            AddTwoNumbers22((a, b) => {
                Console.WriteLine(a + b);
            });

            var a = new int[] { 1, 2, 3, 4, 5, 6, 78, 9, 92, };
            a.ForEach(a => {
                Console.WriteLine(a);
            });
#endif

            var aa = new int[] { 1, 2, 3, 4, 5, 6, 78, 9, 92, };
            Console.WriteLine(aa[1..^2]);


            var time = Timer.RunTime(() => {
                for (int i = 0; i < 10000000; i++)
                    i++;
            }, Timer.TimeEnum.Second);
            Console.WriteLine($"代码执行的时间：{time}");
        }

        private static int addtuple((int ,int ) b)
        {
            return b.Item1 + b.Item2;
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