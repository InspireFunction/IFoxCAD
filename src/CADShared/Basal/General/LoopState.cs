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

    /// <summary>
    /// 获取一个值，指示循环是否正在运行。
    /// </summary>
    public bool IsRun => _flag == PLS_NONE;
    /// <summary>
    /// 获取一个值，指示循环是否遇到异常。
    /// </summary>
    public bool IsExceptional => (_flag & PLS_EXCEPTIONAL) == PLS_EXCEPTIONAL;
    /// <summary>
    /// 获取一个值，指示循环是否被中断。
    /// </summary>
    public bool IsBreak => (_flag & PLS_BROKEN) == PLS_BROKEN;
    /// <summary>
    /// 获取一个值，指示循环是否已停止。
    /// </summary>
    public bool IsStop => (_flag & PLS_STOPPED) == PLS_STOPPED;
    /// <summary>
    /// 获取一个值，指示循环是否已取消。
    /// </summary>
    public bool IsCancel => (_flag & PLS_CANCELED) == PLS_CANCELED;

    /// <summary>
    /// 设置循环遇到异常状态。
    /// </summary>
    public void Exceptional()
    {
        if ((_flag & PLS_EXCEPTIONAL) != PLS_EXCEPTIONAL)
            _flag |= PLS_EXCEPTIONAL;
    }
    /// <summary>
    /// 设置循环中断状态。
    /// </summary>
    public void Break() => _flag = PLS_BROKEN;
    /// <summary>
    /// 设置循环停止状态。
    /// </summary>
    public void Stop() => _flag = PLS_STOPPED;
    /// <summary>
    /// 设置循环取消状态。
    /// </summary>
    public void Cancel() => _flag = PLS_CANCELED;
    /// <summary>
    /// 重置循环状态。
    /// </summary>
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

    /// <summary>
    /// 获取一个值，指示状态是否为初始状态。
    /// </summary>
    public bool IsNone => _flag == PLS_NONE;
    /// <summary>
    /// 获取一个值，指示程序是否正在运行。
    /// </summary>
    public bool IsRun => (_flag & PLS_RUN) == PLS_RUN;
    /// <summary>
    /// 获取一个值，指示程序是否被中断。
    /// </summary>
    public bool IsBreak => (_flag & PLS_BROKEN) == PLS_BROKEN;
    /// <summary>
    /// 获取一个值，指示程序是否已停止。
    /// </summary>
    public bool IsStop => (_flag & PLS_STOPPED) == PLS_STOPPED;
    /// <summary>
    /// 获取一个值，指示程序是否已取消。
    /// </summary>
    public bool IsCancel => (_flag & PLS_CANCELED) == PLS_CANCELED;
    /// <summary>
    /// 获取一个值，指示程序是否遇到异常。
    /// </summary>
    public bool IsExceptional => (_flag & PLS_EXCEPTIONAL) == PLS_EXCEPTIONAL;

    /// <summary>
    /// 设置程序遇到异常状态。
    /// </summary>
    public void Exceptional()
    {
        if ((_flag & PLS_EXCEPTIONAL) != PLS_EXCEPTIONAL)
            _flag |= PLS_EXCEPTIONAL;
    }
    /// <summary>
    /// 设置程序中断状态。
    /// </summary>
    public void Break() => _flag = PLS_BROKEN;
    /// <summary>
    /// 设置程序停止状态。
    /// </summary>
    public void Stop() => _flag = PLS_STOPPED;
    /// <summary>
    /// 设置程序取消状态。
    /// </summary>
    public void Cancel() => _flag = PLS_CANCELED;
    /// <summary>
    /// 设置程序开始运行状态。
    /// </summary>
    public void Start() => _flag = PLS_RUN;
    /// <summary>
    /// 重置程序为初始状态。
    /// </summary>
    public void None() => _flag = PLS_NONE;
}
#line default