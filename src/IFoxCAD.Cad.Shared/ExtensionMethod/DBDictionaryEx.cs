namespace IFoxCAD.Cad;

/// <summary>
/// 字典扩展类
/// </summary>
public static class DBDictionaryEx
{
    /// <summary>
    /// 获取字典里的全部对象
    /// </summary>
    /// <typeparam name="T">对象类型的泛型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="trans">事务</param>
    /// <returns>对象迭代器</returns>
    [System.Diagnostics.DebuggerStepThrough]
    public static IEnumerable<T> GetAllObjects<T>(this DBDictionary dict, DBTrans? trans = null) where T : DBObject
    {
        trans ??= DBTrans.Top;
        foreach (DBDictionaryEntry e in dict)
        {
            var ent = trans.GetObject<T>(e.Value);
            if (ent is not null)
                yield return ent;
        }
    }

    /// <summary>
    /// 获取字典内指定key的对象
    /// </summary>
    /// <typeparam name="T">对象类型的泛型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="trans">事务</param>
    /// <param name="key">指定的键值</param>
    /// <returns>T 类型的对象</returns>
    public static T? GetAt<T>(this DBDictionary dict, string key, DBTrans? trans = null) where T : DBObject
    {
        trans ??= DBTrans.Top;
        if (dict.Contains(key))
        {
            ObjectId id = dict.GetAt(key);
            if (!id.IsNull)
                return trans.GetObject<T>(id);
        }
        return null;
    }

    /// <summary>
    /// 添加条目（键值对）到字典
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="trans">事务</param>
    /// <param name="key">键</param>
    /// <param name="newValue">值</param>
    public static void SetAt<T>(this DBDictionary dict, string key, T newValue, Transaction? trans = null) where T : DBObject
    {
        trans ??= DBTrans.Top.Transaction;
        using (dict.ForWrite())
        {
            dict.SetAt(key, newValue);
            trans.AddNewlyCreatedDBObject(newValue, true);
        }
    }

    #region XRecord

    /// <summary>
    /// 从字典中获取扩展数据
    /// </summary>
    /// <param name="dict">字典</param>
    /// <param name="key">键值</param>
    /// <returns>扩展数据</returns>
    public static XRecordDataList? GetXRecord(this DBDictionary dict, string key)
    {
        var rec = dict.GetAt<Xrecord>(key);
        if (rec is null)
            return null;
        return rec.Data;
    }

    /// <summary>
    /// 保存扩展数据到字典
    /// </summary>
    /// <param name="rb">扩展数据</param>
    /// <param name="dict">字典</param>
    /// <param name="key">键值</param>
    public static void SetXRecord(this DBDictionary dict, string key, XRecordDataList rb)
    {
        using var newValue = new Xrecord { Data = rb };
        dict.SetAt<Xrecord>(key, newValue);
    }
    #endregion

    /// <summary>
    /// 获取扩展字典
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="trans">事务</param>
    /// <returns>扩展字典对象</returns>
    public static DBDictionary? GetXDictionary(this DBObject obj, DBTrans? trans = null)
    {
        trans ??= DBTrans.Top;
        ObjectId id = obj.ExtensionDictionary;
        if (id.IsNull)
        {
            using (obj.ForWrite())
                obj.CreateExtensionDictionary();

            id = obj.ExtensionDictionary;
        }
        return id.GetObject<DBDictionary>(trans: trans);
    }

    #region 数据表

    /// <summary>
    /// 创建数据表
    /// </summary>
    /// <param name="colTypes">原数据类型的字典</param>
    /// <param name="content">表元素（二维数组）</param>
    /// <returns>数据表</returns>
    public static DataTable CreateDataTable(Dictionary<string, CellType> colTypes, object[,] content)
    {
        DataTable table = new();
        foreach (var t in colTypes)
            table.AppendColumn(t.Value, t.Key);

        var ncol = colTypes.Count;
        var types = new CellType[ncol];
        colTypes.Values.CopyTo(types, 0);

        var nrow = content.GetLength(0);
        for (int i = 0; i < nrow; i++)
        {
            DataCellCollection row = new();
            for (int j = 0; j < ncol; j++)
            {
                var cell = new DataCell();
                cell.SetValue(types[j], content[i, j]);
                row.Add(cell);
            }
            table.AppendRow(row, true);
        }
        return table;
    }

    /// <summary>
    /// 设定单元格数据
    /// </summary>
    /// <param name="cell">单元格</param>
    /// <param name="type">类型</param>
    /// <param name="value">数据</param>
    public static void SetValue(this DataCell cell, CellType type, object value)
    {
        switch (type)
        {
            case CellType.Bool:
            cell.SetBool((bool)value);
            break;

            case CellType.CharPtr:
            cell.SetString((string)value);
            break;

            case CellType.Integer:
            cell.SetInteger((int)value);
            break;

            case CellType.Double:
            cell.SetDouble((double)value);
            break;

            case CellType.ObjectId:
            cell.SetObjectId((ObjectId)value);
            break;

            case CellType.Point:
            cell.SetPoint((Point3d)value);
            break;

            case CellType.Vector:
            cell.SetVector((Vector3d)value);
            break;

            case CellType.HardOwnerId:
            cell.SetHardOwnershipId((ObjectId)value);
            break;

            case CellType.HardPtrId:
            cell.SetHardPointerId((ObjectId)value);
            break;

            case CellType.SoftOwnerId:
            cell.SetSoftOwnershipId((ObjectId)value);
            break;

            case CellType.SoftPtrId:
            cell.SetSoftPointerId((ObjectId)value);
            break;
        }
    }
    #endregion

