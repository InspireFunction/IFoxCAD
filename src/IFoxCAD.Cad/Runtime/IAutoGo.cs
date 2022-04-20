namespace IFoxCAD.Cad;
using System.Diagnostics;

[Flags]
public enum Sequence : byte
{
    First,// 最先
    Last, // 最后
}

public interface IAutoGo
{
    // 控制加载顺序
    Sequence SequenceId();
    // 关闭cad的时候会自动执行
    void Terminate();
    // 打开cad的时候会自动执行
    void Initialize();
}

public class IFoxInitialize : Attribute
{
    internal Sequence Sequence;
    internal bool IsInitialize;
    /// <summary>
    /// 自己制作的一个特性,放在函数上面用来初始化或者结束回收
    /// </summary>
    /// <param name="sequence">优先级</param>
    /// <param name="isInitialize"><see langword="true"/>用于初始化;<see langword="false"/>用于结束回收</param>
    public IFoxInitialize(Sequence sequence = Sequence.Last, bool isInitialize = true)
    {
        Sequence = sequence;
        IsInitialize = isInitialize;
    }
}

//为了解决IExtensionApplication在一个dll内无法多次实现接口的关系
//所以在这里反射加载所有的IAutoGo,以达到能分开写"启动运行"函数的目的
public class RunClass
{
    public Sequence SequenceId { get; }
    MethodInfo _methodInfo;
    object? _instance;

    public RunClass(MethodInfo method, Sequence sequence)
    {
        _methodInfo = method;
        SequenceId = sequence;

        var reftype = _methodInfo.ReflectedType;
        if (reftype == null) return;
        var fullName = reftype.FullName; //命名空间+类
        if (fullName == null) return;
        var type = reftype.Assembly.GetType(fullName);//通过程序集反射创建类+
        if (type == null) return;
        _instance = Activator.CreateInstance(type);
    }

    /// <summary>
    /// 运行方法
    /// </summary>
    public void Run()
    {
        try
        {
            _methodInfo.Invoke(_instance);
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
    const string _iAutoGo = "IAutoGo";

    //打开cad的时候会自动执行
    public void Initialize()
    {
        try
        {
            GetAttributeFunctions();
            GetInterfaceFunctions(_InitializeList, nameof(Initialize));
            //按照 SequenceId 排序_升序
            _InitializeList = _InitializeList.OrderBy(runClass => runClass.SequenceId).ToList();
            RunFunctions(_InitializeList);
            _InitializeList.Clear();
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
            _TerminateList = _TerminateList.OrderByDescending(runClass => runClass.SequenceId).ToList();
            RunFunctions(_TerminateList);
            _TerminateList.Clear();
        }
        catch (System.Exception)
        {
            Debugger.Break();
        }
    }

    /// <summary>
    /// 遍历程序域下所有类型
    /// </summary>
    /// <param name="action">输出每个成员</param>
    public static void AppDomainGetTypes(Action<Type> action)
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
                Type[]? types = null;
                try
                {
                    //获取类型集合,反射时候还依赖其他的dll就会这个错误
                    //此通讯库要跳过,否则会报错.
                    if (Path.GetFileName(assembly.Location) == "AcInfoCenterConn.dll")
                        continue;
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
    /// <param name="methodName"></param>
    /// <returns></returns>
    void GetInterfaceFunctions(List<RunClass> runClassList, string methodName = nameof(Initialize))
    {
        string JoinBoxSequenceId = nameof(Sequence) + "Id";
        AppDomainGetTypes(type => {
            //获取接口集合
            var inters = type.GetInterfaces();
            for (int ii = 0; ii < inters.Length; ii++)
            {
                if (inters[ii].Name == _iAutoGo)//找到接口的函数
                {
                    Sequence? sequence = null;
                    MethodInfo? initialize = null;

                    //获得它的成员函数
                    var mets = type.GetMethods();
                    for (int jj = 0; jj < mets.Length; jj++)
                    {
                        var method = mets[jj];
                        if (method.IsAbstract)
                            continue;
                        if (method.Name == JoinBoxSequenceId)
                        {
                            var obj = method.Invoke();
                            if (obj != null)
                                sequence = (Sequence)obj;
                            continue;
                        }
                        else if (method.Name == methodName)
                        {
                            initialize = method;
                        }
                        if (initialize is not null && sequence is not null)
                            break;
                    }
                    if (initialize is not null)
                    {
                        RunClass runc;
                        if (sequence is not null)
                            runc = new RunClass(initialize, sequence.Value);
                        else
                            runc = new RunClass(initialize, Sequence.Last);
                        runClassList.Add(runc);
                    }
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 收集特性下的函数
    /// </summary>
    void GetAttributeFunctions()
    {
        AppDomainGetTypes(type => {
            var mets = type.GetMethods();//获得它的成员函数
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];

                //找到特性,特性下面的方法要是Public,否则就被编译器优化掉了.
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                {
                    if (attr[jj] is IFoxInitialize jjAtt)
                    {
                        var runc = new RunClass(method, jjAtt.Sequence);
                        if (jjAtt.IsInitialize)
                            _InitializeList.Add(runc);
                        else
                            _TerminateList.Add(runc);
                        break;//特性只会出现一次
                    }
                }
            }
        });
    }

    /// <summary>
    /// 执行收集到的函数
    /// </summary>
    void RunFunctions(List<RunClass> runClassList)
    {
        for (int i = runClassList.Count - 1; i >= 0; i--)
        {
            runClassList[i].Run();
            runClassList.RemoveAt(i);
        }
    }
}