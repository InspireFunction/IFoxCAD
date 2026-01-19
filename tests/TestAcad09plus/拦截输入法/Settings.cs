using ConcurrentCollections;
using IFoxCAD.Cad;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Gstar_IMEFilter;

public class Settings
{
    static string _MyDir = "";
    internal static string MyDir
    {
        get
        {
            if (_MyDir.Length == 0)
                _MyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return _MyDir;
        }
    }

    static string _MySettingsPath = "";
    public static string MySettingsPath
    {
        get
        {
            if (_MySettingsPath.Length == 0)
                _MySettingsPath = Path.Combine(MyDir, nameof(Gstar_IMEFilter) + ".json");
            return _MySettingsPath;
        }
    }

    // 这里更新容器期间会多线程访问
    internal static ConcurrentSet<string> _AutoEn2Cn = ["MTEXT", "DDEDIT", "MTEDIT", "TEXT", "DTEXT", "TEXTEDIT", "EATTEDIT", "TABLEDIT", "MLEADER", "MLEADERCONTENTEDIT", "QLEADER", "-BLOCK", "-GROUP", "GROUP", "GROUPEDIT", "ATTIPEDIT"];

    /// <summary>
    /// 豁免命令组: 默认和配置的
    /// </summary>
    public static ConcurrentSet<string> AutoEn2Cn
    {
        get => _AutoEn2Cn;
        set
        {
            if (_AutoEn2Cn.Count == 0)
                return;
            _AutoEn2Cn = value;
            SaveSettings();
        }
    }

    internal static ConcurrentSet<string> _AutoCn2En = ["BLOCK", "GROUP"];
    /// <summary>
    /// 豁免命令组: 自动切换为英文输入法
    /// </summary>
    public static ConcurrentSet<string> AutoCn2En
    {
        get => _AutoCn2En;
        set
        {
            if (_AutoCn2En.Count == 0)
                return;
            _AutoCn2En = value;
            SaveSettings();
        }
    }

    internal static IMEHookStyle _IMEHookStyle = IMEHookStyle.Global;
    public static IMEHookStyle IMEHookStyle
    {
        get => _IMEHookStyle;
        set
        {
            if (_IMEHookStyle == value)
                return;
            _IMEHookStyle = value;
            SaveSettings();
            IMEControl.SetIMEHook();
        }
    }

    internal static IMESwitchMode _IMEInputSwitch = IMESwitchMode.Shift;
    public static IMESwitchMode IMEInputSwitch
    {
        get => _IMEInputSwitch;
        set
        {
            if (_IMEInputSwitch == value)
                return;
            _IMEInputSwitch = value;
            SaveSettings();
        }
    }

    public static bool LoadSettings()
    {
        if (!File.Exists(MySettingsPath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(MySettingsPath, Encoding.UTF8);
            var settings = MyJson.DeserializeObject<SettingsData>(json);
            if (settings != null)
            {
                _AutoEn2Cn = settings.AutoEn2Cn;
                _AutoCn2En = settings.AutoCn2En;
                _IMEHookStyle = settings.IMEHookStyle;
                _IMEInputSwitch = settings.IMEInputSwitch;
            }
            return true;
        }
        catch (Exception)
        {
            Debugger.Break();
            throw;
        }
    }

    public static void SaveSettings()
    {
        _lock.EnterReadLock(); // 获取读锁
        try
        {
            var settings = new SettingsData
            {
                AutoEn2Cn = Settings.AutoEn2Cn,
                AutoCn2En = Settings.AutoCn2En,
                IMEHookStyle = Settings.IMEHookStyle,
                IMEInputSwitch = Settings.IMEInputSwitch,
            };

            string json = MyJson.SerializeObject(settings, new MyJsonSettings { Formatting = Formatting.Indented });

            Env.Printl(json);
            File.WriteAllText(MySettingsPath, json, Encoding.UTF8);
        }
        catch (Exception)
        {
            Debugger.Break();
            throw;
        }
        finally
        {
            _lock.ExitReadLock(); // 释放读锁
        }
    }

    // 写入文件需要读写锁的
    private static readonly ReaderWriterLockSlim _lock = new();
}


/// <summary>
/// 设置数据类，用于JSON序列化
/// </summary>
public class SettingsData
{
    public ConcurrentSet<string> AutoEn2Cn { get; set; } = [];
    public ConcurrentSet<string> AutoCn2En { get; set; } = [];
    public IMEHookStyle IMEHookStyle { get; set; } = IMEHookStyle.Global;
    public IMESwitchMode IMEInputSwitch { get; set; } = IMESwitchMode.Shift;
}

/// <summary>
/// 钩子样式
/// </summary>
[Flags]
public enum IMEHookStyle : byte
{
    Global,//全局钩子控制
    Process,//进程钩子控制
}

/// <summary>
/// 切换输入法方式<br/>
/// 作用的地方仅为输入豁免命令时候自动切换到中文
/// </summary>
[Flags]
public enum IMESwitchMode : byte
{
    [Description("输入拦截关闭")]
    Disable,
    [Description("输入拦截开启:不切换")]
    NotSwitch,
    [Description("输入拦截开启:Shift")]
    Shift,
    [Description("输入拦截开启:Ctrl")]
    Ctrl,
    [Description("输入拦截开启:CtrlAndSpace")]
    CtrlAndSpace,
    [Description("输入拦截开启:CtrlAndShift")]
    CtrlAndShift,
    [Description("输入拦截开启:WinAndSpace")]
    WinAndSpace,
}