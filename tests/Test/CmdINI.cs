/*
 * 自动执行接口
 * 这里必须要实现一次这个接口,才能使用 IFoxInitialize 特性进行自动执行
 */

/// <summary>
/// 初始化注册表及反射
/// 运行顺序
/// 1: <see cref="AutoRegAssem"/>构造函数
/// 2: <see cref="IFoxInitialize"/>特性..多个
/// 3: <see cref="IFoxAutoGo"/>接口..多个
/// 4: <see cref="CmdINI"/>构造函数
/// </summary>
public class CmdINI : AutoRegAssem
{
    public CmdINI()
    {
        var dm = Application.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage($"\n 开始自动执行{nameof(CmdINI)} \r\n");
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
    public void NameCasualtest()
    {
        var dm = Application.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        ed.WriteMessage("\n 开始自动执行 又一次测试 \r\n");
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