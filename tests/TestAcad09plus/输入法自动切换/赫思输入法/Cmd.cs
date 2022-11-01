using System.Diagnostics;

namespace Gstar_IMEFilter;

public class Cmd
{
    [IFoxInitialize]
    public void Initialize()
    {
        Env.Printl($"※输入法控制※" +
                  $"\n{nameof(Gstar_IMEFilter1)} - 切换开关" +
                  $"\n{nameof(Gstar_IMEFilter2)} - 控制模式" +
                  $"\n{nameof(Gstar_IMEFilterSettings)} - 设置");
        DocReactor.IntialReactor();
        Settings.LoadSettings();
        IMEControl.SetIMEHook();
        Acap.QuitWillStart += (s, e) => {
            IMEControl.UnIMEHook();
        };
    }



    [CommandMethod(nameof(Gstar_IMEFilter1))]
    public void Gstar_IMEFilter1()
    {
        Settings.Use = !Settings.Use;
        Env.Printl("已经 " + (Settings.Use ? "开启" : "禁用") + " 输入法+");
    }

    [CommandMethod(nameof(Gstar_IMEFilter2))]
    public void Gstar_IMEFilter2()
    {
        // 存在重复的关键字首字母,所以用数字代替
        PromptIntegerOptions options = new($"\n输入法切换模式:");
        foreach (var suit in Enum.GetValues(typeof(IMESwitchMode)))
            options.Keywords.Add($"{suit}", $"{suit}", $"{suit}({(int)suit})", true, true);
        options.Keywords.Default = Settings.IMEInputSwitch.ToString();
        var result = Env.Editor.GetInteger(options);

        if (result.Status != PromptStatus.OK)
            return;
        var max = Enum.GetValues(typeof(IMESwitchMode)).Cast<int>().Max();
        if (result.Value > max)
        {
            Env.Printl("设置失败:数值过大");
            return;
        }
        Settings.IMEInputSwitch = (IMESwitchMode)result.Value;
        Env.Printl($"设置为: {Settings.IMEInputSwitch}");
    }

    [CommandMethod(nameof(Gstar_IMEFilterSettings))]
    public void Gstar_IMEFilterSettings()
    {
        /*cad21若使用进程模式,则搜狗拦截不到,并且破坏了内存*/
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        ShowWPFWindowCentered.Show(new SettingsWindow(), true);
    }
}