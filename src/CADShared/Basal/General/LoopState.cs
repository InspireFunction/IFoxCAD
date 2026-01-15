namespace IFoxCAD.Basal;

#line hidden // 调试的时候跳过它
/// <summary>
/// 控制循环结束
/// </summary>
public class LoopState
{
    const int PLS_NONE = 0;
    const int PLS_EXCEPTIONAL = 1;
    const int PLS_BROKEN = 2;
    const int PLS_STOPPED = 4;
    const int PLS_CANCELED = 8;

    private volatile int _flag = PLS_NONE;

    public bool IsRun => _flag == PLS_NONE;
    public bool IsExceptional => (_flag & PLS_EXCEPTIONAL) == PLS_EXCEPTIONAL;
    public bool IsBreak => (_flag & PLS_BROKEN) == PLS_BROKEN;
    public bool IsStop => (_flag & PLS_STOPPED) == PLS_STOPPED;
    public bool IsCancel => (_flag & PLS_CANCELED) == PLS_CANCELED;

    public void Exceptional()
    {
        if ((_flag & PLS_EXCEPTIONAL) != PLS_EXCEPTIONAL)
            _flag |= PLS_EXCEPTIONAL;
    }
    public void Break() => _flag = PLS_BROKEN;
    public void Stop() => _flag = PLS_STOPPED;
    public void Cancel() => _flag = PLS_CANCELED;
    public void Reset() => _flag = PLS_NONE;
}
#line default

/// <summary>
/// 控制程序流程
/// </summary>
public class ProState
{
    const int PLS_NONE = 0; // 初始化(构造就立马运行,将导致构造函数中也被检测,这是浪费性能及挖坑给自己的)
    const int PLS_RUN = 1;  // 运行
    const int PLS_BROKEN = 2;
    const int PLS_STOPPED = 4;
    const int PLS_CANCELED = 8;
    const int PLS_EXCEPTIONAL = 16; // 异常 用于附加状态

    private volatile int _flag = PLS_NONE;

    public bool IsNone => _flag == PLS_NONE;
    public bool IsRun => (_flag & PLS_RUN) == PLS_RUN;
    public bool IsBreak => (_flag & PLS_BROKEN) == PLS_BROKEN;
    public bool IsStop => (_flag & PLS_STOPPED) == PLS_STOPPED;
    public bool IsCancel => (_flag & PLS_CANCELED) == PLS_CANCELED;
    public bool IsExceptional => (_flag & PLS_EXCEPTIONAL) == PLS_EXCEPTIONAL;

    public void Exceptional()
    {
        if ((_flag & PLS_EXCEPTIONAL) != PLS_EXCEPTIONAL)
            _flag |= PLS_EXCEPTIONAL;
    }
    public void Break() => _flag = PLS_BROKEN;
    public void Stop() => _flag = PLS_STOPPED;
    public void Cancel() => _flag = PLS_CANCELED;
    public void Start() => _flag = PLS_RUN;
    public void None() => _flag = PLS_NONE;
}
#line default