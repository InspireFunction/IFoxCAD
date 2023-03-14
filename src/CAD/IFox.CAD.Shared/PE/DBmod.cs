﻿namespace IFoxCAD.Cad;

/// <summary>
/// 获取数据库修改状态
/// <a href="https://knowledge.autodesk.com/support/autocad/learn-explore/caas/CloudHelp/cloudhelp/2020/ENU/AutoCAD-Core/files/GUID-E255E808-2D48-4BDE-A760-FFEA28E5A86F-htm.html">
/// 相关链接</a>
/// </summary>
[Flags]
public enum DBmod : short
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
    public static DBmod DBmod => (DBmod)Env.GetVar("dbmod");

    delegate long DelegateAcdbSetDbmod(IntPtr db, DBmod newValue);
    static DelegateAcdbSetDbmod? acdbSetDbmod;//别改名称

    public static long AcdbSetDbmod(IntPtr db, DBmod newValue)
    {
        acdbSetDbmod ??= AcadPeInfo.GetDelegate<DelegateAcdbSetDbmod>(
                            nameof(acdbSetDbmod), AcadPeEnum.Acdb);
        if (acdbSetDbmod is null)
            return -1;
        return acdbSetDbmod.Invoke(db, newValue);// 调用方法
    }

    /// <summary>
    /// Dbmod 不被修改的任务
    /// </summary>
    /// <param name="action"></param>
    public static void DBmodTask(Action action)
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;
        var doc = dm.MdiActiveDocument;
        if (doc is null)
            return;

        var bak = DBmod;
        action.Invoke();
        if (bak == DBmod.DatabaseNoModifies && DBmod != DBmod.DatabaseNoModifies)
            AcdbSetDbmod(doc.Database.UnmanagedObject, DBmod.DatabaseNoModifies);
    }

    static bool _flag = true;
    /// <summary>
    /// 请在无法处理的初始化才使用它
    /// (源泉在初始化的时候进行了修改数据库,所以必须要用一个新线程等待lisp执行完成才可以)
    /// </summary>
    public static void DatabaseNoModifies()
    {
        if (_flag)// 仅执行一次,在初始化时候
        {
            var dm = Acap.DocumentManager;
            if (dm.Count == 0)
                return;
            var doc = dm.MdiActiveDocument;
            if (doc is null)
                return;

            if (DBmod != DBmod.DatabaseNoModifies)
                AcdbSetDbmod(doc.Database.UnmanagedObject, DBmod.DatabaseNoModifies);
            _flag = false;
        }
    }


    //[CommandMethod(nameof(TestCmd_AcdbSetDbmodChange))]
    //public void TestCmd_AcdbSetDbmodChange()
    //{
    //    DBmodTask(() => {
    //        using DBTrans tr = new();
    //        Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
    //        tr.CurrentSpace.AddEntity(line);
    //    });
    //}
}