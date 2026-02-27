namespace Autodesk.AutoCAD.ApplicationServices;

/// <summary>
/// ��Ԫ����ʱ��ģ��
/// </summary>
public static class Application
{
    public static Version Version { get; set; } = new Version(24, 1);
}

public class Version
{
    public int Major { get; }
    public int Minor { get; }

    public Version(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }
}
