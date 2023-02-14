namespace IFoxCAD.Cad;

[Flags]
public enum BrightEntity : int
{
    /// <summary>
    /// 块更新
    /// </summary>
    RecordGraphicsModified = 1,
    /// <summary>
    /// 标注更新
    /// </summary>
    RecomputeDimensionBlock = 2,
    /// <summary>
    /// 重画
    /// </summary>
    Draw = 4,
    /// <summary>
    /// 亮显
    /// </summary>
    Highlight = 8,
    /// <summary>
    /// 亮显取消
    /// </summary>
    Unhighlight = 16,
    /// <summary>
    /// 显示图元
    /// </summary>
    VisibleTrue = 32,
    /// <summary>
    /// 隐藏图元
    /// </summary>
    VisibleFalse = 64,
    /// <summary>
    /// 平移更新,可以令ctrl+z撤回时候保证刷新
    /// </summary>
    MoveZero = 128,
}

[Flags]
public enum BrightEditor : int
{
    /// <summary>
    /// 刷新屏幕,图元不生成(例如块还是旧的显示)
    /// </summary>
    UpdateScreen = 1,
    /// <summary>
    /// 刷新全图
    /// </summary>
    Regen = 2,
    /// <summary>
    /// 清空选择集
    /// </summary>
    SelectionClean = 4,
    /// <summary>
    /// 视口外
    /// </summary>
    ViewportsFrom = 8,
    /// <summary>
    /// 视口内
    /// </summary>
    ViewportsIn = 16,
}

public static class RedrawEx
{
    /// <summary>
    /// 刷新屏幕
    /// </summary>
    /// <param name="ed">编辑器</param>
    /// <param name="ent">图元,调用时候图元必须提权</param>
    public static void Redraw(this Editor ed, Entity? ent = null)
    {
        // 刷新图元
        ent?.Redraw(BrightEntity.Draw |
                    BrightEntity.RecordGraphicsModified |
                    BrightEntity.RecomputeDimensionBlock |
                    BrightEntity.MoveZero);
        // 刷新
        ed.Redraw(BrightEditor.UpdateScreen);

        /*
         * 我发现命令加 CommandFlags.Redraw 就不需要以下处理了:
         * 数据库事务和文档事务不一样,文档事务有刷新函数.
         * var doc = Acap.DocumentManager.MdiActiveDocument;
         * var ed = doc.Editor;
         * var tm = doc.TransactionManager;
         * tm.QueueForGraphicsFlush();// 如果在最外层事务结束之前需要更新图形,此句把目前为止所做的更改放入 刷新队列
         * tm.FlushGraphics();        // 将当前 刷新队列 的图形提交到显示器
         * ed.UpdateScreen();         // 仅刷新屏幕,图元不生成(例如块还是旧的显示)
         */
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var tm = doc.TransactionManager;
        tm.QueueForGraphicsFlush();
        tm.FlushGraphics();

        // acad2014及以上要加,立即处理队列上面的消息
        System.Windows.Forms.Application.DoEvents();
    }

    /// <summary>
    /// 刷新屏幕
    /// </summary>
    /// <param name="ed">编辑器</param>
    /// <param name="bright">更新的方式</param>
    public static void Redraw(this Editor ed, BrightEditor bright)
    {
        if ((bright & BrightEditor.UpdateScreen) == BrightEditor.UpdateScreen)
        {
            // 两个函数底层差不多
            // Acap.UpdateScreen();
            ed.UpdateScreen();
        }

        if ((bright & BrightEditor.Regen) == BrightEditor.Regen)
            ed.Regen();

        if ((bright & BrightEditor.SelectionClean) == BrightEditor.SelectionClean)
            ed.SetImpliedSelection(new ObjectId[0]);

        if ((bright & BrightEditor.ViewportsFrom) == BrightEditor.ViewportsFrom)
            ed.UpdateTiledViewportsFromDatabase(); // 更新视口外

        if ((bright & BrightEditor.ViewportsIn) == BrightEditor.ViewportsIn)
            ed.UpdateTiledViewportsInDatabase(); // 更新视口内
    }

    /// <summary>
    /// 更改图元显示
    /// </summary>
    /// <param name="ent">图元,调用时候图元必须提权</param>
    /// <param name="bright">更新的方式</param>
    public static void Redraw(this Entity ent, BrightEntity bright)
    {
        // 调用时候图元必须提权,参数true表示关闭图元后进行UpData,实现局部刷新块.
        if ((bright & BrightEntity.RecordGraphicsModified) == BrightEntity.RecordGraphicsModified)
            ent.RecordGraphicsModified(true);

        if ((bright & BrightEntity.RecomputeDimensionBlock) == BrightEntity.RecomputeDimensionBlock)
            if (ent is Dimension dim)
                dim.RecomputeDimensionBlock(true);

        if ((bright & BrightEntity.Draw) == BrightEntity.Draw)
            ent.Draw();

        if ((bright & BrightEntity.Highlight) == BrightEntity.Highlight)
            ent.Highlight();

        if ((bright & BrightEntity.Unhighlight) == BrightEntity.Unhighlight)
            ent.Unhighlight();

        if ((bright & BrightEntity.VisibleTrue) == BrightEntity.VisibleTrue)
            ent.Visible = true;

        if ((bright & BrightEntity.VisibleFalse) == BrightEntity.VisibleFalse)
            ent.Visible = false;

        // 戴耀辉:
        // 删除块内图元的时候需要刷新块,
        // 用 RecordGraphicsModified 显示是没有问题,
        // 但是 ctrl+z 撤销会有显示问题,
        // 所以平移0可以在撤回数据库的时候刷新指定图元
        if ((bright & BrightEntity.MoveZero) == BrightEntity.MoveZero)
            ent.Move(Point3d.Origin, Point3d.Origin);
    }


    #region 实体刷新
    /// <summary>
    /// 刷新实体显示
    /// </summary>
    /// <param name="entity">实体对象</param>
    [Obsolete("此处已经被RedrawEx代替")]
    public static void Flush(this Entity entity)
    {
        var tr = DBTrans.GetTopTransaction(entity.Database);
        entity.RecordGraphicsModified(true);
        tr.TransactionManager.QueueForGraphicsFlush();
        
    }

    /// <summary>
    /// 刷新实体显示
    /// </summary>
    /// <param name="id">实体id</param>
    [Obsolete("此处已经被RedrawEx代替")]
    public static void Flush(this ObjectId id) => Flush(id.GetObject<Entity>()!);
    #endregion
}