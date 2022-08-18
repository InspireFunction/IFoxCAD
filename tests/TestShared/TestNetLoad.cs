using System.Windows.Forms;

namespace Test;

public class NetLoad
{
    static IFoxCAD.LoadEx.LoaderForm? _form;
    [CommandMethod("netloadx")]
    public static void Test_NetLoad()
    {
        _form ??= new();

        if (!_form.Visible)
            _form.Visible = true;

        _form.Show();
        //Acap.ShowModalDialog(_form);
        _form.Focus();
    }
}
