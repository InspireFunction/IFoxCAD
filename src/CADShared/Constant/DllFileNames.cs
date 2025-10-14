namespace IFoxCAD.Cad;

internal static class DllFileNames
{
    public const string TchKernalArx =
#if acad
        "tch_kernal.arx";
#elif zcad
        "tch_kernal.zrx";
#endif
    public const string ZwCadExe = "zwcad.exe";
}