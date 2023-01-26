namespace Test;
public class TestLoop
{
    [CommandMethod(nameof(Test_LoopList))]
    public void Test_LoopList()
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

        loop.SetFirst(loop.Last!);
        Env.Print(loop);
        Env.Print(loop.Min());
        loop.SetFirst(new LoopListNode<int>(loop.Min(), loop));
        Env.Print(loop);
    }
}