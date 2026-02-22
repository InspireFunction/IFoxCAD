// ReSharper disable InconsistentNaming

namespace IFoxCAD.Cad;

/// <summary>
/// cad相关的PInvoke
/// </summary>
public static class PInvokeCad
{
#if zcad
    /// <summary>
    /// EntGet
    /// </summary>
    [DllImport("zwcad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "zcdbEntGet")]
    public static extern IntPtr ZcdbEntGet(AdsName adsName);

    /// <summary>
    /// 安全的EntGet
    /// </summary>
    /// <param name="adsName">实体名称</param>
    /// <returns>实体数据指针，失败返回IntPtr.Zero</returns>
    public static IntPtr ZcdbEntGetSafe(AdsName adsName)
    {
        try
        {
            IntPtr result = ZcdbEntGet(adsName);
            return result;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[PInvokeCad.ZcdbEntGetSafe] 异常: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// EntGet
    /// </summary>
    [DllImport("zwcad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "zcdbEntMod")]
    public static extern int ZcdbEntMod(IntPtr intPtr);

    /// <summary>
    /// 安全的EntMod
    /// </summary>
    /// <param name="intPtr">实体数据指针</param>
    /// <returns>错误状态码，成功返回0</returns>
    public static int ZcdbEntModSafe(IntPtr intPtr)
    {
        if (intPtr == IntPtr.Zero)
        {
            return 1;
        }
        try
        {
            return ZcdbEntMod(intPtr);
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[PInvokeCad.ZcdbEntModSafe] 异常: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// GetZdsName
    /// </summary>
    [DllImport("ZwDatabase.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?zcdbGetZdsName@@YA?AW4ErrorStatus@Zcad@@AEAY01_JVZcDbObjectId@@@Z")]
    public static extern int ZcdbGetZdsName(out AdsName adsName, ObjectId id);

    /// <summary>
    /// EntNext
    /// </summary>
    [DllImport("zwcad.exe", EntryPoint = "zcdbEntNext",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZcdbEntNext(AdsName adsName, out ObjectId id);

    /// <summary>
    /// GetAdsName
    /// </summary>
    public static int GetAdsName(out AdsName adsName, ObjectId id)
    {
        return ZcdbGetZdsName(out adsName, id);
    }
#else
    /// <summary>
    /// Entget
    /// </summary>
    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntGet")]
    public static extern IntPtr AcdbEntGet(ref ads_name adsName);

    /// <summary>
    /// 安全的EntGet
    /// </summary>
    /// <param name="adsName">实体名称</param>
    /// <returns>实体数据指针，失败返回IntPtr.Zero</returns>
    public static IntPtr AcdbEntGetSafe(ref ads_name adsName)
    {
        try
        {
            IntPtr result = AcdbEntGet(ref adsName);
            return result;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[PInvokeCad.AcdbEntGetSafe] 异常: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// EntUpd
    /// </summary>
    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntUpd")]
    public static extern int AcdbEntUpd(ref ads_name adsName);

    /// <summary>
    /// EntMod
    /// </summary>
    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntMod")]
    public static extern int AcdbEntMod(IntPtr intPtr);

    /// <summary>
    /// 安全的EntMod
    /// </summary>
    /// <param name="intPtr">实体数据指针</param>
    /// <returns>错误状态码，成功返回0</returns>
    public static int AcdbEntModSafe(IntPtr intPtr)
    {
        if (intPtr == IntPtr.Zero)
        {
            return 1;
        }
        try
        {
            return AcdbEntMod(intPtr);
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[PInvokeCad.AcdbEntModSafe] 异常: {ex.Message}");
            return 1;
        }
    }

    [DllImport("acdb25.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName25(ref ads_name adsName, ObjectId objectId);

    [DllImport("acdb24.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName24(ref ads_name adsName, ObjectId objectId);

    [DllImport("acdb23.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName23(ref ads_name adsName, ObjectId objectId);

    [DllImport("acdb22.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName22(ref ads_name adsName, ObjectId objectId);

    [DllImport("acdb21.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName21(ref ads_name adsName, ObjectId objectId);

    [DllImport("acdb20.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName20(ref ads_name adsName, ObjectId objectId);

    [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
    private static extern int AcdbGetAdsName19(ref ads_name adsName, ObjectId objectId);
    /// <summary>
    /// GetAdsName - 带异常保护包装
    /// </summary>
    public static int GetAdsName(ref ads_name adsName, ObjectId id)
    {
        try
        {
            var major = Acaop.Version.Major;
            var res = major switch
            {
                25 => AcdbGetAdsName25(ref adsName, id),
                24 => AcdbGetAdsName24(ref adsName, id),
                23 => AcdbGetAdsName23(ref adsName, id),
                22 => AcdbGetAdsName22(ref adsName, id),
                21 => AcdbGetAdsName21(ref adsName, id),
                20 => AcdbGetAdsName20(ref adsName, id),
                19 => AcdbGetAdsName19(ref adsName, id),
                _ => 1,
            };
            return res;
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[PInvokeCad.GetAdsName] BadImageFormatException: {ex.Message}");
            return 1;
        }
        catch (DllNotFoundException ex)
        {
            DebugEx.Printl($"[PInvokeCad.GetAdsName] DllNotFoundException: {ex.Message}");
            return 1;
        }
        catch (EntryPointNotFoundException ex)
        {
            DebugEx.Printl($"[PInvokeCad.GetAdsName] EntryPointNotFoundException: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[PInvokeCad.GetAdsName] 异常: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 刷新指定图层
    /// </summary>
    /// <param name="arrayPtr">ObjectId数组指针</param>
    /// <param name="mode">模式参数</param>
    [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acedRegenLayers@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@H@Z")]
    public static extern void AcedRegenLayers(IntPtr arrayPtr, int mode);

    /// <summary>
    /// 安全的刷新指定图层
    /// </summary>
    /// <param name="arrayPtr">ObjectId数组指针</param>
    /// <param name="mode">模式参数</param>
    public static void AcedRegenLayersSafe(IntPtr arrayPtr, int mode)
    {
        if (arrayPtr == IntPtr.Zero)
        {
            return;
        }
        try
        {
            AcedRegenLayers(arrayPtr, mode);
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[PInvokeCad.AcedRegenLayersSafe] 异常: {ex.Message}");
        }
    }
#endif
}

/// <summary>
/// 用于使用Entget等函数的结构体
/// </summary>
// ReSharper disable once InconsistentNaming
public struct ads_name
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ads_name()
    {
        a = b = IntPtr.Zero;
    }

    // ReSharper disable once NotAccessedField.Local
    private IntPtr a;

    // ReSharper disable once NotAccessedField.Local
    private IntPtr b;
}