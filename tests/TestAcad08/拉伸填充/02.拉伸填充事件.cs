using System.Collections.ObjectModel;

namespace JoinBoxAcad;

public class HatchPick : IDisposable
{
    #region 静态成员
    /// <summary>
    /// 状态
    /// </summary>
    public static ProState State = new();

    /// <summary>
    /// 容差
    /// </summary>
    private static Tolerance Tol = new(1e-6, 1e-6);

    public static void Start()
    {
        State.Start();
    }

    public static void Stop()
    {
        State.Stop();
    }
    #endregion

    #region 动态成员
    /// <summary>
    /// 临时标记(重设选择集会触发一次选择集反应器)
    /// </summary>
    bool _selectChangedStop = false;

    /// <summary>
    /// 鼠标夹点在边界上面 == true
    /// </summary>
    bool _pickInBo = false;

    /// <summary>
    /// 填充id,边界转换器
    /// </summary>
    public readonly Dictionary<ObjectId, HatchConverter> HatchConvMap = [];

    readonly Document _doc;

    public HatchPick(Document doc)
    {
        _doc = doc;
        LoadHelper(true);
    }

    void LoadHelper(bool isLoad)
    {
        if (isLoad)
        {
            _doc.ImpliedSelectionChanged += Md_ImpliedSelectionChanged;
            _doc.CommandWillStart += Md_CommandWillStart;
            _doc.LispWillStart += Md_LispWillStart;
            _doc.CommandEnded += Md_CommandEnded;

            _doc.Database.ObjectErased += DB_ObjectErased;
            _doc.Database.ObjectModified += DB_ObjectModified;
        }
        else
        {
            _doc.ImpliedSelectionChanged -= Md_ImpliedSelectionChanged;
            _doc.CommandWillStart -= Md_CommandWillStart;
            _doc.LispWillStart -= Md_LispWillStart;
            _doc.CommandEnded -= Md_CommandEnded;
            _doc.Database.ObjectErased -= DB_ObjectErased;
            _doc.Database.ObjectModified -= DB_ObjectModified;
        }
    }
    #endregion

    #region 事件
    /// <summary>
    /// 反应器->command命令执行前
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Md_CommandWillStart(object sender, CommandEventArgs e)
    {
        if (!State.IsRun)
            return;

        // 无法使用文档锁,
        // 否则将导致文档锁无法释放,然后ctrl+z失败
        var cmdup = e.GlobalCommandName.ToUpper();
        DebugEx.Printl("Md_CommandWillStart::" + cmdup);

        // 拉伸夹点命令前触发
        if (cmdup != "GRIP_STRETCH")
        {
            EraseAllHatchBorders();
        }
        else
        {
            try
            {
                var screenPos = System.Windows.Forms.Control.MousePosition;
                var mouseStart = Screen.ScreenToCad(screenPos);

                // 获取当前选择的对象,然后提取所有的夹点
                var prompt = Env.Editor.SelectImplied();
                if (prompt.Status != PromptStatus.OK)
                    return;

                using DBTrans tr = new(docLock: true);
                var _hatchIds = prompt.Value.GetObjectIds();
                if (_hatchIds.Length == 0)
                    return;

                var tol = (double)Env.GetVar("viewsize") / 10;
                DebugEx.Printl("tol::" + tol);

                // 0x01 移动了矩形填充中间的夹点,删除边界,并且重新生成填充和边界
                // 0x02 移动了填充边界上的夹点,不处理,然后它会通过关联进行自己修改
                _pickInBo = false;

                foreach (var hatId in _hatchIds)
                {
                    if (!HatchConvMap.TryGetValue(hatId, out var hc))
                        continue;

                    var idss = hc.BoundaryIds ?? hc.BoundaryNewlyIds;

                    idss?.ForEach((id, idState) => {
                        PickBo(id, mouseStart, tr, tol);
                    });

                    // 点在边界上:就不处理了,它会通过cad的关联填充反应器自动修改
                    if (_pickInBo)
                        DebugEx.Printl("夹点在边界上");
                    else
                        DebugEx.Printl("夹点不在边界上");
                }
            }
            catch (Exception ex)
            {
                DebugEx.Printl("Md_CommandWillStart error: " + ex.Message);
                _pickInBo = false;
            }
        }
    }


