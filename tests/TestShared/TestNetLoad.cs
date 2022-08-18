namespace Test;

public class NetLoad
{
    static IFoxCAD.LoadEx.LoaderForm? _form;
    [CommandMethod("netloadx")]
    public static void Test_NetLoad()
    {
        if (_form == null || _form.IsDisposed)
            _form = new();

        if (!_form.Visible)
            _form.Visible = true;

        _form.Show();
        _form.Focus();

        //Acap.ShowModalDialog(_form);
    }
}
