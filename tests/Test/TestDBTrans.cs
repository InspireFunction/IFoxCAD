namespace Test;

public class TestTrans
{
    [CommandMethod("testtr")]
    public void Testtr()
    {
        string filename = @"C:\Users\vic\Desktop\test.dwg";
        using var tr = new DBTrans(filename);
        tr.ModelSpace.AddCircle(new Point3d(10, 10, 0), 20);
        //tr.Database.SaveAs(filename,DwgVersion.Current);
        tr.SaveDwgFile();
    }
    [CommandMethod("testifoxcommit")]
    public void Testifoxcommit()
    {
        
        using var tr = new DBTrans();
        tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
        tr.Abort();
        //tr.Commit();
    }

    // AOP 应用 预计示例：
    // 1. 无参数
    //[AOP]
    //[CommandMethod("TESTAOP")]
    //public void testaop()
    //{
    //    // 不用 using var tr = new DBTrans();
    //    var tr = DBTrans.Top;
    //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
    //}

    // 2. 有参数
    //[AOP("file")]
    //[CommandMethod("TESTAOP")]
    //public void testaop()
    //{
    //    // 不用 using var tr = new DBTrans(file);
    //    var tr = DBTrans.Top;
    //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
    //}


    [CommandMethod("testpt")]
    public void TestPt()
    {
        //var pt = Env.Editor.GetPoint("pick pt:").Value;
        //var pl = Env.Editor.GetEntity("pick pl").ObjectId;

        var tr1 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        using var tr2 = new DBTrans();
        var tr3 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr6 = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr2.Transaction == tr3);
        Env.Print(tr3 == tr6);
        using var tr4 = new DBTrans();
        var tr5 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr7 = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr4.Transaction == tr5);
        Env.Print(tr5 == tr7);
        var trm = HostApplicationServices.WorkingDatabase.TransactionManager;

        //var ptt = tr.GetObject<Polyline>(pl).GetClosestPointTo(pt,false);
        //var pt1 = new Point3d(0, 0.00000000000001, 0);
        //var pt2 = new Point3d(0, 0.00001, 0);
        //Env.Print(Tolerance.Global.EqualPoint);
        //Env.Print(pt1.IsEqualTo(pt2).ToString());
        //Env.Print(pt1.IsEqualTo(pt2,new Tolerance(0.0,1e-6)).ToString());
        //Env.Print((pt1 == pt2).ToString());
        //Env.Print((pt1 != pt2).ToString());



    }

}