    private void PickBo(ObjectId id, Point3d mouseStart, DBTrans tr, double tol)
    {
        if (!id.IsOk())
            return;
        using var boEnt = (Entity)tr.GetObject(id, OpenMode.ForRead);

        // 获取夹点在哪个图元边界上
        HashSet<Point3d> boPts = [];
        if (boEnt is Circle circle)
        {
            // 圆形的边界夹点是: 圆心+半径
            var x = circle.Center.X;
            var y = circle.Center.Y;
            var z = circle.Center.Z;
            var r = circle.Radius;
            boPts.Add(new(x + r, y, z));//上
            boPts.Add(new(x - r, y, z));//下
            boPts.Add(new(x, y - r, z));//左
            boPts.Add(new(x, y + r, z));//右
        }
        else
        {
            // 获取所有的边点
            // 这里圆形会获取圆心,所以剔除圆形
            var tmp = GetEntityPoint3ds(boEnt);
            for (int j = 0; j < tmp.Count; j++)
                boPts.Add(tmp[j]);
        }

        if (boEnt is Arc arc)
        {
            if (!arc.StartPoint.IsEqualTo(arc.EndPoint, Tol))
            {
                // 圆弧的腰点
                var arc2 = arc.GetPointAtDist(arc.GetDistAtPoint(arc.EndPoint) * 0.5);
                boPts.Add(arc2);
            }
        }
        else if (boEnt is Polyline pl)
        {
            for (int j = 0; j < pl.NumberOfVertices; j++)
            {
                var bulge = pl.GetBulgeAt(j);
                if (bulge == 0.0)
                    continue;
                // 有凸度就是有每段的中点
                var pta = pl.GetPoint2dAt(j);
                Point2d ptb;
                if (j + 1 < pl.NumberOfVertices)
                    ptb = pl.GetPoint2dAt(j + 1);
                else
                    ptb = pl.GetPoint2dAt(0);

                var p = MathHelper.GetArcMidPoint(pta, ptb, bulge);
                boPts.Add(p.Point3d());
            }
        }

        boPts.ForEach((pt, ptState) => {
            var dist = pt.DistanceTo(mouseStart);
            //Debugx.Printl("pt::" + pt + "     dist::" + dist);
            if (dist < tol)
            {
                ptState.Break();
                _pickInBo = true;
            }
        });
    }

    /// <summary>
    /// 图元拉伸点
    /// </summary>
    /// <param name="ent"></param>
    /// <returns></returns>
    static List<Point3d> GetEntityPoint3ds(Entity ent)
    {
        using var pts3d = new Point3dCollection();
        ent.GetStretchPoints(pts3d);
        return pts3d.Cast<Point3d>().ToList();
    }

    /// <summary>
    /// 反应器->command命令完成后
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Md_CommandEnded(object sender, CommandEventArgs e)
    {
        if (!State.IsRun)
            return;

        var cmdup = e.GlobalCommandName.ToUpper();
        switch (cmdup)
        {
            case "GRIP_STRETCH":// 拉伸夹点命令后触发
            {
                try
                {
                    // 夹点在边界上,退出
                    if (_pickInBo)
                    {
                        _pickInBo = false; // 重置状态
                        return;
                    }

                    // 夹点不在边界上:
                    // cad会平移填充,在这之后,我们删除填充边界,重建填充边界
                    var prompt = Env.Editor.SelectImplied();
                    if (prompt.Status != PromptStatus.OK)
                    {
                        _pickInBo = false; // 重置状态
                        return;
                    }

                    using DBTrans tr = new(docLock: true);
                    var _hatchIds = prompt.Value.GetObjectIds();
                    if (_hatchIds.Length == 0)
                    {
                        _pickInBo = false; // 重置状态
                        return;
                    }

                    // 删除指定填充的边界,并清理关联反应器
                    HashSet<ObjectId> idsOfSsget = [.. _hatchIds];
                    foreach (var hatId in _hatchIds)
                    {
                        if (!HatchConvMap.TryGetValue(hatId, out var conv))
                        {
                            continue;
                        }

                        bool clearFlag = false;
                        conv.BoundaryNewlyIds?.ForEach(boId => {
                            if (!boId.IsOk())
                                return;
                            using var boEnt = (Entity)tr.GetObject(boId, OpenMode.ForRead);
                            if (!HatchPickEnv.IsMeCreate(boEnt))
                                return;
                            boId.Erase();
                            clearFlag = true;
                        });

                        if (!clearFlag)
                            continue; // 跳过没有清除边界的填充

                        //conv.BoundaryIds.Clear();

                        // 清理填充反应器
                        using var hatchEnt = tr.GetObject(hatId, OpenMode.ForWrite);
                        if (hatchEnt is Hatch hatch)
                        {
                            RemoveAssociative(hatch);
                            CreatHatchConverter(hatch, idsOfSsget);
                        }
                    }

                    SetImpliedSelection(idsOfSsget);
                }
                catch (Exception ex)
                {
                    DebugEx.Printl("Md_CommandEnded error: " + ex.Message);
                }
                finally
                {
                    _pickInBo = false; // 确保状态被重置
                }
            }
            break;
        }
    }

