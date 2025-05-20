// ReSharper disable InconsistentNaming

namespace IFoxCAD.Cad;

/// <summary>
/// cad相关的PInvoke
/// </summary>
public static class PInvokeCad
{
    /// <summary>
    /// Entget
    /// </summary>
    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acdbEntGet")]
    public static extern IntPtr AcdbEntGet(ref ads_name adsName);

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