﻿namespace IFoxCAD.Cad;

/// <summary>
/// 符号表记录扩展类
/// </summary>
public static class SymbolTableRecordEx
{
    #region 块表记录

    #region 克隆实体id
    /// <summary>
    /// 深度克隆id到块表记录
    /// <para>
    /// 0x01 此方法不允许是未添加数据库的图元,因此它是id<br/>
    /// 0x02 若为未添加数据库图元,则利用entity.Clone();同时不需要考虑动态块属性,可以使用entity.GetTransformedCopy
    /// </para>
    /// </summary>
    /// <param name="btr">
    /// <para>
    /// 克隆到当前块表记录,相当于原地克隆<br/>
    /// 克隆到目标块表记录内,相当于制作新块
    /// </para>
    /// </param>
    /// <param name="objIds">图元id集合,注意所有成员都要在同一个空间中</param>
    /// <param name="maoOut">返回克隆后的id词典</param>
    public static void DeepCloneEx(this BlockTableRecord btr, ObjectIdCollection objIds, IdMapping maoOut)
    {
        if (objIds is null || objIds.Count == 0)
            throw new ArgumentNullException(nameof(objIds));

        var db = objIds[0].Database;
        using (btr.ForWrite())
        {
            try
            {
                db.DeepCloneObjects(objIds, btr.ObjectId, maoOut, false);

                // 不在此提取,为了此函数被高频调用
                // 获取克隆键值对(旧块名,新块名)
                // foreach (ObjectId item in blockIds)
                //     result.Add(mapping[item].Value);
            }
            catch (System.Exception e)
            {
               
            }
        }
    }

    /// <summary>
    /// 克隆图元实体(这个函数有问题,会出现偶尔成功,偶尔失败,拖动过变成匿名块)
    /// <para>若为块则进行设置属性,因此控制动态块属性丢失;</para>
    /// </summary>
    /// <param name="ent">图元</param>
    /// <param name="matrix">矩阵</param>
    // public static void EntityTransformedCopy(this Entity ent, Matrix3d matrix)
    // {
    //    var entNew = ent.GetTransformedCopy(matrix);
    //    if (ent is BlockReference blockReference)
    //        entNew.SetPropertiesFrom(blockReference);
    // }

    #endregion

    #region 添加实体
    /// <summary>
    /// 添加实体对象
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="entity">实体</param>
    /// <returns>对象 id</returns>
    public static ObjectId AddEntity(this BlockTableRecord btr, Entity entity)
    {
        // if (entity is null)
        //    throw new ArgumentNullException(nameof(entity), "对象为 null");

        ObjectId id;
        var tr = DBTrans.GetTopTransaction(btr.Database);
        using (btr.ForWrite())
        {
            id = btr.AppendEntity(entity);
            tr.AddNewlyCreatedDBObject(entity, true);
        }

        return id;
    }

    /// <summary>
    /// 添加实体集合
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="btr">块表记录</param>
    /// <param name="ents">实体集合</param>
    /// <param name="trans">事务</param>
    /// <returns>对象 id 列表</returns>
    public static IEnumerable<ObjectId> AddEntity<T>(this BlockTableRecord btr, IEnumerable<T> ents) where T : Entity
    {
        // if (ents.Any(ent => ent is null))
        //    throw new ArgumentNullException(nameof(ents), "实体集合内存在 null 对象");

        var tr = DBTrans.GetTopTransaction(btr.Database);
        using (btr.ForWrite())
        {
            return ents.Select(ent => {
                ObjectId id = btr.AppendEntity(ent);
                tr.AddNewlyCreatedDBObject(ent, true);
                return id;
            }).ToList();
        }
    }

    /// <summary>
    /// 添加多个实体
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="ents">实体集合</param>
    /// <returns>对象 id 列表</returns>
    public static IEnumerable<ObjectId> AddEntity(this BlockTableRecord btr, params Entity[] ents)
    {
        return btr.AddEntity(ents.ToList());
    }
    #endregion

