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

    #region 环境变量
    /// <summary>
    /// 获取cad变量
    /// </summary>
    /// <param name="varName">变量名</param>
    /// <returns>变量值</returns>
    public static object GetVar(string varName)
    {
        return Acap.GetSystemVariable(varName);
    }
    /// <summary>
    /// 设置cad变量
    /// </summary>
    /// <param name="varName">变量名</param>
    /// <param name="value">变量值</param>
    public static void SetVar(string varName, object value)
    {
        try
        {
            Acap.SetSystemVariable(varName, value);
        }
        catch (System.Exception)
        {
            Env.Print($"{varName} 是不存在的变量！");
        }
    }

#if NET35
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedGetEnv")]
    static extern int AcedGetEnv(string envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedSetEnv")]
    static extern int AcedSetEnv(string envName, StringBuilder NewValue);
#elif !HC2020
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("accore.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedGetEnv")]
    static extern int AcedGetEnv(string envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("accore.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedSetEnv")]
    static extern int AcedSetEnv(string envName, StringBuilder NewValue);
#endif

#if HC2020
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("gced.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "gcedGetEnv")]
    static extern int AcedGetEnv(string envName, StringBuilder ReturnValue);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("gced.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "gcedSetEnv")]
    static extern int AcedSetEnv(string envName, StringBuilder NewValue);
#endif

    /// <summary>
    /// 设置环境变量
    /// </summary>
    public static string AcedGetEnv(string name)
    {
        var sbRes = new StringBuilder(1024);
        _ = AcedGetEnv(name, sbRes);
        return sbRes.ToString();
    }

    /// <summary>
    /// 设置环境变量
    /// </summary>
    /// <param name="name">lisp的名称</param>
    /// <param name="var">要设置的值</param>
    /// <returns>成功标识</returns>
    public static int AcedSetEnv(string name, string var)
    {
        return AcedSetEnv(name, new StringBuilder(var));
    }

    /// <summary>
    /// 获取系统环境变量
    /// </summary>
    /// <param name="var">变量名</param>
    /// <returns>指定的环境变量的值；或者如果找不到环境变量，则返回 null</returns>
    public static string? GetEnv(string var)
    {
        // 从当前进程或者从当前用户或本地计算机的 Windows 操作系统注册表项检索环境变量的值
        return Environment.GetEnvironmentVariable(var);
    }
    /// <summary>
    /// 设置系统环境变量
    /// </summary>
    /// <param name="var">变量名</param>
    /// <param name="value">变量值</param>
    public static void SetEnv(string var, string? value)
    {
        // 创建、修改或删除当前进程中或者为当前用户或本地计算机保留的 Windows 操作系统注册表项中存储的环境变量
        Environment.SetEnvironmentVariable(var, value);
    }
    #endregion


    /// <summary>
    /// 命令行打印，会自动调用对象的toString函数
    /// </summary>
    /// <param name="message">要打印的对象</param>
    public static void Print(object message) => Editor.WriteMessage($"{message}\n");
    /// <summary>
    /// 判断当前是否在UCS坐标下
    /// </summary>
    /// <returns>Bool</returns>
    public static bool IsUcs() => (short)GetVar("WORLDUCS") == 0;

    /// <summary>
    /// 获取当前配置文件的保存版本
    /// </summary>
    /// <returns></returns>
    public static DwgVersion GetDefaultFormatForSaveToDwgVersion()
    {
        DwgVersion version;
        var ffs = Env.AcedGetEnv("DefaultFormatForSave");
        version = ffs switch
        {
            "1" => DwgVersion.AC1009,// r12/LT12 dxf
            "8" => DwgVersion.AC1014,// r14/LT98/LT97 dwg 
            "12" => DwgVersion.AC1015,// 2000 dwg
            "13" => DwgVersion.AC1800a,// 2000 dxf
            "24" => DwgVersion.AC1800,// 2004 dwg
            "25" => (DwgVersion)26,// 2004 dxf
            "36" => (DwgVersion)27,// 2007 dwg  DwgVersion.AC1021
            "37" => (DwgVersion)28,// 2007 dxf
                                   //"38" => (DwgVersion),// dwt 样板文件...啊惊没找到这个是什么
            "48" => (DwgVersion)29,// 2010 dwg  DwgVersion.AC1024
            "49" => (DwgVersion)30,// 2010 dxf
            "60" => (DwgVersion)31,// 2013 dwg  DwgVersion.AC1027
            "61" => (DwgVersion)32,// 2013 dxf
            "64" => (DwgVersion)33,// 2018 dwg  DwgVersion.AC1032
            "65" => (DwgVersion)34,// 2018 dxf   
            _ => (DwgVersion)25,
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
            16 => true,// r12/LT12 dxf
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
}
