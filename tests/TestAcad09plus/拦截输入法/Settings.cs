using System.Diagnostics;
using System.Xml;

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
                _MySettingsPath = Path.Combine(MyDir, nameof(Gstar_IMEFilter) + ".xml");
            return _MySettingsPath;
        }
    }

    internal static string _UserFilter_AutoEn2Cn = "";
    public static string UserFilter_AutoEn2Cn
    {
        get => _UserFilter_AutoEn2Cn;
        set
        {
            if (_UserFilter_AutoEn2Cn.Length == 0)
                return;
            _UserFilter_AutoEn2Cn = value;
            SaveSettings();
        }
    }

    internal static string _UserFilter_AutoCn2En = "";
    public static string UserFilter_AutoCn2En
    {
        get => _UserFilter_AutoCn2En;
        set
        {
            if (_UserFilter_AutoCn2En.Length == 0)
                return;
            _UserFilter_AutoCn2En = value;
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

    public static void LoadSettings()
    {
        if (!File.Exists(MySettingsPath))
            return;

        try
        {
            using var xmlReader = XmlReader.Create(MySettingsPath);
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;
                string left = xmlReader.Name.ToLower();
                switch (left)
                {
                    case nameof(UserFilter_AutoEn2Cn):
                    {
                        _UserFilter_AutoEn2Cn = xmlReader.ReadInnerXml().ToUpper();
                    }
                    break;
                    case nameof(UserFilter_AutoCn2En):
                    {
                        _UserFilter_AutoCn2En = xmlReader.ReadInnerXml().ToUpper();
                    }
                    break;
                    case nameof(IMEHookStyle):
                    {
                        int.TryParse(xmlReader.ReadInnerXml(), out int ime);
                        _IMEHookStyle = (IMEHookStyle)ime;
                    }
                    break;
                    case nameof(IMEInputSwitch):
                    {
                        int.TryParse(xmlReader.ReadInnerXml(), out int ime);
                        _IMEInputSwitch = (IMESwitchMode)ime;
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debugger.Break();
            throw ex;
        }
    }

    internal static void SaveSettings()
    {
        try
        {
            XmlWriterSettings settings = new()
            {
                Indent = 1 != 0,
                NewLineChars = Environment.NewLine
            };

            using var xmlWriter = XmlWriter.Create(MySettingsPath, settings);
            xmlWriter.WriteStartDocument(1 != 0);
            xmlWriter.WriteComment("拦截输入法");

            xmlWriter.WriteStartElement(nameof(Settings));
            {
                xmlWriter.WriteStartElement(nameof(UserFilter_AutoEn2Cn));
                xmlWriter.WriteString(UserFilter_AutoEn2Cn);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(nameof(UserFilter_AutoCn2En));
                xmlWriter.WriteString(UserFilter_AutoCn2En);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(nameof(IMEHookStyle));
                xmlWriter.WriteString(((int)IMEHookStyle).ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(nameof(IMEInputSwitch));
                xmlWriter.WriteString(((int)IMEInputSwitch).ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
        }
        catch (Exception ex)
        {
            Debugger.Break();
            throw ex;
        }
    }
}

/// <summary>
/// 钩子样式
/// </summary>
public enum IMEHookStyle : byte
{
    Global,//全局钩子控制
    Process,//进程钩子控制
}

/// <summary>
/// 切换输入法方式<br/>
/// 作用的地方仅为输入豁免命令时候自动切换到中文
/// </summary>
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