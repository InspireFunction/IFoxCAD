namespace IFoxCAD.Cad;
using System.Diagnostics;

/// <summary>
/// 加载时优先级
/// </summary>
[Flags]
public enum Sequence : byte
{
    First,// 最先
    Last, // 最后
}

/// <summary>
/// 加载时自动执行接口
/// </summary>
public interface IFoxAutoGo
{
    // 控制加载顺序
    Sequence SequenceId();
    // 关闭cad的时候会自动执行
    void Terminate();
    // 打开cad的时候会自动执行
    void Initialize();
}

/// <summary>
/// 加载时自动执行特性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class IFoxInitialize : Attribute
{
    /// <summary>
    /// 优先级
    /// </summary>
    internal Sequence SequenceId;
    /// <summary>
    /// <see langword="true"/>用于初始化;<see langword="false"/>用于结束回收
    /// </summary>
    internal bool IsInitialize;
    /// <summary>
    /// 用于初始化和结束回收
    /// </summary>
    /// <param name="sequence">优先级</param>
    /// <param name="isInitialize"><see langword="true"/>用于初始化;<see langword="false"/>用于结束回收</param>
    public IFoxInitialize(Sequence sequence = Sequence.Last, bool isInitialize = true)
    {
        SequenceId = sequence;
        IsInitialize = isInitialize;
    }
}

//为了解决IExtensionApplication在一个dll内无法多次实现接口的关系
//所以在这里反射加载所有的 IAutoGo ,以达到能分开写"启动运行"函数的目的
class RunClass
{
    public Sequence Sequence { get; }

    readonly MethodInfo _methodInfo;

    public RunClass(MethodInfo method, Sequence sequence)
    {
        _methodInfo = method;
        Sequence = sequence;
    }

    /// <summary>
    /// 运行方法
    /// </summary>
    public void Run()
    {
        try
        {
            _methodInfo.Invoke();
        }
        catch (System.Exception)
        {
            Debugger.Break();
        }
    }
}

/// <summary>
/// 此类初始化要在调用类库上面进行一次,否则反射的项目不包含调用类
/// 也就是谁引用了<see langword="IFoxCAD.Cad"/> 谁负责在 <see langword="IExtensionApplication"/> 接口上实例化 <see langword="AutoClass"/>
/// </summary>
public class AutoClass //: IExtensionApplication
{
    static List<RunClass> _InitializeList = new(); //储存方法用于初始化
    static List<RunClass> _TerminateList = new();  //储存方法用于结束释放

    readonly string _DllName;
    /// <summary>
    /// 反射此特性:<see langword="IFoxInitialize"/>进行加载时自动运行
    /// </summary>
    /// <param name="DllName">约束在此dll进行加速</param>
    public AutoClass(string DllName)
    {
        _DllName = DllName;
    }

    //启动cad的时候会自动执行
    public void Initialize()
    {
        try
        {
            //收集特性,包括启动时和关闭时
            GetAttributeFunctions(_InitializeList,_TerminateList);

            GetInterfaceFunctions(_InitializeList, nameof(Initialize));
            //按照 SequenceId 排序_升序
            _InitializeList = _InitializeList.OrderBy(runClass => runClass.Sequence).ToList();
            AutoClass.RunFunctions(_InitializeList);
        }
        catch (System.Exception)
        {
            Debugger.Break();
        }
    }

    //关闭cad的时候会自动执行
    public void Terminate()
    {
        try
        {
            GetInterfaceFunctions(_TerminateList, nameof(Terminate));
            //按照 SequenceId 排序_降序
            _TerminateList = _TerminateList.OrderByDescending(runClass => runClass.Sequence).ToList();
            AutoClass.RunFunctions(_TerminateList);
        }
        catch (System.Exception)
        {
            Debugger.Break();
        }
    }

    /// <summary>
    /// 遍历程序域下所有类型
    /// </summary>
    /// <param name="action">输出每个成员执行</param>
    /// <param name="dllNameWithoutExtension">过滤此dll,不含后缀</param>
    public static void AppDomainGetTypes(Action<Type> action, string? dllNameWithoutExtension = null)
    {
#if DEBUG
        int error = 0;
#endif
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
#if !NET35
            //cad2021出现如下报错
            //System.NotSupportedException:动态程序集中不支持已调用的成员
            assemblies = assemblies.Where(p => !p.IsDynamic).ToArray();
#endif
            //主程序域
            for (int ii = 0; ii < assemblies.Length; ii++)
            {
                var assembly = assemblies[ii];

                //获取类型集合,反射时候还依赖其他的dll就会这个错误
                //此通讯库要跳过,否则会报错.
                var str = Path.GetFileNameWithoutExtension(assembly.Location);
                if (dllNameWithoutExtension != null &&
                    str != dllNameWithoutExtension)
                    continue;
                if (str == "AcInfoCenterConn")//通讯库
                    continue;

                Type[]? types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException) { continue; }

                if (types is null)
                    continue;
                for (int jj = 0; jj < types.Length; jj++)
                {
                    var type = types[jj];
                    if (type is not null)
                    {
#if DEBUG
                        ++error;
#endif
                        action(type);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
#if DEBUG
            Debug.WriteLine($"出错:{nameof(AppDomainGetTypes)};计数{error};错误信息:{e.Message}");
            Debugger.Break();
#endif
        }
    }

    /// <summary>
    /// 收集接口下的函数
    /// </summary>
    /// <param name="runClassList">储存要运行的方法</param>
    /// <param name="methodName">查找方法名</param>
    /// <returns></returns>
    void GetInterfaceFunctions(List<RunClass> runClassList, string methodName)
    {
        const string sqid = nameof(Sequence) + "Id";

        AppDomainGetTypes(type => {
            if (type.IsAbstract)
                return;

            foreach (var inters in type.GetInterfaces())
            {
                if (inters.Name != nameof(IFoxAutoGo))
                    continue;

                Sequence? sequence = null;
                MethodInfo? initialize = null;

                foreach (var method in type.GetMethods())
                {
                    if (method.IsAbstract)
                        continue;
                    if (method.Name == sqid)
                    {
                        var obj = method.Invoke();
                        if (obj is not null)
                            sequence = (Sequence)obj;
                        continue;
                    }
                    else if (method.Name == methodName)
                        initialize = method;
                    if (initialize is not null && sequence is not null)
                        break;
                }

                if (initialize is not null)
                {
                    var runc = sequence is not null ?
                    new RunClass(initialize, sequence.Value) :
                    new RunClass(initialize, Sequence.Last);
                    runClassList.Add(runc);
                }
                break;
            }
        }, _DllName);

    }

    /// <summary>
    /// 收集特性下的函数
    /// </summary>
    void GetAttributeFunctions(List<RunClass> initialize, List<RunClass> terminate)
    {
        AppDomainGetTypes(type => {
            if (type.IsAbstract)
                return;

            var mets = type.GetMethods();
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                {
                    if (attr[jj] is IFoxInitialize jjAtt)
                    {
                        var runc = new RunClass(method, jjAtt.SequenceId);
                        if (jjAtt.IsInitialize)
                            initialize.Add(runc);
                        else
                            terminate.Add(runc);
                        break;
                    }
                }
            }
        }, _DllName);
    }

    /// <summary>
    /// 执行收集到的函数
    /// </summary>
    static void RunFunctions(List<RunClass> runClassList)
    {
        for (int i = 0; i < runClassList.Count; i++)
            runClassList[i].Run();
        runClassList.Clear();
    }
}