using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;

namespace Gstar_IMEFilter;

public class Cmd
{
    private static FileSystemWatcher? _settingsWatcher;
    private static bool _send = false;

    [IFoxInitialize]
    public void Initialize(Document doc)
    {
        Env.Printl($"※拦截输入法控制※");
        DocReactor.IntialReactor();
        if (Settings.LoadSettings())
        {
            // 启动文件监控
            StartWatchingSettingsFile();
            Env.Printl("配置存在: " + Settings.MySettingsPath);
        }
        else
        {
            Env.Printl("配置不存在: " + Settings.MySettingsPath);
            Env.Printl($"※拦截输入法控制※ {nameof(Gstar_IMEFilterSettings)} - 运行进行初始化");
        }
        IMEControl.SetIMEHook();
        StatusBar.IMEAddPane();

        AcadIdleManager.OnIdle += AcadIdleManager_OnIdle;
    }

    [CommandMethod(nameof(Gstar_IMEFilterSettings))]
    public void Gstar_IMEFilterSettings()
    {
#if NET35
        _settingsWatcher?.EnableRaisingEvents = false;

        // 保存配置
        Settings.SaveSettings();

        // 提示用户打开文件
        Env.Printl("配置已保存到: " + Settings.MySettingsPath);
        Env.Printl("请打开该文件进行编辑。");

        // 打开配置文件
        System.Diagnostics.Process.Start("notepad.exe", Settings.MySettingsPath);

        // 启动文件监控
        StartWatchingSettingsFile();
#else
        /*cad21若使用进程模式,则搜狗拦截不到,并且破坏了内存*/
        Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        ShowWPFWindowCentered.Show(new SettingsWindow(), true);
#endif
    }

    /// <summary>
    /// 启动监控配置文件
    /// </summary>
    private static void StartWatchingSettingsFile()
    {
        // 如果已经存在监控实例，先停止
        if (_settingsWatcher != null)
        {
            _settingsWatcher.Dispose();
        }

        // 创建新的文件监控器
        _settingsWatcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(Settings.MySettingsPath),
            Filter = Path.GetFileName(Settings.MySettingsPath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        // 订阅文件变化事件
        _settingsWatcher.Changed += OnSettingsFileChanged;

        // 启动监控
        _settingsWatcher.EnableRaisingEvents = true;

        Env.Printl("※拦截输入法控制※ 已启动配置文件监控,修改后将自动重新加载");
    }

    /// <summary>
    /// 配置文件变化事件处理
    /// 是异步线程,要切入到cad只能用空闲事件
    /// </summary>
    private static void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
    {
        _send = true;
    }

    private static void AcadIdleManager_OnIdle(object sender, EventArgs e)
    {
        if (_send)
        {
            _send = !_send;
            try
            {
                // 重新加载配置(更新了Set就好了)
                Settings.LoadSettings();

                // 不需要更新钩子设置 IMEControl.SetIMEHook();
                // 因为我们是修改Set,会每次都动态使用这个容器

                Env.Printl("※拦截输入法控制※ 配置文件已更新，设置已重新加载。");
            }
            catch (Exception ex)
            {
                Env.Printl("※拦截输入法控制※ 加载配置文件时出错: " + ex.Message);
            }
        }
    }

    [IFoxInitialize(Sequence.EndDocs)]
    public void Terminate(Document doc)
    {
        try
        {
            // 卸载空闲事件
            AcadIdleManager.OnIdle -= AcadIdleManager_OnIdle;

            // 停止文件监控
            if (_settingsWatcher != null)
            {
                _settingsWatcher.EnableRaisingEvents = false;
                _settingsWatcher.Dispose();
                _settingsWatcher = null;
            }

            // 移除文档反应器
            DocReactor.RemoveReactor();

            // 卸载钩子
            IMEControl.UnIMEHook();

            // 移除状态栏面板
            StatusBar.IMERemovePane();

            Env.Printl("※拦截输入法控制※ 已安全卸载");
        }
        catch (Exception ex)
        {
            Env.Printl("※拦截输入法控制※ 卸载时出错: " + ex.Message);
        }
    }
}