#define Marshal

namespace IFoxCAD.Basal;
public partial class WindowsAPI
{
    #region kernel32
    // https://blog.csdn.net/haelang/article/details/45147121
    [DllImport("kernel32.dll")]
    public extern static uint GetLastError();

    [DllImport("kernel32.dll")]
    public static extern long GetHandleInformation(long hObject, ref long lpdwFlags);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string ModuleName);

    [DllImport("kernel32.dll")]
    public static extern int GetCurrentThreadId();

    /// <summary>
    /// 获取要引入的函数,将符号名或标识号转换为DLL内部地址
    /// </summary>
    /// <param name="hModule">exe/dll句柄</param>
    /// <param name="procName">接口名</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    /// <summary>
    /// 锁定内存
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalLock(IntPtr hMem);
    /// <summary>
    /// 解锁内存
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GlobalUnlock(IntPtr hMem);
#if !Marshal
    /*
    const int GMEM_MOVEABLE = 0x0002;
    IntPtr newPtr = WindowsAPI.GlobalAlloc(GMEM_MOVEABLE, Marshal.SizeOf(structObj));
    */
    /// <summary>
    /// 从堆中分配内存
    /// 被代替: Marshal.AllocHGlobal
    /// </summary>
    /// <param name="uFlags">分配方式</param>
    /// <param name="dwBytes">分配的字节数</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GlobalAlloc(uint uFlags, int dwBytes);
    /// <summary>
    /// 释放堆内存
    /// 被代替: Marshal.FreeHGlobal
    /// </summary>
    /// <param name="hMem">由<see cref="GlobalAlloc"/>产生的句柄</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GlobalFree(IntPtr hMem);
#endif
    /// <summary>
    /// 获取内存块大小
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint GlobalSize(IntPtr hMem);

    /// <summary>
    /// 锁定和释放内存
    /// </summary>
    /// <param name="data">锁定数据对象指针</param>
    /// <param name="task">返回锁定的内存片段指针,锁定期间执行任务</param>
    /// <returns>是否锁定成功</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool GlobalLockTask(IntPtr data, Action<IntPtr> task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));
        if (data == IntPtr.Zero)
            return false;

        try
        {
            var ptr = GlobalLock(data);
            // 有几率导致无效锁定:
            // 重复复制同一个图元时,第二次是 IntPtr.Zero,
            // 第三次就又可以复制了
            if (ptr == IntPtr.Zero)
                return false;
            task.Invoke(ptr);
        }
        finally { GlobalUnlock(data); }
        return true;
    }

    /// <summary>
    /// byte数组转结构体
    /// </summary>
    /// <param name="bytes">byte数组</param>
    /// <param name="typeSize">返回的结构大小</param>
    /// <returns>返回的结构体</returns>
    [Obsolete("效率太低", true)]
    public static T? BytesToStruct<T>(byte[] bytes, out int typeSize)
    {
        var structType = typeof(T);
        typeSize = Marshal.SizeOf(structType);
        if (typeSize > bytes.Length)
            return default;

        // 安全写法效率太低了
        // 分配结构体大小的内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(typeSize);

        // 将byte数组拷到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, typeSize);
        // 将内存空间转换为目标结构体;
        // 转类型的时候会拷贝一次,看它们地址验证 &result != &structPtr
        var result = (T)Marshal.PtrToStructure(structPtr, structType);

        // 释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return result;
    }

    /// <summary>
    /// byte数组转结构体
    /// </summary>
    /// <param name="bytes">byte数组</param>
    /// <returns>返回的结构体</returns>
    [MethodImpl]
    public static T? BytesToStruct<T>(byte[] bytes)
    {
        T? result = default;
        unsafe
        {
            // 安全指针方法
            // var pB = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            // 不安全指针方法
            fixed (byte* pB = &bytes[0])
            {
                result = (T?)Marshal.PtrToStructure(new IntPtr(pB), typeof(T));
            }
        }
        return result;
    }

    /// <summary>
    /// 结构体转byte数组
    /// <a href="https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/builtin-types/unmanaged-types">unmanaged</a>
    /// </summary>
    /// <param name="structObj">要转换的结构体</param>
    [MethodImpl]
    public static byte[] StructToBytes<T>(T structObj) where T : unmanaged/*非托管的T从来不为空*/
    {
        // 得到结构体的大小
        var typeSize = Marshal.SizeOf(structObj);
        // 从内存空间拷到byte数组
        var bytes = new byte[typeSize];
        unsafe
        {
            Marshal.Copy(new IntPtr(&structObj), bytes, 0, typeSize);
        }
#if true20221030
         // 安全写法效率太低了
         StructToPtr(structObj, structPtr => {
             Marshal.Copy(structPtr, bytes, 0, typeSize);
         });
#endif
        return bytes;
    }

