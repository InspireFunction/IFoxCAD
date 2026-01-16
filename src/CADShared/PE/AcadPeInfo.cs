using System.Diagnostics;

namespace IFoxCAD.Cad;

/// <summary>
/// 选择模式
/// </summary>
[Flags]
public enum AcadPeEnum : byte
{
    /// <summary>
    /// AcadExe
    /// </summary>
    AcadExe = 1,
    /// <summary>
    /// AccoreDll
    /// </summary>
    AccoreDll = 2,
    /// <summary>
    /// Acdb
    /// </summary>
    Acdb = 4,
    /// <summary>
    /// ExeAndCore
    /// </summary>
    ExeAndCore = AcadExe | AccoreDll,
}

/// <summary>
/// 这里的枚举对应 GetMethodException 错误值
/// </summary>
[Flags]
public enum GetMethodErrorNum : byte
{
    /// <summary>
    /// 
    /// </summary>
    Ok = 0,
    /// <summary>
    /// 
    /// </summary>
    NoModule = 1,
    /// <summary>
    /// 
    /// </summary>
    NoFuncName = 2,
}

/// <summary>
/// 自动获取本工程上面的发送命令的接口
/// </summary>
public class AcadPeInfo
{
    #region 静态单例获取exe/dll信息
    static PeInfo? _PeForAcadExe;
    /// <summary>
    /// 
    /// </summary>
    public static PeInfo? PeForAcadExe
    {
        get
        {
            if (_PeForAcadExe is null)
            {
                // 获取此acad.exe获取所有的函数名
                var processModule = Process.GetCurrentProcess().MainModule;
                if (processModule == null) return _PeForAcadExe;
                var file = processModule.FileName;
                _PeForAcadExe = new PeInfo(file);
            }
            return _PeForAcadExe;
        }
    }

    static PeInfo? _PeForAccoreDll;
    /// <summary>
    /// 
    /// </summary>
    public static PeInfo? PeForAccoreDll
    {
        get
        {
            if (_PeForAccoreDll is null)
            {
                // 获取此dll所有的函数名
                var file = Process.GetCurrentProcess().MainModule!.FileName;
                var dll = Path.GetDirectoryName(file) + "\\accore.dll";
                if (File.Exists(dll))// 08没有,高版本分离的
                    _PeForAccoreDll = new PeInfo(dll);
            }
            return _PeForAccoreDll;
        }
    }

    static PeInfo? _PeForAcdbDll;
    /// <summary>
    /// 
    /// </summary>
    public static PeInfo? PeForAcdbDll
    {
        get
        {
            if (_PeForAcdbDll is null)
            {
                // 获取此dll所有的函数名
                var file = Process.GetCurrentProcess().MainModule!.FileName;
                var dll = Path.GetDirectoryName(file) + $"\\acdb{Acaop.Version.Major}.dll";
                if (File.Exists(dll))
                    _PeForAcdbDll = new PeInfo(dll);
            }
            return _PeForAcdbDll;
        }
    }

    List<PeFunction>? _Methods; // 这个不是静态的
    /// <summary>
    /// 同名函数指针们
    /// </summary>
    public List<PeFunction>? Methods
    {
        get
        {
            if (_Methods is null)
            {
                _Methods = [];

                if ((_acadPeEnum & AcadPeEnum.AcadExe) == AcadPeEnum.AcadExe)
                    GetPeMethod(PeForAcadExe);
                if ((_acadPeEnum & AcadPeEnum.AccoreDll) == AcadPeEnum.AccoreDll)
                    GetPeMethod(PeForAccoreDll);
                if ((_acadPeEnum & AcadPeEnum.Acdb) == AcadPeEnum.Acdb)
                    GetPeMethod(PeForAcdbDll);
            }
            return _Methods;
        }
    }
    #endregion

    #region 字段/构造
    /// <summary>
    /// 用于查找PE不带修饰的函数名
    /// </summary>
    string _findFuncName;
    /// <summary>
    /// 枚举查找对象
    /// </summary>
    AcadPeEnum _acadPeEnum;

    /// <summary>
    /// 通过函数名获取指针,指定类型
    /// </summary>
    /// <param name="methodName">不带修饰的函数名</param>
    /// <param name="acadPeEnum">读取哪个cad内部文件的枚举(目前只支持两个)</param>
    public AcadPeInfo(string methodName, AcadPeEnum acadPeEnum)
    {
        _findFuncName = methodName;
        _acadPeEnum = acadPeEnum;
    }

    /// <summary>
    /// 获取CAD的函数指针
    /// </summary>
    /// <typeparam name="TDelegate">委托</typeparam>
    /// <param name="methodName">不带修饰的函数名</param>
    /// <param name="acadPeEnum">读取哪个cad内部文件的枚举(目前只支持两个)</param>
    /// <returns>委托</returns>
    public static TDelegate? GetDelegate<TDelegate>(string methodName, AcadPeEnum acadPeEnum)
        where TDelegate : class
    {
        return new AcadPeInfo(methodName, acadPeEnum)
                  .GetDelegate<TDelegate>();
    }
    #endregion

    #region 方法
    /// <summary>
    /// 储存旧值，去除修饰函数名（查找的）,带修饰函数名们
    /// </summary>
    static Dictionary<string, List<PeFunction>> _Dict = new();

