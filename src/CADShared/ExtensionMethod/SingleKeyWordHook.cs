using Keys = System.Windows.Forms.Keys;

namespace IFoxCAD.Cad;

/// <summary>
/// 关键字不需要空格钩子
/// By DYH 20230508
/// </summary>
public sealed class SingleKeyWordHook : IDisposable
{
    #region 静态字段

    private static readonly string _enterStr = Convert.ToChar(Keys.Enter).ToString();
    private static readonly string _backStr = Convert.ToChar(Keys.Back).ToString();

    #endregion

    #region 私有字段

    /// <summary>
    /// 关键字合集
    /// </summary>
    private readonly HashSet<Keys> _keyWords;

    private bool _isResponsed;
    private bool _working;
    private Keys _key;
    private readonly SingleKeyWordWorkType _workType;

    #endregion

    #region 公共属性

    /// <summary>
    /// 上一个触发的关键字
    /// </summary>
    public Keys Key => _key;

    /// <summary>
    /// 上一个触发的关键字字符串
    /// </summary>
    public string StringResult => _key.ToString().ToUpper();

    /// <summary>
    /// 是否响应了
    /// </summary>
    public bool IsResponsed => _isResponsed && _working;

    #endregion

    #region 构造

    /// <summary>
    /// 单字母关键字免输回车钩子
    /// </summary>
    /// <param name="workType">使用esc(填false则使用回车)</param>
    public SingleKeyWordHook(SingleKeyWordWorkType workType = SingleKeyWordWorkType.ESCAPE)
    {
        IsDisposed = false;
        _isResponsed = false;
        _keyWords = [];
        _key = Keys.None;
        _working = true;
        _workType = workType;
        Acaop.PreTranslateMessage -= Acap_PreTranslateMessage;
        Acaop.PreTranslateMessage += Acap_PreTranslateMessage;
    }

    #endregion

    #region 方法

    /// <summary>
    /// 添加Keys
    /// </summary>
    /// <param name="values">Keys集合</param>
    public void AddKeys(params Keys[] values) => values.ForEach(value => _keyWords.Add(value));

    /// <summary>
    /// 添加Keys
    /// </summary>
    /// <param name="keywordCollection">关键字集合</param>
    public void AddKeys(KeywordCollection keywordCollection)
    {
        foreach (Keyword item in keywordCollection)
        {
            if (item.LocalName.Length != 1) continue;
            var k = (Keys)item.LocalName[0];
            _keyWords.Add(k);
        }
    }

    /// <summary>
    /// 移除Keys
    /// </summary>
    /// <param name="values">Keys集合</param>
    public void Remove(params Keys[] values) => values.ForEach(value => _keyWords.Remove(value));

    /// <summary>
    /// 清空Keys
    /// </summary>
    public void Clear() => _keyWords.Clear();

    /// <summary>
    /// 复位响应状态，每个循环开始时使用
    /// </summary>
    public void Reset()
    {
        _key = Keys.None;
        _isResponsed = false;
    }

    /// <summary>
    /// 暂停工作
    /// </summary>
    public void Pause()
    {
        _working = false;
    }

    /// <summary>
    /// 开始工作
    /// </summary>
    public void Working()
    {
        _working = true;
    }

    #endregion

    #region 事件

    private void Acap_PreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
    {
        if (!_working || e.Message.message != 256) return;
        var tempKey = IntPtr.Size == 4 ? (Keys)e.Message.wParam.ToInt32() : (Keys)e.Message.wParam.ToInt64();
        var contains = _keyWords.Contains(tempKey);
        if (!contains) return;
        
        // 标记为true，表示此按键已经被处理，Windows不会再进行处理
        if (_workType != SingleKeyWordWorkType.ENTER)
        {
            e.Handled = true;
        }

        if (IsResponsed) return; //放 e.Handled 后是避免在非 ENTER 模式时长按造成动态输入框偶发性闪现关键字以至轻微卡顿问题

        _key = tempKey;
        _isResponsed = true; // 此bool是防止按键被长按时出错
        
        switch (_workType)
        {
            case SingleKeyWordWorkType.ESCAPE:
                // ESC稳妥一些，但是要判断promptResult的顺序
                KeyBoardSendKey(Keys.Escape);
                break;
            case SingleKeyWordWorkType.ENTER:
                KeyBoardSendKey(Keys.Enter);
                break;
            case SingleKeyWordWorkType.WRITE_LINE:
                Utils.SetFocusToDwgView(); // 恢复焦点（如果前面关键字输入错误便会将焦点移至动态输入框）
                Utils.WriteToCommandLine(Convert.ToChar(_key) + _enterStr);
                break;
        }
    }

    #endregion

    #region Dispose

    /// <summary>
    /// 已经销毁
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 拆除事件
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        Acaop.PreTranslateMessage -= Acap_PreTranslateMessage;
        if (disposing)
        {
            _keyWords.Clear();
        }

        IsDisposed = true;
    }

    /// <summary>
    /// 析构里把事件拆了
    /// </summary>
    ~SingleKeyWordHook()
    {
        Dispose(false);
    }

    /// <summary>
    /// 拆除事件并清空关键字
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 静态方法

    /// <summary>
    /// 发送按键
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bScan"></param>
    /// <param name="dwFlags"></param>
    /// <param name="dwExtraInfo"></param>
    private static void KeyBoardSendKey(Keys key, byte bScan = 0, uint dwFlags = 0, uint dwExtraInfo = 0)
    {
        keybd_event(key, bScan, dwFlags, dwExtraInfo);
        keybd_event(key, bScan, 2, dwExtraInfo);
    }

    [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
    private static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    #endregion
}
/// <summary>
/// 单文本关键字钩子扩展
/// </summary>
public static class SingleKeywordHookEx
{
    /// <summary>
    /// 钩取单文本关键字
    /// </summary>
    /// <param name="keywords">关键字集合</param>
    /// <param name="workType">工作模式</param>
    /// <returns>单文本关键字类(需要using)</returns>
    public static SingleKeyWordHook HookSingleKeyword(this KeywordCollection keywords, SingleKeyWordWorkType workType = SingleKeyWordWorkType.WRITE_LINE)
    {
        var singleKeyWordHook = new SingleKeyWordHook(workType);
        singleKeyWordHook.AddKeys(keywords);
        return singleKeyWordHook;
    }
}

/// <summary>
/// 单关键字工作模式
/// </summary>
public enum SingleKeyWordWorkType : byte
{
    /// <summary>
    /// Esc模式
    /// </summary>
    ESCAPE,
    
    /// <summary>
    /// Enter模式
    /// </summary>
    ENTER,
    
    /// <summary>
    /// Write Line 模式
    /// </summary>
    WRITE_LINE,
}
