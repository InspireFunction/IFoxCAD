// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#line hidden // 调试的时候跳过它

namespace IFoxCAD.Basal;

/// <summary>
/// 程序流程状态枚举
/// </summary>
[Flags]
public enum CtrlStateType : int
{
    /// <summary>新建</summary>
    [Description("新建")]
    None = 0,
    /// <summary>就绪(初始化)</summary>
    [Description("就绪")]
    Init = 1,
    /// <summary>运行中</summary>
    [Description("运行中")]
    Running = 1 << 1,
    /// <summary>中断</summary>
    [Description("中断")]
    Break = 1 << 2,
    /// <summary>停止</summary>
    [Description("停止")]
    Stop = 1 << 3,
    /// <summary>取消</summary>
    [Description("取消")]
    Cancel = 1 << 4,
    /// <summary>阻塞</summary>
    [Description("阻塞")]
    Blocked = 1 << 5,
    /// <summary>等待</summary>
    [Description("等待")]
    Waiting = 1 << 6,
    /// <summary>异常</summary>
    [Description("异常")]
    Exceptional = 1 << 7,
}

/// <summary>
/// 控制程序流程
/// </summary>
[DebuggerDisplay("{ToDetailedString(),nq}")]
public class CtrlState
{
    private volatile int _flag = (int)CtrlStateType.None;

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public CtrlStateType State => (CtrlStateType)_flag;

    public CtrlState()
    {
        _flag = (int)CtrlStateType.None;
    }

    public bool IsNone => _flag == (int)CtrlStateType.None;
    public bool IsInit => State.HasFlag(CtrlStateType.Init);
    public bool IsRun => State.HasFlag(CtrlStateType.Running);
    public bool IsBreak => State.HasFlag(CtrlStateType.Break);
    public bool IsStop => State.HasFlag(CtrlStateType.Stop);
    public bool IsCancel => State.HasFlag(CtrlStateType.Cancel);
    public bool IsBlocked => State.HasFlag(CtrlStateType.Blocked);
    public bool IsWaiting => State.HasFlag(CtrlStateType.Waiting);
    public bool IsExceptional => State.HasFlag(CtrlStateType.Exceptional);

    public void None() => _flag = (int)CtrlStateType.None;
    public void Init() => _flag = (int)CtrlStateType.Init;
    public void Reset() => _flag = (int)CtrlStateType.Init;
    public void Start() => _flag = (int)CtrlStateType.Running;
    public void Break() => _flag = (int)CtrlStateType.Break;
    public void Stop() => _flag = (int)CtrlStateType.Stop;
    public void Blocked() => _flag = (int)CtrlStateType.Blocked;
    public void Waiting() => _flag = (int)CtrlStateType.Waiting;
    public void Cancel() => _flag = (int)CtrlStateType.Cancel;
    public void Exceptional()
    {
        if (!IsExceptional)
            _flag |= (int)CtrlStateType.Exceptional;
    }

    /// <summary>
    /// 创建当前状态的克隆
    /// </summary>
    /// <returns>新的 CtrlState 实例，状态与当前实例相同</returns>
    public CtrlState Clone()
    {
        var copy = new CtrlState();
        copy._flag = _flag;
        return copy;
    }

    /// <summary>
    /// 获取当前状态的字符串表示
    /// </summary>
    /// <returns>状态的文字描述</returns>
    public override string ToString() => EnumEx.GetDescription(State);

    /// <summary>
    /// 获取当前状态的详细字符串表示（包含附加状态）
    /// </summary>
    /// <returns>状态的详细文字描述</returns>
    public string ToDetailedString() => EnumEx.PrintNote(State);
}



#line default

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
