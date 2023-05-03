#if true
namespace IFoxCAD.Basal;

// https://blog.csdn.net/qq_43812868/article/details/108587936
[Flags]
public enum TH32CS : uint
{
    /// <summary>
    /// 原因在于如果不采用改参数的话,有可能快照会占用整个堆的空间
    /// </summary>
    TH32CS_SNAPNOHEAPS = 0x40000000,
    /// <summary>
    /// 声明快照句柄是可继承的
    /// </summary>
    TH32CS_INHERIT = 0x80000000,
    /// <summary>
    /// 在快照中包含在th32ProcessID中指定的进程的所有的堆
    /// </summary>
    TH32CS_SNAPHEAPLIST = 0x00000001,
    /// <summary>
    /// 在快照中包含系统中所有的进程
    /// </summary>
    TH32CS_SNAPPROCESS = 0x00000002,
    /// <summary>
    /// 在快照中包含系统中所有的线程
    /// </summary>
    TH32CS_SNAPTHREAD = 0x00000004,
    /// <summary>
    /// 在快照中包含在th32ProcessID中指定的进程的所有的模块
    /// </summary>
    TH32CS_SNAPMODULE = 0x00000008,
    /// <summary>
    /// 在快照中包含系统中所有的进程和线程
    /// </summary>
    TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD | TH32CS_SNAPMODULE,
}

/// <summary>
/// 设置的钩子类型
/// </summary>
[Flags]
public enum HookType : int
{
    /// <summary>
    /// WH_MSGFILTER 和 WH_SYSMSGFILTER Hooks使我们可以监视菜单,滚动
    /// 条,消息框,对话框消息并且发现用户使用ALT+TAB or ALT+ESC 组合键切换窗口。
    /// WH_MSGFILTER Hook只能监视传递到菜单,滚动条,消息框的消息,以及传递到通
    /// 过安装了Hook子过程的应用程序建立的对话框的消息。WH_SYSMSGFILTER Hook
    /// 监视所有应用程序消息。
    ///
    /// WH_MSGFILTER 和 WH_SYSMSGFILTER Hooks使我们可以在模式循环期间
    /// 过滤消息,这等价于在主消息循环中过滤消息。
    ///
    /// 通过调用CallMsgFilter function可以直接的调用WH_MSGFILTER Hook。通过使用这
    /// 个函数,应用程序能够在模式循环期间使用相同的代码去过滤消息,如同在主消息循
    /// 环里一样
    /// </summary>
    WH_MSGFILTER = -1,
    /// <summary>
    /// WH_JOURNALRECORD Hook用来监视和记录输入事件。典型的,可以使用这
    /// 个Hook记录连续的鼠标和键盘事件,然后通过使用WH_JOURNALPLAYBACK Hook
    /// 来回放。WH_JOURNALRECORD Hook是全局Hook,它不能象线程特定Hook一样
    /// 使用。WH_JOURNALRECORD是system-wide local hooks,它们不会被注射到任何行
    /// 程地址空间
    /// </summary>
    WH_JOURNALRECORD = 0,
    /// <summary>
    /// WH_JOURNALPLAYBACK Hook使应用程序可以插入消息到系统消息队列。可
    /// 以使用这个Hook回放通过使用WH_JOURNALRECORD Hook记录下来的连续的鼠
    /// 标和键盘事件。只要WH_JOURNALPLAYBACK Hook已经安装,正常的鼠标和键盘
    /// 事件就是无效的。WH_JOURNALPLAYBACK Hook是全局Hook,它不能象线程特定
    /// Hook一样使用。WH_JOURNALPLAYBACK Hook返回超时值,这个值告诉系统在处
    /// 理来自回放Hook当前消息之前需要等待多长时间（毫秒）。这就使Hook可以控制实
    /// 时事件的回放。WH_JOURNALPLAYBACK是system-wide local hooks,它们不会被
    /// 注射到任何行程地址空间
    /// </summary>
    WH_JOURNALPLAYBACK = 1,
    /// <summary>
    /// 在应用程序中,WH_KEYBOARD Hook用来监视WM_KEYDOWN and
    /// WM_KEYUP消息,这些消息通过GetMessage or PeekMessage function返回。可以使
    /// 用这个Hook来监视输入到消息队列中的键盘消息
    /// </summary>
    WH_KEYBOARD = 2,
    /// <summary>
    /// 应用程序使用WH_GETMESSAGE Hook来监视从GetMessage or PeekMessage函
    /// 数返回的消息。你可以使用WH_GETMESSAGE Hook去监视鼠标和键盘输入,以及
    /// 其它发送到消息队列中的消息
    /// </summary>
    WH_GETMESSAGE = 3,
    /// <summary>
    /// 监视发送到窗口过程的消息,系统在消息发送到接收窗口过程之前调用
    /// </summary>
    WH_CALLWNDPROC = 4,
    /// <summary>
    /// 在以下事件之前,系统都会调用WH_CBT Hook子过程,这些事件包括：
    /// 1. 激活,建立,销毁,最小化,最大化,移动,改变尺寸等窗口事件；
    /// 2. 完成系统指令；
    /// 3. 来自系统消息队列中的移动鼠标,键盘事件；
    /// 4. 设置输入焦点事件；
    /// 5. 同步系统消息队列事件。
    /// Hook子过程的返回值确定系统是否允许或者防止这些操作中的一个
    /// </summary>
    WH_CBT = 5,
    /// <summary>
    /// WH_MSGFILTER 和 WH_SYSMSGFILTER Hooks使我们可以监视菜单,滚动
    /// 条,消息框,对话框消息并且发现用户使用ALT+TAB or ALT+ESC 组合键切换窗口。
    /// WH_MSGFILTER Hook只能监视传递到菜单,滚动条,消息框的消息,以及传递到通
    /// 过安装了Hook子过程的应用程序建立的对话框的消息。WH_SYSMSGFILTER Hook
    /// 监视所有应用程序消息。
    ///
    /// WH_MSGFILTER 和 WH_SYSMSGFILTER Hooks使我们可以在模式循环期间
    /// 过滤消息,这等价于在主消息循环中过滤消息。
    ///
    /// 通过调用CallMsgFilter function可以直接的调用WH_MSGFILTER Hook。通过使用这
    /// 个函数,应用程序能够在模式循环期间使用相同的代码去过滤消息,如同在主消息循
    /// 环里一样
    /// </summary>
    WH_SYSMSGFILTER = 6,
    /// <summary>
    /// WH_MOUSE Hook监视从GetMessage 或者 PeekMessage 函数返回的鼠标消息。
    /// 使用这个Hook监视输入到消息队列中的鼠标消息
    /// </summary>
    WH_MOUSE = 7,
    /// <summary>
    /// 当调用GetMessage 或 PeekMessage 来从消息队列种查询非鼠标、键盘消息时
    /// </summary>
    WH_HARDWARE = 8,
    /// <summary>
    /// 在系统调用系统中与其它Hook关联的Hook子过程之前,系统会调用
    /// WH_DEBUG Hook子过程。你可以使用这个Hook来决定是否允许系统调用与其它
    /// Hook关联的Hook子过程
    /// </summary>
    WH_DEBUG = 9,
    /// <summary>
    /// 外壳应用程序可以使用WH_SHELL Hook去接收重要的通知。当外壳应用程序是
    /// 激活的并且当顶层窗口建立或者销毁时,系统调用WH_SHELL Hook子过程。
    /// WH_SHELL 共有５钟情况：
    /// 1. 只要有个top-level、unowned 窗口被产生、起作用、或是被摧毁；
    /// 2. 当Taskbar需要重画某个按钮；
    /// 3. 当系统需要显示关于Taskbar的一个程序的最小化形式；
    /// 4. 当目前的键盘布局状态改变；
    /// 5. 当使用者按Ctrl+Esc去执行Task Manager（或相同级别的程序）。
    ///
    /// 按照惯例,外壳应用程序都不接收WH_SHELL消息。所以,在应用程序能够接
    /// 收WH_SHELL消息之前,应用程序必须调用SystemParametersInfo function注册它自
    /// 己
    /// </summary>
    WH_SHELL = 10,
    /// <summary>
    /// 当应用程序的前台线程处于空闲状态时,可以使用WH_FOREGROUNDIDLE
    /// Hook执行低优先级的任务。当应用程序的前台线程大概要变成空闲状态时,系统就
    /// 会调用WH_FOREGROUNDIDLE Hook子过程
    /// </summary>
    WH_FOREGROUNDIDLE = 11,
    /// <summary>
    /// 监视发送到窗口过程的消息,系统在消息发送到接收窗口过程之后调用
    /// </summary>
    WH_CALLWNDPROCRET = 12,
    /// <summary>
    /// 监视键盘消息
    /// </summary>
    WH_KEYBOARD_LL = 13,
    /// <summary>
    /// 监视鼠标消息
    /// </summary>
    WH_MOUSE_LL = 14
}

