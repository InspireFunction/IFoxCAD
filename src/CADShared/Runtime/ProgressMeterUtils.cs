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
#if NET35
        throw new ArgumentNullException("SetApplicationStatusBarProgressMeter 不支持");
#elif acad
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
#if NET35
        throw new ArgumentNullException("SetApplicationStatusBarProgressMeter 不支持");
#elif acad
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
#if NET35
        throw new ArgumentNullException("RestoreApplicationStatusBar 不支持");
#elif acad
        Utils.RestoreApplicationStatusBar();
#elif zcad
        ZcedSetStatusBarProgressMeterStop();
#endif
    }

#if zcad

    [DllImport(DllFileNames.ZwCadExe, EntryPoint = "?zcedSetStatusBarProgressMeter@@YAHPEB_WHH@Z",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void ZcedSetStatusBarProgressMeter(
        [MarshalAs(UnmanagedType.LPWStr)] string label, int minPos, int maxPos);

    [DllImport(DllFileNames.ZwCadExe, EntryPoint = "?zcedSetStatusBarProgressMeterPos@@YAHH@Z",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void ZcedSetStatusBarProgressMeterPos(int position);

    [DllImport(DllFileNames.ZwCadExe, EntryPoint = "?zcedSetStatusBarProgressMeterStop@@YAHXZ",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void ZcedSetStatusBarProgressMeterStop();
#endif
}