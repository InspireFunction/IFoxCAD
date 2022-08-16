namespace Test;

/// <summary>
/// 注册中心(自动执行接口):
/// <para>用于启动cad后写入启动注册表及反射调用以下特性和接口</para>
/// <para>netload的工程必须继承<see cref="AutoRegAssem"/>虚函数后才能使用<see cref="IFoxInitialize"/>特性和<see cref="IFoxAutoGo"/>接口</para>
/// <para>启动cad后的执行顺序为:</para>
/// <para>1:<see cref="AutoRegAssem"/>构造函数</para>
/// <para>2:<see cref="IFoxInitialize"/>特性..多个</para>
/// <para>3:<see cref="IFoxAutoGo"/>接口..多个</para>
/// <para>4:本类的构造函数</para>    
/// </summary>
public class CmdINI : AutoRegAssem
{
    public CmdINI() : base(AutoRegConfig.All)
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n {nameof(CmdINI)}构造函数,开始自动执行\r\n");
    }

    ///如果netload之后用 <see cref="IFoxRemoveReg"/> 删除注册表,
    ///由于不是也不能卸载dll,再netload是无法执行自动接口的,
    ///所以此时会产生无法再注册的问题...因此需要暴露此注册函数(硬来)
    [CommandMethod("IFoxAddReg")]
    public void IFoxAddReg()
    {
        base.RegApp();

        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n 加入注册表");
    }

    /// <summary>
    /// 卸载注册表信息
    /// </summary>
    [CommandMethod("IFoxRemoveReg")]
    public void IFoxRemoveReg()
    {
        //执行命令的时候会再次执行构造函数(导致初始化两次),但是再次执行就不会了
        base.UnRegApp(); 

        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n 卸载注册表");
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