    /// <summary>
    /// 返回函数指针
    /// </summary>
    /// <param name="peInfo">Pe信息:可能来自exe/dll</param>
    /// <returns>错误信息</returns>
    GetMethodErrorNum GetPeMethod(PeInfo? peInfo)
    {
        if (peInfo == null)
            return GetMethodErrorNum.NoFuncName;// cad08需要检查 AccoreDll 的时候跳过

        var identifyStr = _findFuncName + ";" + peInfo.FullName;
        if (_Dict.ContainsKey(identifyStr))// 如果已经找过,直接返回
        {
            _Methods = _Dict[identifyStr];
        }
        else
        {
            _Methods ??= [];
            try
            {
                PeFunction.Finds(peInfo, _findFuncName, _Methods);
                if (_Methods.Count != 0)// 此时从不含有
                    _Dict.Add(identifyStr, _Methods);
            }
            catch (GetPeMethodException ex)
            { return (GetMethodErrorNum)ex.ErrorNum; }
        }
        return GetMethodErrorNum.Ok;
    }

    /// <summary>
    /// 转为委托
    /// </summary>
    /// <typeparam name="TDelegate">委托对象</typeparam>
    /// <returns></returns>
    public TDelegate? GetDelegate<TDelegate>() where TDelegate : class
    {
        if (Methods is null || Methods.Count == 0)
            return null;

        TDelegate? func = null;

        /*
         * 0x01
         * 这里永远不报错,但是不代表不会出错.
         * 调用C盘exe/dll时需要权限,
         * 所以会出现:[DLLImport]可以,直接运行cad也可以,但是调试不行.
         * 此时可以提权vs再调试,有时候会出现:调试不显示东西,但是运行是对的.
         *
         * 0x02
         * 出错时候用完整的修饰名
         *
         * 0x03
         * 这里可能同时存在acad.exe和accore.dll相同指针?
         * 所以我是用排序方法找最短的指针,所以它是第First个.
         */

        // 排序,最少长度原则本身就是让完全相同字符串在最前面
        // 这里替换为有序哈希,因为我总是需要不带修饰的返回函数,所以是排序长度的第一个
        _Methods = _Methods?.OrderBy(str => str.CName?.Length)
            .ThenBy(str => str.MethodName.Length)
            .ToList();

        func = Marshal.GetDelegateForFunctionPointer(Methods.First().GetProcAddress(), typeof(TDelegate)) as TDelegate;
        return func;
    }
    #endregion
}

/// <summary>
/// 通过名字查找exe/dll内所有名字
/// </summary>
public class PeFunction
{
    #region 字段/构造
    string? _CName;
    /// <summary>
    /// 纯c语言名
    /// </summary>
    public string? CName
    {
        get
        {
            if (_CName is null && MethodName is not null)
            {
                _CName = MethodName.Replace("?", string.Empty); // 剔除cpp前缀
                var num = _CName.IndexOf("@");
                if (num > -1)
                    _CName = _CName.Substring(0, num); // 剔除参数部分
            }
            return _CName;
        }
    }

    /// <summary>
    /// 模块文件路径
    /// </summary>
    public string? ModuleFullName;
    /// <summary>
    /// 模块指针
    /// </summary>
    public IntPtr ModuleIntPtr;
    /// <summary>
    /// 函数名
    /// </summary>
    public string MethodName;
    /// <summary>
    /// 通过名字查找exe/dll内所有名字
    /// </summary>
    /// <param name="methodName">没修饰的方法名</param>
    public PeFunction(string methodName)
    {
        MethodName = methodName;
    }
    #endregion

    /// <summary>
    /// 获取函数指针
    /// </summary>
    public IntPtr GetProcAddress()
    {
        return WindowsAPI.GetProcAddress(ModuleIntPtr, MethodName);
    }

    /// <summary>
    /// 通过名字查找exe/dll内所有名字
    /// </summary>
    /// <param name="peInfo">pe结构</param>
    /// <param name="findFuncName">用于查找的方法名</param>
    /// <param name="funcAdress_Out">返回函数集合</param>
    public static void Finds(PeInfo peInfo,
        string findFuncName,
        List<PeFunction> funcAdress_Out)
    {
        if (findFuncName == null)
            throw new GetPeMethodException(2, "没有找到对应的函数:" + findFuncName);

        var peModuleFullName = peInfo.FullName;
        if (peModuleFullName == null)
            throw new GetPeMethodException(1, "找不到模块:" + peModuleFullName + "当前程序没有加载这个东西?");
        var hModule = WindowsAPI.GetModuleHandle(peModuleFullName); // 执行前必须加载了先,acad.exe/accore.dll
        if (hModule == IntPtr.Zero)
            throw new GetPeMethodException(1, "找不到模块:" + peModuleFullName + "当前程序没有加载这个东西?");

        // 遍历函数接口名单
        var names = peInfo.ExportDirectory?.FunctionNames();
        if (names == null)
            throw new ArgumentException(nameof(names));

        foreach (var name in names)
        {
            if (name.Contains(findFuncName))// 这里是名称含有,不是容器含有
            {
                var fn = new PeFunction(name)
                {
                    ModuleFullName = peModuleFullName,
                    ModuleIntPtr = hModule
                };
                funcAdress_Out.Add(fn);
            }
        }
    }
}



/// <summary>
/// 错误信息
/// </summary>
public class GetPeMethodException : ApplicationException
{
    /// <summary>
    /// 
    /// </summary>
    public int ErrorNum;
    /// <summary>
    /// 
    /// </summary>
    public string? ErrorMsg;
    /// <summary>
    /// 
    /// </summary>
    public Exception? InnerException1;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    public GetPeMethodException(string msg) : base(msg)
    {
        ErrorMsg = msg;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="errorNum"></param>
    /// <param name="msg"></param>
    public GetPeMethodException(int errorNum, string msg) : base(msg)
    {
        ErrorNum = errorNum;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="innerException"></param>
    public GetPeMethodException(string msg, Exception innerException) : base(msg, innerException)
    {
        InnerException1 = innerException;
        ErrorMsg = msg;
    }
}