/// <summary>
/// 消息类型
/// 作为SendMessage和PostMessage的参数
/// </summary>
[Flags]
public enum WM : uint
{
    /// <summary>
    /// 创建一个窗口
    /// </summary>
    WM_CREATE = 0x01,
    /// <summary>
    /// 当一个窗口被破坏时发送
    /// </summary>
    WM_DESTROY = 0x02,
    /// <summary>
    /// 移动一个窗口
    /// </summary>
    WM_MOVE = 0x03,
    /// <summary>
    /// 改变一个窗口的大小
    /// </summary>
    WM_SIZE = 0x05,
    /// <summary>
    /// 一个窗口被激活或失去激活状态
    /// </summary>
    WM_ACTIVATE = 0x06,
    /// <summary>
    /// 一个窗口获得焦点
    /// </summary>
    WM_SETFOCUS = 0x07,
    /// <summary>
    /// 一个窗口失去焦点
    /// </summary>
    WM_KILLFOCUS = 0x08,
    /// <summary>
    /// 一个窗口改变成Enable状态
    /// </summary>
    WM_ENABLE = 0x0A,
    /// <summary>
    /// 设置窗口是否能重画
    /// </summary>
    WM_SETREDRAW = 0x0B,
    /// <summary>
    /// 应用程序发送此消息来设置一个窗口的文本
    /// </summary>
    WM_SETTEXT = 0x0C,
    /// <summary>
    /// 应用程序发送此消息来复制对应窗口的文本到缓冲区
    /// </summary>
    WM_GETTEXT = 0x0D,
    /// <summary>
    /// 得到与一个窗口有关的文本的长度（不包含空字符）
    /// </summary>
    WM_GETTEXTLENGTH = 0x0E,
    /// <summary>
    /// 要求一个窗口重画自己
    /// </summary>
    WM_PAINT = 0x0F,
    /// <summary>
    /// 当一个窗口或应用程序要关闭时发送一个信号
    /// </summary>
    WM_CLOSE = 0x10,
    /// <summary>
    /// 当用户选择结束对话框或程序自己调用ExitWindows函数
    /// </summary>
    WM_QUERYENDSESSION = 0x11,
    /// <summary>
    /// 用来结束程序运行
    /// </summary>
    WM_QUIT = 0x12,
    /// <summary>
    /// 当用户窗口恢复以前的大小位置时,把此消息发送给某个图标
    /// </summary>
    WM_QUERYOPEN = 0x13,
    /// <summary>
    /// 当窗口背景必须被擦除时（例在窗口改变大小时）
    /// </summary>
    WM_ERASEBKGND = 0x14,
    /// <summary>
    /// 当系统颜色改变时,发送此消息给所有顶级窗口
    /// </summary>
    WM_SYSCOLORCHANGE = 0x15,
    /// <summary>
    /// 当系统进程发出WM_QUERYENDSESSION消息后,此消息发送给应用程序,通知它对话是否结束
    /// </summary>
    WM_ENDSESSION = 0x16,
    /// <summary>
    /// 当隐藏或显示窗口是发送此消息给这个窗口
    /// </summary>
    WM_SHOWWINDOW = 0x18,
    /// <summary>
    /// 发此消息给应用程序哪个窗口是激活的,哪个是非激活的
    /// </summary>
    WM_ACTIVATEAPP = 0x1C,
    /// <summary>
    /// 当系统的字体资源库变化时发送此消息给所有顶级窗口
    /// </summary>
    WM_FONTCHANGE = 0x1D,
    /// <summary>
    /// 当系统的时间变化时发送此消息给所有顶级窗口
    /// </summary>
    WM_TIMECHANGE = 0x1E,
    /// <summary>
    /// 发送此消息来取消某种正在进行的摸态（操作）
    /// </summary>
    WM_CANCELMODE = 0x1F,
    /// <summary>
    /// 如果鼠标引起光标在某个窗口中移动且鼠标输入没有被捕获时,就发消息给某个窗口
    /// </summary>
    WM_SETCURSOR = 0x20,
    /// <summary>
    /// 当光标在某个非激活的窗口中而用户正按着鼠标的某个键发送此消息给当前窗口
    /// </summary>
    WM_MOUSEACTIVATE = 0x21,
    /// <summary>
    /// 发送此消息给MDI子窗口当用户点击此窗口的标题栏或当窗口被激活,移动,改变大小
    /// </summary>
    WM_CHILDACTIVATE = 0x22,
    /// <summary>
    /// 此消息由基于计算机的训练程序发送,通过WH_JOURNALPALYBACK的hook程序分离出用户输入消息
    /// </summary>
    WM_QUEUESYNC = 0x23,
    /// <summary>
    /// 此消息发送给窗口当它将要改变大小或位置
    /// </summary>
    WM_GETMINMAXINFO = 0x24,
    /// <summary>
    /// 发送给最小化窗口当它图标将要被重画
    /// </summary>
    WM_PAINTICON = 0x26,
    /// <summary>
    /// 此消息发送给某个最小化窗口,仅当它在画图标前它的背景必须被重画
    /// </summary>
    WM_ICONERASEBKGND = 0x27,
    /// <summary>
    /// 发送此消息给一个对话框程序去更改焦点位置
    /// </summary>
    WM_NEXTDLGCTL = 0x28,
    /// <summary>
    /// 每当打印管理列队增加或减少一条作业时发出此消息
    /// </summary>
    WM_SPOOLERSTATUS = 0x2A,
    /// <summary>
    /// 当button,combobox,listbox,menu的可视外观改变时发送
    /// </summary>
    WM_DRAWITEM = 0x2B,
    /// <summary>
    /// 当button, combo box, list box, list view control, or menu item 被创建时
    /// </summary>
    WM_MEASUREITEM = 0x2C,
    /// <summary>
    /// 此消息有一个LBS_WANTKEYBOARDINPUT风格的发出给它的所有者来响应WM_KEYDOWN消息
    /// </summary>
    WM_VKEYTOITEM = 0x2E,
    /// <summary>
    /// 此消息由一个LBS_WANTKEYBOARDINPUT风格的列表框发送给他的所有者来响应WM_CHAR消息
    /// </summary>
    WM_CHARTOITEM = 0x2F,
    /// <summary>
    /// 当绘制文本时程序发送此消息得到控件要用的颜色
    /// </summary>
    WM_SETFONT = 0x30,
    /// <summary>
    /// 应用程序发送此消息得到当前控件绘制文本的字体
    /// </summary>
    WM_GETFONT = 0x31,
    /// <summary>
    /// 应用程序发送此消息让一个窗口与一个热键相关连
    /// </summary>
    WM_SETHOTKEY = 0x32,
    /// <summary>
    /// 应用程序发送此消息来判断热键与某个窗口是否有关联
    /// </summary>
    WM_GETHOTKEY = 0x33,
    /// <summary>
    /// 此消息发送给最小化窗口,当此窗口将要被拖放而它的类中没有定义图标,应用程序能返回一个图标或光标的句柄,当用户拖放图标时系统显示这个图标或光标
    /// </summary>
    WM_QUERYDRAGICON = 0x37,
    /// <summary>
    /// 发送此消息来判定combobox或listbox新增加的项的相对位置
    /// </summary>
    WM_COMPAREITEM = 0x39,
    /// <summary>
    /// 显示内存已经很少了
    /// </summary>
    WM_COMPACTING = 0x41,
    /// <summary>
    /// 窗口大小和位置将要被改变时,来调用Setwindowpos函数或其它窗口管理函数
    /// </summary>
    WM_WINDOWPOSCHANGING = 0x46,
    /// <summary>
    /// 窗口大小和位置已经被改变后,来调用Setwindowpos函数或其它窗口管理函数
    /// </summary>
    WM_WINDOWPOSCHANGED = 0x47,
    /// <summary>
    /// 当系统将要进入暂停状态时发送此消息
    /// </summary>
    WM_POWER = 0x48,
    /// <summary>
    /// 当一个应用程序传递数据给另一个应用程序时发送此消息
    /// </summary>
    WM_COPYDATA = 0x4A,
    /// <summary>
    /// 当某个用户取消程序日志激活状态,提交此消息给程序
    /// </summary>
    WM_CANCELJOURNA = 0x4B,
    /// <summary>
    /// 当某个控件的某个事件已经发生或这个控件需要得到一些信息时,发送此消息给它的父窗口
    /// </summary>
    WM_NOTIFY = 0x4E,
    /// <summary>
    /// 当用户选择某种输入语言,或输入语言的热键改变
    /// </summary>
    WM_INPUTLANGCHANGEREQUEST = 0x50,
    /// <summary>
    /// 当平台现场已经被改变后发送此消息给受影响的最顶级窗口
    /// </summary>
    WM_INPUTLANGCHANGE = 0x51,
    /// <summary>
    /// 当程序已经初始化windows帮助例程时发送此消息给应用程序
    /// </summary>
    WM_TCARD = 0x52,
    /// <summary>
    /// 此消息显示用户按下了F1,如果某个菜单是激活的,就发送此消息个此窗口关联的菜单,否则就发送给有焦点的窗口,如果当前都没有焦点,就把此消息发送给当前激活的窗口
    /// </summary>
    WM_HELP = 0x53,
    /// <summary>
    /// 当用户已经登入或退出后发送此消息给所有的窗口,当用户登入或退出时系统更新用户的具体设置信息,在用户更新设置时系统马上发送此消息
    /// </summary>
    WM_USERCHANGED = 0x54,
    /// <summary>
    /// 公用控件,自定义控件和他们的父窗口通过此消息来判断控件是使用ANSI还是UNICODE结构
    /// </summary>
    WM_NOTIFYFORMAT = 0x55,
    /// <summary>
    /// 当用户某个窗口中点击了一下右键就发送此消息给这个窗口
    /// </summary>
    WM_CONTEXTMENU = 0x7B,
    /// <summary>
    /// 当调用SETWINDOWLONG函数将要改变一个或多个窗口的风格时发送此消息给那个窗口
    /// </summary>
    WM_STYLECHANGING = 0x7C,
    /// <summary>
    /// 当调用SETWINDOWLONG函数一个或多个窗口的风格后发送此消息给那个窗口
    /// </summary>
    WM_STYLECHANGED = 0x7D,
    /// <summary>
    /// 当显示器的分辨率改变后发送此消息给所有的窗口
    /// </summary>
    WM_DISPLAYCHANGE = 0x7E,
    /// <summary>
    /// 此消息发送给某个窗口来返回与某个窗口有关连的大图标或小图标的句柄
    /// </summary>
    WM_GETICON = 0x7F,
    /// <summary>
    /// 程序发送此消息让一个新的大图标或小图标与某个窗口关联
    /// </summary>
    WM_SETICON = 0x80,
    /// <summary>
    /// 当某个窗口第一次被创建时,此消息在WM_CREATE消息发送前发送
    /// </summary>
    WM_NCCREATE = 0x81,
    /// <summary>
    /// 此消息通知某个窗口,非客户区正在销毁
    /// </summary>
    WM_NCDESTROY = 0x82,
    /// <summary>
    /// 当某个窗口的客户区域必须被核算时发送此消息
    /// </summary>
    WM_NCCALCSIZE = 0x83,
    /// <summary>
    /// 移动鼠标/按住/释放鼠标时
    /// </summary>
    WM_NCHITTEST = 0x84,
    /// <summary>
    /// 程序发送此消息给某个窗口当它（窗口）的框架必须被绘制时
    /// </summary>
    WM_NCPAINT = 0x85,
    /// <summary>
    /// 此消息发送给某个窗口仅当它的非客户区需要被改变来显示是激活还是非激活状态
    /// </summary>
    WM_NCACTIVATE = 0x86,
    /// <summary>
    /// 发送此消息给某个与对话框程序关联的控件,widdows控制方位键和TAB键使输入进入此控件通过应
    /// </summary>
    WM_GETDLGCODE = 0x87,
    /// <summary>
    /// 当光标在一个窗口的非客户区内移动时发送此消息给这个窗口 非客户区为：窗体的标题栏及窗的边框体
    /// </summary>
    WM_NCMOUSEMOVE = 0xA0,
    /// <summary>
    /// 当光标在一个窗口的非客户区同时按下鼠标左键时提交此消息
    /// </summary>
    WM_NCLBUTTONDOWN = 0xA1,
    /// <summary>
    /// 当用户释放鼠标左键同时光标某个窗口在非客户区时发送此消息
    /// </summary>
    WM_NCLBUTTONUP = 0xA2,
    /// <summary>
    /// 当用户双击鼠标左键同时光标某个窗口在非客户区时发送此消息
    /// </summary>
    WM_NCLBUTTONDBLCLK = 0xA3,
    /// <summary>
    /// 当用户按下鼠标右键同时光标又在窗口的非客户区时发送此消息
    /// </summary>
    WM_NCRBUTTONDOWN = 0xA4,
    /// <summary>
    /// 当用户释放鼠标右键同时光标又在窗口的非客户区时发送此消息
    /// </summary>
    WM_NCRBUTTONUP = 0xA5,
    /// <summary>
    /// 当用户双击鼠标右键同时光标某个窗口在非客户区时发送此消息
    /// </summary>
    WM_NCRBUTTONDBLCLK = 0xA6,
    /// <summary>
    /// 当用户按下鼠标中键同时光标又在窗口的非客户区时发送此消息
    /// </summary>
    WM_NCMBUTTONDOWN = 0xA7,
    /// <summary>
    /// 当用户释放鼠标中键同时光标又在窗口的非客户区时发送此消息
    /// </summary>
    WM_NCMBUTTONUP = 0xA8,
    /// <summary>
    /// 当用户双击鼠标中键同时光标又在窗口的非客户区时发送此消息
    /// </summary>
    WM_NCMBUTTONDBLCLK = 0xA9,