    /// <summary>
    ///  反应器->lisp命令
    /// </summary>
    void Md_LispWillStart(object sender, LispWillStartEventArgs e)
    {
        if (!State.IsRun)
            return;
        using DBTrans tr = new(Acap.DocumentManager.MdiActiveDocument, true);
        EraseAllHatchBorders();
    }

    /// <summary>
    /// 反应器->选择集
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Md_ImpliedSelectionChanged(object sender, EventArgs e)
    {
        if (!State.IsRun)
            return;

        if (_selectChangedStop)
        {
            _selectChangedStop = false;
            return;
        }
        DebugEx.Printl("Md_ImpliedSelectionChanged");

        // 选中之后esc进入!ok状态
        var prompt = Env.Editor.SelectImplied();
        if (prompt.Status != PromptStatus.OK)
        {
            EraseAllHatchBorders();
            return;
        }


        // 直接选中进入此处
        HashSet<ObjectId> setImpSelect = [];
        //using var _ = _doc.LockDocument();
        using (DBTrans tr = new(Acap.DocumentManager.MdiActiveDocument, docLock: true))
        {
            // 获取图层锁定的记录,用于跳过
            HashSet<string> islocks = [];
            foreach (var layerRecord in tr.LayerTable.GetRecords())
                if (!layerRecord.IsErased && layerRecord.IsLocked)// 08符号表记录保留了这个
                    islocks.Add(layerRecord.Name);

            // 遍历选择,创建边界转换器
            // 重设选择集
            var ids = prompt.Value.GetObjectIds();
            setImpSelect.Add(ids);
            foreach (var entId in ids)
            {
                using var ent = tr.GetObject(entId, openLockedLayer: true);
                if (ent is not Hatch hatch || islocks.Contains(hatch.Layer))
                    continue;
                // 重复选择
                if (HatchConvMap.ContainsKey(entId))
                    continue;

                // 在为编辑期间并且不是块内图元,就跳过
                var doc = Acap.DocumentManager.MdiActiveDocument;
                if (LongTransactionManager.RefeditRun(doc) && !LongTransactionManager.WorkSetHas(doc, entId))
                {
                    DebugEx.Printl($"在为编辑期间并且不是块内图元,就跳过: {entId}");
                    continue;
                }

                CreatHatchConverter(hatch, setImpSelect);
            }
        }
        SetImpliedSelection(setImpSelect);
    }

