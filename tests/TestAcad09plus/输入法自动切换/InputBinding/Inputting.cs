#pragma warning disable 0169

/*
 * 0x01 赫思是键盘钩子:
 *      可以在08.+使用;
 * 0x02 InputBinding是切换输入法:
 *      目前仅高版本,
 *      08需要把文档栏工程的子类化拉取过来才可以使用
 */

namespace InputBinding;

using Autodesk.AutoCAD.Internal;
using System.Threading;
using System.Windows.Forms;
using static WindowsAPI;

public class Inputting
{
    static AppMessageFilter? _appMsgFilter = null;
    static bool _shiftYN1 = false;
    static bool _shiftYN2 = false;
    static readonly ManualResetEvent _mre1 = new(false);
    static readonly ManualResetEvent _mre2 = new(false);
    static readonly ManualResetEvent _mre3 = new(false);
    static readonly ManualResetEvent _mre4 = new(false);
    static readonly ManualResetEvent _mre5 = new(false);
    static int _iMode1 = -1;
    static int _iMode2 = -1;
    static int _iMode3 = -1;
    /// <summary>
    /// 是否卸载输入法切换
    /// </summary>
    static bool _unloadYN = false;
    /// <summary>
    /// 当前输入法的状态:true中文,false英文
    /// </summary>
    static bool _status = false;
    static long _time;

    static int _hanCount1;
    static int _hanCount1Add;

    /// <summary>
    /// 具有焦点的窗口的句柄
    /// </summary>
    static IntPtr _hwndFocus;
    /// <summary>
    /// 显示插入记号的窗口的句柄
    /// </summary>
    static IntPtr _hwndCaret;
    /// <summary>
    /// 捕获鼠标的窗口的句柄
    /// </summary>
    static IntPtr _hwndCapture;
    /// <summary>
    /// 线程内活动窗口的句柄
    /// </summary>
    static IntPtr _hwndActive;

    [CommandMethod(nameof(ModalDialog))]
    public void ModalDialog()
    {
        Acap.ShowModalDialog(new InputtingForm());
    }

    #region NewThread1
    //[IFoxInitialize] 需要自动加载时候启动它,但是它和赫思冲突
    public void InputBindingInitialize()
    {
        InputHelper.LoadConfig();
        Acap.PreTranslateMessage += Acap_PreTranslateMessage;
    }

    /// <summary>
    /// 载入和卸载输入法
    /// </summary>
    [CommandMethod(nameof(IFoxInput))]
    public static void IFoxInput()
    {
        if (!_unloadYN)
        {
            _unloadYN = true;
            Acap.PreTranslateMessage += Acap_PreTranslateMessage;
            Env.Printl("已卸载输入法自动切换");
        }
        else
        {
            _unloadYN = false;
            Acap.PreTranslateMessage += Acap_PreTranslateMessage;
            Env.Printl("已加载输入法自动切换");
        }
    }

    public static void Acap_PreTranslateMessage(object sender, PreTranslateMessageEventArgs e)
    {
        if (!_unloadYN)
        {
            _appMsgFilter = new();
            Application.AddMessageFilter(_appMsgFilter);
        }
        else
        {
            Application.RemoveMessageFilter(_appMsgFilter);
        }
        Acap.PreTranslateMessage -= Acap_PreTranslateMessage;
    }

    public class AppMessageFilter : IMessageFilter
    {
        bool AcadVersion19()
        {
            return Acap.Version.Major >= 19
                && InputVar.CmdActive
                && IsTrigger()
                && _hwndCapture == IntPtr.Zero
                && GetKeyboardLayout(GetCurrentThreadId()) < 0;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 515)
            {
                new Thread(new ThreadStart(NewThread1)).Start();
                _mre1.Set();
                return false;
            }

            if (m.Msg == 512 && _shiftYN1 && AcadVersion19())
            {
                _hanCount1Add = GetWindowTextLength(_hwndCaret);
                StringBuilder text = new(2 * _hanCount1Add);
                GetWindowText(_hwndFocus, text, text.Capacity);
                if (!text.ToString().Contains(InputVar.StatusBar))
                {
                    Utils.SetFocusToDwgView();
                    _shiftYN1 = false;
                }
                return false;
            }

