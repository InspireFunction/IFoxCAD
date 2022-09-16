namespace IFoxCAD.Cad;

/// <summary>
/// 系统管理类
/// <para>
/// 封装了一些系统 osmode;cmdecho;dimblk 系统变量<br/>
/// 封装了常用的 文档 编辑器 数据库等对象为静态变量<br/>
/// 封装了配置页面的注册表信息获取函数
/// </para>
/// </summary>
public static class Env
{
    #region Goal

    /// <summary>
    /// 当前的数据库
    /// </summary>
    public static Database Database => HostApplicationServices.WorkingDatabase;

    /// <summary>
    /// 当前文档
    /// </summary>
    public static Document Document => Acap.DocumentManager.MdiActiveDocument;

    /// <summary>
    /// 编辑器对象
    /// </summary>
    public static Editor Editor => Document.Editor;

    /// <summary>
    /// 图形管理器
    /// </summary>
    public static Manager GsManager => Document.GraphicsManager;

    #endregion Goal

    #region Preferences

#if !zcad // 中望官方的问题
    /// <summary>
    /// 获取当前配置的数据
    /// </summary>
    /// <param name="subSectionName">小节名</param>
    /// <param name="propertyName">数据名</param>
    /// <returns>对象</returns>
    public static object GetCurrentProfileProperty(string subSectionName, string propertyName)
    {
        UserConfigurationManager ucm = Acap.UserConfigurationManager;
        IConfigurationSection cpf = ucm.OpenCurrentProfile();
        IConfigurationSection ss = cpf.OpenSubsection(subSectionName);
        return ss.ReadProperty(propertyName, "");
    }


    /// <summary>
    /// 获取对话框配置的数据
    /// </summary>
    /// <param name="dialog">对话框对象</param>
    /// <returns>配置项</returns>
    public static IConfigurationSection GetDialogSection(object dialog)
    {
        UserConfigurationManager ucm = Acap.UserConfigurationManager;
        IConfigurationSection ds = ucm.OpenDialogSection(dialog);
        return ds;
    }

    /// <summary>
    /// 获取公共配置的数据
    /// </summary>
    /// <param name="propertyName">数据名</param>
    /// <returns>配置项</returns>
    public static IConfigurationSection GetGlobalSection(string propertyName)
    {
        UserConfigurationManager ucm = Acap.UserConfigurationManager;
        IConfigurationSection gs = ucm.OpenGlobalSection();
        IConfigurationSection ss = gs.OpenSubsection(propertyName);
        return ss;
    }
#endif
    #endregion Preferences

    #region Enum
    /// <summary>
    /// 控制在AutoLISP的command函数运行时AutoCAD是否回显提示和输入， <see langword="true"/> 为显示， <see langword="false"/> 为不显示
    /// </summary>
    public static bool CmdEcho
    {
        get => Convert.ToInt16(Acap.GetSystemVariable("cmdecho")) == 1;
        set => Acap.SetSystemVariable("cmdecho", Convert.ToInt16(value));
    }

    /// <summary>
    /// 控制在光标是否为正交模式， <see langword="true"/> 为打开正交， <see langword="false"/> 为关闭正交
    /// </summary>
    public static bool OrthoMode
    {
        get => Convert.ToInt16(Acap.GetSystemVariable("ORTHOMODE")) == 1;
        set => Acap.SetSystemVariable("ORTHOMODE", Convert.ToInt16(value));
    }

    #region Dimblk

    /// <summary>
    /// 标注箭头类型
    /// </summary>
    public enum DimblkType
    {
        /// <summary>
        /// 实心闭合
        /// </summary>
        Defult,

        /// <summary>
        /// 点
        /// </summary>
        Dot,

        /// <summary>
        /// 小点
        /// </summary>
        DotSmall,

        /// <summary>
        /// 空心点
        /// </summary>
        DotBlank,

        /// <summary>
        /// 原点标记
        /// </summary>
        Origin,

        /// <summary>
        /// 原点标记2
        /// </summary>
        Origin2,

        /// <summary>
        /// 打开
        /// </summary>
        Open,

        /// <summary>
        /// 直角
        /// </summary>
        Open90,

        /// <summary>
        /// 30度角
        /// </summary>
        Open30,

        /// <summary>
        /// 闭合
        /// </summary>
        Closed,

