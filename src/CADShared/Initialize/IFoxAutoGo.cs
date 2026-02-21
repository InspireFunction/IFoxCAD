#define parallel

#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

namespace IFoxCAD.Cad;

using ConcurrentCollections;
using System.Linq;
using System.Reflection;

#region 工具
[Flags]
public enum Sequence : int
{
    StartFirst = 1, // 进程开启,后台最先
    StartLast = 1 << 1, // 进程开启,后台最后
    StartOnce = 1 << 2, // 文档首次开启,单例(特性默认/打印信息/发送命令/必然拥有前台文档)
    StartDocs = 1 << 3, // 文档每次开启

    EndDocs = 1 << 4, // 文档每次将要关闭
    EndOnce = 1 << 5, // 文档最后一个将要关闭,不是单例

    EndDestroyed = 1 << 6, // 文档每次已经关闭
    EndDestroyedOnce = 1 << 7, // 文档最后一个已经关闭,不是单例

    ProcessFirst = 1 << 8, // 进程关闭,最先,此时已经没有前台
    ProcessLast = 1 << 9 // 进程关闭,最后
}

// 初始化接口,仿IExtensionApplication
public interface IFoxAutoGo
{
    // 控制加载顺序
    Sequence SequenceId();
    // 被cad加载时候会自动执行
    void Initialize();
    // 关闭cad的时候会自动执行
    void Terminate();
}

// 初始化特性,放在命令函数上用来初始化
[AttributeUsage(AttributeTargets.Class/*没有用*/
   | AttributeTargets.Method
   | AttributeTargets.Property/*没有用*/,
   AllowMultiple = true)]
public class IFoxInitializeAttribute : Attribute
{
    public Sequence SequenceId { get; }
    public IFoxInitializeAttribute(Sequence sq = Sequence.StartOnce)
    {
        SequenceId = sq;
    }
}


// 执行器
public class Actuator : IEquatable<Actuator>, IComparable<Actuator>
{
    public Sequence SequenceId { get; }
    MethodInfo _methodInfo;

    public Actuator(MethodInfo method, Sequence sequence)
    {
        SequenceId = sequence;
        _methodInfo = method;
    }

    public bool Equals(Actuator other)
    {
        if (other is null) return false;
        return (int)SequenceId == (int)other.SequenceId
            && _methodInfo.Equals(other._methodInfo);
    }
    public override bool Equals(object obj)
    {
        if (obj is not Actuator other) return false;
        return Equals(other);
    }
    public override int GetHashCode()
    {
        return (int)SequenceId;
    }

    // SortedSet/SortedList/SortedDictionary都是不能重复,
    // 而且要实现IComparable<T>接口,而且不是鸭子类型
    public int CompareTo(Actuator other)
    {
        if ((int)SequenceId > (int)other.SequenceId)
            return 1;
        if ((int)SequenceId < (int)other.SequenceId)
            return -1;
        return 0;
    }

    /// <summary>
    /// 运行方法
    /// </summary>
    public Actuator Run(object[]? args = null)
    {
        try
        {
            args ??= [];
            TypeCache.Invoke(_methodInfo, args);
            return this;
        }
        catch (System.Exception e)
        {
            var methodName = _methodInfo.Name;
            var className = _methodInfo.ReflectedType?.FullName ?? _methodInfo.DeclaringType?.FullName ?? "UnknownClass";
            var parameters = _methodInfo.GetParameters();

            // 检查参数数量和类型是否都正确
            bool isParamCountMatch = parameters.Length == args?.Length;
            bool isParamTypeMatch = true;
            if (isParamCountMatch && args != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var argValue = args[i];

                    // 如果参数值不为null，检查类型是否兼容
                    if (argValue != null)
                    {
                        var argType = argValue.GetType();
                        // 检查参数类型是否兼容（允许派生类型）
                        if (!paramType.IsAssignableFrom(argType))
                        {
                            isParamTypeMatch = false;
                            break;
                        }
                    }
                    // 如果参数值是null，检查参数类型是否允许null（值类型不允许null，除非是可空类型）
                    else if (paramType.IsValueType && Nullable.GetUnderlyingType(paramType) == null)
                    {
                        isParamTypeMatch = false;
                        break;
                    }
                }
            }

            // 如果参数数量和类型都正确，则抛出原生错误
            if (isParamCountMatch && isParamTypeMatch)
            {
                Debug.WriteLine("════════════════════════════════════════════════");
                Debug.WriteLine(e);
                Debug.WriteLine("════════════════════════════════════════════════");
                Debugger.Break();
                return this;
            }

            // 构建正确调用字符串
            var correctCall = $"{className}.{methodName}({string.Join(", ", [.. args.Select(a => a?.ToString() ?? "null")])})";

            // 构建错误调用字符串
            var paramString = string.Join(", ", [.. parameters.Select(p => p.Name)]);
            var errorCall = parameters.Length == 0
                ? $"{className}.{methodName}()"
                : $"{className}.{methodName}({paramString})";

            Debug.WriteLine("════════════════════════════════════════════════");
            Debug.WriteLine("🔥 参数调用错误修复指南");
            Debug.WriteLine("");
            Debug.WriteLine($"❌ 您这样写是错误的:");
            Debug.WriteLine($"   {errorCall}");
            Debug.WriteLine("");
            Debug.WriteLine($"✅ 您应该这样写:");
            Debug.WriteLine($"   {correctCall}");
            Debug.WriteLine("");

            if (parameters.Length > 0)
            {
                Debug.WriteLine("📋 需要传入以下参数:");
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    Debug.WriteLine($"   {i + 1}. {param.Name} : {param.ParameterType.FullName}");
                }
            }

