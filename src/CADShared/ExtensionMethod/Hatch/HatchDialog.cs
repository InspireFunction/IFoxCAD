#if acad

namespace IFoxCAD.Cad;

/// <summary>
/// 填充图案选择对话框<br/>
/// 只是为了保持和其他Cad内置Dialog的使用一致性，并非真正的Dialog
/// </summary>
public class HatchDialog
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public HatchDialog(string? name = null, bool showCustom = true)
    {
        // 默认填充图案名称为当前名称
        Name = name ?? SystemVariableManager.HPName;
        ShowCustom = showCustom;
    }

    /// <summary>
    /// 填充图案名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 显示自定义图案
    /// </summary>
    public bool ShowCustom { get; set; }

    /// <summary>
    /// 模态显示图案选择对话框
    /// </summary>
    /// <returns>成功返回<c>true</c></returns>
    public bool ShowDialog()
    {
        var dr = ShowHatchPaletteDialog(Name, ShowCustom, out var newPattern);
        if (dr)
        {
            Name = Marshal.PtrToStringAuto(newPattern) ?? "";
        }

        return dr;
    }

    [DllImport("acad.exe", CharSet = CharSet.Auto,
        EntryPoint = "?acedHatchPalletteDialog@@YA_NPEB_W_NAEAPEA_W@Z")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowHatchPaletteDialog(string currentPattern,
        [MarshalAs(UnmanagedType.Bool)] bool showCustom, out IntPtr newPattern);
}
#endif