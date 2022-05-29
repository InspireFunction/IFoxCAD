// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.ComponentModel;
//using IFoxCAD.Basal;
//using static TestConsole.Program;
using TestConsole;
using System.Runtime.CompilerServices;

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