            Debug.WriteLine("");
            Debug.WriteLine($"💡 提示: 该方法要求 {args?.Length} 个参数，但您没有接收传入的参数 {parameters.Length} 个");
            Debug.WriteLine("════════════════════════════════════════════════");

            Debugger.Break();
            return this;
        }
    }
}

public static class TypeCache
{
    // 所有实例化映射表<方法,实例化对象>
    static ConcurrentDictionary<MethodInfo, object> _cache1 = new();
    // 所有实例化映射表<方法全名称,实例化对象>
    static ConcurrentDictionary<string, object> _instanceCache = new();
    public static void Clear()
    {
        _cache1.Clear();
        _instanceCache.Clear();
    }

    public static object? Invoke(MethodInfo methodInfo, object[] args)
    {
        var pas = methodInfo.GetParameters();
        if (pas.Length != args.Length)
        {
            var methodName = $"{methodInfo.ReflectedType?.FullName}.{methodInfo.Name}";
            var expectedParams = string.Join(", ", pas.Select(p => $"{p.ParameterType.Name} {p.Name}").ToArray());
            var actualParams = args.Length == 0
                ? "none"
                : string.Join(", ", args.Select(a => a?.GetType().Name ?? "null").ToArray());
            throw new ArgumentException(
                $"参数数量不对应\n" +
                $"方法: {methodName}\n" +
                $"需要参数: {pas.Length} ({expectedParams})\n" +
                $"传入参数: {args.Length} ({actualParams})");
        }

        if (methodInfo.IsStatic)
        {
            // args = new object[pas.Length];
            return methodInfo.Invoke(null, args);
        }

        // 非静态调用,实例化类
        if (!_cache1.TryGetValue(methodInfo, out var instance))
        {
            var fullName = methodInfo.ReflectedType.FullName;
            if (!_instanceCache.TryGetValue(fullName, out instance))
            {
                var type = methodInfo.ReflectedType.Assembly.GetType(fullName);
                instance = Activator.CreateInstance(type);
                _instanceCache.TryAdd(fullName, instance);
            }
            _cache1.TryAdd(methodInfo, instance);
        }
        return methodInfo.Invoke(instance, args);
    }
}
#endregion

public class AutoReflection
{
    // 只反射本dll的程序集
    private readonly bool _constraint = true;
    private readonly Dictionary<Sequence, List<Actuator>> _actuatorMap = new();
    // 加载之后有文档立即执行(单次执行)
    private int _isOnceExecuted = 0;

    private string _assName;
    private AutoRegConfig _autoRegConfig;
    public AutoReflection(string name, AutoRegConfig autoRegConfig)
    {
        _assName = name;
        _autoRegConfig = autoRegConfig;
    }

