namespace Test;

/// <summary>
/// 注册中心(自动执行接口):
/// <para>用于启动cad后写入启动注册表及反射调用以下特性和接口<br/>
/// netload的工程必须继承<see cref="AutoRegAssem"/>虚函数后才能使用<see cref="IFoxInitialize"/>特性和<see cref="IFoxAutoGo"/>接口<br/>
/// 启动cad后的执行顺序为:<br/>
/// 1:<see cref="AutoRegAssem"/>构造函数<br/>
/// 2:<see cref="IFoxInitialize"/>特性..多个<br/>
/// 3:<see cref="IFoxAutoGo"/>接口..多个<br/>
/// 4:本类的构造函数<br/>
/// <code>
/// **** 警告 ****
/// 如果不写一个<see cref="CmdInitEx.CmdInit"/> 储存这个对象,
/// 而是直接写卸载命令在此,
/// 第一次加载的时候会初始化完成,然后这个类生命就结束了,
/// 第二次通过命令进入,会引发构造函数再次执行,留意构造函数的打印信息即可发现
/// </code>
/// </para>
/// </summary>
public class CmdInit : AutoRegAssem
{
    public CmdInit() : base(AutoRegConfig.All)
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n {nameof(CmdInit)}构造函数,开始自动执行\r\n");

        CmdInitEx.CmdInit = this;
    }
}

public class CmdInitEx
{
    public static CmdInit? CmdInit;

    ///如果netload之后用 <see cref="IFoxRemoveReg"/> 删除注册表,
    ///由于不是也不能卸载dll,再netload是无法执行自动接口的,
    ///所以此时会产生无法再注册的问题...因此需要暴露此注册函数(硬来)
    [CommandMethod("IFoxAddReg")]
    public void IFoxAddReg()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n 加入注册表");

        CmdInit?.RegApp();
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    [CommandMethod("IFoxRemoveReg")]
    public void IFoxRemoveReg()
    { 
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n 卸载注册表");

        //执行命令的时候会再次执行构造函数(导致初始化两次),但是再次执行就不会了
        CmdInit?.UnRegApp();
    }
}


/*
 * 自动执行特性例子:
 */
public class Cmd_IFoxInitialize
{
    [IFoxInitialize]
    public void NameCasual()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage("\n 开始自动执行 可以分开多个类和多个函数 \r\n");
    }

    [IFoxInitialize]
    public void NameCasualtest()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage("\n 开始自动执行 又一次测试 \r\n");
    }

    [IFoxInitialize]
    public void Initialize()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage("\n 开始自动执行 Initialize \r\n");
    }

    [IFoxInitialize(isInitialize: false)]
    public void Terminate()
    {
        //try
        //{
        //    var dm = Acap.DocumentManager;
        //    var doc = dm.MdiActiveDocument;
        //    var ed = doc.Editor; //注意此时编辑器已经回收,所以此句没用,并引发错误
        //    ed.WriteMessage("\n 结束自动执行 Terminate \r\n");
        //}
        //catch (System.Exception)
        //{
        //}
    }

    //[IFoxInitialize]
    //public void Initialize()
    //{
    //    //文档管理器将比此接口前创建,因此此句会执行
    //    Acap.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nload....");
    //}
    //[IFoxInitialize(Sequence.First, false)]
    //public void Terminate()
    //{
    //    //文档管理器将比此接口前死亡,因此此句不会执行
    //    Acap.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\nunload....");
    //}
}