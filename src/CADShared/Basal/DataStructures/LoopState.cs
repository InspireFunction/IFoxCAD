// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
using System.Diagnostics;

namespace IFoxCAD.Basal;

#region 枚举定义

public static class EnumExtensions
{
    /// <summary>
    /// 检查枚举是否包含指定的标志
    /// .NET 3.5 兼容版本
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T 必须是枚举类型");
        }

        long longValue = value.ToInt64(null);
        long longFlag = flag.ToInt64(null);

        return (longValue & longFlag) == longFlag;
    }
}

/// <summary>
/// 循环状态枚举（用于 LoopState 类）
/// </summary>
[Flags]
public enum LoopStateType : int
{
    /// <summary>运行中（初始状态）</summary>
    None = 0,
    /// <summary>异常</summary>
    Exceptional = 1,
    /// <summary>中断</summary>
    Break = 2,
    /// <summary>停止</summary>
    Stop = 4,
    /// <summary>取消</summary>
    Cancel = 8
}

/// <summary>
/// 程序流程状态枚举（用于 ProState 类）
/// </summary>
[Flags]
public enum ProStateType : int
{
    /// <summary>未开始</summary>
    None = 0,
    /// <summary>运行中</summary>
    Run = 1,
    /// <summary>中断</summary>
    Break = 2,
    /// <summary>停止</summary>
    Stop = 4,
    /// <summary>取消</summary>
    Cancel = 8,
    /// <summary>异常</summary>
    Exceptional = 16
}

#endregion

#region 状态枚举扩展方法