    // 被cad加载时候自动执行
    public void Initialize()
    {
        try
        {
            var dm = Acap.DocumentManager ?? throw new System.Exception("不可思议的为空Acap.DocumentManager");

            // 初始化字典
            foreach (Sequence seq in Enum.GetValues(typeof(Sequence)))
            {
                _actuatorMap[seq] = [];
            }

            // 获取特性下面全部方法
            var a = GetAttributeFunc();
            foreach (var ac in a)
            {
                _actuatorMap[ac.SequenceId].Add(ac);
            }
            // 获取接口下面全部方法
            var b = GetInterfaceFunc();
            foreach (var ac in b)
            {
                _actuatorMap[ac.SequenceId].Add(ac);
            }

            // 执行任务,此时即使调用doc.Editor输出也是无效的
            var tasks = _actuatorMap[Sequence.StartFirst];
            foreach (var item in tasks)
            {
                item.Run();
            }
            tasks = _actuatorMap[Sequence.StartLast];
            foreach (var item in tasks)
            {
                item.Run();
            }

            // 为了能够无论何种加载都能doc.Editor输出:
            // x01,通过注册表加载StartFirst/Last,有doc没有doc.Editor,所以不会输出.
            // 用订阅文档事件等待,其后会立即触发一次,文档事件内就可以发送打印了.
            // x02,通过netload命令加载,虽然有订阅文档事件,
            // 它会在下次创建文档(ctrl+n)触发,此时必然有doc.Editor需要直接触发.
            dm.DocumentCreated += DmCreated;
            dm.DocumentToBeDestroyed += DmToBeDestroyed;
            dm.DocumentDestroyed += DmDestroyed;
            AcadIdleManager.OnIdle += OnIdle;
        }
        catch (System.Exception e)
        {
            Debugger.Break();
            Env.Printl("AutoClass.Initialize出错::" + e.Message);
        }
    }


