#if !NET35
using Cursor = System.Windows.Forms.Cursor;

namespace IFoxCAD.Cad;

/// <summary>
/// 空闲执行
/// by DYH
/// 20230114
/// </summary>
public static class IdleAction
{
    /// <summary>
    /// 是否已经加载
    /// </summary>
    private static bool alreadyLoad;

    /// <summary>
    /// 委托列表
    /// </summary>
    private static readonly Queue<Action> _actions = new();

    /// <summary>
    /// 未处理的委托数量
    /// </summary>
    public static int Count => _actions.Count;

    /// <summary>
    /// 添加空闲执行委托
    /// </summary>
    /// <param name="action">委托</param>
    public static void Add(Action action)
    {
        _actions.Enqueue(action);
        if (alreadyLoad)
            return;
        Acaop.Idle -= Acap_Idle;
        Acaop.Idle += Acap_Idle;
        alreadyLoad = true;
    }

    /// <summary>
    /// 空闲处理事件
    /// </summary>
    /// <param name="sender">Acap</param>
    /// <param name="e">事件参数</param>
    private static void Acap_Idle(object? sender, EventArgs e)
    {
        if (Count == 0)
        {
            alreadyLoad = false;
            Acaop.Idle -= Acap_Idle;
            return;
        }

        try
        {
            _actions.Dequeue()?.Invoke();
        }
        catch
        {
            // ignored
        }

        Cursor.Position = Cursor.Position;
    }
}
#endif