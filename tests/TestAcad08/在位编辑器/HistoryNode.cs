namespace Test;

/// <summary>
/// 历史记录节点 - 不可变类型，存储某一时刻的完整状态快照
/// </summary>
public class HistoryNode
{
    /// <summary>
    /// 历史节点索引（对应链表中的位置）
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// 触发历史记录的命令名称
    /// </summary>
    public string CommandName { get; private set; }

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRun { get; private set; }

    /// <summary>
    /// 块参照ID
    /// </summary>
    public ObjectId BlockReferenceId { get; private set; }

    /// <summary>
    /// 当前空间ID
    /// </summary>
    public ObjectId CurrentSpaceId { get; private set; }

    /// <summary>
    /// 工作集 - 不可变
    /// </summary>
    public ImmutableHashSet<ObjectId> Workset { get; private set; }

    /// <summary>
    /// 锁定的图层 - 不可变
    /// </summary>
    public ImmutableHashSet<ObjectId> LockedLayers { get; private set; }

    /// <summary>
    /// 参照集添加的ID - 不可变
    /// </summary>
    public ImmutableHashSet<ObjectId> RefsetAddIds { get; private set; }

    /// <summary>
    /// 参照集移除的ID - 不可变
    /// </summary>
    public ImmutableHashSet<ObjectId> RefsetRemoveIds { get; private set; }

    /// <summary>
    /// 创建历史节点
    /// </summary>
    public HistoryNode(
        int index,
        string commandName,
        bool isRun,
        ObjectId blockReferenceId,
        ObjectId currentSpaceId,
        ImmutableHashSet<ObjectId> workset,
        ImmutableHashSet<ObjectId> lockedLayers,
        ImmutableHashSet<ObjectId> refsetAddIds,
        ImmutableHashSet<ObjectId> refsetRemoveIds)
    {
        Index = index;
        CommandName = commandName;
        IsRun = isRun;
        BlockReferenceId = blockReferenceId;
        CurrentSpaceId = currentSpaceId;
        Workset = workset ?? ImmutableHashSet<ObjectId>.Empty;
        LockedLayers = lockedLayers ?? ImmutableHashSet<ObjectId>.Empty;
        RefsetAddIds = refsetAddIds ?? ImmutableHashSet<ObjectId>.Empty;
        RefsetRemoveIds = refsetRemoveIds ?? ImmutableHashSet<ObjectId>.Empty;
    }
}