    // 空闲事件判断
    void OnIdle(object sender, EventArgs e)
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc is null) return;
        AcadIdleManager.OnIdle -= OnIdle;

        Env.Printl("空闲事件判断");
        if (Interlocked.CompareExchange(ref _isOnceExecuted, 1, 0) == 0)
        {
            var docArgs = new object[] { doc };
            _actuatorMap[Sequence.StartOnce].ForEach(ac => ac.Run(docArgs));
            _actuatorMap[Sequence.StartDocs].ForEach(ac => ac.Run(docArgs));
        }
    }

    // 文档开启(多次执行)
    void DmCreated(object sender, DocumentCollectionEventArgs e)
    {
        try
        {
            var doc = e.Document;
            var docArgs = new object[] { doc };
            _actuatorMap[Sequence.StartDocs].ForEach(ac => ac.Run(docArgs));
        }
        catch (System.Exception ex)
        {
            Debugger.Break();
            Debug.WriteLine($"{nameof(AutoReflection.DmCreated)} 出错::{ex.Message}");
        }
    }

    // 文档已经关闭(多次执行)
    // 传递的是文件名,不是文档
    private void DmDestroyed(object sender, DocumentDestroyedEventArgs e)
    {
        try
        {
            var fileName = e.FileName;
            var docArgs = new object[] { fileName };
            _actuatorMap[Sequence.EndDestroyed].ForEach(ac => ac.Run(docArgs));
            // 最后关闭的文档,它不是单例,因为重复多次.
            if (Acap.DocumentManager.Count == 1)
            {
                _actuatorMap[Sequence.EndDestroyedOnce].ForEach(ac => ac.Run(docArgs));
            }
        }
        catch (System.Exception ex)
        {
            Debugger.Break();
            Debug.WriteLine($"{nameof(AutoReflection.DmToBeDestroyed)}出错::" + ex.Message);
        }
    }

    // 文档将要关闭(多次执行)
    void DmToBeDestroyed(object sender, DocumentCollectionEventArgs e)
    {
        try
        {
            var doc = e.Document;
            var docArgs = new object[] { doc };
            _actuatorMap[Sequence.EndDocs].ForEach(ac => ac.Run(docArgs));
            // 最后关闭的文档,它不是单例,因为重复多次.
            if (Acap.DocumentManager.Count == 1)
            {
                _actuatorMap[Sequence.EndOnce].ForEach(ac => ac.Run(docArgs));
            }
        }
        catch (System.Exception ex)
        {
            Debugger.Break();
            Debug.WriteLine($"{nameof(AutoReflection.DmToBeDestroyed)}出错::" + ex.Message);
        }
    }

    // 关闭cad的时候会自动执行
    public void Terminate()
    {
        try
        {
            // 虽然可以获取dm,但是下面的事件已经被清空了
            var dm = Acap.DocumentManager ?? throw new System.Exception("不可思议的为空Acap.DocumentManager");
            if (dm.Count > 0)
            {
                // 这里从不进入
                dm.DocumentCreated -= DmCreated;
                dm.DocumentToBeDestroyed -= DmToBeDestroyed;
            }

            // 执行任务
            _actuatorMap[Sequence.ProcessFirst].ForEach(ac => ac.Run());
            _actuatorMap[Sequence.ProcessLast].ForEach(ac => ac.Run());

            // 释放缓存,类析构会在此之后.
            TypeCache.Clear();
        }
        catch (System.Exception e)
        {
            Debugger.Break();
            Debug.WriteLine("AutoClass.Terminate出错::" + e.Message);
        }
    }

    /// <summary>
    /// 遍历程序域下所有类型
    /// </summary>
    /// <param name="dllNameWithoutExtension">约束此文件反射,不含扩展名</param>
    public static IEnumerable<Type> AppDomainGetTypes(string? dllNameWithoutExtension = null)
    {
        try
        {

            // 01,过滤ass.IsDynamic,因为Acad2021报错:
            // System.NotSupportedException:动态程序集中不支持已调用的成员
            // 02,过滤AcInfoCenterConn.dll,通讯库,因为反射它会依赖其他造成报错:
            // ReflectionTypeLoadException
            // 03,WPF的报错 https://dev59.com/rWIi5IYBdhLWcg3w_QmH
            // Microsoft.Expression.Interactions程序集
            // 跳过全部微软的包就好了?

            const string str1 = "AcInfoCenterConn";
            const string str2 = "Microsoft";


#if parallel
            System.Diagnostics.Trace.WriteLine("这是一条Trace消息,此时是并行");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Where(ass => !ass.IsDynamic())
                .Where(ass => !ass.GetName().Name.Contains(str2))
                .Where(ass => Path.GetFileNameWithoutExtension(ass.Location) != str1);

            // 约束在此dll中反射.
            if (dllNameWithoutExtension is not null)
            {
                assemblies = assemblies.Where(ass => Path.GetFileNameWithoutExtension(ass.Location) == dllNameWithoutExtension);
            }
            // 只反射公开
            var types = assemblies.SelectMany(ass => ass.GetExportedTypes());
#else
/*
        // 找名称
        var names = AppDomain.CurrentDomain.GetAssemblies()
            .Where(ass => !ass.IsDynamic
            && Path.GetFileNameWithoutExtension(ass.Location) == str1)
            .Select(ass => ass); // ass.GetName().Name 为什么是空?
        MessageBox.Show(string.Join("\n\r", names.ToString()));
*/
        // 串行测试
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(ass => !ass.IsDynamic())
            .Where(ass => !ass.GetName().Name.Contains(str2))
            .Where(ass => Path.GetFileNameWithoutExtension(ass.Location) != str1);

        // 约束在此dll中反射.
        if (dllNameWithoutExtension is not null) {
            assemblies = assemblies.Where(ass =>
                Path.GetFileNameWithoutExtension(ass.Location)
                == dllNameWithoutExtension);
        }

        int error = 0;
        var types = assemblies.SelectMany(ass => {
            try {
                ++error;
                // return ass.GetTypes();
                return ass.GetExportedTypes(); // 只反射公开
            } catch(ReflectionTypeLoadException e) {
                Debug.WriteLine($"出错:App;计数{error};错误信息:{e}");
                foreach (Type type in e.Types) {
                    if (type is null) continue;
                    Debug.WriteLine($"可用类型:{type.FullName}");
                }
                Debugger.Break();
                return null;
            } catch(Exception e) {
                Debug.WriteLine($"出错:App2;计数{error};程序集:{ass}");
                Debug.WriteLine($"出错:App2;计数{error};错误信息:{e}");
                Debugger.Break();
                return null;
            }
        });
#endif
            return types;
        }
        catch (System.Exception e)
        {
            Debugger.Break();
            Debug.WriteLine($"出错:AppDomainGetTypes; 错误信息:{e.Message}");
            return [];
        }
    }

    /// <summary>
    /// 收集接口下的函数
    /// </summary>
    IEnumerable<Actuator> GetInterfaceFunc()
    {
        // 约束只反射本dll的类
        string? dll = null;
        if (_constraint)
        {
            //"IFoxCAD.Acad08" 不对,要是: TestAcad08
            //var ass = Assembly.GetExecutingAssembly();
            //dll = Path.GetFileNameWithoutExtension(ass.Location); 
            dll = _assName;
        }

        var ts = AppDomainGetTypes(dll);

#if parallel
        System.Diagnostics.Trace.WriteLine("这是一条Trace消息,此时是并行");

        // 我要实例化派生类(跳过委托/抽象和静态)
        var acs = ts.Where(type => type.IsClass)
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetInterfaces().FirstOrDefault(
                iface => iface.Name == nameof(IFoxAutoGo)) is not null)
            // .SelectMany(type => type.GetMethods()); // 方法展开会错序.
            .Select(type => CreateActuator2(type))
            .ToArray();

        // 串行加入容器
        foreach (var ac2 in acs)
        {
            if (ac2 is null) continue;
            yield return ac2.Value.Init;
            yield return ac2.Value.Term;
        }
