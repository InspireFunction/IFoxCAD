// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace IFoxCAD.Basal;

#line hidden // 调试的时候跳过它
/// <summary>
/// 控制循环结束
/// </summary>
public class LoopState
{
    private const int PlsNone = 0;
    private const int PlsExceptional = 1;
    private const int PlsBroken = 2;
    private const int PlsStopped = 4;
    private const int PlsCanceled = 8;

    private volatile int _flag = PlsNone;

    public bool IsRun => _flag == PlsNone;
    public bool IsExceptional => (_flag & PlsExceptional) == PlsExceptional;
    public bool IsBreak => (_flag & PlsBroken) == PlsBroken;
    public bool IsStop => (_flag & PlsStopped) == PlsStopped;
    public bool IsCancel => (_flag & PlsCanceled) == PlsCanceled;

    public void Exceptional()
    {
        if ((_flag & PlsExceptional) != PlsExceptional)
            _flag |= PlsExceptional;
    }
    public void Break() => _flag = PlsBroken;
    public void Stop() => _flag = PlsStopped;
    public void Cancel() => _flag = PlsCanceled;
    public void Reset() => _flag = PlsNone;
}
#line default

/// <summary>
/// 控制程序流程
/// </summary>
public class ProState
{
    private const int PlsNone = 0; // 初始化(构造就立马运行,将导致构造函数中也被检测,这是浪费性能及挖坑给自己的)
    private const int PlsRun = 1;  // 运行
    private const int PlsBroken = 2;
    private const int PlsStopped = 4;
    private const int PlsCanceled = 8;
    private const int PlsExceptional = 16; // 异常 用于附加状态

    private volatile int _flag = PlsNone;

    public bool IsNone => _flag == PlsNone;
    public bool IsRun => (_flag & PlsRun) == PlsRun;
    public bool IsBreak => (_flag & PlsBroken) == PlsBroken;
    public bool IsStop => (_flag & PlsStopped) == PlsStopped;
    public bool IsCancel => (_flag & PlsCanceled) == PlsCanceled;
    public bool IsExceptional => (_flag & PlsExceptional) == PlsExceptional;

    public void Exceptional()
    {
        if ((_flag & PlsExceptional) != PlsExceptional)
            _flag |= PlsExceptional;
    }
    public void Break() => _flag = PlsBroken;
    public void Stop() => _flag = PlsStopped;
    public void Cancel() => _flag = PlsCanceled;
    public void Start() => _flag = PlsRun;
    public void None() => _flag = PlsNone;
}
#line default

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释