            if (m.Msg != 256)
                return false;

            if (m.WParam != (IntPtr)229)
            {
                if (!_shiftYN1 && AcadVersion19())
                {
                    _hanCount1Add = GetWindowTextLength(_hwndCaret);
                    StringBuilder text = new(2 * _hanCount1Add);
                    GetWindowText(_hwndFocus, text, text.Capacity);
                    if (!text.ToString().Contains(InputVar.StatusBar))
                    {
                        Utils.SetFocusToDwgView();
                        _shiftYN1 = true;
                    }
                }
                return false;
            }

            if (_shiftYN2)
            {
                _shiftYN2 = false;
                return false;
            }

            IntPtr window = IntPtr.Zero;

            var virtualKey = ImmGetVirtualKey(m.HWnd);
            var vk = (int)virtualKey;
            if (vk != 16 && vk != 17 && vk != 18 && vk != 91 && InputVar.CmdActive)
            {
                bool containCmdStr = false;
                if (_hwndFocus == _hwndCaret)
                {
                    Env.Printl(InputVar.CmdStr);
                    StringBuilder text = new(2 * GetWindowTextLength(_hwndCaret));
                    GetWindowText(_hwndCaret, text, text.Capacity);
                    if (text.ToString().Contains(InputVar.CmdStr))
                        containCmdStr = true;
                }

                if (containCmdStr || IsTrigger())
                {
                    _hanCount1Add = GetWindowTextLength(_hwndCaret);
                    StringBuilder text = new(2 * _hanCount1Add);
                    GetWindowText(_hwndFocus, text, text.Capacity);

                    if (text.ToString().Contains(InputVar.StatusBar))
                        return false;

                    ImmGetConversionStatus(ImmGetContext(_hwndFocus), out _iMode3, out _);
                    if (_hwndCapture == IntPtr.Zero)
                        Utils.SetFocusToDwgView();

                    PressKey();
                    if (_hwndFocus == _hwndCaret)
                    {
                        if (containCmdStr)
                            window = _hwndCaret;
                        else if (!InputVar.DY)
                        {
                            var doc = Acap.DocumentManager.MdiActiveDocument;
                            window = GetWindow(GetTopWindow(doc.Window.Handle), 2U);
                        }
                    }
                    else if (!InputVar.DY || _hwndCapture != IntPtr.Zero)
                        window = m.HWnd;
                }
            }
            else if (InputVar.Windows10)
                window = m.HWnd;

