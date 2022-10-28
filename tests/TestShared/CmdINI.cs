//#define givePeopleTest

using System.Diagnostics;

namespace Test;

/// <summary>
/// 注册中心(自动执行接口):
/// <para>
/// 继承<see cref="AutoRegAssem"/>虚函数后才能使用<br/>
/// 0x01 netload加载之后自动执行,写入启动注册表,下次就不需要netload了<br/>
/// 0x02 反射调用<see cref="IFoxInitialize"/>特性和<see cref="IFoxAutoGo"/>接口<br/>
/// 启动cad后的执行顺序为:<br/>
/// 1:<see cref="AutoRegAssem"/>构造函数<br/>
/// 2:<see cref="IFoxInitialize"/>特性..多个<br/>
/// 3:<see cref="IFoxAutoGo"/>接口..多个<br/>
/// 4:本类的构造函数<br/>
/// <code>
/// **** 警告 ****
/// 如果不写一个 <see cref="CmdInit.AutoRegAssemEx"/> 储存这个对象,
/// 而是直接写卸载命令在此,
/// 第一次加载的时候会初始化完成,然后这个类生命就结束了,
/// 第二次通过命令进入,会引发构造函数再次执行,留意构造函数的打印信息即可发现
/// </code>
/// </para>
/// </summary>
public class AutoRegAssemEx : AutoRegAssem
{
    public AutoRegAssemEx() : base(AutoRegConfig.All)
    {
        CmdInit.AutoRegAssemEx = this;
#if givePeopleTest
#if Debug
        // 此处用来反射本程序集,检查是否存在重复命令
        AutoReflection.DebugCheckCmdRecurrence();
#endif
        Env.Printl($"{nameof(AutoRegAssemEx)}构造函数,开始自动执行\r\n");
#endif
    }
}


public class CmdInit
{
    public static AutoRegAssemEx? AutoRegAssemEx;

    /// 如果netload之后用 <see cref="IFoxRemoveReg"/> 删除注册表,
    /// 由于不是也不能卸载dll,再netload是无法执行自动接口的,
    /// 所以此时会产生无法再注册的问题...因此需要暴露此注册函数(硬来)
    [CommandMethod(nameof(IFoxAddReg))]
    public void IFoxAddReg()
    {
        Env.Printl($"加入注册表");

        AutoRegAssemEx ??= new();
        AutoRegAssemEx.RegApp();
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    [CommandMethod(nameof(IFoxRemoveReg))]
    public void IFoxRemoveReg()
    {
        Env.Printl($"卸载注册表");

        // 防止卸载两次,不然会报错的
        AutoRegAssemEx?.UnRegApp();
        AutoRegAssemEx = null;
    }
}

#if givePeopleTest
/*
 * 自动执行:特性
 */
public class Cmd_IFoxInitialize
{
    int TestInt = 0;

    [IFoxInitialize]
    public void Initialize()
    {
        Env.Printl($"开始自动执行,可以分开多个类和多个函数:{nameof(Cmd_IFoxInitialize)}.{nameof(Initialize)}+{TestInt}");
    }

    [IFoxInitialize]
    public void Initialize2()
    {
        Env.Printl($"开始自动执行,可以分开多个类和多个函数,又一次测试:{nameof(Cmd_IFoxInitialize)}.{nameof(Initialize2)}");
    }

    //[IFoxInitialize(isInitialize: false)]
    //public void Terminate()
    //{
    //    try
    //    {
    //        // 注意此时编辑器已经回收,所以此句引发错误
    //        // 您可以写一些其他的释放动作,例如资源回收之类的
    //        Env.Printl($"\n 结束自动执行 Terminate \r\n");
    //        // 改用
    //        Debug.WriteLine($"\n 结束自动执行 Terminate \r\n");
    //    }
    //    catch (System.Exception e)
    //    {
    //        System.Windows.Forms.MessageBox.Show(e.Message);
    //    }
    //}

    [IFoxInitialize]
    public static void StaticInitialize()
    {
        Env.Printl($"开始自动执行,静态调用:{nameof(Cmd_IFoxInitialize)}.{nameof(StaticInitialize)}");
    }
}


/*
 * 自动执行:接口
 */
public class Cmd_IFoxInitializeInterface : IFoxAutoGo
{
    int TestInt = 0;
    public Cmd_IFoxInitializeInterface()
    {
        Env.Printl($"开始自动执行,{nameof(IFoxAutoGo)}接口调用:{nameof(Cmd_IFoxInitializeInterface)}::{TestInt}");
    }

    public Sequence SequenceId()
    {
        return Sequence.Last;
    }

    public void Initialize()
    {
        Env.Printl($"开始自动执行,{nameof(IFoxAutoGo)}接口调用:{nameof(Initialize)}::{TestInt}");
    }

    public void Terminate()
    {
        Debug.WriteLine($"开始自动执行,{nameof(IFoxAutoGo)}接口调用:{nameof(Terminate)}::{TestInt}");
        //    try
        //    {
        //        // 注意此时编辑器已经回收,所以此句没用,并引发错误
        //        Env.Printl($"结束自动执行 {nameof(Cmd_IFoxInitializeInterface)}.Terminate \r\n");
        //    }
        //    catch (System.Exception e)
        //    {
        //        System.Windows.Forms.MessageBox.Show(e.Message);
        //    }
    }
}
#endif