#if true20221030
    /// <summary>
    /// 结构体转指针
    /// </summary>
    /// <param name="structObj">要转换的结构体</param>
    /// <param name="task">输出指针</param>
    /// <param name="freeHGlobal">释放申请的内存</param>
    /// <param name="lockPrt">是否锁定内存</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void StructToPtr<T>(T structObj,
                                   Action<IntPtr>? task = null,
                                   bool freeHGlobal = true,
                                   bool lockPrt = true)
    {
        IntPtr newPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structObj));
        if (newPtr == IntPtr.Zero)
            throw new ArgumentException(nameof(newPtr));

        try
        {
            // 剪贴板写入的时候不允许锁定内存,否则在频繁触发剪贴板将导致卡死程序
            if (lockPrt)
            {
                GlobalLockTask(newPtr, ptr => {
                    // 将结构体拷到分配好的内存空间
                    Marshal.StructureToPtr(structObj, newPtr, true);
                    task?.Invoke(newPtr);
                });
            }
            else
            {
                // 将结构体拷到分配好的内存空间
                Marshal.StructureToPtr(structObj, newPtr, true);
                task?.Invoke(newPtr);
            }
        }
        catch (Exception e)
        {
            Debugger.Break();
            Debugx.Printl(e.Message);
        }
        finally
        {
            if (freeHGlobal && newPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(newPtr);
        }
    }
#endif
    #endregion
}

public partial class WindowsAPI
{
    #region imm32
    /// <summary>
    /// 获取输入法的虚拟键码
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetVirtualKey(IntPtr hWnd);
    /// <summary>
    /// 获取输入法状态
    /// </summary>
    /// <param name="himc">输入法标识符</param>
    /// <param name="lpdw">输入模式</param>
    /// <param name="lpdw2">指向函数在其中检索句子模式值的变量的指针</param>
    /// <returns></returns>
    [DllImport("imm32.dll")]
    public static extern bool ImmGetConversionStatus(IntPtr himc, out int lpdw, out int lpdw2);

    /// <summary>
    /// 获取指定窗口的输入法状态
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <returns></returns>
    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hwnd);
    /// <summary>
    /// 设置输入法的当前状态
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="fOpen"></param>
    /// <returns></returns>
    [DllImport("imm32.dll")]
    public static extern bool ImmSetOpenStatus(IntPtr hwnd, bool fOpen);
    /// <summary>
    /// 输入法打开状态
    /// </summary>
    /// <param name="hwnd"></param>
    /// <returns>非0打开,0关闭;(true中文,false英文)</returns>
    [DllImport("imm32.dll")]
    public static extern bool ImmGetOpenStatus(IntPtr hwnd);
    #endregion
}

public partial class WindowsAPI
{
    #region user32

    /// <summary>
    /// 获取窗口客户区的大小,客户区为窗口中除标题栏,菜单栏之外的地方
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="lpRect"></param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetClientRect")]
    public static extern bool GetClientRect(IntPtr hwnd, out IntRect lpRect);

