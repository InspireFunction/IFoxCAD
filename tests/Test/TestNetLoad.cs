namespace Test;

public class NetLoad
{
    [CommandMethod("Test_NetLoad")]
    public static void Test_NetLoad()
    {
        LoadEx.LoaderForm form = new();
        Acap.ShowModalDialog(form);
    }
}