/// <summary>
/// 状态枚举扩展方法
/// </summary>
public static class StateExtensions
{
    /// <summary>
    /// 获取状态的文字描述
    /// </summary>
    /// <param name="state">循环状态</param>
    /// <returns>状态文字</returns>
    public static string ToDisplayString(this LoopStateType state)
    {
        return state switch
        {
            LoopStateType.None => "运行中",
            LoopStateType.Exceptional => "异常",
            LoopStateType.Break => "中断",
            LoopStateType.Stop => "停止",
            LoopStateType.Cancel => "取消",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 获取状态的文字描述
    /// </summary>
    /// <param name="state">程序状态</param>
    /// <returns>状态文字</returns>
    public static string ToDisplayString(this ProStateType state)
    {
        return state switch
        {
            ProStateType.None => "未开始",
            ProStateType.Run => "运行中",
            ProStateType.Exceptional => "异常",
            ProStateType.Break => "中断",
            ProStateType.Stop => "停止",
            ProStateType.Cancel => "取消",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 获取循环状态的详细字符串表示（包含附加状态）
    /// </summary>
    /// <param name="state">循环状态</param>
    /// <returns>详细状态文字</returns>
    public static string ToDetailedString(this LoopStateType state)
    {
        var states = new List<string>();

        // 主要状态
        if (state == LoopStateType.None)
            states.Add("运行中");
        else if (state.HasFlag(~LoopStateType.Exceptional))
        {
            // 有非异常的主要状态
            if (state.HasFlag(LoopStateType.Break))
                states.Add("中断");
            if (state.HasFlag(LoopStateType.Stop))
                states.Add("停止");
            if (state.HasFlag(LoopStateType.Cancel))
                states.Add("取消");
        }
        else
        {
            states.Add("运行中");
        }

        // 附加状态
        if (state.HasFlag(LoopStateType.Exceptional))
            states.Add("异常");

        return states.Count > 0 ? string.Join(" | ", states.ToArray()) : "未知状态";
    }

    /// <summary>
    /// 获取程序状态的详细字符串表示（包含附加状态）
    /// </summary>
    /// <param name="state">程序状态</param>
    /// <returns>详细状态文字</returns>
    public static string ToDetailedString(this ProStateType state)
    {
        var states = new List<string>();

        // 主要状态
        if (state.HasFlag(ProStateType.Run))
            states.Add("运行中");
        else if (state == ProStateType.None)
            states.Add("未开始");

        // 其他状态
        if (state.HasFlag(ProStateType.Break))
            states.Add("中断");
        if (state.HasFlag(ProStateType.Stop))
            states.Add("停止");
        if (state.HasFlag(ProStateType.Cancel))
            states.Add("取消");
        if (state.HasFlag(ProStateType.Exceptional))
            states.Add("异常");

        return states.Count > 0 ? string.Join(" | ", states.ToArray()) : "未知状态";
    }
}

#endregion

#line hidden // 调试的时候跳过它
/// <summary>
/// 控制循环结束
/// </summary>
[DebuggerDisplay("{ToDetailedString(),nq}")]
public class LoopState
{
    private volatile int _flag = (int)LoopStateType.None;

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public LoopStateType State => (LoopStateType)_flag;

    public bool IsRun => _flag == (int)LoopStateType.None;
    public bool IsExceptional => State.HasFlag(LoopStateType.Exceptional);
    public bool IsBreak => State.HasFlag(LoopStateType.Break);
    public bool IsStop => State.HasFlag(LoopStateType.Stop);
    public bool IsCancel => State.HasFlag(LoopStateType.Cancel);

    public void Exceptional()
    {
        if (!IsExceptional)
            _flag |= (int)LoopStateType.Exceptional;
    }
    public void Break() => _flag = (int)LoopStateType.Break;
    public void Stop() => _flag = (int)LoopStateType.Stop;
    public void Cancel() => _flag = (int)LoopStateType.Cancel;
    public void Reset() => _flag = (int)LoopStateType.None;

    /// <summary>
    /// 获取当前状态的字符串表示
    /// </summary>
    /// <returns>状态的文字描述</returns>
    public override string ToString() => State.ToDisplayString();

    /// <summary>
    /// 获取当前状态的详细字符串表示（包含附加状态）
    /// </summary>
    /// <returns>状态的详细文字描述</returns>
    public string ToDetailedString() => State.ToDetailedString();
}
#line default

/// <summary>
/// 控制程序流程
/// </summary>
[DebuggerDisplay("{ToDetailedString(),nq}")]
public class ProState
{
    private volatile int _flag = (int)ProStateType.None;

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public ProStateType State => (ProStateType)_flag;

    public ProState()
    {
        _flag = (int)ProStateType.None;
    }

    public bool IsNone => _flag == (int)ProStateType.None;
    public bool IsRun => State.HasFlag(ProStateType.Run);
    public bool IsBreak => State.HasFlag(ProStateType.Break);
    public bool IsStop => State.HasFlag(ProStateType.Stop);
    public bool IsCancel => State.HasFlag(ProStateType.Cancel);
    public bool IsExceptional => State.HasFlag(ProStateType.Exceptional);

    public void Exceptional()
    {
        if (!IsExceptional)
            _flag |= (int)ProStateType.Exceptional;
    }
    public void Break() => _flag = (int)ProStateType.Break;
    public void Stop() => _flag = (int)ProStateType.Stop;
    public void Cancel() => _flag = (int)ProStateType.Cancel;
    public void Start() => _flag = (int)ProStateType.Run;
    public void None() => _flag = (int)ProStateType.None;

    /// <summary>
    /// 创建当前状态的克隆
    /// </summary>
    /// <returns>新的 ProState 实例，状态与当前实例相同</returns>
    public ProState Clone()
    {
        var copy = new ProState();
        copy._flag = _flag;
        return copy;
    }

    /// <summary>
    /// 获取当前状态的字符串表示
    /// </summary>
    /// <returns>状态的文字描述</returns>
    public override string ToString() => State.ToDisplayString();

    /// <summary>
    /// 获取当前状态的详细字符串表示（包含附加状态）
    /// </summary>
    /// <returns>状态的详细文字描述</returns>
    public string ToDetailedString() => State.ToDetailedString();
}
#line default

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