    #region 添加图元
    /// <summary>
    /// 在指定绘图空间添加图元
    /// </summary>
    /// <typeparam name="T">图元类型</typeparam>
    /// <param name="btr">绘图空间</param>
    /// <param name="ent">图元对象</param>
    /// <param name="action">图元属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>图元id</returns>
    private static ObjectId AddEnt<T>(this BlockTableRecord btr, T ent,
                                      Action<T>? action) where T : Entity
    {
        action?.Invoke(ent);
        return btr.AddEntity(ent);
    }
    /// <summary>
    /// 委托式的添加图元
    /// </summary>
    /// <param name="btr">块表</param>
    /// <param name="action">返回图元的委托</param>
    /// <param name="trans">事务</param>
    /// <returns>图元id，如果委托返回 null，则为 ObjectId.Null</returns>
    public static ObjectId AddEnt(this BlockTableRecord btr, Func<Entity> action, Transaction? trans = null)
    {
        var ent = action.Invoke();
        if (ent is null)
            return ObjectId.Null;

        return btr.AddEntity(ent);
    }

    /// <summary>
    /// 在指定绘图空间添加直线
    /// </summary>
    /// <param name="trans">事务管理器</param>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="btr">绘图空间</param>
    /// <param name="action">直线属性设置委托</param>
    /// <returns>直线的id</returns>
    public static ObjectId AddLine(this BlockTableRecord btr, Point3d start, Point3d end,
                                   Action<Line>? action = null)
    {
        var line = new Line(start, end);
        return btr.AddEnt(line, action);
    }
    /// <summary>
    /// 在指定绘图空间X-Y平面添加圆
    /// </summary>
    /// <param name="btr">绘图空间</param>
    /// <param name="center">圆心</param>
    /// <param name="radius">半径</param>
    /// <param name="action">圆属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>圆的id</returns>
    public static ObjectId AddCircle(this BlockTableRecord btr, Point3d center, double radius,
                                     Action<Circle>? action = null, Transaction? trans = null)
    {
        var circle = new Circle(center, Vector3d.ZAxis, radius);
        return btr.AddEnt(circle, action);
    }

