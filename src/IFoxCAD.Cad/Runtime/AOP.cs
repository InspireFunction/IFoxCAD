using IFoxCAD.Cad;
using HarmonyLib;

/*
 * 在所有的命令末尾注入清空事务栈函数
 */
public class AOP
{
    /// <summary>
    /// 遍历程序域下所有类型
    /// </summary>
    /// <param name="action"></param>
    public static void AppDomainGetTypes(Action<Type> action)
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
            try
            {
                var assembly = assemblies[ii];
                //引用到test工程之后再调用
                //if (!assembly.GetName(true).Name.Contains(nameof(IFoxCAD)))
                //    continue;

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
                        action(type);
                }
            }
            catch
            { }
        }
    }

    public static void Run()
    {
        Dictionary<string, (CommandMethodAttribute Cmd, Type MetType, MethodInfo MetInfo)> cmdDic = new();
        AppDomainGetTypes(type => {
            var mets = type.GetMethods();//获得它的成员函数
            for (int i = 0; i < mets.Length; i++)
            {
                var method = mets[i];
                //找到特性,特性下面的方法要是Public,否则就被编译器优化掉了.
                var attr = method.GetCustomAttributes(true);
                for (int j = 0; j < attr.Length; j++)
                    if (attr[j] is CommandMethodAttribute cmdAtt)
                        cmdDic.Add(cmdAtt.GlobalName, (cmdAtt, type, method));
            }
        });

        //运行的命令写在了Test.dll,当然不是ifox内了....
        if (cmdDic.Count == 0)
            return;

        var harmony = new Harmony(nameof(IFoxCAD));
        var mPrefix = SymbolExtensions.GetMethodInfo(() => IFoxCmdAddFirst());//进入函数前
        var mPostfix = SymbolExtensions.GetMethodInfo(() => IFoxCmdAddLast());//进入函数后
        var mp1 = new HarmonyMethod(mPrefix);
        var mp2 = new HarmonyMethod(mPostfix);

        foreach (var item in cmdDic)
        {
            //原函数执行(空间type,函数名)
            var mOriginal = AccessTools.Method(item.Value.MetType, item.Value.MetInfo.Name);
            //mOriginal.Invoke();
            //新函数执行:创造两个函数加入里面               
            var newMet = harmony.Patch(mOriginal, mp1, mp2);
            //newMet.Invoke();
        }
    }

    public static void IFoxCmdAddFirst()
    {
        //此生命周期会在事务栈上面,被无限延长
        var _ = DBTrans.Top;
    }

    public static void IFoxCmdAddLast()
    {
        var db = Application.DocumentManager.MdiActiveDocument.Database;
        DBTrans.FinishDatabase(db);
    }
}