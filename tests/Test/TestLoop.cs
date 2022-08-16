namespace Test;


using IFoxCAD.Basal;

using System.Diagnostics.CodeAnalysis;

public class TestLoop
{
    [CommandMethod("testloop")]
    public void Testloop()
    {
        var loop = new LoopList<int>
        {
            0,
            1,
            2,
            3,
            4,
            5
        };

        


        Env.Print(loop);

        loop.SetFirst(loop.Last);
        Env.Print(loop);
        Env.Print(loop.Min());
        loop.SetFirst(new LoopListNode<int> (loop.Min() ,loop));
        Env.Print(loop);



    }
}
