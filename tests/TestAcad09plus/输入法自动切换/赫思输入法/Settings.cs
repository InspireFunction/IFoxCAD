using System.Xml;

namespace Gstar_IMEFilter;

public class Settings
{
    static string _MyDir = "";
    static string _MySettingsPath = "";
    static bool _Use = true;
    internal static string _UserFilter = "";
    internal static IMEStyleS _IMEStyle = IMEStyleS.Global;

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

    public static bool Use
    {
        get => _Use;
        set
        {
            if (_Use == value)
                return;
            _Use = value;
            SaveSettings();
            IMEControl.SetIMEHook();
        }
    }

    public static IMEStyleS IMEStyle
    {
        get => _IMEStyle;
        set
        {
            if (_IMEStyle == value)
                return;
            _IMEStyle = value;
            SaveSettings();
            IMEControl.SetIMEHook();
        }
    }

    public static void LoadSettings()
    {
        if (!File.Exists(MySettingsPath))
            return;

        using var xmlReader = XmlReader.Create(MySettingsPath);
        while (xmlReader.Read())
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                continue;
            string left = xmlReader.Name.ToLower();
            switch (left)
            {
                case "use":
                bool.TryParse(xmlReader.ReadInnerXml(), out _Use);
                break;
                case "userfilter":
                _UserFilter = xmlReader.ReadInnerXml().ToUpper();
                break;
                case "imestyle":
                {
                    int.TryParse(xmlReader.ReadInnerXml(), out int imes);
                    _IMEStyle = (IMEStyleS)imes;
                }
                break;
            }
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

            XmlWriter xmlWriter = XmlWriter.Create(MySettingsPath, settings);
            xmlWriter.WriteStartDocument(1 != 0);
            xmlWriter.WriteComment("输入法＋");

            xmlWriter.WriteStartElement("settings");
            xmlWriter.WriteStartElement("use");
            xmlWriter.WriteString(Use.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("userfilter");
            xmlWriter.WriteString(UserFilter);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("imestyle");

            xmlWriter.WriteString(((int)IMEStyle).ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

            xmlWriter.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public enum IMEStyleS
    {
        Global,
        Process,
    }
}