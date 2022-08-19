namespace Test;

public class NetLoad
{
    static IFoxCAD.LoadEx.LoaderForm? _form;
    [CommandMethod("loadx")]
    public static void Test_NetLoadx()
    {
        if (_form == null || _form.IsDisposed)
            _form = new();

#if NET35
        _form.DllPath = "G:\\K01.惊惊连盒\\net35\\JoinBoxAcad.dll";
#else
        _form.DllPath = "G:\\K01.惊惊连盒\\net48\\JoinBoxAcad.dll";
#endif

        if (!_form.Visible)
            _form.Visible = true;

        _form.Show();
        _form.Focus();

        //Acap.ShowModalDialog(_form);
    }
}