    // 所有的键盘消息只有中间的八种,也就是 WM_KEYDOWN 到 WM_SYSDEADCHAR
    /// <summary>
    /// 按下一个键 == WM_KEYDOWN
    /// </summary>
    WM_KEYFIRST = 0x0100,
    /// <summary>
    /// 按下一个键
    /// </summary>
    WM_KEYDOWN = 0x0100,
    /// <summary>
    /// 释放一个键
    /// </summary>
    WM_KEYUP = 0x0101,
    /// <summary>
    /// 按下某键,并已发出WM_KEYDOWN, WM_KEYUP消息
    /// </summary>
    WM_CHAR = 0x102,
    /// <summary>
    /// 当用translatemessage函数翻译WM_KEYUP消息时发送此消息给拥有焦点的窗口
    /// </summary>
    WM_DEADCHAR = 0x103,
    /// <summary>
    /// 当用户按住ALT键同时按下其它键时提交此消息给拥有焦点的窗口
    /// </summary>
    WM_SYSKEYDOWN = 0x104,
    /// <summary>
    /// 当用户释放一个键同时ALT 键还按着时提交此消息给拥有焦点的窗口
    /// </summary>
    WM_SYSKEYUP = 0x105,
    /// <summary>
    /// 当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后提交此消息给拥有焦点的窗口
    /// </summary>
    WM_SYSCHAR = 0x106,
    /// <summary>
    /// 当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后发送此消息给拥有焦点的窗口
    /// </summary>
    WM_SYSDEADCHAR = 0x107,
    /// <summary>
    /// 在一个对话框程序被显示前发送此消息给它,通常用此消息初始化控件和执行其它任务
    /// </summary>
    WM_INITDIALOG = 0x110,
    /// <summary>
    /// 当用户选择一条菜单命令项或当某个控件发送一条消息给它的父窗口
    /// </summary>
    WM_COMMAND = 0x111,

