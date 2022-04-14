using IFoxCAD.Cad;
using HarmonyLib;

/*
 * 在所有的命令末尾注入清空事务栈函数
 */
public class AOP
{
    public static void Run()
    {
        Dictionary<string, (CommandMethodAttribute Cmd, Type MetType, MethodInfo MetInfo)> cmdDic = new();
        AutoClass.AppDomainGetTypes(type => {
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