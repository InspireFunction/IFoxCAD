// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.ComponentModel;
//using IFoxCAD.Basal;
using static TestConsole.Program;
using TestConsole;




// display the description attribute from the enum
foreach (Colour type in (Colour[])Enum.GetValues(typeof(Colour)))
{
    Console.WriteLine(EnumExtensions.ToName(type));
}

var de = Colour.Yellow.GetAttribute<DescriptionAttribute>();
Console.WriteLine(de);

// Get the array from the description
string xStr = "Yellow";
Colour thisColour = EnumExtensions.FromName<Colour>(xStr);

string xStr1 = "Colour Green";
Colour thisColour1 = EnumExtensions.FromName<Colour>(xStr1);



Console.ReadLine();
