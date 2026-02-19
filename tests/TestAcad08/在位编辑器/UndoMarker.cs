﻿using System;
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
        using var tr = DBTrans.Create(openCloseTrans: true);
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


    /// <summary>
    /// 可撤事务制造回滚点
    /// </summary>
    /// <param name="newIndex"></param>
    /// <param name="tr">删除事件直接使用传递过来的事务,避免双开事务</param>
    public static void UndoMarkNodWrite(int newIndex, Transaction? tr = null)
    {
        // 可撤事务
        // 撤销时会触发 ObjectUnappended 事件,我们就能捕获到,然后从而知道当前撤销到了哪个索引了.
        bool dis = false;
        if (tr is null)
        {
            tr = DBTrans.Create();
            dis = true;
        }

        var worksetDictObj = (DBDictionary)tr.GetObject(_undoMarkNod, OpenMode.ForWrite, true, true);

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

        if (dis)
        {
            tr.Commit();
        }
    }


    /// <summary>
    /// 读取撤回点
    /// </summary>
    /// <param name="markerDict"></param>
    /// <returns></returns>
    public static int UndoMarkNodRead(DBDictionary markerDict)
    {
        var arr = markerDict.XData?.AsArray();
        if (arr is null)
            return -1;

        int index = -1;
        if (arr.Length >= 2 &&
            arr[0].TypeCode == 1001 &&
            arr[0].Value.ToString() == MainNameUndo &&
            arr[1].TypeCode == (int)DxfCode.ExtendedDataInteger32)
        {
            index = (int)arr[1].Value;
        }
        return index;
    }


    /// <summary>
    /// 清理回滚标记字典
    /// </summary>
    /// <param name="doc">文档</param>
    public static void Clear(Document doc)
    {
        if (_undoMarkNod == ObjectId.Null)
            return;
        var db = doc.Database;
        using var tr = DBTrans.Create();
        var nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite, true, true);
        if (nod.Contains(MainNameUndo))
        {
            // 删除回滚标记字典
            var markerDict = (DBDictionary)tr.GetObject(_undoMarkNod, OpenMode.ForWrite, true, true);
            // 清空字典中的所有条目
            var entriesToRemove = new HashSet<string>();
            foreach (var entry in markerDict)
            {
                entriesToRemove.Add(entry.Key);
            }
            foreach (var key in entriesToRemove)
            {
                markerDict.Remove(key);
            }
            // 从命名对象字典中移除
            nod.Remove(MainNameUndo);
        }

        // 重置静态字段
        _undoMarkNod = ObjectId.Null;
    }
}