        /// <summary>
        /// 空心小点
        /// </summary>
        Small,

        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 倾斜
        /// </summary>
        Oblique,

        /// <summary>
        /// 实心框
        /// </summary>
        BoxFilled,

        /// <summary>
        /// 方框
        /// </summary>
        BoxBlank,

        /// <summary>
        /// 空心闭合
        /// </summary>
        ClosedBlank,

        /// <summary>
        /// 实心基准三角形
        /// </summary>
        DatumFilled,

        /// <summary>
        /// 基准三角形
        /// </summary>
        DatumBlank,

        /// <summary>
        /// 完整标记
        /// </summary>
        Integral,

        /// <summary>
        /// 建筑标记
        /// </summary>
        ArchTick
    }

    private static readonly Dictionary<string, DimblkType> dimdescdict = new()
    {
        { "实心闭合", DimblkType.Defult },
        { "点", DimblkType.Dot },
        { "小点", DimblkType.DotSmall },
        { "空心点", DimblkType.DotBlank },
        { "原点标记", DimblkType.Origin },
        { "原点标记 2", DimblkType.Origin2 },
        { "打开", DimblkType.Open },
        { "直角", DimblkType.Open90 },
        { "30 度角", DimblkType.Open30 },
        { "闭合", DimblkType.Closed },
        { "空心小点", DimblkType.Small },
        { "无", DimblkType.None },
        { "倾斜", DimblkType.Oblique },
        { "实心框", DimblkType.BoxFilled },
        { "方框", DimblkType.BoxBlank },
        { "空心闭合", DimblkType.ClosedBlank },
        { "实心基准三角形", DimblkType.DatumFilled },
        { "基准三角形", DimblkType.DatumBlank },
        { "完整标记", DimblkType.Integral },
        { "建筑标记", DimblkType.ArchTick },

        { "", DimblkType.Defult },
        { "_DOT", DimblkType.Dot },
        { "_DOTSMALL", DimblkType.DotSmall },
        { "_DOTBLANK", DimblkType.DotBlank },
        { "_ORIGIN", DimblkType.Origin },
        { "_ORIGIN2", DimblkType.Origin2 },
        { "_OPEN", DimblkType.Open },
        { "_OPEN90", DimblkType.Open90 },
        { "_OPEN30", DimblkType.Open30 },
        { "_CLOSED", DimblkType.Closed },
        { "_SMALL", DimblkType.Small },
        { "_NONE", DimblkType.None },
        { "_OBLIQUE", DimblkType.Oblique },
        { "_BOXFILLED", DimblkType.BoxFilled },
        { "_BOXBLANK", DimblkType.BoxBlank },
        { "_CLOSEDBLANK", DimblkType.ClosedBlank },
        { "_DATUMFILLED", DimblkType.DatumFilled },
        { "_DATUMBLANK", DimblkType.DatumBlank },
        { "_INTEGRAL", DimblkType.Integral },
        { "_ARCHTICK", DimblkType.ArchTick },
    };



    /// <summary>
    /// 标注箭头属性
    /// </summary>
    public static DimblkType Dimblk
    {
        get
        {
            string s = ((string)Acap.GetSystemVariable("dimblk")).ToUpper();
            // if (string.IsNullOrEmpty(s))
            // {
            //    return DimblkType.Defult;
            // }
            // else
            // {
            //    if (dimdescdict.TryGetValue(s, out DimblkType value))
            //    {
            //        return value;
            //    }
            //    return s.ToEnum<DimblkType>();
            //    // return s.FromDescName<DimblkType>();
            // }
            return dimdescdict[s];
        }
        set
        {
            string s = GetDimblkName(value);
            Acap.SetSystemVariable("dimblk", s);
        }
    }

    /// <summary>
    /// 获取标注箭头名
    /// </summary>
    /// <param name="dimblk">标注箭头类型</param>
    /// <returns>箭头名</returns>
    public static string GetDimblkName(DimblkType dimblk)
    {
        return
            dimblk == DimblkType.Defult
            ?
            "."
            :
            "_" + dimblk.GetName();
    }

