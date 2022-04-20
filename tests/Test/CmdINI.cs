/*
 * 自动执行接口
 * 这里必须要实现一次这个接口,才能使用特性
 */
public class CmdINI : IExtensionApplication
{
    AutoClass ac;
    public void Initialize()
    {
        var ara = new AutoRegAssem();
        ac = new AutoClass(ara.Info.Name);
        ac.Initialize();
        //实例化了 AutoClass 之后会自动执行 IFoxAutoGo 接口下面的类,
        //以及自动执行特性 [IFoxInitialize]
        //类库用户不在此处进行其他代码,而是实现特性
    }

    public void Terminate()
    {
        ac.Terminate();
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
        var dm = Application.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage("\n 开始自动执行 可以分开多个类和多个函数 \r\n");
    }

    [IFoxInitialize]
    public void Initialize()
    {
        var dm = Application.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage("\n 开始自动执行 Initialize \r\n");
    }

    [IFoxInitialize(isInitialize: false)]
    public void Terminate()
    {
        //try
        //{
        //    var dm = Application.DocumentManager;
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
    //    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nload....");
    //}
    //[IFoxInitialize(Sequence.First, false)]
    //public void Terminate()
    //{
    //    //文档管理器将比此接口前死亡,因此此句不会执行
    //    Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\nunload....");
    //}
}