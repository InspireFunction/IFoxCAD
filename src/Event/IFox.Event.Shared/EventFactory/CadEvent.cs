namespace IFoxCAD.Event;

[Flags]
public enum CadEvent
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,
    /// <summary>
    /// 全选
    /// </summary>
    All = -1,
    /// <summary>
    /// 系统变量修改
    /// </summary>
    SystemVariableChanged = 1 << 1,
    /// <summary>
    /// 文档锁定事件
    /// </summary>
    DocumentLockModeChanged = 1 << 2,
    /// <summary>
    /// 开始双击
    /// </summary>
    BeginDoubleClick = 1 << 3,
    /// <summary>
    /// 文档激活
    /// </summary>
    DocumentActivated = 1 << 4,
}