    /// <summary>
    /// 当用户选择窗口菜单的一条命令或最大化最小化时窗口前会收到此消息
    /// </summary>
    WM_SYSCOMMAND = 0x112,
    /// <summary>
    /// 发生了定时器事件
    /// </summary>
    WM_TIMER = 0x113,
    /// <summary>
    /// 当一个窗口标准水平滚动条产生一个滚动事件时发送此消息给那个窗口,也发送给拥有它的控件
    /// </summary>
    WM_HSCROLL = 0x114,
    /// <summary>
    /// 当一个窗口标准垂直滚动条产生一个滚动事件时发送此消息给那个窗口也,发送给拥有它的控件
    /// </summary>
    WM_VSCROLL = 0x115,
    /// <summary>
    /// 当一个菜单将要被激活时发送此消息,它发生在用户菜单条中的某项或按下某个菜单键,它允许程序在显示前更改菜单
    /// </summary>
    WM_INITMENU = 0x116,
    /// <summary>
    /// 当一个下拉菜单或子菜单将要被激活时发送此消息,它允许程序在它显示前更改菜单,而不要改变全部
    /// </summary>
    WM_INITMENUPOPUP = 0x117,
    /// <summary>
    /// 当用户选择一条菜单项时发送此消息给菜单的所有者（一般是窗口）
    /// </summary>
    WM_MENUSELECT = 0x11F,
    /// <summary>
    /// 当菜单已被激活用户按下了某个键（不同于加速键）,发送此消息给菜单的所有者
    /// </summary>
    WM_MENUCHAR = 0x120,
    /// <summary>
    /// 当一个模态对话框或菜单进入空载状态时发送此消息给它的所有者,一个模态对话框或菜单进入空载状态就是在处理完一条或几条先前的消息后没有消息它的列队中等待
    /// </summary>
    WM_ENTERIDLE = 0x121,
    /// <summary>
    /// 在windows绘制消息框前发送此消息给消息框的所有者窗口,通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置消息框的文本和背景颜色
    /// </summary>
    WM_CTLCOLORMSGBOX = 0x132,
    /// <summary>
    /// 当一个编辑型控件将要被绘制时发送此消息给它的父窗口通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置编辑框的文本和背景颜色
    /// </summary>
    WM_CTLCOLOREDIT = 0x133,

