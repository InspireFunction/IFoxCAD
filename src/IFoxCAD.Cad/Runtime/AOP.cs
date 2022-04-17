namespace IFoxCAD.Cad;
using HarmonyLib;

public class IFoxRefuseInjectionTransaction : Attribute
{
    /// <summary>
    /// 拒绝注入事务
    /// </summary>
    public IFoxRefuseInjectionTransaction()
    {
    }
}

/*
 * 例子
 * public class AutoAOP
 * {
 *      [IFoxInitialize]//自执行接口
 *      public void Initialize()
 *      {
 *          AOP.Run(nameof(Test));
 *      }
 * }
 *  
 * 类库用户想侵入的命名空间是用户的,
 * 所以需要用户手动进行AOP.Run(),
 * 默认情况不侵入用户的命令,必须用户手动启用此功能;
 * 启动执行策略之后,侵入命名空间下的命令,
 * 此时有拒绝特性的策略保证括免,因为用户肯定是想少写一个事务注入的特性;
 */

public class AOP
{
    /// <summary>
    /// 在此命名空间下的命令末尾注入清空事务栈函数
    /// </summary>
    public static void Run(string nameSpace)
    {
        Dictionary<string, (CommandMethodAttribute Cmd, Type MetType, MethodInfo MetInfo)> cmdDic = new();
        AutoClass.AppDomainGetTypes(type => {
            if (type.Namespace != nameSpace)
                return;
            //类上面特性
            if (type.IsClass)
            {
                var attr = type.GetCustomAttributes(true);
                if (RefuseInjectionTransaction(attr))
                    return;
            }

            //函数上面特性
            var mets = type.GetMethods();//获得它的成员函数
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];
                //找到特性,特性下面的方法要是Public,否则就被编译器优化掉了.
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                    if (attr[jj] is CommandMethodAttribute cmdAtt)
                    {
                        if (!RefuseInjectionTransaction(attr))
                            cmdDic.Add(cmdAtt.GlobalName, (cmdAtt, type, method));
                    }
            }
        });

        //运行的命令写在了Test.dll,当然不是ifox.cad类库内了....
        if (cmdDic.Count == 0)
            return;

        var harmony = new Harmony(nameSpace);
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

    /// <summary>
    /// 拒绝注入事务
    /// </summary>
    /// <param name="attr">属性</param>
    /// <returns></returns>
    private static bool RefuseInjectionTransaction(object[] attr)
    {
        bool refuseInjectionTransaction = false;
        for (int kk = 0; kk < attr.Length; kk++)
        {
            if (attr[kk] is IFoxRefuseInjectionTransaction)
            {
                refuseInjectionTransaction = true;
                break;
            }
        }
        return refuseInjectionTransaction;
    }

    public static void IFoxCmdAddFirst()
    {
        //此生命周期会在静态事务栈上面,被无限延长
        var _ = DBTrans.Top;
    }

    public static void IFoxCmdAddLast()
    {
        DBTrans.FinishDatabase();
    }
}