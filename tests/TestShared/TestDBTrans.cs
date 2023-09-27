namespace Test;

public class TestTrans
{
    [CommandMethod(nameof(Test_DBTrans))]
    public void Test_DBTrans()
    {
        using DBTrans tr = new();
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
    
    
    
    
    [CommandMethod(nameof(CmdTest_DBTransActiveOpenDwg), CommandFlags.Session)]
    public static void CmdTest_DBTransActiveOpenDwg()
    {
        using DBTrans tr = new(@"D:\桌面\AA.dwg", activeOpen: true);
    }

    [CommandMethod(nameof(CmdTest_ForEachDemo))]
    public static void CmdTest_ForEachDemo()
    {
        using DBTrans tr = new();

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



    // 后台:不存在路径的dwg会在桌面进行临时保存
    [CommandMethod(nameof(FileNotExist))]
    public void FileNotExist()
    {
        using DBTrans tr = new("test.dwg");
        tr.Database.SaveFile((DwgVersion)24, false);
    }

    // 前台:由于是弹出面板,此时路径不会起任何作用
    [CommandMethod(nameof(FileNotExist2))]
    public void FileNotExist2()
    {
        using DBTrans tr = new();
        tr.Database.SaveFile(saveAsFile: "D:\\");
    }

    // 后台:只有路径,没有文件名
    [CommandMethod(nameof(FileNotExist3))]
    public void FileNotExist3()
    {
        using DBTrans tr = new("D:\\");
        tr.Database.SaveDwgFile();

        using DBTrans tr2 = new("D:\\");
        tr2.Database.SaveFile(saveAsFile: "D:\\");
    }


    [CommandMethod(nameof(Test_SaveDwgFile))]
    public void Test_SaveDwgFile()
    {
        string filename = @"C:\Users\vic\Desktop\test.dwg";
        using DBTrans tr = new(filename);
        var circle = CircleEx.CreateCircle(new Point3d(10, 10, 0), 20)!;
        tr.ModelSpace.AddEntity(circle);
        // tr.Database.SaveAs(filename,DwgVersion.Current);
        tr.Database.SaveDwgFile();
    }
    [CommandMethod(nameof(Test_DBTransAbort))]
    public void Test_DBTransAbort()
    {
        using DBTrans tr = new();
        var circle = CircleEx.CreateCircle(new Point3d(10, 10, 0), 20)!;
        tr.ModelSpace.AddEntity(circle);
        tr.Abort();
        // tr.Commit();
    }

    // AOP 应用 预计示例：
    // 1. 无参数
    // [AOP]
    // [CommandMethod(nameof(Test_AOP1))]
    // public void TestAOP1()
    // {
    //    // 不用 using DBTrans tr = new();
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
        using DBTrans tr2 = new();
        var tr3 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr6 = Acaop.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr2.Transaction == tr3);
        Env.Print(tr3 == tr6);
        using DBTrans tr4 = new();
        var tr5 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr7 = Acaop.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr4.Transaction == tr5);
        Env.Print(tr5 == tr7);
        var trm = HostApplicationServices.WorkingDatabase.TransactionManager;

    }

    [CommandMethod(nameof(Test_DBTrans_BlockCount))]
    public void Test_DBTrans_BlockCount()
    {
        using var tr = new DBTrans();
        var i = tr.CurrentSpace
            .GetEntities<BlockReference>()
            .Where(ent => ent.GetBlockName() == "自定义块");
            
        var block = i.ToList()[0];
        Env.Print(i.Count());
    }

    
}