    /// <summary>
    /// 当一个列表框控件将要被绘制前发送此消息给它的父窗口通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置列表框的文本和背景颜色
    /// </summary>
    WM_CTLCOLORLISTBOX = 0x134,
    /// <summary>
    /// 当一个按钮控件将要被绘制时发送此消息给它的父窗口通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置按纽的文本和背景颜色
    /// </summary>
    WM_CTLCOLORBTN = 0x135,
    /// <summary>
    /// 当一个对话框控件将要被绘制前发送此消息给它的父窗口通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置对话框的文本背景颜色
    /// </summary>
    WM_CTLCOLORDLG = 0x136,
    /// <summary>
    /// 当一个滚动条控件将要被绘制时发送此消息给它的父窗口通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置滚动条的背景颜色
    /// </summary>
    WM_CTLCOLORSCROLLBAR = 0x137,
    /// <summary>
    /// 当一个静态控件将要被绘制时发送此消息给它的父窗口通过响应这条消息,所有者窗口可以通过使用给定的相关显示设备的句柄来设置静态控件的文本和背景颜色
    /// </summary>
    WM_CTLCOLORSTATIC = 0x138,

    /// <summary>
    /// 当鼠标轮子转动时发送此消息个当前有焦点的控件
    /// </summary>
    WM_MOUSEWHEEL = 0x20A,
    /// <summary>
    /// 双击鼠标中键
    /// </summary>
    WM_MBUTTONDBLCLK = 0x209,
    /// <summary>
    /// 释放鼠标中键
    /// </summary>
    WM_MBUTTONUP = 0x208,
    /// <summary>
    /// 移动鼠标时发生,同WM_MOUSEFIRST
    /// </summary>
    WM_MOUSEMOVE = 0x200,
    /// <summary>
    /// 按下鼠标左键
    /// </summary>
    WM_LBUTTONDOWN = 0x201,
    /// <summary>
    /// 释放鼠标左键
    /// </summary>
    WM_LBUTTONUP = 0x202,
    /// <summary>
    /// 双击鼠标左键
    /// </summary>
    WM_LBUTTONDBLCLK = 0x203,
    /// <summary>
    /// 按下鼠标右键
    /// </summary>
    WM_RBUTTONDOWN = 0x204,
    /// <summary>
    /// 释放鼠标右键
    /// </summary>
    WM_RBUTTONUP = 0x205,
    /// <summary>
    /// 双击鼠标右键
    /// </summary>
    WM_RBUTTONDBLCLK = 0x206,
    /// <summary>
    /// 按下鼠标中键
    /// </summary>
    WM_MBUTTONDOWN = 0x207,

    WM_USER = 0x0400,

    /// <summary>
    /// 执行复制成功
    /// </summary>
    WM_CLIPBOARDUPDATE = 0x031D,
}

