namespace JoinBoxAcad;

// 动作类型枚举
public enum ActionType
{
    OtherOperation,      // 系统操作(根节点使用)

    DatabaseAdd,         // 数据库添加
    DatabaseDelete,       // 数据库删除
    DatabaseModify,       // 数据库修改

    RefSetAdd,           // 添加到在位编辑
    RefSetRemove,        // 从在位编辑移除
    RefEdit,              // 在位编辑起点
    RefEditUndo,         // 在位编辑起点
    RefClose,             // 在位编辑终点
    RefCloseUndo,         // 在位编辑终点

    BlockEdit,              // 块编辑开始
    BlockEditUndo,         // 块编辑开始的撤销
    BlockEditClose,         // 块编辑保存
    BlockEditCloseUndo      // 块编辑保存结束
}

// 动作接口
public interface IAction
{
    /// <summary>
    /// 唯一id
    /// </summary>
    string GuId { get; }
    /// <summary>
    /// 动作类型
    /// </summary>
    ActionType Type { get; }
    /// <summary>
    /// 描述
    /// </summary>
    string Description { get; }
    /// <summary>
    /// 时间 
    /// </summary>
    DateTime Timestamp { get; }
    /// <summary>
    /// 执行
    /// </summary>
    void Execute();
    /// <summary>
    /// 逆向动作
    /// </summary>
    /// <returns></returns>
    IAction GetInverseAction();
    /// <summary>
    /// 克隆动作
    /// </summary>
    /// <returns></returns>
    IAction Clone();
    /// <summary>
    /// 能否合并动作
    /// </summary>
    /// <param name="otherAction"></param>
    /// <returns></returns>
    bool CanMergeWith(IAction otherAction);
    /// <summary>
    /// 合并动作
    /// </summary>
    /// <param name="otherAction"></param>
    /// <returns></returns>
    IAction MergeWith(IAction otherAction);
}