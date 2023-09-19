namespace IFoxCAD.Cad;

/// <summary>
/// 自动加载辅助类
/// </summary>
public class AutoReg
{
    /// <summary>
    /// 获取自动加载注册表位置节点
    /// </summary>
    /// <returns>注册表节点</returns>
    public static RegistryKey GetAcAppKey()
    {
        string key = HostApplicationServices.Current.UserRegistryProductRootKey;
        RegistryKey ackey = Registry.CurrentUser.OpenSubKey(key, true);
        return ackey.CreateSubKey("Applications");
    }
    /// <summary>
    /// 是否已经自动加载
    /// </summary>
    /// <param name="info">程序集信息</param>
    /// <returns>已经设置返回true，反之返回false</returns>
    public static bool SearchForReg(AssemInfo info)
    {
        // 在使用netloadx的时候,此处注册表是失效的,具体原因要进行netloadx测试
        var appkey = GetAcAppKey();
        if (appkey.SubKeyCount == 0)
            return false;

        var regApps = appkey.GetSubKeyNames();
        if (regApps.Contains(info.Name))
        {
            // 20220409 bug:文件名相同,路径不同,需要判断路径
            var subkey = appkey.OpenSubKey(info.Name);
            return subkey.GetValue("LOADER")?.ToString().ToLower() == info.Loader.ToLower();
        }
        return false;
    }

    /// <summary>
    /// 在注册表写入自动加载的程序集信息
    /// </summary>
    /// <param name="info">程序集信息</param>
    public static void RegApp(AssemInfo info)
    {
        RegistryKey appkey = GetAcAppKey();
        RegistryKey rk = appkey.CreateSubKey(info.Name);
        rk.SetValue("DESCRIPTION", info.Fullname, RegistryValueKind.String);
        rk.SetValue("LOADCTRLS", info.LoadType, RegistryValueKind.DWord);
        rk.SetValue("LOADER", info.Loader, RegistryValueKind.String);
        rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);
        appkey.Close();
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    public static bool UnRegApp(AssemInfo info)
    {
        var appkey = GetAcAppKey();
        if (appkey.SubKeyCount == 0)
            return false;

        var regApps = appkey.GetSubKeyNames();
        if (regApps.Contains(info.Name))
        {
            appkey.DeleteSubKey(info.Name, false);
            return true;
        }
        return false;
    }
}
