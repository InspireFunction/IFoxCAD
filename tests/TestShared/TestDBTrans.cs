using Autodesk.AutoCAD.DatabaseServices;

namespace Test;

public class TestTrans
{
    [CommandMethod(nameof(Test_DBTrans))]
    public void Test_DBTrans()
    {
        using var tr = DBTrans.Create();
        if (tr.Editor is null)
            return;
        tr.Editor.WriteMessage("\n测试 Editor 属性是否工作！");
        tr.Editor.WriteMessage("\n----------开始测试--------------");
        tr.Editor.WriteMessage("\n测试document属性是否工作");
        if (tr.Document == Getdoc())
        {
            tr.Editor.WriteMessage("\ndocument 正常");
        }
        tr.Editor.WriteMessage("\n测试database属性是否工作");
        if (tr.Database == Getdb())
        {
            tr.Editor.WriteMessage("\ndatabase 正常");
        }
    }

    private static Database Getdb()
    {
        var db = Acaop.DocumentManager.MdiActiveDocument.Database;
        return db;
    }

    private static Document Getdoc()
    {
        var doc = Acaop.DocumentManager.MdiActiveDocument;
        return doc;
    }
    const string _file = @"D:\桌面\AA.dwg";
    [CommandMethod(nameof(CmdTest_DBTransActiveOpenDwg), CommandFlags.Session)]
    public static void CmdTest_DBTransActiveOpenDwg()
    {
        // 前台打开,并且设置为当前文档
        var doc = DBTrans.OpenFileForFrontend(_file, FileOpenMode.OpenForReadAndAllShare, null);
        Acap.DocumentManager.MdiActiveDocument = doc;
    }

    [CommandMethod(nameof(CmdTest_ForEachDemo))]
    public static void CmdTest_ForEachDemo()
    {
        using var tr = DBTrans.Create();

        // 泛型扩展(用变量名来使用它)
        tr.BlockTable.ForEach(action: (id) => {
            //Debugger.Break();// 为什么cad工程不能断点进入呢?
            id.Print();
            Console.WriteLine(id);
        });

        //tr.BlockTable.ForEach(asdad);
        //void asdad(object id)
        //{
        //    id.Print();
        //}

        tr.BlockTable.ForEach(action: (id) => {
            id.Print();
        });
        tr.BlockTable.ForEach(action: (id, state, index) => {
            id.Print();
        });

        // 符号表扩展(会顶替泛型扩展)
        tr.BlockTable.ForEach((btr) => { // 预处理设置不进入ForEach函数体内
            btr.Print();// 此处可以设置断点
        }, OpenMode.ForRead, checkIdOk: true);
        tr.BlockTable.ForEach((btr, state) => {// 预处理设置不进入ForEach函数体内
            btr.Print();// 此处可以设置断点
        }, OpenMode.ForRead, checkIdOk: true);
        tr.BlockTable.ForEach((btr, state, index) => { // 预处理设置不进入ForEach函数体内
            btr.Print();// 此处可以设置断点
        }, OpenMode.ForRead, checkIdOk: true);

        // 修改:此处有缺陷:cad08会获取已经删除的块表记录,需要检查id.IsOk(),用ForEach代替
        // tr.BlockTable.Change("块表记录", btr => {
        // });

        // 修改:此处无缺陷
        tr.BlockTable.Change(tr.ModelSpace.ObjectId, modelSpace => { // 特性设置不进入函数体内
            var ents = modelSpace.GetEntities<Entity>();  // 此处不会检查id.IsOk()

            modelSpace.ForEach(id => {  // 利用遍历检查id.IsOk()
                if (id.IsOk())
                    id.Print();
            });
        });
    }


    // 更换了事务栈,可以更方便处理前台后台,而且可以判断路径再保存
    // 后台:不存在路径的dwg会在桌面进行临时保存
    [CommandMethod(nameof(FileNotExist))]
    public void FileNotExist()
    {
        if (!File.Exists(_file))
        {
            throw new FileNotFoundException("文件不存在", _file);
        }

        // 后台:不存在路径的dwg会在桌面进行临时保存
        using var tr = DBTrans.OpenPushToBackend(_file);
        tr.ModelSpace.AddCircle(new Point3d(10, 10, 0), 20);
        DatabaseEx.SaveDwgFile(tr.Database);
    }

    [CommandMethod(nameof(Test_DBTransAbort))]
    public void Test_DBTransAbort()
    {
        using var tr = DBTrans.Create();
        tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
        tr.Abort();
        // tr.Commit();
    }

    // AOP 应用 预计示例：
    // 1. 无参数
    // [AOP]
    // [CommandMethod(nameof(Test_AOP1))]
    // public void TestAOP1()
    // {
    //    // 不用 using var tr = DBTrans.Create();
    //    var tr = DBTrans.Top;
    //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
    // }

    // 2. 有参数
    // [AOP("file")]
    // [CommandMethod(nameof(Test_AOP2))]
    // public void TestAOP2()
    // {
    //    // 不用 using var tr = new DBTrans(file);
    //    var tr = DBTrans.Top;
    //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
    // }


    [CommandMethod(nameof(Test_TopTransaction))]
    public void Test_TopTransaction()
    {
        // var pt = Env.Editor.GetPoint("pick pt:").Value;
        // var pl = Env.Editor.GetEntity("pick pl").ObjectId;

        var tr1 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        using var tr2 = DBTrans.Create();
        var tr3 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr6 = Acap.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr2 == tr3);
        Env.Print(tr3 == tr6);
        using var tr4 = DBTrans.Create();
        var tr5 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr7 = Acap.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr4 == tr5);
        Env.Print(tr5 == tr7);
        var trm = HostApplicationServices.WorkingDatabase.TransactionManager;

        // var ptt = tr.GetObject<Polyline>(pl).GetClosestPointTo(pt,false);
        // var pt1 = new Point3d(0, 0.00000000000001, 0);
        // var pt2 = new Point3d(0, 0.00001, 0);
        // Env.Print(Tolerance.Global.EqualPoint);
        // Env.Print(pt1.IsEqualTo(pt2).ToString());
        // Env.Print(pt1.IsEqualTo(pt2,new Tolerance(0.0,1e-6)).ToString());
        // Env.Print((pt1 == pt2).ToString());
        // Env.Print((pt1 != pt2).ToString());
    }

    [CommandMethod(nameof(Test_DBTrans_BlockCount))]
    public void Test_DBTrans_BlockCount()
    {
        using var tr = DBTrans.Create();
        var i = tr.CurrentSpace
            .GetEntities<BlockReference>()
            .Where(ent => ent.GetBlockName() == "自定义块");

        var block = i.ToList()[0];
        Env.Print(i.Count());
    }
}