using System.Diagnostics;
using System.Xml;

namespace Gstar_IMEFilter;
public class Settings
{
    static string _MyDir = "";
    static string _MySettingsPath = "";
    internal static string _UserFilter = "";
    internal static IMEHookStyle _IMEHookStyle = IMEHookStyle.Global;
    internal static IMESwitchMode _IMEInputSwitch = IMESwitchMode.Shift;

    internal static string MyDir
    {
        get
        {
            if (_MyDir.Length == 0)
                _MyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return _MyDir;
        }
    }

    public static string MySettingsPath
    {
        get
        {
            if (_MySettingsPath.Length == 0)
                _MySettingsPath = Path.Combine(MyDir, nameof(Gstar_IMEFilter) + ".xml");
            return _MySettingsPath;
        }
    }

    public static string UserFilter
    {
        get => _UserFilter;
        set
        {
            if (_UserFilter.Length == 0)
                return;
            _UserFilter = value;
            SaveSettings();
        }
    }

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
                    case "userfilter":
                    {
                        _UserFilter = xmlReader.ReadInnerXml().ToUpper();
                    }
                    break;
                    case "imehookstyle":
                    {
                        int.TryParse(xmlReader.ReadInnerXml(), out int ime);
                        _IMEHookStyle = (IMEHookStyle)ime;
                    }
                    break;
                    case "imeinputswitch":
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
            xmlWriter.WriteComment("输入法＋");

            xmlWriter.WriteStartElement("settings");
            {
                xmlWriter.WriteStartElement("userfilter");
                xmlWriter.WriteString(UserFilter);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("imehookstyle");
                xmlWriter.WriteString(((int)IMEHookStyle).ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("imeinputswitch");
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
/// 切换输入法方式
/// </summary>
public enum IMESwitchMode : byte
{
    [Description("输入法关闭")]
    Disable,
    [Description("输入法开启:Shift")]
    Shift,
    [Description("输入法开启:Ctrl")]
    Ctrl,
    [Description("输入法开启:CtrlAndSpace")]
    CtrlAndSpace,
    [Description("输入法开启:CtrlAndShift")]
    CtrlAndShift,
    [Description("输入法开启:WinAndSpace")]
    WinAndSpace,
}