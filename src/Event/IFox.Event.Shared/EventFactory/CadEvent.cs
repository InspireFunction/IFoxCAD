namespace IFoxCAD.Event;

[Flags]
public enum CadEvent
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0b0,
    /// <summary>
    /// 全选
    /// </summary>
    All = 0b1111111111111111111111111111111,
    /// <summary>
    /// 系统变量修改
    /// </summary>
    SystemVariableChanged = 0b1,
    /// <summary>
    /// 文档锁定事件
    /// </summary>
    DocumentLockModeChanged = 0b10,
    /// <summary>
    /// 开始双击
    /// </summary>
    BeginDoubleClick=0b100,
}