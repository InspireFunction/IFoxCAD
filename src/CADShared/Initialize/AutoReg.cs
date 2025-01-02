namespace IFoxCAD.Cad;

/// <summary>
/// 自动加载辅助类
/// </summary>
public static class AutoReg
{
    /// <summary>
    /// 获取自动加载注册表位置节点
    /// </summary>
    /// <returns>注册表节点</returns>
    public static RegistryKey? GetAcAppKey()
    {
        var key = HostApplicationServices.Current.UserRegistryProductRootKey;
        var acKey = Registry.CurrentUser.OpenSubKey(key, true);
        return acKey?.CreateSubKey("Applications");
    }

    /// <summary>
    /// 是否已经自动加载
    /// </summary>
    /// <param name="info">程序集信息</param>
    /// <returns>已经设置返回true，反之返回false</returns>
    public static bool SearchForReg(AssemInfo info)
    {
        if (GetAcAppKey() is not { } appKey || appKey.SubKeyCount == 0)
            return false;

        var regApps = appKey.GetSubKeyNames();
        if (!regApps.Contains(info.Name))
            return false;
        // 20220409 文件名相同,路径不同,需要判断路径
        var subKey = appKey.OpenSubKey(info.Name);
        return string.Equals(subKey?.GetValue("LOADER")?.ToString(), info.Loader, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// 在注册表写入自动加载的程序集信息
    /// </summary>
    /// <param name="info">程序集信息</param>
    public static void RegApp(AssemInfo info)
    {
        using var appKey = GetAcAppKey();
        var rk = appKey?.CreateSubKey(info.Name);
        rk?.SetValue("DESCRIPTION", info.Fullname, RegistryValueKind.String);
        rk?.SetValue("LOADCTRLS", info.LoadType, RegistryValueKind.DWord);
        rk?.SetValue("LOADER", info.Loader, RegistryValueKind.String);
        rk?.SetValue("MANAGED", 1, RegistryValueKind.DWord);
        appKey?.Close();
    }

    /// <summary>
    /// 在注册表写入自动加载的程序集信息
    /// </summary>
    /// <param name="assembly">程序集</param>
    public static void RegApp(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        var info = new AssemInfo(assembly);
        RegApp(info);
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    public static bool UnRegApp(AssemInfo info)
    {
        using var appKey = GetAcAppKey();
        if (appKey is { SubKeyCount: 0 })
            return false;

        var regApps = appKey?.GetSubKeyNames();
        if (regApps != null && !regApps.Contains(info.Name)) return false;
        appKey?.DeleteSubKey(info.Name, false);
        return true;
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    /// <param name="assembly">程序集</param>
    public static void UnRegApp(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        var info = new AssemInfo(assembly);
        UnRegApp(info);
    }
}