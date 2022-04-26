// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using IFoxCAD.Basal;

Console.WriteLine("Hello, World!");
var loop = new LoopList<int>
            {
                0,
                1,
                2,
                3,
                4,
                5
            };


loop.SetFirst(loop.Last!);
Console.WriteLine(loop);
Console.WriteLine(loop.Min());

//loop.SetFirst(new LoopListNode<int>(loop.Min(), loop));

//Console.WriteLine(loop);



var linkset = new LinkedHashSet<int>();
linkset.Add(0);
linkset.Add(1);
linkset.Add(2);
linkset.Add(3);
linkset.Add(4);
linkset.Add(5);

linkset.SetFirst(linkset.Last!);


Console.WriteLine(linkset);

linkset.SetFirst(linkset.MinNode!);

Console.WriteLine(linkset);

linkset.For(linkset.First!, (i, next, pre) => {
    Console.WriteLine($"{i} - ({next},{pre})");

});
Console.ReadKey();