    #region 子字典
    /// <summary>
    /// 获取子字典
    /// </summary>
    /// <param name="dict">根字典</param>
    /// <param name="trans">事务</param>
    /// <param name="createSubDictionary">是否创建子字典</param>
    /// <param name="dictNames">键值列表</param>
    /// <returns>字典</returns>
    public static DBDictionary? GetSubDictionary(this DBDictionary dict,
        bool createSubDictionary,
        IEnumerable<string> dictNames,
        DBTrans? trans = null)
    {
        DBDictionary? newdict = null;
        trans ??= DBTrans.Top;
        if (createSubDictionary)
        {
            using (dict.ForWrite())
                dict.TreatElementsAsHard = true;

            foreach (string name in dictNames)
            {
                if (dict.Contains(name))
                {
                    newdict = dict.GetAt<DBDictionary>(name, trans);
                }
                else
                {
                    DBDictionary subDict = new();
                    dict.SetAt(name, subDict, trans);
                    newdict = subDict;
                    newdict.TreatElementsAsHard = true;
                }
            }
        }
        else
        {
            foreach (string name in dictNames)
            {
                if (dict.Contains(name))
                    newdict = dict.GetAt<DBDictionary>(name, trans);
                else
                    return null;
            }
        }
        return newdict;
    }


    ///// <summary>
    ///// 获取对象扩展字典的子字典
    ///// </summary>
    ///// <param name="obj">对象</param>
    ///// <param name="tr">事务</param>
    ///// <param name="createSubDictionary">是否创建子字典</param>
    ///// <param name="dictNames">键值列表</param>
    ///// <returns>字典</returns>
    // public static DBDictionary GetSubDictionary(this DBObject obj, bool createSubDictionary, params string[] dictNames)
    // {
    //    return obj.GetXDictionary().GetSubDictionary(createSubDictionary, dictNames);
    // }

    #endregion

    #region 组字典
    /// <summary>
    /// 添加编组
    /// </summary>
    /// <param name="name">组名</param>
    /// <param name="ids">实体Id集合</param>
    /// <returns>编组Id</returns>
    public static ObjectId AddGroup(this DBDictionary dict, string name, ObjectIdCollection ids)
    {
        if (dict.Contains(name))
            return ObjectId.Null;

        using (dict.ForWrite())
        {
            Group g = new();
            g.Append(ids);
            dict.SetAt(name, g);
            DBTrans.Top.Transaction.AddNewlyCreatedDBObject(g, true);
            return g.ObjectId;
        }
    }

    /// <summary>
    /// 添加编组
    /// </summary>
    /// <param name="name">组名</param>
    /// <param name="ids">实体Id集合</param>
    /// <returns>编组Id</returns>
    public static ObjectId AddGroup(this DBDictionary dict, string name, IEnumerable<ObjectId> ids)
    {
        if (dict.Contains(name))
            return ObjectId.Null;

        using ObjectIdCollection idc = new(ids.ToArray());//需要using吗? 暂无测试
        return dict.AddGroup(name, idc);
    }


    /// <summary>
    /// 按选择条件获取编组集合
    /// </summary>
    /// <param name="func">选择条件，过滤函数</param>
    /// <example><![CDATA[var groups = dict.GetGroups(g => g.NumEntities < 2);]]></example>
    /// <returns>编组集合</returns>
    public static IEnumerable<Group> GetGroups(this DBDictionary dict, Func<Group, bool> func)
    {
        return dict.GetAllObjects<Group>()
                   .Where(func);
    }

    /// <summary>
    /// 返回实体的所在编组的集合
    /// </summary>
    /// <param name="ent">图元实体</param>
    /// <returns>编组集合</returns>
    public static IEnumerable<Group> GetGroups(this Entity ent)
    {
        return ent.GetPersistentReactorIds()
                  .Cast<ObjectId>()
                  .Select(id => id.GetObject<Group>())
                  .OfType<Group>();
    }

    /// <summary>
    /// 移除所有的空组
    /// </summary>
    /// <returns>被移除编组的名称集合</returns>
    public static List<string> RemoveNullGroup(this DBDictionary dict)
    {
        var groups = dict.GetGroups(g => g.NumEntities < 2);
        List<string> names = new();
        foreach (Group g in groups)
        {
            names.Add(g.Name);
            using (g.ForWrite())
                g.Erase();
        }
        return names;
    }

    /// <summary>
    /// 移除所有空组
    /// </summary>
    /// <param name="func">过滤条件，过滤要删除的组名的规则函数</param>
    /// <example>RemoveNullGroup(g => g.StartsWith("hah"))</example>
    /// <returns>被移除编组的名称集合</returns>
    public static List<string> RemoveNullGroup(this DBDictionary dict, Func<string, bool> func)
    {
        var groups = dict.GetGroups(g => g.NumEntities < 2);
        List<string> names = new();
        foreach (Group g in groups)
        {
            if (func(g.Name))
            {
                names.Add(g.Name);
                using (g.ForWrite())
                    g.Erase();
            }
        }
        return names;
    }
    #endregion
}