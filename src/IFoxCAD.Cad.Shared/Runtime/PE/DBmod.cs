namespace IFoxCAD.Cad;

/// <summary>
/// 获取数据库修改状态
/// <a href="https://knowledge.autodesk.com/support/autocad/learn-explore/caas/CloudHelp/cloudhelp/2020/ENU/AutoCAD-Core/files/GUID-E255E808-2D48-4BDE-A760-FFEA28E5A86F-htm.html">
/// 相关链接</a>
/// </summary>
[Flags]
public enum DBmod : byte
{
    [Description("数据库冇修改")]
    DatabaseNoModifies = 0,
    [Description("数据库有修改")]
    Database = 1,
    [Description("变量有修改")]
    Value = 4,
    [Description("窗口有修改")]
    Window = 8,
    [Description("视图有修改")]
    View = 16,
    [Description("字段有修改")]
    Field = 32
}

public class DBmodEx
{
    public static DBmod DBmod => (DBmod)byte.Parse(Env.GetVar("dbmod").ToString());

    delegate long DelegateAcdbSetDbmod(IntPtr db, DBmod newValue);
    static DelegateAcdbSetDbmod? _AcdbSetDbmod;
    public static long AcdbSetDbmod(IntPtr db, DBmod newValue)
    {
        if (_AcdbSetDbmod is null)
        {
            string str = "acdbSetDbmod";
            _AcdbSetDbmod =
                AcadPeInfo.GetDelegate<DelegateAcdbSetDbmod>(str, AcadPeEnum.Acdb);
        }
        if (_AcdbSetDbmod is null)
            return -1;
        return _AcdbSetDbmod.Invoke(db, newValue);// 调用方法
    }

    /// <summary>
    /// Dbmod 不被修改的任务
    /// </summary>
    /// <param name="action"></param>
    public static void DbmodReductionTask(Action action)
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;
        var doc = dm.MdiActiveDocument;
        if (doc is null)
            return;
        var db = doc.Database;
        var old = DBmod;
        action.Invoke();
        if (old == DBmod.DatabaseNoModifies && DBmod != DBmod.DatabaseNoModifies)
            AcdbSetDbmod(db.UnmanagedObject, DBmod.DatabaseNoModifies);
    }

    //[CommandMethod(nameof(TestCmd_AcdbSetDbmodChange))]
    //public void TestCmd_AcdbSetDbmodChange()
    //{
    //    DbmodReductionTask(() => {
    //        using DBTrans tr = new();
    //        Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
    //        tr.CurrentSpace.AddEntity(line);
    //    });
    //}
}