    /// <summary>
    /// 创建填充和填充边界转换器
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="outSsgetIds"></param>
    /// <param name="tr"></param>
    void CreatHatchConverter(Hatch hatch, HashSet<ObjectId> outSsgetIds)
    {
        var tr = DBTrans.GetTop(hatch.Database);
        var hc = new HatchConverter(hatch);
        ObjectId newid;

        // 如果边界在图纸上没有删除(删除就不是关联的),
        // 那就不创建新的,然后选中它们
        if (hc.BoundaryIds is not null)
        {
            DebugEx.Printl("CreatHatchConverter:: 加入了现有边界到选择集");

            // 加入选择集
            outSsgetIds.Add(hc.BoundaryIds);
            outSsgetIds.Add(hatch.ObjectId);
            newid = hatch.ObjectId;
        }
        else
        {
            DebugEx.Printl("CreatHatchConverter:: 创建新填充和边界");

            // 创建新填充和边界
            hc.GetBoundarysData();
            newid = hc.CreateBoundarysAndHatchToMsPs();

            if (hc.BoundaryNewlyIds is null)
                return;

            HatchPickEnv.SetMeXData(newid, hc.BoundaryNewlyIds);

            // 加入选择集
            outSsgetIds.Remove(hatch.ObjectId);
            outSsgetIds.Add(hc.BoundaryNewlyIds);
            outSsgetIds.Add(newid);

            // 重建了新填充就删除旧的
            if (newid != hatch.ObjectId)
            {
                hatch.ObjectId.Erase();
                // 清理上次,删除边界和填充
                if (HatchConvMap.TryGetValue(hatch.ObjectId, out var hcx))
                {
                    if (hcx.BoundaryNewlyIds is not null)
                    {
                        foreach (var bo in hcx.BoundaryNewlyIds)
                            bo.Erase();
                        HatchConvMap.Remove(hatch.ObjectId);
                    }
                }
            }
        }

        HatchConvMap[newid] = hc;
    }

    /// <summary>
    /// 重设选择集
    /// </summary>
    /// <param name="setImpSelect">加入选择集的成员</param>
    public void SetImpliedSelection(HashSet<ObjectId> setImpSelect)
    {
        // 设置选择集,没有标记的话会死循环
        _selectChangedStop = true;
        Env.Editor.SetImpliedSelection(setImpSelect.ToArray());
    }