// https://blog.csdn.net/biyusr/article/details/108376195
public enum MOUSEEVENTF : int
{
    /// <summary>
    /// 移动鼠标
    /// </summary>
    MOVE = 0x0001,
    /// <summary>
    /// 模拟鼠标左键按下
    /// </summary>
    LEFTDOWN = 0x0002,
    /// <summary>
    /// 模拟鼠标左键抬起
    /// </summary>
    LEFTUP = 0x0004,
    /// <summary>
    /// 模拟鼠标右键按下
    /// </summary>
    RIGHTDOWN = 0x0008,
    /// <summary>
    /// 模拟鼠标右键抬起
    /// </summary>
    RIGHTUP = 0x0010,
    /// <summary>
    /// 模拟鼠标中键按下
    /// </summary>
    MIDDLEDOWN = 0x0020,
    /// <summary>
    /// 模拟鼠标中键抬起
    /// </summary>
    MIDDLEUP = 0x0040,
    /// <summary>
    /// 标示是否采用绝对坐标
    /// </summary>
    ABSOLUTE = 0x8000,
    /// <summary>
    /// 模拟鼠标滚轮滚动操作,必须配合dwData参数
    /// </summary>
    WHEEL = 0x0800,
}

// C#使用SendMessage发送组合键
// https://www.cnblogs.com/johnsonton/articles/2331430.html
// Windows 使用的256个虚拟键码
[Flags]// 打印的时候可以有名称输出,而不是值输出
public enum VK : int
{
    VK_LBUTTON = 0x1,
    VK_RBUTTON = 0x2,
    VK_CANCEL = 0x3,
    VK_MBUTTON = 0x4,
    VK_BACK = 0x8,
    VK_TAB = 0x9,
    VK_CLEAR = 0xC,
    VK_RETURN = 0xD,
    VK_SHIFT = 0x10,
    VK_CONTROL = 0x11,
    VK_MENU = 0x12,// VK_ALT
    VK_ALT = 0x12,
    VK_PAUSE = 0x13,
    VK_CAPITAL = 0x14,
    VK_ESCAPE = 0x1B,
    VK_SPACE = 0x20,
    VK_PRIOR = 0x21,
    VK_NEXT = 0x22,
    VK_END = 0x23,
    VK_HOME = 0x24,
    VK_LEFT = 0x25,
    VK_UP = 0x26,
    VK_RIGHT = 0x27,
    VK_DOWN = 0x28,
    VK_Select = 0x29,
    VK_PRINT = 0x2A,
    VK_EXECUTE = 0x2B,
    VK_SNAPSHOT = 0x2C,
    VK_Insert = 0x2D,
    VK_Delete = 0x2E,
    VK_HELP = 0x2F,
    VK_0 = 0x30,
    VK_1 = 0x31,
    VK_2 = 0x32,
    VK_3 = 0x33,
    VK_4 = 0x34,
    VK_5 = 0x35,
    VK_6 = 0x36,
    VK_7 = 0x37,
    VK_8 = 0x38,
    VK_9 = 0x39,
    VK_A = 0x41,
    VK_B = 0x42,
    VK_C = 0x43,
    VK_D = 0x44,
    VK_E = 0x45,
    VK_F = 0x46,
    VK_G = 0x47,
    VK_H = 0x48,
    VK_I = 0x49,
    VK_J = 0x4A,
    VK_K = 0x4B,
    VK_L = 0x4C,
    VK_M = 0x4D,
    VK_N = 0x4E,
    VK_O = 0x4F,
    VK_P = 0x50,
    VK_Q = 0x51,
    VK_R = 0x52,
    VK_S = 0x53,
    VK_T = 0x54,
    VK_U = 0x55,
    VK_V = 0x56,
    VK_W = 0x57,
    VK_X = 0x58,
    VK_Y = 0x59,
    VK_Z = 0x5A,
    VK_STARTKEY = 0x5B,
    VK_CONTEXTKEY = 0x5D,
    VK_NUMPAD0 = 0x60,
    VK_NUMPAD1 = 0x61,
    VK_NUMPAD2 = 0x62,
    VK_NUMPAD3 = 0x63,
    VK_NUMPAD4 = 0x64,
    VK_NUMPAD5 = 0x65,
    VK_NUMPAD6 = 0x66,
    VK_NUMPAD7 = 0x67,
    VK_NUMPAD8 = 0x68,
    VK_NUMPAD9 = 0x69,
    VK_MULTIPLY = 0x6A,
    VK_ADD = 0x6B,
    VK_SEPARATOR = 0x6C,
    VK_SUBTRACT = 0x6D,
    VK_DECIMAL = 0x6E,
    VK_DIVIDE = 0x6F,
    VK_F1 = 0x70,
    VK_F2 = 0x71,
    VK_F3 = 0x72,
    VK_F4 = 0x73,
    VK_F5 = 0x74,
    VK_F6 = 0x75,
    VK_F7 = 0x76,
    VK_F8 = 0x77,
    VK_F9 = 0x78,
    VK_F10 = 0x79,
    VK_F11 = 0x7A,
    VK_F12 = 0x7B,
    VK_F13 = 0x7C,
    VK_F14 = 0x7D,
    VK_F15 = 0x7E,
    VK_F16 = 0x7F,
    VK_F17 = 0x80,
    VK_F18 = 0x81,
    VK_F19 = 0x82,
    VK_F20 = 0x83,
    VK_F21 = 0x84,
    VK_F22 = 0x85,
    VK_F23 = 0x86,
    VK_F24 = 0x87,
    VK_NUMLOCK = 0x90,
    VK_OEM_SCROLL = 0x91,
    VK_OEM_1 = 0xBA,
    VK_OEM_PLUS = 0xBB,
    VK_OEM_COMMA = 0xBC,
    VK_OEM_MINUS = 0xBD,
    VK_OEM_PERIOD = 0xBE,
    VK_OEM_2 = 0xBF,
    VK_OEM_3 = 0xC0,
    VK_OEM_4 = 0xDB,
    VK_OEM_5 = 0xDC,
    VK_OEM_6 = 0xDD,
    VK_OEM_7 = 0xDE,
    VK_OEM_8 = 0xDF,
    VK_ICO_F17 = 0xE0,
    VK_ICO_F18 = 0xE1,
    VK_OEM102 = 0xE2,
    VK_ICO_HELP = 0xE3,
    VK_ICO_00 = 0xE4,
    VK_ICO_CLEAR = 0xE6,
    VK_OEM_RESET = 0xE9,
    VK_OEM_JUMP = 0xEA,
    VK_OEM_PA1 = 0xEB,
    VK_OEM_PA2 = 0xEC,
    VK_OEM_PA3 = 0xED,
    VK_OEM_WSCTRL = 0xEE,
    VK_OEM_CUSEL = 0xEF,
    VK_OEM_ATTN = 0xF0,
    VK_OEM_FINNISH = 0xF1,
    VK_OEM_COPY = 0xF2,
    VK_OEM_AUTO = 0xF3,
    VK_OEM_ENLW = 0xF4,
    VK_OEM_BACKTAB = 0xF5,
    VK_ATTN = 0xF6,
    VK_CRSEL = 0xF7,
    VK_EXSEL = 0xF8,
    VK_EREOF = 0xF9,
    VK_PLAY = 0xFA,
    VK_ZOOM = 0xFB,
    VK_NONAME = 0xFC,
    VK_PA1 = 0xFD,
    VK_OEM_CLEAR = 0xFE,
}

