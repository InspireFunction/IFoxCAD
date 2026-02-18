using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test;

public class UndoMarker
{
    /// <summary>
    /// 回滚标记名称
    /// </summary>
    public const string MainNameUndo = "RefEdit_Undo";
    /// <summary>
    /// 回滚字典
    /// </summary>
    private static ObjectId _undoMarkNod = ObjectId.Null;

    public static void Init()
    {
        // 无撤事务
        // 初始化官方的undoMarker点,
        // 才可以用可撤事务进行回滚捕获
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            var db = tr.Database;
            var nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

            // 注册扩展数据应用程序名
            var regAppTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!regAppTable.Has(MainNameUndo))
            {
                regAppTable.UpgradeOpen();
                var regAppRecord = new RegAppTableRecord();
                regAppRecord.Name = MainNameUndo;
                regAppTable.Add(regAppRecord);
                tr.AddNewlyCreatedDBObject(regAppRecord, true);
            }

            if (nod.Contains(MainNameUndo))
            {
                _undoMarkNod = nod.GetAt(MainNameUndo);
            }
            else
            {
                var d = new DBDictionary();
                nod.SetAt(MainNameUndo, d);
                tr.AddNewlyCreatedDBObject(d, true);
                _undoMarkNod = d.ObjectId;
            }
        }
    }


    public static int UndoMarkNodRead(DBDictionary markerDict)
    {
        var arr = markerDict.XData?.AsArray();
        if (arr is null)
            return -1;

        int index = -1;
        if (arr.Length >= 2 &&
            arr[0].TypeCode == 1001 &&
            arr[0].Value.ToString() == UndoMarker.MainNameUndo &&
            arr[1].TypeCode == (int)DxfCode.ExtendedDataInteger32)
        {
            index = (int)arr[1].Value;
        }
        return index;
    }


    /// <summary>
    /// 可撤事务制造回滚点
    /// </summary>
    /// <param name="newIndex"></param>
    public static void UndoMarkNodWrite(int newIndex)
    {
        // 可撤事务
        // 撤销时会触发 ObjectUnappended 事件,我们就能捕获到,然后从而知道当前撤销到了哪个索引了.
        using (var tr = DBTrans.Create())
        {
            var worksetDictObj = (DBDictionary)tr.GetObject(_undoMarkNod, OpenMode.ForWrite);

            // 可撤事务,字典套字典存储索引值
            // undo 时会触发 ObjectUnappended 事件，我们就能捕获到
            var indexDict = new DBDictionary();
            // XData需要以1001开头的扩展数据，注册应用名
            indexDict.XData = new ResultBuffer(
                new TypedValue(1001, MainNameUndo),
                new TypedValue((int)DxfCode.ExtendedDataInteger32, newIndex)
            );

            worksetDictObj.SetAt($"Index_{newIndex}", indexDict);
            tr.AddNewlyCreatedDBObject(indexDict, true);
        }
    }
}
