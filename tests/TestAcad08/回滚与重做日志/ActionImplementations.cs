namespace JoinBoxAcad;

/// <summary>
/// 基础动作实现
/// </summary>
public abstract class BaseAction : IAction
{
    public string GuId { get; } = Guid.NewGuid().ToString();
    public abstract ActionType Type { get; }
    public abstract string Description { get; }
    public DateTime Timestamp { get; } = DateTime.Now;

    public abstract void Execute();
    public abstract IAction GetInverseAction();
    public abstract IAction Clone();

    public abstract bool CanMergeWith(IAction otherAction);
    public abstract IAction MergeWith(IAction otherAction);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <returns></returns>
    public virtual string Serialize()
    {
        try
        {
            var result = MyJson.SerializeObject(this, Formatting.Indented);
            Env.Printl($"Action Serialize Success: {this.GetType().Name}, Result Length: {result.Length}");
            return result;
        }
        catch (Exception ex)
        {
            Env.Printl($"Action Serialize Error: {this.GetType().Name}, Message: {ex.Message}");
            Env.Printl($"Action Serialize Error Details: {ex}");
            throw;
        }
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IAction? Deserialize(string json)
    {
        if (json is null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        try
        {
            Env.Printl($"Action Deserialize Input Json Length: {json?.Length}, Content Preview: {(json?.Length > 100 ? json.Substring(0, 100) : json)}");
            var s = MyJson.DeserializeObject<BaseAction>(json);
            Env.Printl($"Action Deserialize Success: {(s != null ? s.GetType().Name : "null")}");
            if (s is null)
                return null;
            return s;
        }
        catch (Exception ex)
        {
            Env.Printl($"Action Deserialize Error, Message: {ex.Message}");
            Env.Printl($"Action Deserialize Error Details: {ex}");
            Env.Printl($"Problematic JSON: {(json?.Length > 200 ? json.Substring(0, 200) + "..." : json)}");
            throw;
        }
    }
}


/// <summary>
/// 实体动作基类
/// </summary>
public abstract class EntityAction : BaseAction
{
    /// <summary>
    /// id
    /// </summary>
    public ObjectId DBObjectId { get; internal set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string? EntityType { get; internal set; }

    protected EntityAction(ObjectId entityId)
    {
        DBObjectId = entityId;
        using var tr = DBTrans.Create(entityId.Database);
        using var obj = tr.GetObject(entityId, OpenMode.ForWrite, true, true);
        EntityType = obj?.GetType().Name;
    }
}

/// <summary>
/// 创建实体动作
/// </summary>
public class CreateEntityAction : EntityAction
{
    public override ActionType Type => ActionType.DatabaseAdd;
    public override string Description => $"创建实体: {EntityType}";

    /// <summary>
    /// 创建实体动作
    /// </summary>
    public CreateEntityAction(ObjectId entityId) : base(entityId) { }

    /// <summary>
    /// 正向动作(由于cad已经创建,这里不需要)
    /// </summary>
    public override void Execute()
    {
        using var tr = DBTrans.Create(DBObjectId.Database);
        using var obj = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (obj is not null && obj.IsErased)
            obj.Erase(false); // 使用false参数恢复被删除的对象
    }

    /// <summary>
    /// 逆向动作
    /// </summary>
    public override IAction GetInverseAction() => new DeleteEntityAction(DBObjectId);

    public override IAction Clone()
    {
        return new CreateEntityAction(DBObjectId);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("创建动作不能合并");
}

/// <summary>
/// 删除实体动作
/// </summary>
public class DeleteEntityAction : EntityAction
{
    public override ActionType Type => ActionType.DatabaseDelete;
    public override string Description => $"删除实体: {EntityType}";

    /// <summary>
    /// 删除实体动作
    /// </summary>
    public DeleteEntityAction(ObjectId entityId) : base(entityId) { }

    public override void Execute()
    {
        using var tr = DBTrans.Create(DBObjectId.Database);
        using var obj = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (obj != null && !obj.IsErased)
            obj.Erase(true); // 使用true参数确保正确删除
    }

    public override IAction GetInverseAction()
    {
        return new CreateEntityAction(DBObjectId);
    }

    public override IAction Clone() => new DeleteEntityAction(DBObjectId);

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("删除动作不能合并");
}


/// <summary>
/// 修改的动作
/// </summary>
public class ModifyEntityAction : EntityAction
{
    public Dictionary<string, (object OldValue, object NewValue)> FieldChanges { get; }

    public override ActionType Type => ActionType.DatabaseModify;
    public override string Description => $"修改实体: {EntityType} ({FieldChanges.Count} 个字段)";

    public ModifyEntityAction(ObjectId entityId,
        Dictionary<string, (object, object)> changes) : base(entityId)
    {
        FieldChanges = changes;
    }

    public override void Execute()
    {
        using var tr = DBTrans.Create(DBObjectId.Database);
        using var entity = tr.GetObject(DBObjectId, OpenMode.ForWrite, true, true);
        if (entity != null)
            ApplyFields(entity, FieldChanges, true);
    }

    public override IAction GetInverseAction()
    {
        var inverseChanges = FieldChanges.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.NewValue, kvp.Value.OldValue)
        );
        return new ModifyEntityAction(DBObjectId, inverseChanges);
    }

    public override IAction Clone()
    {
        return new ModifyEntityAction(DBObjectId, new Dictionary<string, (object, object)>(FieldChanges));
    }

    public override bool CanMergeWith(IAction otherAction)
    {
        if (otherAction is not ModifyEntityAction otherModify)
            return false;

        return DBObjectId == otherModify.DBObjectId &&
               CanMergeFields(otherModify.FieldChanges);
    }

    public override IAction MergeWith(IAction otherAction)
    {
        if (!CanMergeWith(otherAction))
            throw new InvalidOperationException("动作不能合并");

        var otherModify = (ModifyEntityAction)otherAction;
        var mergedChanges = MergeFieldChanges(FieldChanges, otherModify.FieldChanges);

        return new ModifyEntityAction(
            DBObjectId,
            mergedChanges
        );
    }

    private bool CanMergeFields(Dictionary<string, (object OldValue, object NewValue)> otherChanges)
    {
        // 检查字段变化是否可以合并
        foreach (var kvp in otherChanges)
        {
            if (FieldChanges.TryGetValue(kvp.Key, out var currentChange))
            {
                // 如果字段被多次修改，需要检查是否兼容
                if (!(currentChange.NewValue?.Equals(kvp.Value.OldValue) ?? false))
                    return false;
            }
        }
        return true;
    }

    private Dictionary<string, (object, object)> MergeFieldChanges(
        Dictionary<string, (object, object)> changes1,
        Dictionary<string, (object, object)> changes2)
    {
        var merged = new Dictionary<string, (object, object)>();

        // 合并第一个变化集
        foreach (var kvp in changes1)
        {
            merged[kvp.Key] = kvp.Value;
        }

        // 合并第二个变化集
        foreach (var kvp in changes2)
        {
            merged[kvp.Key] = (changes1.TryGetValue(kvp.Key, out var change) ?
                change.Item1 : kvp.Value.Item1, kvp.Value.Item2);
        }

        return merged;
    }

    private void ApplyFields(
        DBObject entity,
        Dictionary<string, (object OldValue, object NewValue)> changes,
        bool applyNewValue)
    {
        foreach (var change in changes)
        {
            var value = applyNewValue ? change.Value.NewValue : change.Value.OldValue;

            // 优先尝试直接操作字段
            bool success = MemberHelper.TrySetMember(entity, change.Key, value, true);
            if (!success)
            {
                // 字段操作失败，尝试属性操作
                MemberHelper.TrySetMember(entity, change.Key, value, false);
            }
        }
    }
}

/// <summary>
/// 在位编辑添加动作
/// </summary>
public class InPlaceAddAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }

    public override ActionType Type => ActionType.InPlaceAdd;
    public override string Description => $"添加到在位编辑: {ObjectIds.Length} 个对象";

    public InPlaceAddAction(IEnumerable<ObjectId> objectIds)
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        // 选中 ObjectIds, 然后发送命令,在位编辑添加
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            // 发送异步命令需要添加进容器,外部 IsExecutingUndoRedo 就不会设置
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFSET");
            doc.Editor?.SetImpliedSelection(ObjectIds);
            doc.SendStringToExecute("REFSET\nA\n", true, false, false);
        }
        Env.Printl($"[DEBUG] 执行在位编辑添加操作，对象数: {ObjectIds.Length}");
    }

    public override IAction GetInverseAction()
    {
        // 逆向动作是从在位编辑中移除这些对象
        return new InPlaceRemoveAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceAddAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑添加动作不能合并");
}

/// <summary>
/// 在位编辑移除动作
/// </summary>
public class InPlaceRemoveAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }

    public override ActionType Type => ActionType.InPlaceRemove;
    public override string Description => $"从在位编辑块: {ObjectIds.Length} 个对象";

    public InPlaceRemoveAction(IEnumerable<ObjectId> objectIds)
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        // 选中 ObjectIds, 然后发送命令,在位编辑移除
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            // 发送异步命令需要添加进容器,外部 IsExecutingUndoRedo 就不会设置
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFSET");
            doc.Editor?.SetImpliedSelection(ObjectIds);
            doc.SendStringToExecute("REFSET\nR\n", true, false, false);
        }
        Env.Printl($"[DEBUG] 执行在位编辑移除操作，对象数: {ObjectIds.Length}");
    }

    public override IAction GetInverseAction()
    {
        // 逆向动作是添加这些对象到在位编辑
        return new InPlaceAddAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceRemoveAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑移除动作不能合并");
}


/// <summary>
/// 在位编辑动作:开始
/// </summary>
public class InPlaceCreateAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.InPlaceCreate;
    public override string Description => $"在位编辑开始";

    public InPlaceCreateAction(IEnumerable<ObjectId> objectIds) // 这里传入了编辑块
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFEDIT");
            doc.SendStringToExecute("REFEDIT\n", true, false, false);
        }
    }

    /// <summary>
    /// 开始在位编辑-撤销
    /// </summary>
    /// <returns></returns>
    public override IAction GetInverseAction()
    {
        return new InPlaceCreateEndAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceCreateAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑开始动作不能合并");
}


