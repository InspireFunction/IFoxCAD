namespace IFoxCAD.Cad;

/// <summary>
/// 进度条
/// </summary>
public static class ProgressMeterUtils
{
    /// <summary>
    /// 设置状态栏进度条
    /// </summary>
    public static void SetApplicationStatusBarProgressMeter(string str, int mixPos, int maxPos)
    {
#if acad
        Utils.SetApplicationStatusBarProgressMeter(str, mixPos, maxPos);
#elif zcad
        ZcedSetStatusBarProgressMeter(str, mixPos, maxPos);
#endif
    }

    /// <summary>
    /// 设置状态栏进度条
    /// </summary>
    public static void SetApplicationStatusBarProgressMeter(int nPos)
    {
#if acad
        Utils.SetApplicationStatusBarProgressMeter(nPos);
#elif zcad
        ZcedSetStatusBarProgressMeterPos(nPos);
#endif
    }

    /// <summary>
    /// 关闭进度条
    /// </summary>
    public static void RestoreApplicationStatusBar()
    {
#if acad
        Utils.RestoreApplicationStatusBar();
#elif zcad
        ZcedSetStatusBarProgressMeterStop();
#endif
    }

#if zcad
    private const string kDllName = "zwcad.exe";

    [DllImport(kDllName, EntryPoint = "?zcedSetStatusBarProgressMeter@@YAHPEB_WHH@Z",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void ZcedSetStatusBarProgressMeter(
        [MarshalAs(UnmanagedType.LPWStr)] string label, int minPos, int maxPos);

    [DllImport(kDllName, EntryPoint = "?zcedSetStatusBarProgressMeterPos@@YAHH@Z",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void ZcedSetStatusBarProgressMeterPos(int position);

    [DllImport(kDllName, EntryPoint = "?zcedSetStatusBarProgressMeterStop@@YAHXZ",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void ZcedSetStatusBarProgressMeterStop();
#endif
}