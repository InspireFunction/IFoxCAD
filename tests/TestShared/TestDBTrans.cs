namespace Test;

public class TestTrans
{
    // 后台:不存在路径的dwg会在桌面进行临时保存
    [CommandMethod(nameof(FileNotExist))]
    public void FileNotExist()
    {
        using DBTrans tr = new("test.dwg");
        tr.SaveFile((DwgVersion)24, false);
    }

    // 前台:由于是弹出面板,此时路径不会起任何作用
    [CommandMethod(nameof(FileNotExist2))]
    public void FileNotExist2()
    {
        using DBTrans tr = new();
        tr.SaveFile(saveAsFile: "D:\\");
    }

    // 后台:只有路径,没有文件名
    [CommandMethod(nameof(FileNotExist3))]
    public void FileNotExist3()
    {
        using DBTrans tr = new("D:\\");
        tr.SaveDwgFile();

        using DBTrans tr2 = new("D:\\");
        tr2.SaveFile(saveAsFile: "D:\\");
    }


    [CommandMethod("testtr")]
    public void Testtr()
    {
        string filename = @"C:\Users\vic\Desktop\test.dwg";
        using DBTrans tr = new(filename);
        tr.ModelSpace.AddCircle(new Point3d(10, 10, 0), 20);
        // tr.Database.SaveAs(filename,DwgVersion.Current);
        tr.SaveDwgFile();
    }
    [CommandMethod("testifoxcommit")]
    public void Testifoxcommit()
    {
        using DBTrans tr = new();
        tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
        tr.Abort();
        // tr.Commit();
    }

    // AOP 应用 预计示例：
    // 1. 无参数
    // [AOP]
    // [CommandMethod("TESTAOP")]
    // public void testaop()
    // {
    //    // 不用 using DBTrans tr = new();
    //    var tr = DBTrans.Top;
    //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
    // }

    // 2. 有参数
    // [AOP("file")]
    // [CommandMethod("TESTAOP")]
    // public void testaop()
    // {
    //    // 不用 using var tr = new DBTrans(file);
    //    var tr = DBTrans.Top;
    //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
    // }


    [CommandMethod("testpt")]
    public void TestPt()
    {
        // var pt = Env.Editor.GetPoint("pick pt:").Value;
        // var pl = Env.Editor.GetEntity("pick pl").ObjectId;

        var tr1 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        using DBTrans tr2 = new();
        var tr3 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr6 = Acap.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr2.Transaction == tr3);
        Env.Print(tr3 == tr6);
        using DBTrans tr4 = new();
        var tr5 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr7 = Acap.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr4.Transaction == tr5);
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
}