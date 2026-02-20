// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
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
    /// <summary>异常</summary>
    [Description("异常")]
    Exceptional = 1 << 0,
    /// <summary>错误</summary>
    [Description("错误")]
    Error = 1 << 1,
    /// <summary>初始化</summary>
    [Description("初始化")]
    Init = 1 << 2,
    /// <summary>运行中</summary>
    [Description("运行中")]
    Running = 1 << 3,
    /// <summary>中断</summary>
    [Description("中断")]
    Break = 1 << 4,
    /// <summary>继续</summary>
    [Description("继续")]
    Continue = 1 << 5,
    /// <summary>停止</summary>
    [Description("停止")]
    Stop = 1 << 6,
    /// <summary>取消</summary>
    [Description("取消")]
    Cancel = 1 << 7,
    /// <summary>阻塞</summary>
    [Description("阻塞")]
    Blocked = 1 << 8,
    /// <summary>等待</summary>
    [Description("等待")]
    Waiting = 1 << 9,
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
    public bool IsError => State.HasFlag(CtrlStateType.Error);
    public bool IsContinue => State.HasFlag(CtrlStateType.Continue);

    // 主状态掩码（用于清除主状态位，保留附加状态位）
    private const int MainStateMask = (int)(
        CtrlStateType.Init
        | CtrlStateType.Running
        | CtrlStateType.Break
        | CtrlStateType.Continue
        | CtrlStateType.Stop
        | CtrlStateType.Cancel
        | CtrlStateType.Blocked
        | CtrlStateType.Waiting);

    public void Init() => _flag = (int)CtrlStateType.Init;
    public void Reset() => _flag = (int)CtrlStateType.Init;
    public void Start() => SetMainState(CtrlStateType.Running);
    public void Break() => SetMainState(CtrlStateType.Break);
    public void Stop() => SetMainState(CtrlStateType.Stop);
    public void Blocked() => SetMainState(CtrlStateType.Blocked);
    public void Waiting() => SetMainState(CtrlStateType.Waiting);
    public void Cancel() => SetMainState(CtrlStateType.Cancel);
    public void Continue() => SetMainState(CtrlStateType.Continue);

    /// <summary>
    /// 获取附加状态位（Exceptional 和 Error）
    /// </summary>
    private int GetAttachedStates() => _flag & (~MainStateMask);

    /// <summary>
    /// 设置主状态，保留附加状态
    /// </summary>
    private void SetMainState(CtrlStateType mainState)
    {
        var attached = GetAttachedStates();
        _flag = (int)mainState | attached;
    }

    /// <summary>
    /// 设置或取消异常状态
    /// </summary>
    /// <param name="value">true设置状态,false清除状态</param>
    public void Exceptional(bool value)
    {
        if (value)
        {
            if (!IsExceptional)
                _flag |= (int)CtrlStateType.Exceptional;
        }
        else
        {
            if (IsExceptional)
                _flag &= ~(int)CtrlStateType.Exceptional;
        }
    }

    /// <summary>
    /// 设置或取消错误状态
    /// </summary>
    /// <param name="value">true设置状态,false清除状态</param>
    public void Error(bool value)
    {
        if (value)
        {
            if (!IsError)
                _flag |= (int)CtrlStateType.Error;
        }
        else
        {
            if (IsError)
                _flag &= ~(int)CtrlStateType.Error;
        }
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
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