/// <summary>
/// 在位编辑开始的撤销
/// </summary>
/// <returns></returns>
public class InPlaceCreateEndAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.InPlaceCreateEnd;
    public override string Description => $"在位编辑开始的撤销";

    public InPlaceCreateEndAction(IEnumerable<ObjectId> objectIds) // 这里传入了编辑块
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFCLOSE");
            doc?.SendStringToExecute($"_.REFCLOSE _D\n", true, false, false);
        }
    }

    public override IAction GetInverseAction()
    {
        return new InPlaceCreateAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceCreateEndAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑开始的撤销_动作不能合并");
}


/// <summary>
/// 在位编辑保存
/// </summary>
public class InPlaceSaveAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.InPlaceSave;
    public override string Description => $"在位编辑保存";

    public InPlaceSaveAction(IEnumerable<ObjectId> objectIds) // 这里传入了编辑块
    {
        ObjectIds = objectIds.ToArray();
    }

    // 在位编辑-保存在位-撤销
    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFEDIT");
            doc?.SendStringToExecute($"REFEDIT\n", true, false, false);
        }
    }

    public override IAction GetInverseAction()
    {
        return new InPlaceSaveEndAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceSaveAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑保存不能合并");
}



public class InPlaceSaveEndAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.InPlaceSaveEnd;
    public override string Description => $"在位编辑保存的结束";

    public InPlaceSaveEndAction(IEnumerable<ObjectId> objectIds) // 这里传入了编辑块
    {
        ObjectIds = objectIds.ToArray();
    }

    // 在位编辑-保存在位-撤销-重做
    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("REFCLOSE");
            doc.SendStringToExecute("_.REFCLOSE _S\n", true, false, false);
        }
    }

    public override IAction GetInverseAction()
    {
        return new InPlaceSaveAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InPlaceSaveEndAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("在位编辑保存结束不能合并");
}


/// <summary>
/// 块编辑保存动作
/// </summary>
public class InBlockEditSaveAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.InPlaceSave;
    public override string Description => "块编辑保存";

    public InBlockEditSaveAction(IEnumerable<ObjectId> objectIds)
    {
        ObjectIds = objectIds.ToArray();
    }

    // 块编辑-保存块-撤销
    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("BEDIT");
            doc?.SendStringToExecute($"BEDIT\n", true, false, false);
        }
    }

    public override IAction GetInverseAction()
    {
        return new InBlockEditSaveEndAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InBlockEditSaveAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("块编辑保存动作不能合并");
}


public class InBlockEditSaveEndAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.InPlaceSaveEnd;
    public override string Description => "块编辑保存的结束";

    public InBlockEditSaveEndAction(IEnumerable<ObjectId> objectIds)
    {
        ObjectIds = objectIds.ToArray();
    }

    // 块编辑-保存块-撤销-重做
    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("BCLOSE");
            doc.SendStringToExecute("_.BCLOSE _S\n", true, false, false);
        }
    }

    public override IAction GetInverseAction()
    {
        return new InBlockEditSaveAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new InBlockEditSaveEndAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("块编辑保存结束不能合并");
}


/// <summary>
/// 块编辑动作:开始
/// </summary>
public class BlockEditCreateAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.BlockEditCreate;
    public override string Description => "块编辑开始";

    public BlockEditCreateAction(IEnumerable<ObjectId> objectIds) // 这里传入了编辑块
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor?.SetImpliedSelection(ObjectIds);
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("BEDIT");
            doc.SendStringToExecute("BEDIT\n", true, false, false);
        }
    }

    /// <summary>
    /// 开始块编辑-撤销
    /// </summary>
    /// <returns></returns>
    public override IAction GetInverseAction()
    {
        return new BlockEditCreateEndAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new BlockEditCreateAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("块编辑开始动作不能合并");
}


/// <summary>
/// 块编辑开始的撤销
/// </summary>
/// <returns></returns>
public class BlockEditCreateEndAction : BaseAction
{
    public ObjectId[] ObjectIds { get; }
    public override ActionType Type => ActionType.BlockEditCreateEnd;
    public override string Description => "块编辑开始的撤销";

    public BlockEditCreateEndAction(IEnumerable<ObjectId> objectIds) // 这里传入了编辑块
    {
        ObjectIds = objectIds.ToArray();
    }

    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            var logger = EnhancedUndoRedoManager.GetLogger(doc);
            logger?.AsyncCmdsPush("BCLOSE");
            doc?.SendStringToExecute($"BCLOSE\n", true, false, false);
        }
    }

    public override IAction GetInverseAction()
    {
        return new BlockEditCreateAction(ObjectIds);
    }

    public override IAction Clone()
    {
        return new BlockEditCreateEndAction(ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException("块编辑开始的撤销_动作不能合并");
}