    /// <summary>
    /// 获取标注箭头ID
    /// </summary>
    /// <param name="dimblk">标注箭头类型</param>
    /// <returns>箭头ID</returns>
    public static ObjectId GetDimblkId(DimblkType dimblk)
    {
        DimblkType oldDimblk = Dimblk;
        Dimblk = dimblk;
        ObjectId id = HostApplicationServices.WorkingDatabase.Dimblk;
        Dimblk = oldDimblk;
        return id;
    }

    #endregion Dimblk

    #region OsMode

    /// <summary>
    /// 捕捉模式系统变量类型
    /// </summary>
    public enum OSModeType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 端点
        /// </summary>
        End = 1,

        /// <summary>
        /// 中点
        /// </summary>
        Middle = 2,

        /// <summary>
        /// 圆心
        /// </summary>
        Center = 4,

        /// <summary>
        /// 节点
        /// </summary>
        Node = 8,

        /// <summary>
        /// 象限点
        /// </summary>
        Quadrant = 16,

        /// <summary>
        /// 交点
        /// </summary>
        Intersection = 32,

        /// <summary>
        /// 插入点
        /// </summary>
        Insert = 64,

        /// <summary>
        /// 垂足
        /// </summary>
        Pedal = 128,

        /// <summary>
        /// 切点
        /// </summary>
        Tangent = 256,

        /// <summary>
        /// 最近点
        /// </summary>
        Nearest = 512,

        /// <summary>
        /// 几何中心
        /// </summary>
        Quick = 1024,

        /// <summary>
        /// 外观交点
        /// </summary>
        Appearance = 2048,

        /// <summary>
        /// 延伸
        /// </summary>
        Extension = 4096,

        /// <summary>
        /// 平行
        /// </summary>
        Parallel = 8192
    }

    /// <summary>
    /// 捕捉模式系统变量
    /// </summary>
    public static OSModeType OSMode
    {
        get
        {
            return (OSModeType)Convert.ToInt16(Acap.GetSystemVariable("osmode"));
        }
        set
        {
            Acap.SetSystemVariable("osmode", (int)value);
        }
    }
    /// <summary>
    /// 捕捉模式osm1是否包含osm2
    /// </summary>
    /// <param name="osm1">原模式</param>
    /// <param name="osm2">要比较的模式</param>
    /// <returns>包含时返回 <see langword="true"/>，不包含时返回 <see langword="false"/></returns>
    public static bool Include(this OSModeType osm1, OSModeType osm2)
    {
        return (osm1 & osm2) == osm2;
    }
    #endregion OsMode


    private static string GetName<T>(this T value)
    {
        return Enum.GetName(typeof(T), value);
    }

    #endregion Enum

    #region 系统变量
    /// <summary>
    /// 获取cad系统变量
    /// </summary>
    /// <param name="varName">变量名</param>
    /// <returns>变量值</returns>
    public static object GetVar(string? varName)
    {
        return Acap.GetSystemVariable(varName);
    }
    /// <summary>
    /// 设置cad系统变量<br/>
    /// 0x01 建议先获取现有变量值和设置的是否相同,否则直接设置会发生异常<br/>
    /// 0x02 建议锁文档,否则 Psltscale 设置发生异常<br/>
    /// 发生异常的时候vs输出窗口会打印一下,但是如果不介意也没啥问题
    /// </summary>
    /// <param name="varName">变量名</param>
    /// <param name="value">变量值</param>
    /// <param name="echo">输出异常,默认true;此设置仅为打印到命令栏,无法控制vs输出</param>
    public static void SetVar(string? varName, object? value, bool echo = true)
    {
        try
        {
            Acap.SetSystemVariable(varName, value);
        }
        catch (System.Exception)
        {
            if (echo)
                Env.Print($"{varName} 是不存在的变量！");
        }
    }
    #endregion

    #region 环境变量
#if acad
#if NET35
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedGetEnv")]
    static extern int AcedGetEnv(string? envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedSetEnv")]
    static extern int AcedSetEnv(string? envName, StringBuilder NewValue);
#else
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("accore.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedGetEnv")]
    static extern int AcedGetEnv(string? envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("accore.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedSetEnv")]
    static extern int AcedSetEnv(string? envName, StringBuilder NewValue);
#endif
#endif