    /// <summary>
    /// 查找主线程<br/>
    /// 代替<see cref="AppDomain.GetCurrentThreadId()"/><br/>
    /// 托管线程和他们不一样: <see cref="Thread.CurrentThread.ManagedThreadId"/>
    /// </summary>
    /// <param name="hWnd">主窗口</param>
    /// <param name="lpdwProcessId">进程ID</param>
    /// <returns>线程ID</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /// <summary>
    /// 设置焦点
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    /// <summary>
    /// 获取当前窗口
    /// </summary>
    /// <returns>当前窗口标识符</returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    /// <summary>
    /// 将一个消息的组成部分合成一个消息并放入对应线程消息队列的方法
    /// </summary>
    /// <param name="hhwnd">控件句柄</param>
    /// <param name="msg">消息是什么。键盘按键、鼠标点击还是其他</param>
    /// <param name="wparam"></param>
    /// <param name="lparam"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hhwnd, int msg, IntPtr wparam, IntPtr lparam);
    /// <summary>
    /// 发送击键
    /// </summary>
    /// <param name="bVk"></param>
    /// <param name="bScan"></param>
    /// <param name="dwFlags"></param>
    /// <param name="dwExtraInfo"></param>
    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void KeybdEvent(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
    /// <summary>
    /// 获取窗口文字的长度
    /// </summary>
    /// <param name="hWnd">窗口标识符</param>
    /// <returns>文字长度</returns>
    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);
    /// <summary>
    /// 获取窗口的标题
    /// </summary>
    /// <param name="hWnd">窗口标识符</param>
    /// <param name="text">窗口文字</param>
    /// <param name="count">文字长度</param>
    /// <returns></returns>
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

    // [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    // internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


    /// <summary>
    /// 获取某个线程的输入法布局
    /// </summary>
    /// <param name="threadid">线程ID</param>
    /// <returns>布局码</returns>
    [DllImport("user32.dll")]
    public static extern int GetKeyboardLayout(int threadid);


    /// <summary>
    /// 获取按键的当前状态
    /// </summary>
    /// <param name="nVirtKey">按键虚拟代码</param>
    /// <returns>表示没按下&gt;0;按下&lt;0</returns>
    [DllImport("user32.dll")]
    public static extern short GetKeyState(int nVirtKey);
    /// <summary>
    /// 检索指定窗口所属的类的名称。
    /// </summary>
    /// <param name="hWnd">窗口标识符</param>
    /// <param name="lpClassName"></param>
    /// <param name="nMaxCount"></param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.DLL", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr GetTopWindow(IntPtr hWnd);


    /// <summary>
    /// 获取线程对应的窗体信息
    /// </summary>
    /// <param name="idThread">线程</param>
    /// <param name="lpgui"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