[Flags]
public enum SC : uint
{
    // 窗体关闭消息
    SC_CLOSE = 0xf060,
    // 窗体最小化消息
    SC_MINIMIZE = 0xf020,
    // 窗体最大化消息
    SC_MAXIMIZE = 0xf030,
    // 窗体正常态消息 SC_RESTORE = 0xf120,
    SC_NOMAL = 0xf120,
}

[Flags]
public enum NCmdShow : uint
{
    /// <summary>
    /// 隐藏窗口并激活其他窗口。nCmdShow
    /// </summary>
    SW_HIDE = 0,
    /// <summary>
    /// 正常态的窗口(非最大化和非最小化)
    /// 激活并显示一个窗口。如果窗口被最小化或最大化,系统将其恢复到原来的尺寸和大小。应用程序在第一次显示窗口的时候应该指定此标志
    /// </summary>
    SW_SHOWNORMAL = 1,
    /// <summary>
    /// 激活窗口并将其最小化
    /// </summary>
    SW_SHOWMINIMIZED = 2,
    /// <summary>
    /// 激活窗口并将其最大化
    /// </summary>
    SW_SHOWMAXIMIZED = 3,
    /// <summary>
    /// 最大化指定的窗口
    /// </summary>
    SW_MAXIMIZE = 3,
    /// <summary>
    /// 以窗口最近一次的大小和状态显示窗口。激活窗口仍然维持激活状态
    /// </summary>
    SW_SHOWNOACTIVATE = 4,
    /// <summary>
    /// 在窗口原来的位置以原来的尺寸激活和显示窗口
    /// </summary>
    SW_SHOW = 5,
    /// <summary>
    /// 最小化指定的窗口并且激活在Z序中的下一个顶层窗口
    /// </summary>
    SW_MINIMIZE = 6,
    /// <summary>
    /// 窗口最小化,激活窗口仍然维持激活状态
    /// </summary>
    SW_SHOWMINNOACTIVE = 7,
    /// <summary>
    /// 以窗口原来的状态显示窗口。激活窗口仍然维持激活状态
    /// </summary>
    SW_SHOWNA = 8,
    /// <summary>
    /// 激活并显示窗口。如果窗口最小化或最大化,则系统将窗口恢复到原来的尺寸和位置。在恢复最小化窗口时,应用程序应该指定这个标志
    /// </summary>
    SW_RESTORE = 9,
    /// <summary>
    /// 依据在STARTUPINFO结构中指定的SW_FLAG标志设定显示状态,STARTUPINFO 结构是由启动应用程序的程序传递给CreateProcess函数的
    /// </summary>
    SW_SHOWDEFAULT = 10,
    /// <summary>
    /// 在WindowNT5.0中最小化窗口,即使拥有窗口的线程被挂起也会最小化。在从其他线程最小化窗口时才使用这个参数
    /// </summary>
    SW_FORCEMINIMIZE = 11,
}

public enum WS : uint
{
    // 窗口风格
    WS_CAPTION = 0xC00000,         // 带标题栏的窗口
    WS_MAXIMIZEBOX = 0x10000,      // 带最大化按钮的窗口
    WS_MINIMIZEBOX = 0x20000,      // 带最小化按钮的窗口
    WS_SYSMENU = 0x80000,          // 带系统菜单的窗口
    WS_CLIPSIBLINGS = 0x4000000,   // 不重绘层叠子窗口
    WS_CLIPCHILDREN = 0x2000000,   // 绘图时排子窗口区域
    WS_OVERLAPPED = 0x0,           // 具有标题栏和边框的层叠窗口
    WS_THICKFRAME = 0x40000,       // 具有可调边框

    // 具有标题栏、窗口菜单、可调边框和最大化、最小化按钮的窗口
    WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

    WS_GROUP = 0x20000,            // 指定一组控制的第一个控制
    WS_POPUP = 0x80000000,         // 弹出式窗口
    WS_BORDER = 0x800000,          // 单边框窗口
    WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU, // 具有单边框、标题栏菜单的弹出式窗口
    WS_MINIMIZE = 0x20000000,      // 窗口最小化
    WS_VISIBLE = 0x10000000,       // 窗口可见
    WS_DISABLED = 0x8000000,       // 窗口被禁用
    WS_MAXIMIZE = 0x1000000,       // 窗口最大化
    WS_DLGFRAME = 0x400000,        // 对话框边框风格
    WS_VSCROLL = 0x200000,         // 具有垂直滚动条
    WS_HSCROLL = 0x100000,         // 具有水平滚动条
    WS_TABSTOP = 0x10000,          // 具有TAB键控制
    WS_CHILD = 0x40000000,         // 设置窗口属性为child 多文档界面的子窗体
    WS_CHILDWINDOW = WS_CHILD,     // 具有子窗口

