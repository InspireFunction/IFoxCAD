namespace Test.wpf;

public class Cmd
{
    [CommandMethod(nameof(Test_WPf))]
    public void Test_WPf()
    {
        var test = new TestView();
        Acap.ShowModalWindow(test);
    }
}