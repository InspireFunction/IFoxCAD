namespace Test;

public class NetLoad
{
    [CommandMethod("netloadx")]
    public static void Test_NetLoad()
    {
        IFoxCAD.LoadEx.LoaderForm form = new();
        Acap.ShowModalDialog(form);
    }
}