    // 扩展风格
    WS_EX_WINDOWEDGE = 0x100,         // 窗口具有凸起的3D边框
    WS_EX_CLIENTEDGE = 0x200,         // 窗口具有阴影边界
    WS_EX_TOOLWINDOW = 0x80,          // 小标题工具窗口
    WS_EX_TOPMOST = 0x8,              // 窗口总在顶层  const int WS_EX_TOPMOST = 0x00000008;
    WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE, // WS_EX-CLIENTEDGE和WS_EX_WINDOWEDGE的组合
    WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST, // WS_EX_WINDOWEDGE和WS_EX_TOOLWINDOW和WS_EX_TOPMOST的组合
    WS_EX_DLGMODALFRAME = 0x1,        // 带双边的窗口
    WS_EX_NOPARENTNOTIFY = 0x4,       // 窗口在创建和销毁时不向父窗口发送WM_PARENTNOTIFY消息
    WS_EX_TRANSPARENT = 0x20,         // 窗口透眀
    WS_EX_MDICHILD = 0x40,            // MDI子窗口
    WS_EX_CONTEXTHELP = 0x400,        // 标题栏包含问号联机帮助按钮
    WS_EX_RIGHT = 0x1000,             // 窗口具有右对齐属性
    WS_EX_RTLREADING = 0x2000,        // 窗口文本自右向左显示
    WS_EX_LEFTSCROLLBAR = 0x4000,     // 标题栏在客户区的左边
    WS_EX_CONTROLPARENT = 0x10000,     // 允许用户使用Tab键在窗口的子窗口间搜索
    WS_EX_STATICEDGE = 0x20000,        // 为不接受用户输入的项创建一个三维边界风格
    WS_EX_APPWINDOW = 0x40000,         // 在任务栏上显示顶层窗口的标题按钮
    WS_EX_LAYERED = 0x80000,           // 窗口具有透眀属性(Win2000)以上
    WS_EX_NOINHERITLAYOUT = 0x100000, // 窗口布局不传递给子窗口(Win2000)以上
    WS_EX_LAYOUTRTL = 0x400000,        // 水平起点在右边的窗口
    WS_EX_NOACTIVATE = 0x8000000,      // 窗口不会变成前台窗口(Win2000)以上
    WS_EX_LEFT = 0x0,                 // 窗口具有左对齐属性
    WS_EX_LTRREADING = 0x0,           // 窗口文本自左向右显示
    WS_EX_RIGHTSCROLLBAR = 0x0,       // 垂直滚动条在窗口的右边界
    WS_EX_ACCEPTFILES = 0x10,         // 接受文件拖曳
    WS_EX_COMPOSITED = 0x2000000,      // 窗体所有子窗口使用双缓冲从低到高绘制(XP)
}

public enum GWL : int
{
    /// <summary>
    /// 获取、设置窗口过程的地址
    /// </summary>
    GWL_WNDPROC = -4,
    /// <summary>
    /// 获取应用程序的实例句柄
    /// </summary>
    GWL_HINSTANCE = -6,
    /// <summary>
    /// 获取父窗口句柄
    /// </summary>
    GWL_HWNDPARENT = -8,
    /// <summary>
    /// 获取窗口标识
    /// </summary>
    GWL_ID = -12,
    /// <summary>
    /// 获取、设置窗口样式
    /// </summary>
    GWL_STYLE = -16,
    /// <summary>
    /// 获取、设置窗口扩展样式
    /// </summary>
    GWL_EXSTYLE = -20,
    /// <summary>
    /// 获取、设置与窗口关联的自定义数据
    /// </summary>
    GWL_USERDATA = -21,
}

public enum GetWindowCmd : uint
{
    /// <summary>
    /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
    /// 如果指定窗口是最高端窗口,则该句柄标识了在Z序最高端的最高端窗口；
    /// 如果指定窗口是顶层窗口,则该句柄标识了在z序最高端的顶层窗口：
    /// 如果指定窗口是子窗口,则句柄标识了在Z序最高端的同属窗口。
    /// </summary>
    GW_HWNDFIRST = 0,
    /// <summary>
    /// 返回的句柄标识了在z序最低端的相同类型的窗口。
    /// 如果指定窗口是最高端窗口,则该柄标识了在z序最低端的最高端窗口：
    /// 如果指定窗口是顶层窗口,则该句柄标识了在z序最低端的顶层窗口；
    /// 如果指定窗口是子窗口,则句柄标识了在Z序最低端的同属窗口。
    /// </summary>
    GW_HWNDLAST = 1,
    /// <summary>
    /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
    /// 如果指定窗口是最高端窗口,则该句柄标识了在指定窗口下的最高端窗口：
    /// 如果指定窗口是顶层窗口,则该句柄标识了在指定窗口下的顶层窗口；
    /// 如果指定窗口是子窗口,则句柄标识了在指定窗口下的同属窗口。
    /// </summary>
    GW_HWNDNEXT = 2,
    /// <summary>
    /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
    /// 如果指定窗口是最高端窗口,则该句柄标识了在指定窗口上的最高端窗口；
    /// 如果指定窗口是顶层窗口,则该句柄标识了在指定窗口上的顶层窗口；
    /// 如果指定窗口是子窗口,则句柄标识了在指定窗口上的同属窗口。
    /// </summary>
    GW_HWNDPREV = 3,
    /// <summary>
    /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
    /// GW_OWNER与GW_CHILD不是相对的参数,没有父窗口的含义,如果想得到父窗口请使用GetParent()。
    /// 例如：例如有时对话框的控件的GW_OWNER,是不存在的。
    /// </summary>
    GW_OWNER = 4,
    /// <summary>
    /// 如果指定窗口是父窗口,则获得的是在Tab序顶端的子窗口的句柄,否则为NULL。
    /// 函数仅检查指定父窗口的子窗口,不检查继承窗口。
    /// </summary>
    GW_CHILD = 5,
    /// <summary>
    /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
    /// 如果无使能窗口,则获得的句柄与指定窗口相同。
    /// </summary>
    GW_ENABLEDPOPUP = 6
}
#endif