#if gcad
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("gced.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "gcedGetEnv")]
    static extern int AcedGetEnv(string? envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("gced.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "gcedSetEnv")]
    static extern int AcedSetEnv(string? envName, StringBuilder NewValue);
#endif

#if zcad // TODO: 中望没有测试,此处仅为不报错;本工程所有含有"中望"均存在问题
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("zced.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zcedGetEnv")]
    static extern int AcedGetEnv(string? envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("zced.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zcedSetEnv")]
    static extern int AcedSetEnv(string? envName, StringBuilder NewValue);
#endif

    /// <summary>
    /// 读取acad环境变量<br/>
    /// 也能获取win环境变量
    /// </summary>
    /// <param name="name">变量名</param>
    /// <returns>返回值从不为null,需判断<see cref="string.Empty"/></returns>
    public static string GetEnv(string? name)
    {
        // 它将混合查询以下路径:
        // acad2008注册表路径: 计算机\HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD\R17.1\ACAD - 6001:804\FixedProfile\General
        // 用户: 计算机\HKEY_CURRENT_USER\Environment
        // 系统: 计算机\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment

        // GetEnv("Path")长度很长:
        // 可用内存 (最新格式) 1 MB (标准格式)
        // https://docs.microsoft.com/zh-cn/windows/win32/sysinfo/registry-element-size-limits

        var sbRes = new StringBuilder(1 << 23);
        _ = AcedGetEnv(name, sbRes);
        return sbRes.ToString();
    }

    /// <summary>
    /// 设置acad环境变量<br/>
    /// 它是不会报错的,但是直接设置会写入注册表的,<br/>
    /// 如果是设置高低版本cad不同的变量,建议先读取判断再设置<br/>
    /// </summary>
    /// <param name="name">变量名</param>
    /// <param name="var">变量值</param>
    /// <returns></returns>
    public static int SetEnv(string? name, string? var)
    {
        return AcedSetEnv(name, new StringBuilder(var));
    }
    #endregion

    #region win环境变量/由于 Aced的 能够同时获取此变量与cad内的,所以废弃
    // /// <summary>
    // /// 获取系统环境变量
    // /// </summary>
    // /// <param name="var">变量名</param>
    // /// <returns>指定的环境变量的值；或者如果找不到环境变量，则返回 null</returns>
    // public static string? GetEnv(string? var)
    // {
    //     // 从当前进程或者从当前用户或本地计算机的 Windows 操作系统注册表项检索环境变量的值
    //     // 用户: 计算机\HKEY_CURRENT_USER\Environment
    //     // 系统: 计算机\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment
    //     return Environment.GetEnvironmentVariable(var);
    // }
    // /// <summary>
    // /// 设置系统环境变量
    // /// </summary>
    // /// <param name="var">变量名</param>
    // /// <param name="value">变量值</param>
    // public static void SetEnv(string? var, string? value)
    // {
    //     // 创建、修改或删除当前进程中或者为当前用户或本地计算机保留的 Windows 操作系统注册表项中存储的环境变量
    //     Environment.SetEnvironmentVariable(var, value);
    // }
    #endregion


    /// <summary>
    /// 命令行打印，会自动调用对象的toString函数
    /// </summary>
    /// <param name="message">要打印的对象</param>
    public static void Print(object message) => Editor.WriteMessage($"{message}\n");
    public static void Printl(object message) => Editor.WriteMessage($"{Environment.NewLine}{message}\n");

    /// <summary>
    /// 判断当前是否在UCS坐标下
    /// </summary>
    /// <returns>Bool</returns>
    public static bool IsUcs() => (short)GetVar("WORLDUCS") == 0;


    #region dwg版本号/cad版本号/年份
    /// <summary>
    /// 获取当前配置文件的保存版本
    /// </summary>
    /// <returns></returns>
    public static DwgVersion GetDefaultDwgVersion()
    {
        DwgVersion version;
        var ffs = Env.GetEnv("DefaultFormatForSave");
        version = ffs switch
        {
            "1" => DwgVersion.AC1009,// R12/LT12 dxf
            "8" => DwgVersion.AC1014,// R14/LT98/LT97 dwg
            "12" => DwgVersion.AC1015,// 2000 dwg
            "13" => DwgVersion.AC1800a,// 2000 dxf
            "24" => DwgVersion.AC1800,// 2004 dwg
            "25" => (DwgVersion)26,// 2004 dxf
            "36" => (DwgVersion)27,// 2007 dwg  DwgVersion.AC1021
            "37" => (DwgVersion)28,// 2007 dxf

            // "38" => (DwgVersion),// dwt 样板文件...啊惊没找到这个是什么
            "48" => (DwgVersion)29,// 2010 dwg  DwgVersion.AC1024
            "49" => (DwgVersion)30,// 2010 dxf
            "60" => (DwgVersion)31,// 2013 dwg  DwgVersion.AC1027
            "61" => (DwgVersion)32,// 2013 dxf
            "64" => (DwgVersion)33,// 2018 dwg  DwgVersion.AC1032
            "65" => (DwgVersion)34,// 2018 dxf
            _ => throw new NotImplementedException(),// 提醒维护
        };
        return version;
    }

    /// <summary>
    /// 是否为dxf版本号
    /// </summary>
    /// <param name="dwgVersion"></param>
    /// <returns></returns>
    public static bool IsDxfVersion(this DwgVersion dwgVersion)
    {
        var result = (int)dwgVersion switch
        {
            16 => true,// R12/LT12 dxf
            24 => true,// 2000 dxf
            26 => true,// 2004 dxf
            28 => true,// 2007 dxf
            30 => true,// 2010 dxf
            32 => true,// 2013 dxf
            34 => true,// 2018 dxf
            _ => false,
        };
        return result;
    }

    /// <summary>
    /// 获取cad年份
    /// </summary>
    /// <exception cref="NotImplementedException">超出年份就报错</exception>
    public static int GetAcadVersion()
    {
        var ver = Acap.Version.Major + "." + Acap.Version.Minor;
        int acarVarNum = ver switch
        {
            "16.2" => 2006,
            "17.0" => 2007,
            "17.1" => 2008,
            "17.2" => 2009,
            "18.0" => 2010,
            "18.1" => 2011,
            "18.2" => 2012,
            "19.0" => 2013,
            "19.1" => 2014,
            "20.0" => 2015,
            "20.1" => 2016,
            "21.0" => 2017,
            "22.0" => 2018,
            "23.0" => 2019,
            "23.1" => 2020,
            "24.0" => 2021,
            "24.1" => 2022,
            _ => throw new NotImplementedException(),
        };
        return acarVarNum;
    }

    public static string GetAcapVersionDll(string str = "acdb")
    {
        return str + Acap.Version.Major + ".dll";
    }
    #endregion


    #region cad变量功能延伸
    /// <summary>
    /// 设置cad系统变量<br/>
    /// 提供一个反序列化后,无cad异常输出的功能<br/>
    /// 注意,您需要再此执行时候设置文档锁<br/>
    /// <see cref="Acap.DocumentManager.MdiActiveDocument.LockDocument()"/><br/>
    /// 否则也将导致修改数据库异常<br/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>成功返回当前值,失败null</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static object? SetVarEx(string? key, string? value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var currentVar = Env.GetVar(key);
        if (currentVar == null)
            return null;

        object? valueType = currentVar.GetType().Name switch
        {
            "String" => value.Replace("\"", string.Empty),
            "Double" => double.Parse(value),
            "Int16" => short.Parse(value),
            "Int32" => int.Parse(value),
            _ => throw new NotImplementedException(),
        };

        // 相同的参数进行设置会发生一次异常
        if (currentVar.ToString().ToUpper() != valueType!.ToString().ToUpper())
            Env.SetVar(key, valueType);

        return currentVar;
    }

    /// <summary>
    /// 设置新系统变量,返回现有系统变量
    /// </summary>
    /// <param name="args">设置的变量词典</param>
    /// <returns>返回现有变量词典,然后下次就可以利用它进行设置回来了</returns>
    public static Dictionary<string, string> SaveCadVar(Dictionary<string, string> args)
    {
        if (args is null)
            throw new ArgumentNullException(nameof(args));

        var dict = new Dictionary<string, string>();
        foreach (var item in args)
        {
            // 判断是否为系统变量
            var ok = SetVarEx(item.Key, item.Value);
            if (ok != null)
            {
                dict.Add(item.Key, ok.ToString());
                continue;
            }

            // 判断是否为系统变量
            var envstr = Env.GetEnv(item.Key);
            if (!string.IsNullOrEmpty(envstr))
            {
                Env.SetEnv(item.Key, item.Value);
                dict.Add(item.Key, envstr);
            }
        }
        return dict;
    }
    #endregion
}