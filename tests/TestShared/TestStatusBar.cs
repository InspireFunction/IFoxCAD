namespace Test;
using Autodesk.AutoCAD.Windows;
using IFoxCAD.Cad;

public class TestStatusBar
{
    static string _name = nameof(Gstar_IMEFilter);
    static Pane? _pane = null;

    [IFoxInitialize]
    [CommandMethod(nameof(TestAddPane))]
    public static void TestAddPane()
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
            Text = "打开",
            Style = PaneStyles.Command | PaneStyles.PopUp/* | PaneStyles.NoBorders | PaneStyles.Stretch | PaneStyles.PopUp|PaneStyles.PopOut*/,
        };
        _pane.MouseDown += Pane_MouseDown;
        panes.Insert(0, _pane);
        Acap.StatusBar.Update();
    }

    // 删除Pane
    [CommandMethod(nameof(TestRemovePane))]
    public static void TestRemovePane()
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

    [CommandMethod(nameof(PaneSwitch))]
    public static void PaneSwitch()
    {
        if (_pane is null)
            TestAddPane();
        else
            TestRemovePane();
    }

    /// <summary>
    /// 改变状态既能命令又能点击
    /// </summary>
    [CommandMethod(nameof(ChangePaneType))]
    public static void ChangePaneType()
    {
        if (_pane is null)
            return;
        _pane.Text = _pane.Text == "打开" ? "关闭" : "打开";
        //cad08要加这个才会变字,而高版本不用
        Acap.StatusBar.Update();
    }

    static void Pane_MouseDown(object sender, StatusBarMouseDownEventArgs e)
    {
        switch (e.Button)
        {
            case System.Windows.Forms.MouseButtons.Left:
            {
                ChangePaneType();
                Env.Printl("按了左键");
            }
            break;
            case System.Windows.Forms.MouseButtons.None:
            break;
            case System.Windows.Forms.MouseButtons.Right:
            {
                Env.Printl("按了右键,弹菜单");
            }
            break;
            case System.Windows.Forms.MouseButtons.Middle:
            break;
            case System.Windows.Forms.MouseButtons.XButton1:
            break;
            case System.Windows.Forms.MouseButtons.XButton2:
            break;
            default:
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