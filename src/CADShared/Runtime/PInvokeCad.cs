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
    /// EntGet
    /// </summary>
    [DllImport("zwcad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "zcdbEntMod")]
    public static extern int ZcdbEntMod(IntPtr intPtr);

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
    /// GetAdsName
    /// </summary>
    public static int GetAdsName(ref ads_name adsName, ObjectId id)
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

    /// <summary>
    /// 刷新指定图层
    /// </summary>
    /// <param name="arrayPtr">ObjectId数组指针</param>
    /// <param name="mode">模式参数</param>
    [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acedRegenLayers@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@H@Z")]
    public static extern void AcedRegenLayers(IntPtr arrayPtr, int mode);
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