    /// <summary>
    /// 获取线程对应的窗体信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GuiThreadInfo
    {
        public int cbSize;
        public int flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public System.Drawing.Rectangle rcCaret;

        public static GuiThreadInfo Create(uint windowThreadProcessId)
        {
            if (windowThreadProcessId == 0)
                throw new ArgumentNullException(nameof(windowThreadProcessId));

            GuiThreadInfo gti = new();
            gti.cbSize = Marshal.SizeOf(gti);
            GetGUIThreadInfo(windowThreadProcessId, ref gti);
            return gti;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetFocus();

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ToAscii(int uVirtKey, int uScancode, byte[] lpdKeyState, byte[] lpwTransKey, int fuState);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern long GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsIconic(int hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowEnabled(IntPtr hWnd);
    #endregion

    #region 键盘钩子
    public delegate IntPtr CallBack(int nCode, int wParam, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowsHookEx(HookType idHook, CallBack lpfn, IntPtr hmod, int dwThreadId);
    [DllImport("user32.dll")]
    public static extern IntPtr UnhookWindowsHookEx(IntPtr hHook);
    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hHook, int ncode, int wParam, IntPtr lParam);
    /// <summary>
    /// Hook键盘数据结构
    /// </summary>
    [ComVisible(true)]
    [Serializable]
    //[DebuggerDisplay("{DebuggerDisplay,nq}")]
    //[DebuggerTypeProxy(typeof(KeyboardHookStruct))]
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardHookStruct
    {
        public int VkCode;        // 键码,该代码必须有一个价值的范围1至254
        public int ScanCode;      // 指定的硬件扫描码的关键
        public int Flags;         // 键标志
        public int Time;          // 指定的时间戳记的这个讯息
        public int DwExtraInfo;   // 指定额外信息相关的信息

        public static KeyboardHookStruct Create(IntPtr lParam)
        {
            return (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
        }
        public void ToPtr(IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }
    }
    /// <summary>
    /// 注册表增加低级钩子超时处理,防止系统不允许,
    /// 否则:偶发性出现 键盘钩子不能用了,而且退出时产生 1404 错误
    /// https://www.cnblogs.com/songr/p/5131655.html
    /// </summary>
    public static void CheckLowLevelHooksTimeout(int setLowLevel = 25000)
    {
        const string llh = "LowLevelHooksTimeout";
        using var registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
        if ((int)registryKey.GetValue(llh, 0) < setLowLevel)
            registryKey.SetValue(llh, setLowLevel, RegistryValueKind.DWord);
    }
    #endregion
}

public partial class WindowsAPI
{
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref IntRect lpRect);

    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(IntRect))]
    public struct IntRect
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"(Left:{_Left},Top:{_Top},Right:{_Right},Bottom:{_Bottom})";

        int _Left;
        int _Top;
        int _Right;
        int _Bottom;
        public int Left => _Left;
        public int Top => _Top;
        public int Right => _Right;
        public int Bottom => _Bottom;
        public int Width => checked(Right - Left);
        public int Height => checked(Bottom - Top);

        public IntRect(int left, int top, int right, int bottom)
        {
            _Left = left;
            _Top = top;
            _Right = right;
            _Bottom = bottom;
        }

        static readonly IntRect _Zero = new(0, 0, 0, 0);
        public static IntRect Zero => _Zero;

        public override string ToString() => $"({_Left},{_Top},{_Right},{_Bottom})";

        #region 重载运算符_比较
        public bool Equals(IntRect other)
        {
            return
            _Left == other._Left &&
            _Top == other._Top &&
            _Right == other._Right &&
            _Bottom == other._Bottom;
        }
        public static bool operator !=(IntRect a, IntRect b)
        {
            return !(a == b);
        }
        public static bool operator ==(IntRect a, IntRect b)
        {
            return a.Equals(b);
        }
        public override bool Equals(object obj)
        {
            return obj is IntRect d && Equals(d);
        }
        public override int GetHashCode()
        {
            return ((_Left, _Top).GetHashCode(), _Right).GetHashCode() ^ _Bottom.GetHashCode();
        }

        public IntRect Clone()
        {
            return (IntRect)MemberwiseClone();
        }
        #endregion
    }

    [ComVisible(true)]
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(IntSize))]
    [StructLayout(LayoutKind.Sequential)]
    public struct IntSize
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"(Hight:{Hight},Width:{Width})";
        public int Hight;
        public int Width;

        public IntSize(int cx, int cy)
        {
            Hight = cx;
            Width = cy;
        }
        public override string ToString() => $"({Hight},{Width})";
    }

    [ComVisible(true)]
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(Point3D))]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point3D : IEquatable<Point3D>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"(X:{X},Y:{Y},Z:{Z})";

        /* 由于此类是用来优化,从而实现字段修改,因此直接暴露字段减少栈帧 */
        public double X;
        public double Y;
        public double Z;

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        //public static implicit operator Point3D(Point3d pt)
        //{
        //    return new Point3D(pt.X, pt.Y, pt.Z);
        //}
        //public static implicit operator Point3d(Point3D pt)
        //{
        //    return new Point3d(pt.X, pt.Y, pt.Z);
        //}
        public override string ToString() => $"({X},{Y},{Z})";

        public static Point3D Create(IntPtr lParam)
        {
            return (Point3D)Marshal.PtrToStructure(lParam, typeof(Point3D));
        }

        public void ToPtr(IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }


        #region 重载运算符_比较
        public bool Equals(Point3D other)
        {
            return
            X == other.X &&
            Y == other.Y &&
            Z == other.Z;
        }
        public static bool operator !=(Point3D a, Point3D b)
        {
            return !(a == b);
        }
        public static bool operator ==(Point3D a, Point3D b)
        {
            return a.Equals(b);
        }
        public override bool Equals(object obj)
        {
            return obj is Point3D d && Equals(d);
        }
        public override int GetHashCode()
        {
            return (X, Y).GetHashCode() ^ Z.GetHashCode();
        }
        #endregion
    }
}