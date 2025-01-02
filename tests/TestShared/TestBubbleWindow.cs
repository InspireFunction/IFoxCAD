namespace TestAcad2025;

public static class TestBubbleWindow
{
    [CommandMethod(nameof(TestBubbleWindow))]
    public static void Run()
    {
        IFoxUtils.ShowBubbleWindow(5, "测试", "测试");
    }
}