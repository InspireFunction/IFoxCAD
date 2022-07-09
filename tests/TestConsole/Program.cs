// See https://aka.ms/new-console-template for more information
using System;
using System.Text;
using IFoxCAD.Basal;
//using static TestConsole.Program;


/*测试遍历枚举*/
//Season a = Season.Autumn;
//Console.WriteLine($"Integral value of {a} is {(int)a}");  // output: Integral value of Autumn is 2
//foreach (var enumItem in Enum.GetValues(typeof(Season)))
//    Console.WriteLine((byte)enumItem);

var sb = new StringBuilder();
var enums = Enum.GetValues(typeof(Season)).GetEnumerator();
while (enums.MoveNext())
{
    sb.Append(((byte)enums.Current).ToString());
    sb.Append(",");
}
Console.WriteLine(sb);

sb.Remove(sb.Length - 1, 1);//剔除末尾,
//因为有返回值所以容易理解成 sb = sb.Remove(sb.Length - 1, 1);
Console.WriteLine(sb);


/*下面是元组测试*/
var valuetuple = (1, 2);

Console.WriteLine(valuetuple.ToString());

int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
int lastElement = someArray[^1]; // lastElement = 5
Console.WriteLine(lastElement);
int midElement = someArray[^3];
Console.WriteLine(midElement);
var range = someArray[1..3];
foreach (var item in range)
{
    Console.WriteLine(item);
}
Console.ReadLine();


public enum Season : byte
{
    Spring,
    Summer,
    Autumn,
    Winter
}

