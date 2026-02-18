namespace Test;

/// <summary>
/// 工作区 - 可编辑的当前状态
/// </summary>
public class WorkingArea
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRun { get; set; } = false;

    /// <summary>
    /// 块参照ID
    /// </summary>
    public ObjectId BlockReferenceId { get; set; } = ObjectId.Null;

    /// <summary>
    /// 当前空间ID
    /// </summary>
    public ObjectId CurrentSpaceId { get; set; } = ObjectId.Null;

    /// <summary>
    /// 工作集 - 可变
    /// </summary>
    public HashSet<ObjectId> Workset { get; set; } = [];

    /// <summary>
    /// 锁定的图层 - 可变
    /// </summary>
    public HashSet<ObjectId> LockedLayers { get; set; } = [];

    /// <summary>
    /// 参照集添加的ID - 可变
    /// </summary>
    public HashSet<ObjectId> RefsetAddIds { get; set; } = [];

    /// <summary>
    /// 参照集移除的ID - 可变
    /// </summary>
    public HashSet<ObjectId> RefsetRemoveIds { get; set; } = [];

    /// <summary>
    /// 触发历史记录的命令名称
    /// </summary>
    public string CommandName { get; set; } = string.Empty;

    /// <summary>
    /// 从工作区创建历史节点快照
    /// </summary>
    /// <param name="index">历史节点索引</param>
    /// <param name="commandName">触发历史记录的命令名称</param>
    public HistoryNode CreateSnapshot(int index, string commandName)
    {
        return new HistoryNode(
            index,
            commandName,
            IsRun,
            BlockReferenceId,
            CurrentSpaceId,
            new ImmutableHashSet<ObjectId>(Workset),
            new ImmutableHashSet<ObjectId>(LockedLayers),
            new ImmutableHashSet<ObjectId>(RefsetAddIds),
            new ImmutableHashSet<ObjectId>(RefsetRemoveIds)
        );
    }

    /// <summary>
    /// 从历史节点恢复到工作区
    /// </summary>
    public void RestoreFrom(HistoryNode node)
    {
        CommandName = node.CommandName;
        IsRun = node.IsRun;
        BlockReferenceId = node.BlockReferenceId;
        CurrentSpaceId = node.CurrentSpaceId;
        Workset = node.Workset.ToMutable();
        LockedLayers = node.LockedLayers.ToMutable();
        RefsetAddIds = node.RefsetAddIds.ToMutable();
        RefsetRemoveIds = node.RefsetRemoveIds.ToMutable();
    }

    /// <summary>
    /// 清理工作区
    /// </summary>
    public void Clear()
    {
        IsRun = false;
        BlockReferenceId = ObjectId.Null;
        CurrentSpaceId = ObjectId.Null;
        Workset.Clear();
        LockedLayers.Clear();
        RefsetAddIds.Clear();
        RefsetRemoveIds.Clear();
        CommandName = string.Empty;
    }
}
