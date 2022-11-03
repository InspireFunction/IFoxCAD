namespace Gstar_IMEFilter;

public class Cmd
{
    [IFoxInitialize]
    public void Initialize()
    {
        Env.Printl($"※输入法＋控制※{nameof(Gstar_IMEFilterSettings)} - 设置");
        DocReactor.IntialReactor();
        Settings.LoadSettings();
        IMEControl.SetIMEHook();
        Acap.QuitWillStart += (s, e) => {
            IMEControl.UnIMEHook();
            IMEControl.SaveFt();
        };
        StatusBar.IMEAddPane();
    }

    [CommandMethod(nameof(Gstar_IMEFilterSettings))]
    public void Gstar_IMEFilterSettings()
    {
        /*cad21若使用进程模式,则搜狗拦截不到,并且破坏了内存*/
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        ShowWPFWindowCentered.Show(new SettingsWindow(), true);
    }
}