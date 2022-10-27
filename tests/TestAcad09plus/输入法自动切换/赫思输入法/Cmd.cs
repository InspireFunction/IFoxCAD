namespace Gstar_IMEFilter;

public class Cmd
{
    [IFoxInitialize]
    public void Initialize()
    {
        Env.Printl($"※输入法控制※\n{nameof(Gstar_IMEFilter)} - 切换开关\n" + nameof(Gstar_IMEFilterSettings) + " - 设置\n");
        DocReactor.IntialReactor();
        Settings.LoadSettings();
        IMEControl.SetIMEHook();
        Acap.QuitWillStart += (s, e) => {
            IMEControl.UnIMEHook();
        };
    }

    [CommandMethod(nameof(Gstar_IMEFilter))]
    public void Gstar_IMEFilter()
    {
        Settings.Use = !Settings.Use;
        Env.Printl("已经 " + (Settings.Use ? "开启" : "禁用") + " 输入法+");
    }

    [CommandMethod(nameof(Gstar_IMEFilterSettings))]
    public void Gstar_IMEFilterSettings()
    {
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        ShowWPFWindowCentered.Show(new SettingsWindow(), true);
    }
}