using System.Windows;
using System.Windows.Input;

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
        Settings._UserFilter = ExCMD.Text;
        Settings._IMEHookStyle = (IMEHookStyle)CBox.SelectedIndex;
        Settings.SaveSettings();
        IMEControl.SetIMEHook();
        IMEControl.SaveFt();
        DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CBox.SelectedIndex = (int)Settings.IMEHookStyle;
        ExCMD.Text = Settings.UserFilter;
        DeCMD.Text = string.Join(",", IMEControl.DefaultCmds.ToArray());
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;
        DialogResult = false;
    }
}