            if (window != IntPtr.Zero)
            {
                PostMessage(window, m.Msg, virtualKey, m.LParam);
                return true;
            }
            return false;
        }
    }

    public static void NewThread1()
    {
        _mre1.WaitOne();
        Thread.Sleep(200);
        if (InputVar.DK)
        {
            InputVar.DK = false;
            return;
        }

        if ((!InputVar.CmdActive || IsTrigger())
            && (InputVar.CmdActive || !InputVar.TextEditor || (_hwndCaret == IntPtr.Zero))
            && (InputVar.CmdActive || !InputVar.TextEditor || _hwndActive == _hwndFocus))
            return;

        _hanCount1Add = GetWindowTextLength(_hwndFocus);
        StringBuilder text = new(2 * _hanCount1Add);
        GetWindowText(_hwndFocus, text, text.Capacity);

        var textStr = text.ToString();
        if (textStr != string.Empty && !textStr.Contains(InputVar.StatusBar))
        {
            var context = ImmGetContext(_hwndFocus);
            ImmGetConversionStatus(context, out _iMode1, out _);
            _status = ImmGetOpenStatus(context);

            _hanCount1 = GetHanNum(textStr);
            var length = _hanCount1Add - 2 * _hanCount1;

            if (InputVar.Windows10 || InputVar.CK || InputVar.CS || InputVar.AS)
            {
                if (!InputVar.CMD)
                {
                    if ((_status && length > _hanCount1) ||
                       (!_status && length <= _hanCount1))
                    {
                        PressKey();
                        InputVar.CMD = false;
                    }
                }
                else
                    InputVar.CMD = false;
            }
            else if (InputVar.SF || InputVar.CT)
            {
                if (_iMode3 != -1)
                {
                    if ((length <= _hanCount1 && _iMode3 == _iMode1) ||
                        (length > _hanCount1 && _iMode3 > _iMode1))
                        InputVar.CMD = false;
                }
                else if (!InputVar.CMD)
                {
                    PressKey();
                    new Thread(new ThreadStart(NewThread100)).Start();
                    _mre2.Set();
                }
            }
        }
    }
    #endregion


    /// <summary>
    /// lisp Input判断文字中中英文的数量决定切换中英文
    /// </summary>
    /// <param name="rb">文字</param>
    [LispFunction(nameof(InputLispDoubleClick))]
    public static ResultBuffer InputLispDoubleClick(ResultBuffer rb)
    {
        if (rb is null || _unloadYN)
            return null!;

        InputVar.DK = true;
        string str = rb.AsArray()[0].ToString();

        var hanCount = GetHanNum(str);
        var hanCountAdd = str.Length + hanCount;
        var length = hanCountAdd - 2 * hanCount;

        IsTrigger();
        var context = ImmGetContext(_hwndFocus);
        ImmGetConversionStatus(context, out _iMode1, out _);
        _status = ImmGetOpenStatus(context);

        new Thread(() => {
            _mre3.WaitOne();
            Thread.Sleep(50);
            if (InputVar.CMD)
            {
                InputVar.CMD = false;
                return;
            }

            Thread.Sleep(50);
            if (InputVar.Windows10 || InputVar.CK || InputVar.CS || InputVar.AS)
            {
                if (!(_status && length > hanCount))
                    if (_status || length > hanCount)
                        return;
                PressKey();
            }
            else
            {
                if (!InputVar.SF
                    && !InputVar.CT || length <= hanCount
                    && _iMode3 == _iMode1
                    && _iMode3 != -1 || length > hanCount
                    && _iMode3 > _iMode1
                    && _iMode3 != -1)
                    return;

                PressKey();
                _hanCount1Add = hanCountAdd;
                _hanCount1 = hanCount;
                new Thread(new ThreadStart(NewThread100)).Start();
                _mre2.Set();
            }
        }).Start();

        _mre3.Set();
        return null!;
    }

    /// <summary>
    /// lisp Input命令开始
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    [LispFunction(nameof(InputLispStart))]
    public static ResultBuffer InputLispStart(ResultBuffer rb)
    {
        if (rb is null || _unloadYN)
            return null!;

        string str = rb.AsArray()[0].ToString();
        if (str != "True")
            return null!;

        InputVar.CMD = true;
        var hanCount = GetHanNum(str);
        var hanCountAdd = str.Length + hanCount;
        var length = hanCountAdd - 2 * hanCount;

        new Thread(() => {
            _mre4.WaitOne();
            Thread.Sleep(100);

            IsTrigger();
            var context = ImmGetContext(_hwndFocus);
            ImmGetConversionStatus(context, out _iMode1, out _);
            _status = ImmGetOpenStatus(context);

            if (InputVar.Windows10 || InputVar.CK || InputVar.CS || InputVar.AS)
            {
                if (_status && length > hanCount)
                {
                    Thread.Sleep(100);
                    PressKey();
                    Thread.Sleep(500);
                    InputVar.CMD = false;
                }
                else if (!_status && length <= hanCount)
                {
                    Thread.Sleep(100);
                    PressKey();
                    Thread.Sleep(500);
                    InputVar.CMD = false;
                }
                else
                {
                    Thread.Sleep(500);
                    InputVar.CMD = false;
                }
            }
            else
            {
                if (!InputVar.SF && !InputVar.CT)
                    return;
                if (length <= hanCount && _iMode3 == _iMode1 && _iMode3 != -1)
                {
                    Thread.Sleep(500);
                    InputVar.CMD = false;
                }
                else if (length > hanCount && _iMode3 > _iMode1 && _iMode3 != -1)
                {
                    Thread.Sleep(500);
                    InputVar.CMD = false;
                }
                else
                {
                    Thread.Sleep(100);
                    PressKey();
                    _hanCount1Add = hanCountAdd;
                    _hanCount1 = hanCount;
                    new Thread(new ThreadStart(NewThread100)).Start();
                    _mre2.Set();
                }
            }
        }).Start();

        _mre4.Set();
        return null!;
    }

    /// <summary>
    /// lisp Input命令结束
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    [LispFunction(nameof(InputLispEnd))]
    public static ResultBuffer InputLispEnd(ResultBuffer rb)
    {
        if (rb is null || _unloadYN)
            return null!;

        string str = rb.AsArray()[0].ToString();
        if (str != "True")
        {
            var hanCount = GetHanNum(str);
            var hanCountAdd = str.Length + hanCount;
            var length = hanCountAdd - 2 * hanCount;

            new Thread(() => {
                _mre5.WaitOne();
                Thread.Sleep(100);

                IsTrigger();
                var context = ImmGetContext(_hwndFocus);
                ImmGetConversionStatus(context, out _iMode1, out _);
                _status = ImmGetOpenStatus(context);

                if (InputVar.Windows10 || InputVar.CK || InputVar.CS || InputVar.AS)
                {
                    if (_status && length > hanCount)
                        PressKey();
                    if (_status || length > hanCount)
                        return;
                    PressKey();
                }
                else
                {
                    if (!InputVar.SF
                        && !InputVar.CT || length <= hanCount
                        && _iMode3 == _iMode1
                        && _iMode3 != -1 || length > hanCount
                        && _iMode3 > _iMode1
                        && _iMode3 != -1)
                        return;

                    PressKey();
                    _hanCount1Add = hanCountAdd;
                    _hanCount1 = hanCount;
                    new Thread(new ThreadStart(NewThread100)).Start();
                    _mre2.Set();
                }
            }).Start();


            _mre5.Set();
        }
        return null!;
    }

    /// <summary>
    /// 处理按键事件
    /// </summary>
    public static void PressKey()
    {
        if (InputVar.SF)
        {
            if (_iMode3 == -1)
                Thread.Sleep(400);
            KeybdEvent(16, 0, 0, 0);
            KeybdEvent(16, 0, 2, 0);
        }
        else if (InputVar.CT)
        {
            if (_iMode3 == -1)
                Thread.Sleep(400);
            KeybdEvent(17, 0, 0, 0);
            KeybdEvent(17, 0, 2, 0);
        }
        else if (InputVar.CK)
        {
            if (_iMode3 == -1)
                Thread.Sleep(400);
            KeybdEvent(17, 0, 0, 0);
            KeybdEvent(32, 0, 0, 0);
            KeybdEvent(32, 0, 2, 0);
            KeybdEvent(17, 0, 2, 0);
            if (GetKeyState(20) != 1)
                return;
            KeybdEvent(20, 0, 0, 0);
            KeybdEvent(20, 0, 2, 0);
        }
        else if (InputVar.CS)
        {
            if (_iMode3 == -1)
                Thread.Sleep(400);
            KeybdEvent(16, 0, 0, 0);
            KeybdEvent(17, 0, 0, 0);
            KeybdEvent(17, 0, 2, 0);
            KeybdEvent(16, 0, 2, 0);
            if (GetKeyState(20) != 1)
                return;
            KeybdEvent(20, 0, 0, 0);
            KeybdEvent(20, 0, 2, 0);
        }
        else if (InputVar.AS)
        {
            if (_iMode3 == -1)
                Thread.Sleep(400);
            KeybdEvent(91, 0, 0, 0);
            KeybdEvent(32, 0, 0, 0);
            KeybdEvent(32, 0, 2, 0);
            KeybdEvent(91, 0, 2, 0);
            if (GetKeyState(20) != 1)
                return;
            KeybdEvent(20, 0, 0, 0);
            KeybdEvent(20, 0, 2, 0);
        }
        else
        {
            var t1 = DateTime.Now.Ticks / 10000L;
            _shiftYN2 = Math.Abs(_time - t1) < 80L;
            _time = t1;
        }
    }

    public static void NewThread100()
    {
        _mre2.WaitOne();
        Thread.Sleep(100);

        IsTrigger();
        var context = ImmGetContext(_hwndFocus);
        ImmGetConversionStatus(context, out _iMode2, out _);
        _status = ImmGetOpenStatus(context);

        var length = _hanCount1Add - 2 * _hanCount1;

        if (length <= _hanCount1
            && _iMode1 > _iMode2
            && _iMode1 != _iMode3)
            PressKey();

        if (length > _hanCount1
            && _iMode1 < _iMode2
            && _iMode1 != _iMode3)
            PressKey();

        if (_iMode1 > _iMode2 && _iMode1 > _iMode3)
            _iMode3 = _iMode1;

        if (_iMode1 < _iMode2 && _iMode3 < _iMode2)
            _iMode3 = _iMode2;

        Thread.Sleep(300);
        InputVar.DK = InputVar.CMD = false;
    }

    /// <summary>
    /// 窗体是否激活状态
    /// </summary>
    /// <returns></returns>
    public static bool IsTrigger()
    {
        var pid = GetWindowThreadProcessId(GetForegroundWindow(), out uint lpdwProcessId);
        var lpgui = GuiThreadInfo.Create(pid);
        _hwndActive = lpgui.hwndActive;
        _hwndFocus = lpgui.hwndFocus;
        _hwndCaret = lpgui.hwndCaret;
        _hwndCapture = lpgui.hwndCapture;
        return lpgui.flags == 0;
    }

    /// <summary>
    /// 判断汉字的数量
    /// </summary>
    /// <param name="str">文字</param>
    /// <returns></returns>
    public static int GetHanNum(string str)
    {
        // TODO 20221025 此处用正则效率低下,应该改用ASCII
        int num = 0;
        Regex regex = new(@"^[\u4E00-\u9FA5]{0,}$"); //匹配中文字符

        for (int index = 0; index < str.Length; ++index)
            if (regex.IsMatch(str[index].ToString()))
                ++num;

        return num;
    }
}