    /// <summary>
    /// 删除当前文档全部填充边界
    /// </summary>
    public void EraseAllHatchBorders(bool docLockFlag = true)
    {
        if (HatchConvMap.Count == 0)
            return;

        DocumentLock? documentLock = null;
        if (docLockFlag)
            documentLock = _doc.LockDocument();

        try
        {
            foreach (var dict in HatchConvMap)
            {
                if (dict.Value.BoundaryNewlyIds is null)
                    continue;
                foreach (var boId in dict.Value.BoundaryNewlyIds)
                {
                    if (!boId.IsOk())
                        continue; // 跳过无效ID，继续处理其他边界

                    try
                    {
                        using DBTrans tr = new(boId.Database);
                        using var boEnt = (Entity)tr.GetObject(boId);
                        // 删除填充边界并清理关联反应器
                        if (!HatchPickEnv.IsMeCreate(boEnt))
                            continue;
                        boEnt.UpgradeOpen();
                        boEnt.Erase();
                        if (dict.Key.IsOk())
                        {
                            using var ent2 = tr.GetObject(dict.Key, OpenMode.ForWrite, openLockedLayer: true);
                            if (ent2 is Hatch hatch)
                                RemoveAssociative(hatch);
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugEx.Printl("EraseAllHatchBorders error: " + ex.Message);
                        continue;
                    }
                }
            }

            HatchConvMap.Clear();
        }
        finally
        {
            documentLock?.Dispose();
        }
    }

    /// <summary>
    /// 移除关联反应器
    /// </summary>
    /// <param name="hatch"></param>
    static void RemoveAssociative(Hatch hatch)
    {
        // 撤回填充,没有边界就移除关联反应器
        if (!hatch.Associative)
            return;

        // 填充边界反应器
        using var assIds = hatch.GetAssociatedObjectIds();
        bool isok = true;
        foreach (ObjectId id in assIds)
        {
            if (!id.IsOk())
            {
                isok = false;
                break;
            }
        }
        // 这里边界id已经删除了,所以移除会导致异常
        if (isok)
            hatch.RemoveAssociatedObjectIds();

        // 取消关联反应器才能生成的正确
        hatch.Associative = false;
    }



    /// <summary>
    /// 撤回事件(获取删除对象)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DB_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        if (!State.IsRun)
            return;
        DebugEx.Printl($"{nameof(DB_ObjectErased)}: {e.DBObject} , {e.DBObject.IsUndoing} , {e.DBObject.IsErased}");

        // object erased.
        if (e.Erased)
        {
            return;
        }

        // UNDO
        if (e.DBObject is Hatch hatch)
        {
            if (HatchPickEnv.IsMeCreate(hatch))
                RemoveAssociative(hatch);
        }
        else if (e.DBObject is Entity boEnt)
        {
            // 撤回边界
            if (HatchPickEnv.IsMeCreate(boEnt))
            {
                try
                {
                    boEnt.Erase();
                    // 通过xdata回溯填充,清理关联反应器
                    if (boEnt.XData != null)
                    {
                        using DBTrans tr = new();
                        var hatchId = HatchPickEnv.GetXdataHatch(boEnt);
                        if (hatchId.IsOk())
                        {
                            using var hatchEnt = (Hatch)tr.GetObject(hatchId, OpenMode.ForWrite);
                            if (hatchEnt != null)
                                RemoveAssociative(hatchEnt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugEx.Printl("DB_ObjectErased error: " + ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// 撤回事件(更改时触发)
    /// 它会获取有修改步骤的图元id
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DB_ObjectModified(object sender, ObjectEventArgs e)
    {
        if (!State.IsRun)
            return;
        DebugEx.Printl($"{nameof(DB_ObjectModified)}: {e.DBObject} , {e.DBObject.IsUndoing} , {e.DBObject.IsErased}");

        // 然后删除我制造的拉伸填充上面的关联反应器
        if (!e.DBObject.IsUndoing)
            return;
        if (e.DBObject.IsErased)
            return;
        // 是我生成的填充才删除关联
        if (e.DBObject is Hatch hatch)
        {
            if (HatchPickEnv.IsMeCreate(hatch))
                RemoveAssociative(hatch);
        }
    }

    #endregion

    #region IDisposable接口相关函数
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// 手动调用释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数调用释放
    /// </summary>
    ~HatchPick()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        // 不重复释放
        if (IsDisposed) return;
        IsDisposed = true;

        if (_doc.IsDisposed)
            return;
        LoadHelper(false);
    }
    #endregion
}


public static class HatchPickEnv
{
    const string _appName = "JoinBox";
    const string _dataName = "JoinBoxCreateBoundary";

    /// <summary>
    /// 判断图元是否由 我的转换器 创建(相对的是直接提取现有图元边界)
    /// </summary>
    /// <param name="entity">任何图元</param>
    /// <returns></returns>
    public static bool IsMeCreate(Entity entity)
    {
        if (entity.XData == null)
            return false;
        var xl = (XDataList)entity.XData;
        return xl.Contains(_appName, _dataName);
    }

    /// <summary>
    /// 我的转换器 xdata数据模板
    /// </summary>
    /// <param name="hatchHandle"></param>
    /// <param name="tr"></param>
    /// <returns></returns>
    public static ResultBuffer GetMeBuffer(Handle hatchHandle, DBTrans? trans = null)
    {
        trans ??= DBTrans.Top;
        trans.RegAppTable.Add(_appName); // add函数会默认的在存在这个名字的时候返回这个名字的regapp的id,不存在就新建
        ResultBuffer resBuf = new()
        {
            new((int)DxfCode.ExtendedDataRegAppName, _appName),
            new((int)DxfCode.ExtendedDataAsciiString,_dataName),
            new((int)DxfCode.ExtendedDataHandle, hatchHandle),//边界回溯这个填充的句柄,如果创建新填充,就需要再去改
        };
        return resBuf;
    }

    /// <summary>
    /// 填充和边界上面增加xdata,实现区分原生和我的数据
    /// </summary>
    /// <param name="newHatchId"></param>
    /// <param name="boIds"></param>
    /// <param name="trans"></param>
    public static void SetMeXData(ObjectId newHatchId, ReadOnlyCollection<ObjectId>? boIds)
    {
        var trans = DBTrans.GetTop(newHatchId.Database);
        var hatchEnt = (Hatch)trans.GetObject(newHatchId);
        using (hatchEnt.ForWrite())
            hatchEnt.XData = GetMeBuffer(hatchEnt.Handle, trans); // 设置xdata仅仅为debug可以通过鼠标悬停看见它数据,因此设置为自己

        // 修改边界的xdata为新填充的
        boIds?.ForEach(id => {
            var boEnt = (Entity)trans.GetObject(id);
            using (boEnt.ForWrite())
            {
                boEnt.RemoveXData(_appName);
                boEnt.XData = GetMeBuffer(newHatchId.Handle, trans);
            }
        });
    }

    /// <summary>
    /// 通过边界ent获取填充id
    /// </summary>
    /// <param name="boEntity">边界图元</param>
    /// <param name="trans"></param>
    /// <returns></returns>
    public static ObjectId GetXdataHatch(Entity boEntity, DBTrans? trans = null)
    {
        if (boEntity.XData == null)
            return ObjectId.Null;
        XDataList data = boEntity.XData;

        if (!data.Contains(_appName, _dataName))
            return ObjectId.Null;

        var indexs = data.GetXdataAppIndex(_appName, new DxfCode[] { DxfCode.ExtendedDataHandle });
        if (indexs.Count == 0)
            return ObjectId.Null;

        trans ??= DBTrans.Top;
        return trans.GetObjectId(data[indexs[0]].Value.ToString());
    }
}

public static class MathHelper
{
    /// <summary>
    /// 圆弧的腰点
    /// </summary>
    /// <param name="arc1">圆弧点1</param>
    /// <param name="arc3">圆弧点3</param>
    /// <param name="bulge">凸度</param>
    /// <returns>返回腰点</returns>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2d GetArcMidPoint(Point2d arc1, Point2d arc3, double bulge)
    {
        if (bulge == 0)
            throw new ArgumentException("凸度为0,此线是平的");

        var center = GetArcBulgeCenter(arc1, arc3, bulge);
        var angle1 = center.GetVectorTo(arc1).GetAngle2XAxis();
        var angle3 = center.GetVectorTo(arc3).GetAngle2XAxis();
        // 利用边点进行旋转,就得到腰点,旋转角/2
        // 需要注意镜像的多段线
        double angle = angle3 - angle1;
        if (bulge > 0)
        {
            if (angle < 0)
                angle += Math.PI * 2;
        }
        else
        {
            if (angle > 0)
                angle += Math.PI * 2;
        }
        return arc1.RotateBy(angle / 2, center);
    }

    /// http://bbs.xdcad.net/thread-722387-1-1.html
    /// https://blog.csdn.net/jiangyb999/article/details/89366912
    /// <summary>
    /// 凸度求圆心
    /// </summary>
    /// <param name="arc1">圆弧头点</param>
    /// <param name="arc3">圆弧尾点</param>
    /// <param name="bulge">凸度</param>
    /// <returns>圆心</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2d GetArcBulgeCenter(Point2d arc1, Point2d arc3, double bulge)
    {
        if (bulge == 0)
            throw new ArgumentException("凸度为0,此线是平的");

        var x1 = arc1.X;
        var y1 = arc1.Y;
        var x2 = arc3.X;
        var y2 = arc3.Y;

        var b = (1 / bulge - bulge) / 2;
        var x = (x1 + x2 - b * (y2 - y1)) / 2;
        var y = (y1 + y2 + b * (x2 - x1)) / 2;
        return new Point2d(x, y);
    }

    /// <summary>
    /// X轴到向量的弧度,cad的获取的弧度是1PI,所以转换为2PI(上小,下大)
    /// </summary>
    /// <param name="ve">向量</param>
    /// <returns>X轴到向量的弧度</returns>
    public static double GetAngle2XAxis(this Vector2d ve, double tolerance = 1e-6)
    {
        const double Tau = Math.PI + Math.PI;
        // 世界重合到用户 Vector3d.XAxis->两点向量
        double al = Vector2d.XAxis.GetAngleTo(ve);
        al = ve.Y > 0 ? al : Tau - al; // 逆时针为正,大于0是上半圆,小于则是下半圆,如果-负值控制正反
        al = Math.Abs(Tau - al) <= tolerance ? 0 : al;
        return al;
    }
}