#else
        var types = ts.ToArray();
        for (int i = 0; i < types.Length; i++) {
            var type = types[i];
            if (type is null) continue;   
            // 我要实例化派生类(跳过委托/抽象和静态)
            if (!type.IsClass || type.IsAbstract) continue;
            var hasIface = type.GetInterfaces().FirstOrDefault(
                iface => iface.Name == nameof(IAutoGo));
            if (hasIface is null) continue;
            var ac2 = CreateActuator2(type);
            if(ac2 is null) continue;
            yield return ac2.Value.Init;
            yield return ac2.Value.Term;
        }
#endif
    }

    const string _sequenceId = nameof(Sequence) + "Id";
    const string _in = "Initialize";
    const string _te = "Terminate";

    (Actuator Init, Actuator Term)? CreateActuator2(Type type)
    {
        // 获取接口实现的成员函数,虽然它们只会出现一次,
        // 但万一别人写了重载呢,要参数数量是0才行.
        var mets = type.GetMethods();
        var im = mets.FirstOrDefault(m => m.Name == _in
            && m.GetParameters().Length == 0
            && !m.IsAbstract);
        if (im is null) return null;

        var tm = mets.FirstOrDefault(m => m.Name == _te
            && m.GetParameters().Length == 0
            && !m.IsAbstract);
        if (tm is null) return null;

        var sq = Sequence.StartOnce;
        var sm = mets.FirstOrDefault(m => m.Name == _sequenceId
            && m.GetParameters().Length == 0
            && !m.IsAbstract);
        if (sm is not null)
        {
            // 这里是多线程环境,缓存需要线程安全容器
            var a = TypeCache.Invoke(sm, new object[0]);
            if (a is null)
            {
                return null;
            }
            sq = (Sequence)a;
        }

        // 如果派生类写的是End需要映射回Start.
        if (sq == Sequence.ProcessFirst)
            sq = Sequence.StartFirst;
        else if (sq == Sequence.ProcessLast)
            sq = Sequence.StartLast;
        else if (sq == Sequence.EndDocs)
            sq = Sequence.StartDocs;
        else if (sq == Sequence.EndOnce)
            sq = Sequence.StartOnce;

        Sequence sq2;
        if (sq == Sequence.StartFirst)
            sq2 = Sequence.ProcessFirst;
        else if (sq == Sequence.StartLast)
            sq2 = Sequence.ProcessLast;
        else if (sq == Sequence.StartDocs)
            sq2 = Sequence.EndDocs;
        else if (sq == Sequence.StartOnce)
            sq2 = Sequence.EndOnce;
        else throw new ArgumentException("出错类型");

        return (new Actuator(im, sq), new Actuator(tm, sq2));
    }

    // 根据特性创建类型,允许同一个方法多个特性
    IEnumerable<Actuator> CreateActuator(MethodInfo methodInfo)
    {
        return methodInfo.GetCustomAttributes(true)
            .OfType<IFoxInitializeAttribute>()
            .Select(att => new Actuator(methodInfo, att.SequenceId));
    }

    /// <summary>
    /// 收集特性下的函数
    /// </summary>
    IEnumerable<Actuator> GetAttributeFunc()
    {
        // 特性会出现在同一个类中的多个方法,
        // 特性下的方法要public,否则就被编译器优化掉了.
        // 约束只反射本dll的类
        string? dll = null;
        if (_constraint)
        {
            //"IFoxCAD.Acad08" 不对,要是: TestAcad08
            //var ass = Assembly.GetExecutingAssembly();
            //dll = Path.GetFileNameWithoutExtension(ass.Location);
            dll = _assName;
        }

        var ts = AppDomainGetTypes(dll);

#if parallel
        System.Diagnostics.Trace.WriteLine("这是一条Trace消息,此时是并行");
        var acs = ts.SelectMany(type => type.GetMethods())
            .SelectMany(CreateActuator).ToArray();
        // 串行加入
        foreach (var ac in acs)
        {
            yield return ac;
        }
#else
        var types = ts.ToArray();
        for (int i = 0; i < types.Length; i++) {
            var mets = types[i].GetMethods();
            for (int j = 0; j < mets.Length; j++) {
                yield return CreateActuator(mets[j]);
            }
        }
#endif
    }
}
