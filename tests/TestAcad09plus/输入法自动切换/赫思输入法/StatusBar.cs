namespace Gstar_IMEFilter;

using Autodesk.AutoCAD.Windows;
using System.Windows.Forms;

public class StatusBar
{
    static string _name = nameof(Gstar_IMEFilter);
    static Pane? _pane = null;

    public static void IMEAddPane()
    {
        if (_pane is not null)
            return;

        // 遍历当前
        var panes = Acap.StatusBar.Panes;
        for (int i = 0; i < panes.Count(); i++)
        {
            if (panes[i].ToolTipText == _name)
            {
                _pane = panes[i];
                break;
            }
        }

        if (_pane is not null)
            return;

        // 没有找到的话,进行初始化
        _pane = new()
        {
            ToolTipText = _name,
            Text = GetUseText(),
            Style = PaneStyles.Command | PaneStyles.PopUp,
            /* PaneStyles.NoBorders |
             * PaneStyles.Stretch |
             * PaneStyles.PopOut
             */
        };
        _pane.MouseDown += Pane_MouseDown;
        panes.Insert(0, _pane);
        Acap.StatusBar.Update();
    }

    public static void IMERemovePane()
    {
        if (_pane is null)
            return;

        // cad08需要用这样的方式才能保证移除后更新界面
        var panes = Acap.StatusBar.Panes;
        for (int i = panes.Count() - 1; i >= 0; i--)
            if (panes[i] == _pane)
                panes.RemoveAt(i);
        _pane.Dispose();
        _pane = null;

        // cad08用这样的方式移除后无法更新界面,而高版本可以
        //Acap.StatusBar.Panes.Remove(_pane);
        //Acap.StatusBar.Update();
        //_pane.Dispose();
        //_pane = null;
        //Acap.UpdateScreen();
        //RedrawEx.Redraw();
        //System.Windows.Forms.Application.DoEvents();
    }

    static string GetUseText()
    {
        return EnumEx.GetDesc(Settings.IMEInputSwitch);
    }
    static readonly IMESwitchMode _ismMax = Enum.GetValues(typeof(IMESwitchMode)).Cast<IMESwitchMode>().Max();

    static void Pane_MouseDown(object sender, StatusBarMouseDownEventArgs e)
    {
        if (_pane is null)
            return;

        // 它就只支持两个枚举
        switch (e.Button)
        {
            case MouseButtons.Left:
            {
                // 防白痴,一个环形选择模式
                if (Settings.IMEInputSwitch < _ismMax)
                    ++Settings.IMEInputSwitch;
                else
                    Settings.IMEInputSwitch = 0;
                _pane.Text = GetUseText();
                Acap.StatusBar.Update();
            }
            break;
            case MouseButtons.Right:
            {
                // 右键可以直接关闭
                Settings.IMEInputSwitch = 0;
                _pane.Text = GetUseText();
                Acap.StatusBar.Update();
            }
            break;
        }
    }
}

public static class PaneHelper
{
    public static int Count(this PaneCollection panes)
    {
#if NET35
        return panes.get_Count();
#else
        return panes.Count;
#endif
    }
}