public class InputVar
{
    public static bool DY;
    public static bool SF;
    public static bool CT;
    public static bool CK;
    public static bool CS;
    public static bool AS;
    public static bool SL;
    public static bool DK;
    public static bool CMD;

    public static bool Windows10 => Environment.OSVersion.Version.CompareTo(new Version("6.2")) >= 0;
    public static string StatusBar = "StatusBar";
    public static string CmdStr = "命令:";

    //[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool CmdActive => (short)Acap.GetSystemVariable("cmdactive") == 0;
    public static bool TextEditor => (short)Acap.GetSystemVariable("texteditor") == 0;
}

public class InputHelper
{
    /// <summary>
    /// 写入.ini配置
    /// </summary>
    [DllImport("kernel32.dll")]
    public static extern long WritePrivateProfileString(string section, string key, string value, string filepath);

    /// <summary>
    /// 读取.ini配置
    /// </summary>
    [DllImport("kernel32.dll")]
    public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string? filepath);

    /// <summary>
    /// 返回键值对的值
    /// </summary>
    /// <param name="section">配置名称</param>
    /// <param name="key">键值对的键</param>
    /// <returns></returns>
    public static string GetValue(string section, string key)
    {
        StringBuilder result = new();
        GetPrivateProfileString(section, key, "", result, 1024, InputHelper.SectionFile);
        return result.ToString();
    }

    public static string SectionFile => $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\{Section}.ini";
    public const string Section = "输入法配置";

    /// <summary>
    /// 加载配置
    /// </summary>
    public static void LoadConfig()
    {
        if (!File.Exists(Section))
        {
            InputVar.SF = true;
            return;
        }
        var str = GetValue(Section, "Shift切换");
        InputVar.SF = str == "True";
        str = GetValue(Section, "Ctrl切换");
        InputVar.CT = str == "True";
        str = GetValue(Section, "Ctrl+空格");
        InputVar.CK = str == "True";
        str = GetValue(Section, "Ctrl+Shift");
        InputVar.CS = str == "True";
        str = GetValue(Section, "Win+空格");
        InputVar.AS = str == "True";
        str = GetValue(Section, "去多余字母");
        InputVar.DY = str == "True";
        str = GetValue(Section, "增加字母");
        InputVar.SL = str == "True";
    }
}