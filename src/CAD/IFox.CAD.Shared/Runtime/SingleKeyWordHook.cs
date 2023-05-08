namespace IFoxCAD.Cad;
/// <summary>
/// 关键字不需要空格钩子
/// By DYH 20230508
/// </summary>
public class SingleKeyWordHook : IDisposable
{
    #region 私有字段
    /// <summary>
    /// 关键字合集
    /// </summary>
    private readonly HashSet<Keys> keyWords;
    private bool isResponsed;
    private bool working;
    private Keys key;
    #endregion

    #region 公共属性
    /// <summary>
    /// 上一个触发的关键字
    /// </summary>
    public Keys Key => key;
    /// <summary>
    /// 上一个触发的关键字字符串
    /// </summary>
    public string StringResult => key.ToString().ToUpper();
    /// <summary>
    /// 是否响应了
    /// </summary>
    public bool IsResponsed => isResponsed && working;
    #endregion

    #region 构造
    /// <summary>
    /// 单字母关键字免输回车钩子
    /// </summary>
    public SingleKeyWordHook()
    {
        isDisposed = false;
        isResponsed = false;
        keyWords = new HashSet<Keys>();
        key = Keys.None;
        working = true;
        Acap.PreTranslateMessage += Acap_PreTranslateMessage;
    }
    #endregion

    #region 方法
    /// <summary>
    /// 添加Keys
    /// </summary>
    /// <param name="values">Keys集合</param>
    public void AddKeys(params Keys[] values) => values.ForEach(value => keyWords.Add(value));
    /// <summary>
    /// 添加Keys
    /// </summary>
    /// <param name="keywordCollection">关键字集合</param>
    public void AddKeys(KeywordCollection keywordCollection)
    {
        foreach (Keyword item in keywordCollection)
        {
            if (item.LocalName.Length == 1)
            {
                Keys k = (Keys)item.LocalName.ToCharArray()[0];
                keyWords.Add(k);
            }
        }
    }
    /// <summary>
    /// 移除Keys
    /// </summary>
    /// <param name="values">Keys集合</param>
    public void Remove(params Keys[] values) => values.ForEach(value => keyWords.Remove(value));
    /// <summary>
    /// 清空Keys
    /// </summary>
    public void Clear() => keyWords.Clear();
    /// <summary>
    /// 复位响应状态，每个循环开始时使用
    /// </summary>
    public void Reset()
    {
        isResponsed = false;
    }
    /// <summary>
    /// 暂停工作
    /// </summary>
    public void Pause()
    {
        working = false;
    }
    /// <summary>
    /// 开始工作
    /// </summary>
    public void Working()
    {
        working = true;
    }
    #endregion

    #region 事件
    private void Acap_PreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
    {
        if (!working || e.Message.message != 256) return;
        var tempKey = IntPtr.Size == 4 ? (Keys)e.Message.wParam.ToInt32() : (Keys)e.Message.wParam.ToInt64();
        bool contains = keyWords.Contains(tempKey);
        if (contains || tempKey == Keys.ProcessKey)
        {
            // 标记为true，表示此按键已经被处理，Windows不会再进行处理
            e.Handled = true;
            if (contains)
                key = tempKey;
            if (!isResponsed)
            {
                // 此bool是防止按键被长按时出错
                isResponsed = true;
                // 这里选择发送回车或者ESC//ESC稳妥一些，但是要promptResult的判断顺序
                SingleKeyWordHook.KeyBoardSendKey(Keys.Escape);
            }
        }
    }
    #endregion

    #region Dispose
    private bool isDisposed;
    /// <summary>
    /// 已经销毁
    /// </summary>
    public bool IsDisposed => isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                Acap.PreTranslateMessage -= Acap_PreTranslateMessage;
                keyWords.Clear();
            }
            isDisposed = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
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