    /// <summary>
    /// 在指定绘图空间X-Y平面3点画外接圆
    /// </summary>
    /// <param name="btr">绘图空间</param>
    /// <param name="p0">第一点</param>
    /// <param name="p1">第二点</param>
    /// <param name="p2">第三点</param>
    /// <param name="action">圆属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>三点有外接圆则返回圆的id，否则返回ObjectId.Null</returns>
    public static ObjectId AddCircle(this BlockTableRecord btr, Point3d p0, Point3d p1, Point3d p2,
                                     Action<Circle>? action = null)
    {
        var circle = CircleEx.CreateCircle(p0, p1, p2);
        // return circle is not null ? btr.AddEnt(circle, action, trans) : throw new ArgumentNullException(nameof(circle), "对象为 null");
        //if (circle is null)
        //    throw new ArgumentNullException(nameof(circle), "对象为 null");
        circle.NotNull(nameof(circle));
        return btr.AddEnt(circle, action);
    }
    /// <summary>
    /// 在指定的绘图空间添加轻多段线
    /// </summary>
    /// <param name="btr">绘图空间</param>
    /// <param name="bvws">多段线信息</param>
    /// <param name="constantWidth">线宽</param>
    /// <param name="isClosed">是否闭合</param>
    /// <param name="action">轻多段线属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>轻多段线id</returns>
    public static ObjectId AddPline(this BlockTableRecord btr,
                                    List<BulgeVertexWidth> bvws,
                                    double? constantWidth = null,
                                    bool isClosed = true,
                                    Action<Polyline>? action = null, Transaction? trans = null)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        if (constantWidth is not null)
        {
            for (int i = 0; i < bvws.Count; i++)
                pl.AddVertexAt(i, bvws[i].Vertex, bvws[i].Bulge, constantWidth.Value, constantWidth.Value);
        }
        else
        {
            for (int i = 0; i < bvws.Count; i++)
                pl.AddVertexAt(i, bvws[i].Vertex, bvws[i].Bulge, bvws[i].StartWidth, bvws[i].EndWidth);
        }
        pl.Closed = isClosed;// 闭合
        return btr.AddEnt(pl, action);
    }
    /// <summary>
    /// 在指定的绘图空间添加轻多段线
    /// </summary>
    /// <param name="btr">绘图空间</param>
    /// <param name="pts">端点表</param>
    /// <param name="bulges">凸度表</param>
    /// <param name="startWidths">端点的起始宽度</param>
    /// <param name="endWidths">端点的终止宽度</param>
    /// <param name="action">轻多段线属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>轻多段线id</returns>
    public static ObjectId AddPline(this BlockTableRecord btr,
                                    List<Point3d> pts,
                                    List<double>? bulges = null,
                                    List<double>? startWidths = null,
                                    List<double>? endWidths = null,
                                    Action<Polyline>? action = null,
                                    Transaction? trans = null)
    {
        bulges ??= new(new double[pts.Count]);
        startWidths ??= new(new double[pts.Count]);
        endWidths ??= new(new double[pts.Count]);

        Polyline pl = new();
        pl.SetDatabaseDefaults();

        for (int i = 0; i < pts.Count; i++)
            pl.AddVertexAt(i, pts[i].Point2d(), bulges[i], startWidths[i], endWidths[i]);
        return btr.AddEnt(pl, action);
    }

    /// <summary>
    /// 在指定的绘图空间添加轻多段线
    /// </summary>
    /// <param name="btr">绘图空间</param>
    /// <param name="pts">端点表,利用元组(Point3d pt, double bulge, double startWidth, double endWidth)</param>
    /// <param name="action">轻多段线属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>轻多段线id</returns>
    public static ObjectId AddPline(this BlockTableRecord btr,
                                    List<(Point3d pt, double bulge, double startWidth, double endWidth)> pts,
                                    Action<Polyline>? action = null,
                                    Transaction? trans = null)
    {
        Polyline pl = new();
        pl.SetDatabaseDefaults();

        pts.ForEach((vertex, state, index) => {
            pl.AddVertexAt(index, vertex.pt.Point2d(), vertex.bulge, vertex.startWidth, vertex.endWidth);
        });

        return btr.AddEnt(pl, action);
    }

    /// <summary>
    /// 在指定绘图空间X-Y平面3点画圆弧
    /// </summary>
    /// <param name="btr">绘图空间</param>
    /// <param name="startPoint">圆弧起点</param>
    /// <param name="pointOnArc">圆弧上的点</param>
    /// <param name="endPoint">圆弧终点</param>
    /// <param name="action">圆弧属性设置委托</param>
    /// <param name="trans">事务管理器</param>
    /// <returns>圆弧id</returns>
    public static ObjectId AddArc(this BlockTableRecord btr,
                                  Point3d startPoint, Point3d pointOnArc, Point3d endPoint,
                                  Action<Arc>? action = null, Transaction? trans = null)
    {
        var arc = ArcEx.CreateArc(startPoint, pointOnArc, endPoint);
        return btr.AddEnt(arc, action);
    }
    #endregion

    #region 获取实体/实体id
    /// <summary>
    /// 获取块表记录内的指定类型的实体
    /// (此处不会检查id.IsOk())
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="btr">块表记录</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="trans">事务</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>实体集合</returns>
    public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr,
                                                OpenMode openMode = OpenMode.ForRead,
                                                bool openErased = false,
                                                bool openLockedLayer = false) where T : Entity
    {
        return
            btr
            .Cast<ObjectId>()
            .Select(id => id.GetObject<T>(openMode, openErased, openLockedLayer))
            .OfType<T>();
    }

    /// <summary>
    /// 按类型获取实体Id
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="btr">块表记录</param>
    /// <returns>实体Id集合</returns>
    public static IEnumerable<ObjectId> GetObjectIds<T>(this BlockTableRecord btr) where T : Entity
    {
        string dxfName = RXClass.GetClass(typeof(T)).DxfName;
        return btr.Cast<ObjectId>()
                  .Where(id => id.ObjectClass.DxfName == dxfName);
    }

    /// <summary>
    /// 按类型获取实体Id的分组
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <returns>实体Id分组</returns>
    public static IEnumerable<IGrouping<string, ObjectId>> GetObjectIds(this BlockTableRecord btr)
    {
        return btr.Cast<ObjectId>()
                  .GroupBy(id => id.ObjectClass.DxfName);
    }


    /// <summary>
    /// 获取绘制顺序表
    /// </summary>
    /// <param name="btr">块表</param>
    /// <param name="trans">事务</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>绘制顺序表</returns>
    public static DrawOrderTable GetDrawOrderTable(this BlockTableRecord btr,
                                                    Transaction? trans = null,
                                                    bool openErased = false,
                                                    bool openLockedLayer = false)
    {
        var tr = DBTrans.GetTopTransaction(btr.Database);
        return tr.GetObject<DrawOrderTable>(btr.DrawOrderTableId, OpenMode.ForRead, openErased, openLockedLayer);
    }
    #endregion

    #region 插入块参照
    /// <summary>
    /// 插入块参照
    /// </summary>
    /// <param name="blockTableRecord">块表记录</param>
    /// <param name="position">插入点</param>
    /// <param name="blockName">块名</param>
    /// <param name="scale">块插入比例，默认为1</param>
    /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
    /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
    /// <param name="trans">事务</param>
    /// <returns>块参照对象id</returns>
    public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord, Point3d position,
                                       string blockName,
                                       Scale3d scale = default,
                                       double rotation = default,
                                       Dictionary<string, string>? atts = null)
    {
        var tr = DBTrans.GetTop(blockTableRecord.Database);
        if (!tr.BlockTable.Has(blockName))
        {
            tr.Editor?.WriteMessage($"\n不存在名字为{blockName}的块定义。");
            return ObjectId.Null;
        }
        return blockTableRecord.InsertBlock(position, tr.BlockTable[blockName], scale, rotation, atts);
    }
    /// <summary>
    /// 插入块参照
    /// </summary>
    /// <param name="position">插入点</param>
    /// <param name="blockId">块定义id</param>
    /// <param name="scale">块插入比例，默认为1</param>
    /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
    /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
    /// <returns>块参照对象id</returns>
    public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord,
                                       Point3d position,
                                       ObjectId blockId,
                                       Scale3d scale = default,
                                       double rotation = default,
                                       Dictionary<string, string>? atts = null)
    {
        //trans ??= DBTrans.Top.Transaction;
        var tr = DBTrans.GetTop(blockTableRecord.Database);
        
        if (!tr.BlockTable.Has(blockId))
        {
            tr.Editor?.WriteMessage($"\n不存在块定义。");
            return ObjectId.Null;
        }
            

        using var blockref = new BlockReference(position, blockId)
        {
            ScaleFactors = scale,
            Rotation = rotation
        };
        var objid = blockTableRecord.AddEntity(blockref);

        if (atts != null)
        {
            var btr = tr.GetObject<BlockTableRecord>(blockref.BlockTableRecord)!;
            if (btr.HasAttributeDefinitions)
            {
                var attdefs = btr.GetEntities<AttributeDefinition>();
                foreach (var attdef in attdefs)
                {
                    using AttributeReference attref = new();
                    attref.SetDatabaseDefaults();
                    attref.SetAttributeFromBlock(attdef, blockref.BlockTransform);
                    attref.Position = attdef.Position.TransformBy(blockref.BlockTransform);
                    attref.AdjustAlignment(tr.Database);

                    if (atts.ContainsKey(attdef.Tag))
                        attref.TextString = atts[attdef.Tag];

                    blockref.AttributeCollection.AppendAttribute(attref);
                    tr.Transaction.AddNewlyCreatedDBObject(attref, true);
                }
            }
        }
        return objid;
    }
    #endregion

    #endregion

    #region 遍历
