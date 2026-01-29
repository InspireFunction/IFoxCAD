namespace JoinBoxAcad;

// 动作类型枚举
public enum ActionType
{
    OtherOperation,      // 系统操作(根节点使用)

    CommandExecution,    // 执行命令
    DatabaseAdd,         // 数据库添加
    DatabaseDelete,       // 数据库删除
    DatabaseModify,       // 数据库修改

    InPlaceAdd,           // 添加到在位编辑
    InPlaceRemove,        // 从在位编辑移除
    InPlaceCreate,        // 在位编辑起点
    InPlaceCreateEnd,        // 在位编辑起点
    InPlaceSave,         // 在位编辑终点
    InPlaceSaveEnd,         // 在位编辑终点
    
    BlockEditCreate,        // 块编辑开始
    BlockEditCreateEnd,     // 块编辑开始的撤销
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
    /// 正向动作
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