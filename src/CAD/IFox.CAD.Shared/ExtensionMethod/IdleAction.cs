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
    private static bool alreadyLoad = false;
    /// <summary>
    /// 委托列表
    /// </summary>
    private static readonly List<Action> _actions = new();
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
        _actions.Add(action);
        if (!alreadyLoad)
        {
            Acap.Idle -= Acap_Idle;
            Acap.Idle += Acap_Idle;
            alreadyLoad = true;
        }
    }
    /// <summary>
    /// 空闲处理事件
    /// </summary>
    /// <param name="sender">Acap</param>
    /// <param name="e">事件参数</param>
    private static void Acap_Idle(object sender, EventArgs e)
    {
        if (Count == 0)
        {
            alreadyLoad = false;
            Acap.Idle -= Acap_Idle;
            return;
        }
        try
        {
            _actions[0]?.Invoke();
        }
        finally
        {
            _actions.RemoveAt(0);
        }
        System.Windows.Forms.Cursor.Position = System.Windows.Forms.Cursor.Position;
    }
}
