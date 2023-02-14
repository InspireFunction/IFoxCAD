using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static IFoxCAD.Cad.PostCmd;

namespace JoinBoxAcad;

public class HatchPickEvent : IDisposable
{
    #region 静态成员
    public static ProState State = new();
    // 选择集过滤器
    public static readonly SelectionFilter FilterForHatch = new(new TypedValue[] { new TypedValue((int)DxfCode.Start, "HATCH") });
    // 临时标记(重设选择集会触发一次选择集反应器)
    static bool _selectChangedStop = false;
    // 临时选择集用
    static List<ObjectId> _hatchIds = new();
    // 获取夹点在哪个图元边界上面,是为true
    static bool _pickInBo = false;
    static bool _vetoProperties = false;
    private Tolerance _tol = new(1e-6, 1e-6);

    public static void AddInit()
    {
        HatchHook.SetHook();
        // 全局事件重复+=是不需要担心的
        Acap.DocumentManager.DocumentLockModeChanged += Dm_VetoCommand;
        State.Start();
    }

    public static void RemoveInit()
    {
        State.Stop();
        Acap.DocumentManager.DocumentLockModeChanged -= Dm_VetoCommand;
        HatchHook.RemoveHook();
    }

    /// <summary>
    /// 反应器->命令否决触发命令前(不可锁文档)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void Dm_VetoCommand(object sender, DocumentLockModeChangedEventArgs e)
    {
        if (!State.IsRun)
            return;
        if (string.IsNullOrEmpty(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;
        switch (e.GlobalCommandName.ToUpper())
        {
            case "PROPERTIES": // 特性面板
            {
                // 事件顺序问题:
                // 开cad之后第一次双击必弹出特性面板
                // 所以这里直接删除填充边界
                HatchPick.MapDocHatchPickEvent[e.Document].SetPropertiesInfoTask();
                if (_vetoProperties)
                {
                    Debugx.Printl("Dm_VetoCommand 否决了");
                    e.Veto();
                    _vetoProperties = false;
                    // 发送编辑填充命令
                    SendCommand("_hatchedit ", RunCmdFlag.AcedPostCommand);
                    return;
                }
                Debugx.Printl("Dm_VetoCommand 没否决");
            }
            break;
        }
    }


    #endregion

    #region 动态成员
    /// <summary>
    /// 在位编辑器 <see langword="Refedit命令执行启动前"/> 记录全图选择集的填充
    /// </summary>
    readonly HashSet<ObjectId> _refeditSsgeting = new();
    /// <summary>
    /// 在位编辑器 <see langword="Refedit命令执行后"/> 获取当前选择集做差集=>内部填充
    /// </summary>
    readonly HashSet<ObjectId> _refeditSsgeted = new();
    // map<填充id,边界转换器>
    readonly Dictionary<ObjectId, HatchConverter> _mapHatchConv = new();
    readonly Document _doc;
    public HatchPickEvent(Document doc)
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
    /// 反应器->command命令完成前
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Md_CommandWillStart(object sender, CommandEventArgs e)
    {
        if (!State.IsRun)
            return;

        // 此处无法使用文档锁,否则将导致文档锁无法释放,然后ctrl+z失败
        var cmdup = e.GlobalCommandName.ToUpper();
        Debugx.Printl("Md_CommandWillStart::" + cmdup);

        switch (cmdup)
        {
            case "REFEDIT":
            {
                // 在位编辑命令,执行前,获取当前空间所有填充
                var prompt = Env.Editor.SelectAll(FilterForHatch);
                if (prompt.Status != PromptStatus.OK)
                    return;
                using DBTrans tr = new();
                GetHatchIds(prompt);
                if (_hatchIds.Count == 0)
                    return;
                for (int i = 0; i < _hatchIds.Count; i++)
                    _refeditSsgeting.Add(_hatchIds[i]);
            }
            break;
        }

        // 拉伸夹点命令前触发
        if (cmdup != "GRIP_STRETCH")
        {
            EraseAllHatchBorders();
        }
        else
        {
            var mp = HatchHook.MouseStartPoint;
            var mouseStart = Screen.ScreenToCad(mp);
            Debugx.Printl("mouseStart,屏幕点::" + mp);
            Debugx.Printl("mouseStart,cad点::" + mouseStart);

            // 获取当前选择的对象,然后提取所有的夹点
            var prompt = Env.Editor.SelectImplied();
            if (prompt.Status != PromptStatus.OK)
                return;
            using DBTrans tr = new();
            GetHatchIds(prompt);
            if (_hatchIds.Count == 0)
                return;

            // TODO 屏幕像素点转cad点的误差,要随着视口高度而动态计算....这里的计算可能不太正确
            var tol = (double)Env.GetVar("viewsize") / 10;
            Debugx.Printl("tol::" + tol);

            // 0x01 移动了矩形填充中间的夹点,删除边界,并且重新生成填充和边界
            // 0x02 移动了填充边界上的夹点,不处理,然后它会通过关联进行自己修改
            _pickInBo = false;
            for (int i = 0; i < _hatchIds.Count; i++)
            {
                var hatId = _hatchIds[i];
                if (!_mapHatchConv.ContainsKey(hatId))
                    continue;

                _mapHatchConv[hatId].BoundaryIds.ForEach((id, idState) => {
                    var boEnt = tr.GetObject<Entity>(id);
                    if (boEnt == null)
                        return;

                    // 获取夹点在哪个图元边界上
                    HashSet<Point3d> boPts = new();
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
                        if (!arc.StartPoint.IsEqualTo(arc.EndPoint, _tol))
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
                });

                // 点在边界上:就不处理了,它会通过cad的关联填充反应器自动修改
                if (_pickInBo)
                    Debugx.Printl("夹点在边界上");
                else
                    Debugx.Printl("夹点不在边界上");
            }
        }
    }

    /// <summary>
    /// 图元拉伸点
    /// </summary>
    /// <param name="ent"></param>
    /// <returns></returns>
    static List<Point3d> GetEntityPoint3ds(Entity ent)
    {
        var pts3d = new Point3dCollection();
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
            case "REFEDIT":
            {
                Debugx.Printl("Md_CommandEnded:: REFEDIT");

                // 在位编辑命令,执行后,获取当前空间所有填充
                var prompt = Env.Editor.SelectAll(FilterForHatch);
                if (prompt.Status != PromptStatus.OK)
                    return;

                using DBTrans tr = new();
                GetHatchIds(prompt);
                if (_hatchIds.Count == 0)
                    return;

                for (int i = 0; i < _hatchIds.Count; i++)
                    if (!_refeditSsgeting.Contains(_hatchIds[i]))//Except
                        _refeditSsgeted.Add(_hatchIds[i]);

                var sb = new StringBuilder();
                foreach (var id in _refeditSsgeted)
                    sb.AppendLine(id.ToString());
                Env.Printl("块内填充id:" + sb.ToString());
            }
            break;
            case "REFSET": // 加减在位编辑图元
            {
                Debugx.Printl("Md_CommandEnded:: REFSET");

                // 命令历史的最后一行是:添加/删除
                var last = Env.GetVar("lastprompt").ToString();
                if (last is null)
                    return;

                // 完成后必然有上次选择集
                var prompt = Env.Editor.SelectPrevious();
                if (prompt.Status != PromptStatus.OK)
                    return;
                using DBTrans tr = new();
                GetHatchIds(prompt);
                if (_hatchIds.Count == 0)
                    return;

                // 就是因为无法遍历到在位编辑的块内图元,只能进行布尔运算
                if (last.Contains("添加") || last.Contains("Added"))// 中英文cad
                {
                    for (int i = 0; i < _hatchIds.Count; i++)
                    {
                        _refeditSsgeting.Remove(_hatchIds[i]);
                        _refeditSsgeted.Add(_hatchIds[i]);
                    }
                    return;
                }
                if (last.Contains("删除") || last.Contains("Removed"))// 中英文cad
                {
                    for (int i = 0; i < _hatchIds.Count; i++)
                    {
                        _refeditSsgeted.Remove(_hatchIds[i]);
                        _refeditSsgeting.Add(_hatchIds[i]);
                    }
                    return;
                }
            }
            break;
            case "REFCLOSE":// 保存块,清空集合
            {
                Debugx.Printl("Md_CommandEnded:: REFCLOSE");
                _refeditSsgeted.Clear();
                _refeditSsgeting.Clear();
            }
            break;
            case "GRIP_STRETCH":// 拉伸夹点命令后触发
            {
                // 夹点在边界上,退出
                if (_pickInBo)
                    return;

                // 夹点不在边界上:
                // cad会平移填充,在这之后,我们删除填充边界,重建填充边界
                var prompt = Env.Editor.SelectImplied();
                if (prompt.Status != PromptStatus.OK)
                    return;
                using DBTrans tr = new();
                GetHatchIds(prompt);
                if (_hatchIds.Count == 0)
                    return;

                // 删除指定填充的边界,并清理关联反应器
                HashSet<ObjectId> idsOfSsget = new();
                foreach (var hatId in _hatchIds)
                {
                    idsOfSsget.Add(hatId);

                    if (!_mapHatchConv.ContainsKey(hatId))
                        continue;
                    bool clearFlag = false;
                    _mapHatchConv[hatId].BoundaryIds.ForEach(boId => {
                        if (!boId.IsOk())
                            return;
                        var boEnt = tr.GetObject<Entity>(boId);
                        if (boEnt == null)
                            return;
                        if (!HatchPickEnv.IsMeCreate(boEnt))
                            return;
                        boId.Erase();
                        clearFlag = true;
                    });

                    if (!clearFlag)
                        return;

                    _mapHatchConv[hatId].BoundaryIds.Clear();

                    // 清理填充反应器
                    var hatch = tr.GetObject<Hatch>(hatId);
                    if (hatch == null)
                        return;
                    using (hatch.ForWrite())
                        RemoveAssociative(hatch);
                    CreatHatchConverter(hatch, idsOfSsget);
                }
                SetImpliedSelection(idsOfSsget);
            }
            break;
        }
    }

    /// <summary>
    /// 获取选择集上的填充,在缓存内提取<see cref="_hatchIds"/>
    /// </summary>
    /// <param name="psr"></param>
    /// <param name="tr"></param>
    static void GetHatchIds(PromptSelectionResult psr, DBTrans? tr = null)
    {
        tr ??= DBTrans.Top;
        _hatchIds.Clear();
        var ids = psr.Value.GetObjectIds();
        for (int i = 0; i < ids.Length; i++)
        {
            var hatch = tr.GetObject<Hatch>(ids[i]);
            if (hatch is not null)
                _hatchIds.Add(ids[i]);
        }
    }

    /// <summary>
    ///  反应器->lisp命令
    /// </summary>
    void Md_LispWillStart(object sender, LispWillStartEventArgs e)
    {
        if (!State.IsRun)
            return;

        using DBTrans tr = new(doclock: true);
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

        // 此处必须要文档锁
        if (_selectChangedStop)
        {
            _selectChangedStop = false;
            return;
        }
        Debugx.Printl("Md_ImpliedSelectionChanged");

        using DBTrans tr = new(doclock: true);
        var prompt = Env.Editor.SelectImplied();
        if (prompt.Status != PromptStatus.OK)
        {
            EraseAllHatchBorders();
            return;
        }

        // 获取图层锁定的记录,用于跳过
        Dictionary<string, bool> islocks = new();
        foreach (var layerRecord in tr.LayerTable.GetRecords())
            if (!layerRecord.IsErased)// 08符号表记录保留了这个
                islocks.Add(layerRecord.Name, layerRecord.IsLocked);

        // 遍历选择,创建边界转换器
        // 重设选择集
        HashSet<ObjectId> idsOfSsget = new();
        foreach (var entId in prompt.Value.GetObjectIds())
        {
            idsOfSsget.Add(entId);
            var hatch = tr.GetObject<Hatch>(entId, openLockedLayer: true);
            if (hatch is null)
                continue;
            if (islocks[hatch.Layer])
                continue;
            // 重复选择 || 在位编辑外
            if (_mapHatchConv.ContainsKey(entId) || _refeditSsgeting.Contains(entId))
                continue;
            CreatHatchConverter(hatch, idsOfSsget);
        }
        SetImpliedSelection(idsOfSsget);
    }

    /// <summary>
    /// 创建填充和填充边界转换器
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="outSsgetIds"></param>
    /// <param name="tr"></param>
    void CreatHatchConverter(Hatch hatch, HashSet<ObjectId> outSsgetIds)
    {
        //tr ??= DBTrans.Top;
        var tr = DBTrans.GetTopTransaction(hatch.Database);
        var hc = new HatchConverter(hatch);
        ObjectId newid;

        // 如果边界在图纸上没有删除(删除就不是关联的),
        // 那就不创建新的,然后选中它们
        if (hc.BoundaryIds.Count != 0)
        {
            Debugx.Printl("CreatHatchConverter:: 加入了现有边界到选择集");

            // 加入选择集
            foreach (var item in hc.BoundaryIds)
                outSsgetIds.Add(item);
            outSsgetIds.Add(hatch.ObjectId);
            newid = hatch.ObjectId;
        }
        else
        {
            Debugx.Printl("CreatHatchConverter:: 创建新填充和边界");

            // 创建新填充和边界
            hc.GetBoundarysData();

            newid = hc.CreateBoundarysAndHatchToMsPs((BlockTableRecord)tr.GetObject(hatch.Database.CurrentSpaceId,OpenMode.ForWrite));
            HatchPickEnv.SetMeXData(newid, hc.BoundaryIds);

            // 清理上次,删除边界和填充
            if (_mapHatchConv.ContainsKey(hatch.ObjectId))
            {
                var boIds = _mapHatchConv[hatch.ObjectId].BoundaryIds;
                for (int i = 0; i < boIds.Count; i++)
                    boIds[i].Erase();
                _mapHatchConv.Remove(hatch.ObjectId);
            }
            // 删除选中的
            hatch.ObjectId.Erase();
        }

        if (!_mapHatchConv.ContainsKey(newid))
            _mapHatchConv.Add(newid, hc);
        else
            _mapHatchConv[newid] = hc;

        if (newid == hatch.ObjectId)
            return;

        // 优先: 块内含有旧的,就加入新的
        if (_refeditSsgeted.Contains(hatch.ObjectId))
        {
            _refeditSsgeted.Remove(hatch.ObjectId);
            _refeditSsgeted.Add(newid);
        }
        else if (_refeditSsgeting.Contains(hatch.ObjectId))
        {
            _refeditSsgeting.Remove(hatch.ObjectId);
            _refeditSsgeting.Add(newid);
        }
    }

    /// <summary>
    /// 重设选择集
    /// </summary>
    /// <param name="setImpSelect">加入选择集的成员</param>
    void SetImpliedSelection(HashSet<ObjectId> setImpSelect)
    {
        // 获取填充
        foreach (var id in _mapHatchConv.Keys)
            setImpSelect.Add(id);

        // 获取填充边界
        foreach (var item in _mapHatchConv.Values)
            foreach (var id in item.BoundaryIds)
                setImpSelect.Add(id);

        // 设置选择集,没有标记的话会死循环
        _selectChangedStop = true;
        Env.Editor.SetImpliedSelection(setImpSelect.ToArray());
    }

    /// <summary>
    /// 删除全部填充边界
    /// </summary>
    void EraseAllHatchBorders()
    {
        if (_mapHatchConv.Count == 0)
            return;
        foreach (var dict in _mapHatchConv)
        {
            dict.Value.BoundaryIds.ForEach(boId => {
                if (!boId.IsOk())
                    return;
                using DBTrans tr = new(database: boId.Database);
                var boEnt = tr.GetObject<Entity>(boId, OpenMode.ForWrite);
                if (boEnt == null)
                    return;
                // 删除填充边界并清理关联反应器
                if (!HatchPickEnv.IsMeCreate(boEnt))
                    return;
                boEnt.Erase();
                if (dict.Key.IsOk())
                {
                    var hatch = tr.GetObject<Hatch>(dict.Key, OpenMode.ForWrite);
                    if (hatch == null)
                        return;
                    RemoveAssociative(hatch);
                }
            });
        }
        _mapHatchConv.Clear();
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
        var assIds = hatch.GetAssociatedObjectIds();
        if (assIds == null)
            return;
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
    static void DB_ObjectErased(object sender, ObjectErasedEventArgs e)
    {
        if (!State.IsRun)
            return;

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
                boEnt.Erase();
                // 通过xdata回溯填充,清理关联反应器
                if (boEnt.XData != null)
                {
                    using DBTrans tr = new();
                    var hatchId = HatchPickEnv.GetXdataHatch(boEnt);
                    if (hatchId.IsOk())
                    {
                        var hatchEnt = tr.GetObject<Hatch>(hatchId, OpenMode.ForWrite);
                        if (hatchEnt != null)
                            RemoveAssociative(hatchEnt);
                    }
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
    static void DB_ObjectModified(object sender, ObjectEventArgs e)
    {
        if (!State.IsRun)
            return;

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

    void SetPropertiesInfoTask()
    {
        // 原有选择集
        var prompt = Env.Editor.SelectImplied();
        if (prompt.Status != PromptStatus.OK)
            return;

        using DBTrans tr = new();

        // 获取记录的边界
        HashSet<ObjectId> boAll = new();
        foreach (var hc in _mapHatchConv.Values)
            foreach (var boid in hc.BoundaryIds)
                boAll.Add(boid);

        // 获取选择集上面所有的填充,如果没有填充就结束(不屏蔽特性面板)
        bool hasHatch = false;
        HashSet<ObjectId> idsOfSsget = new();
        foreach (var id in prompt.Value.GetObjectIds())
        {
            // 含有填充
            if (_mapHatchConv.ContainsKey(id))
                hasHatch = true;
            // 排除边界的加入
            if (!boAll.Contains(id))
                idsOfSsget.Add(id);
        }
        if (!hasHatch)
            return;

        // 删除填充边界,并清理关联反应器
        EraseAllHatchBorders();

        // 重设选择集 提供给后续命令判断
        SetImpliedSelection(idsOfSsget);

        // 如果有填充才否决
        _vetoProperties = idsOfSsget.Count != 0;
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
    ~HatchPickEvent()
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

/// <summary>
/// 填充的鼠标钩子
/// </summary>
public static class HatchHook
{
    static readonly MouseHook MouseHook;
    // 夹点拉伸前的点,拉伸后利用命令后反应器去处理"GRIP_STRETCH"
    static volatile int _X;
    static volatile int _Y;
    public static Point MouseStartPoint { get => new(_X, _Y); }

    /// <summary>
    /// 鼠标双击事件
    /// </summary>
    public static event EventHandler? DoubleClick;

    static HatchHook()
    {
        MouseHook = new();
    }

    /// <summary>
    /// 查找主线程<br/>
    /// 代替<see cref="AppDomain.GetCurrentThreadId()"/><br/>
    /// 托管线程和他们不一样: <see cref="Thread.CurrentThread.ManagedThreadId"/>
    /// </summary>
    /// <param name="hWnd">主窗口</param>
    /// <param name="lpdwProcessId">进程ID</param>
    /// <returns>线程ID</returns>
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern bool IsWindowEnabled(IntPtr hWnd);
    /// <summary>
    /// 获取当前窗口
    /// </summary>
    /// <returns>当前窗口标识符</returns>
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public static void SetHook()
    {
        // 如果是全局钩子会发生偶尔失效的情况,改用进程钩子反而好多了
        MouseHook.SetHook(true);
        MouseHook.MouseDown += (sender, e) => {
            // 此处断点时候就会使得钩子失效
            if (!IsWindowEnabled(Acap.MainWindow.Handle))
                return;
            // 进程号拦截
            GetWindowThreadProcessId(GetForegroundWindow(), out uint winId);
            if (MouseHook.Process.Id != winId)
                return;
            if (e.Button == MouseButtons.Left)
            {
                _X = e.X;
                _Y = e.Y;
            }
        };

        MouseHook.DoubleClick += (sender, e) => {
            // 此处断点时候就会使得钩子失效
            if (!IsWindowEnabled(Acap.MainWindow.Handle))
                return;
            // 进程号拦截
            GetWindowThreadProcessId(GetForegroundWindow(), out uint winId);
            if (MouseHook.Process.Id != winId)
                return;
            DoubleClick?.Invoke(sender, e);
        };
    }

    public static void RemoveHook()
    {
        MouseHook?.Dispose();
    }
}

public static class HatchPickEnv
{
    //static readonly string _appName = nameof(JoinBox);
    //static readonly string _data = nameof(CreateBoundary);

    static readonly string _appName = "JoinBox";
    static readonly string _data = "CreateBoundary";

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
        return xl.Contains(_appName, _data);
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
            new((int)DxfCode.ExtendedDataAsciiString,_data),
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
    public static void SetMeXData(ObjectId newHatchId, List<ObjectId> boIds, DBTrans? trans = null)
    {
        trans ??= DBTrans.Top;
        var hatchEnt = trans.GetObject<Hatch>(newHatchId);
        if (hatchEnt != null)
            using (hatchEnt.ForWrite())
                hatchEnt.XData = GetMeBuffer(hatchEnt.Handle, trans); // 设置xdata仅仅为debug可以通过鼠标悬停看见它数据,因此设置为自己

        // 修改边界的xdata为新填充的
        boIds.ForEach(id => {
            var boEnt = trans.GetObject<Entity>(id);
            if (boEnt is null)
                return;
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

        if (!data.Contains(_appName, _data))
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
    [MethodImpl]
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
    [MethodImpl]
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