#line hidden // 调试的时候跳过它
    /// <summary>
    /// 遍历符号表记录,执行委托
    /// </summary>
    /// <param name="task">要运行的委托</param>
    public static void ForEach<TRecord>(this TRecord record, Action<ObjectId> task)
          where TRecord : SymbolTableRecord, IEnumerable
    {
        foreach (ObjectId id in record)
            task.Invoke(id);
    }

    /// <summary>
    /// 遍历符号表记录,执行委托(允许循环中断)
    /// </summary>
    /// <param name="task">要执行的委托</param>
    public static void ForEach<TRecord>(this TRecord record, Action<ObjectId, LoopState> task)
          where TRecord : SymbolTableRecord, IEnumerable
    {
        LoopState state = new();/*这种方式比Action改Func更友好*/
        foreach (ObjectId id in record)
        {
            task.Invoke(id, state);
            if (!state.IsRun)
                break;
        }
    }

    /// <summary>
    /// 遍历符号表记录,执行委托(允许循环中断,输出索引值)
    /// </summary>
    /// <param name="task">要执行的委托</param>
    [System.Diagnostics.DebuggerStepThrough]
    public static void ForEach<TRecord>(this TRecord record, Action<ObjectId, LoopState, int> task)
        where TRecord : SymbolTableRecord, IEnumerable
    {
        //if (task == null)
        //    throw new ArgumentNullException(nameof(task));
        task.NotNull(nameof(task));
        int i = 0;
        LoopState state = new();/*这种方式比Action改Func更友好*/
        foreach (ObjectId id in record)
        {
            task.Invoke(id, state, i);
            if (!state.IsRun)
                break;
            i++;
        }
    }
#line default
    #endregion
}