using System.Windows;
using System.Windows.Input;
using Window = System.Windows.Window;

namespace Gstar_IMEFilter;

/// <summary>
/// SettingsWindow.xaml 的交互逻辑
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += Window_Loaded;
        PreviewKeyDown += Window_PreviewKeyDown;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Settings._AutoEn2Cn = [.. ExCMD.Text.Split(';')];
        Settings._IMEHookStyle = (IMEHookStyle)CBox.SelectedIndex;
        Settings.SaveSettings();
        IMEControl.SetIMEHook();
        DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CBox.SelectedIndex = (int)Settings.IMEHookStyle;
        ExCMD.Text = string.Join(";", Settings.AutoEn2Cn.ToArray());
        // TODO 高版本这里没有测试,惊惊电脑没有装
        // DeCMD.Text = string.Join(",", IMEControl.DefaultCmds_AutoEn2Cn.ToArray());
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;
        DialogResult = false;
    }
}