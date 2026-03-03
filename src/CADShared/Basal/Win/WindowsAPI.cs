#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#define Marshal
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
namespace IFoxCAD.Basal;

/// <summary>
/// Windows API 封装类，提供常用的 Windows 系统 API 调用方法。
/// 包含 kernel32、imm32 和 user32 等 DLL 的函数声明。
/// </summary>
public partial class WindowsAPI
{
    #region 错误处理辅助方法

    /// <summary>
    /// 检查Win32 API调用是否成功，如果失败则记录错误信息
    /// </summary>
    /// <param name="functionName">调用的函数名称</param>
    /// <param name="result">API调用结果（句柄或指针）</param>
    /// <returns>是否成功（result不为IntPtr.Zero）</returns>
    public static bool CheckWin32Error(string functionName, IntPtr result)
    {
        if (result != IntPtr.Zero)
            return true;

        uint errorCode = GetLastError();
        if (errorCode != 0)
        {
            DebugEx.Printl($"[Win32错误] {functionName} 失败，错误码: {errorCode} (0x{errorCode:X8})");
        }
        return false;
    }

    /// <summary>
    /// 检查Win32 API调用是否成功，如果失败则记录错误信息
    /// </summary>
    /// <param name="functionName">调用的函数名称</param>
    /// <param name="result">API调用结果（布尔值）</param>
    /// <returns>API调用结果</returns>
    public static bool CheckWin32Error(string functionName, bool result)
    {
        if (!result)
        {
            uint errorCode = GetLastError();
            if (errorCode != 0)
            {
                DebugEx.Printl($"[Win32错误] {functionName} 失败，错误码: {errorCode} (0x{errorCode:X8})");
            }
        }
        return result;
    }

    /// <summary>
    /// 检查Win32 API调用是否成功，如果失败则记录错误信息
    /// </summary>
    /// <param name="functionName">调用的函数名称</param>
    /// <param name="result">API调用结果（32位整数）</param>
    /// <returns>是否成功（result不为0）</returns>
    public static bool CheckWin32Error(string functionName, int result)
    {
        if (result != 0)
            return true;

        uint errorCode = GetLastError();
        if (errorCode != 0)
        {
            DebugEx.Printl($"[Win32错误] {functionName} 失败，错误码: {errorCode} (0x{errorCode:X8})");
        }
        return false;
    }

    #endregion

    #region kernel32
    // https://blog.csdn.net/haelang/article/details/45147121
    /// <summary>
    /// 获取最近一次错误代码
    /// </summary>
    /// <returns>错误代码</returns>
    [DllImport("kernel32.dll")]
    public extern static uint GetLastError();

    /// <summary>
    /// 获取指定句柄的标志信息
    /// </summary>
    /// <param name="hObject">对象句柄</param>
    /// <param name="lpdwFlags">接收标志的变量引用</param>
    /// <returns>执行结果</returns>
    [DllImport("kernel32.dll")]
    public static extern long GetHandleInformation(long hObject, ref long lpdwFlags);

    /// <summary>
    /// 获取模块句柄
    /// </summary>
    /// <param name="ModuleName">模块名称</param>
    /// <returns>模块句柄</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string ModuleName);

    /// <summary>
    /// 获取模块句柄（带错误检查）
    /// </summary>
    /// <param name="moduleName">模块名称</param>
    /// <returns>模块句柄，失败返回IntPtr.Zero</returns>
    public static IntPtr GetModuleHandleSafe(string moduleName)
    {
        var result = GetModuleHandle(moduleName);
        CheckWin32Error(nameof(GetModuleHandle), result);
        return result;
    }

    /// <summary>
    /// 获取当前线程ID
    /// </summary>
    /// <returns>当前线程ID</returns>
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
    /// 安全获取要引入的函数,将符号名或标识号转换为DLL内部地址
    /// </summary>
    /// <param name="hModule">exe/dll句柄</param>
    /// <param name="procName">接口名</param>
    /// <returns>函数地址，失败返回IntPtr.Zero</returns>
    public static IntPtr GetProcAddressSafe(IntPtr hModule, string procName)
    {
        if (hModule == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }
        IntPtr result = GetProcAddress(hModule, procName);
        CheckWin32Error(nameof(GetProcAddress), result);
        return result;
    }

    /// <summary>
    /// 锁定内存
    /// </summary>
    /// <param name="hMem">内存句柄</param>
    /// <returns>内存指针</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalLock(IntPtr hMem);
    /// <summary>
    /// 解锁内存
    /// </summary>
    /// <param name="hMem">内存句柄</param>
    /// <returns>解锁是否成功</returns>
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

    /// <summary>
    /// 安全释放堆内存
    /// </summary>
    /// <param name="hMem">由<see cref="GlobalAlloc"/>产生的句柄</param>
    /// <returns>如果释放成功,返回IntPtr.Zero</returns>
    public static IntPtr GlobalFreeSafe(IntPtr hMem)
    {
        if (hMem == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }
        return GlobalFree(hMem);
    }
#endif
    /// <summary>
    /// 获取内存块大小
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint GlobalSize(IntPtr hMem);

    /// <summary>
    /// 安全获取内存块大小
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns>内存大小，单位为字节，失败返回0</returns>
    public static uint GlobalSizeSafe(IntPtr hMem)
    {
        if (hMem == IntPtr.Zero)
        {
            return 0;
        }
        return GlobalSize(hMem);
    }

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
        if (structPtr == IntPtr.Zero)
            return default;

        try
        {
            // 将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, typeSize);
            // 将内存空间转换为目标结构体;
            // 转类型的时候会拷贝一次,看它们地址验证 &result != &structPtr
            var result = (T)Marshal.PtrToStructure(structPtr, structType);
            return result;
        }
        finally
        {
            // 释放内存空间
            Marshal.FreeHGlobal(structPtr);
        }
    }

    /// <summary>
    /// byte数组转结构体
    /// </summary>
    /// <param name="bytes">byte数组</param>
    /// <returns>返回的结构体</returns>
    [MethodImpl]
    public static T? BytesToStruct<T>(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            DebugEx.Printl("[BytesToStruct] 错误: 输入数组为空");
            return default;
        }

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
        if (typeSize == 0)
        {
            DebugEx.Printl("[StructToBytes] 错误: 结构体大小为0");
            return new byte[0];
        }

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


    /// <summary>
    /// 是窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);

    /// <summary>
    /// 检查是否有模态窗口正在活动
    /// </summary>
    /// <returns></returns>
    public static bool IsModalWindowActive(IntPtr MainWindowHandle)
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero || foregroundWindow == MainWindowHandle)
        {
            // 如果没有前台窗口或前台就是主窗口，则没有模态窗口
            return false;
        }

        // 获取前台窗口的进程ID
        GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);
        // 获取主窗口的进程ID
        GetWindowThreadProcessId(MainWindowHandle, out uint mainProcessId);

        // 如果前台窗口与主窗口属于同一进程，且不是主窗口本身，则很可能是模态对话框
        return foregroundProcessId == mainProcessId && foregroundWindow != MainWindowHandle;
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

/// <summary>
/// 系统Api
/// </summary>
public static partial class WindowsAPI
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
    /// 托管线程和他们不一样: <see cref="Thread.ManagedThreadId"/>
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
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// 获取当前前台窗口（带错误检查）
    /// 没有键盘焦点(比如弹出了模态对话框),它依然返回主窗口句柄.
    /// </summary>
    /// <returns>前台窗口句柄，失败返回IntPtr.Zero</returns>
    public static IntPtr GetForegroundWindowSafe()
    {
        var result = GetForegroundWindow();
        if (result == IntPtr.Zero)
        {
            CheckWin32Error(nameof(GetForegroundWindow), result);
        }
        return result;
    }
    /// <summary>
    /// 将一个消息的组成部分合成一个消息并放入对应线程消息队列的方法
    /// </summary>
    /// <param name="hhwnd">控件句柄</param>
    /// <param name="msg">消息是什么。键盘按键、鼠标点击还是其他</param>
    /// <param name="wparam"></param>
    /// <param name="lparam"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostMessage(IntPtr hhwnd, int msg, IntPtr wparam, IntPtr lparam);

    /// <summary>
    /// 发送消息到指定窗口（带错误检查）
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="msg">消息类型</param>
    /// <param name="wParam">消息参数</param>
    /// <param name="lParam">消息参数</param>
    /// <returns>是否成功</returns>
    public static bool PostMessageSafe(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        var result = PostMessage(hWnd, msg, wParam, lParam);
        if (!result)
        {
            CheckWin32Error(nameof(PostMessage), result);
        }
        return result;
    }
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
    /// <param name="nMaxCount">文字长度</param>
    /// <returns></returns>
    [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

    /// <summary>
    /// 获取窗口标题（带错误检查）
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="text">接收标题的StringBuilder</param>
    /// <param name="nMaxCount">最大字符数</param>
    /// <returns>是否成功</returns>
    public static bool GetWindowTextSafe(IntPtr hWnd, StringBuilder text, int nMaxCount)
    {
        var result = GetWindowText(hWnd, text, nMaxCount);
        if (result == 0)
        {
            uint errorCode = GetLastError();
            if (errorCode != 0)
            {
                DebugEx.Printl($"[Win32错误] GetWindowText 失败，错误码: {errorCode} (0x{errorCode:X8})");
            }
            return false;
        }
        return true;
    }

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

    /// <summary>
    /// 获取窗口类名（带错误检查）
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="lpClassName">接收类名的StringBuilder</param>
    /// <param name="nMaxCount">最大字符数</param>
    /// <returns>是否成功</returns>
    public static bool GetClassNameSafe(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
    {
        var result = GetClassName(hWnd, lpClassName, nMaxCount);
        if (result == 0)
        {
            CheckWin32Error(nameof(GetClassName), result);
            return false;
        }
        return true;
    }

    /// <summary>
    /// 获取窗口
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="uCmd">命令</param>
    /// <returns>窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    /// <summary>
    /// 获取顶层窗口
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>顶层窗口句柄</returns>
    [DllImport("user32.DLL", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr GetTopWindow(IntPtr hWnd);


    /// <summary>
    /// 获取线程对应的窗体信息
    /// </summary>
    /// <param name="idThread">线程ID</param>
    /// <param name="lpgui">GUI线程信息</param>
    /// <returns>获取是否成功</returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

    /// <summary>
    /// 获取线程对应的窗体信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GuiThreadInfo
    {
        /// <summary>
        /// 结构体大小
        /// </summary>
        public int cbSize;

        /// <summary>
        /// 标志位
        /// </summary>
        public int flags;

        /// <summary>
        /// 活动窗口句柄
        /// </summary>
        public IntPtr hwndActive;

        /// <summary>
        /// 焦点窗口句柄
        /// </summary>
        public IntPtr hwndFocus;

        /// <summary>
        /// 捕获鼠标输入的窗口句柄
        /// </summary>
        public IntPtr hwndCapture;

        /// <summary>
        /// 菜单拥有者窗口句柄
        /// </summary>
        public IntPtr hwndMenuOwner;

        /// <summary>
        /// 正在移动或调整大小的窗口句柄
        /// </summary>
        public IntPtr hwndMoveSize;

        /// <summary>
        /// 插入符窗口句柄
        /// </summary>
        public IntPtr hwndCaret;

        /// <summary>
        /// 插入符矩形区域
        /// </summary>
        public System.Drawing.Rectangle rcCaret;

        /// <summary>
        /// 创建并初始化GuiThreadInfo结构体
        /// </summary>
        /// <param name="windowThreadProcessId">窗口线程进程ID</param>
        /// <returns>初始化后的GuiThreadInfo结构体</returns>
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

    /// <summary>
    /// 获取当前焦点窗口
    /// </summary>
    /// <returns>焦点窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetFocus();

    /// <summary>
    /// 获取当前焦点窗口（带错误检查）
    /// 拥有键盘输入焦点的窗口
    /// </summary>
    /// <returns>焦点窗口句柄，失败返回IntPtr.Zero</returns>
    public static IntPtr GetFocusSafe()
    {
        var result = GetFocus();
        if (result == IntPtr.Zero)
        {
            CheckWin32Error(nameof(GetFocus), result);
        }
        return result;
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <summary>
    /// 向窗口发送消息
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="msg">消息类型</param>
    /// <param name="wParam">消息参数</param>
    /// <param name="lParam">消息参数</param>
    /// <returns>消息处理结果</returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 获取父窗口
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>父窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    /// <summary>
    /// 获取父窗口（带错误检查）
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>父窗口句柄，失败返回IntPtr.Zero</returns>
    public static IntPtr GetParentSafe(IntPtr hWnd)
    {
        var result = GetParent(hWnd);
        // GetParent返回NULL表示没有父窗口或出错，需要通过GetLastError区分
        if (result == IntPtr.Zero)
        {
            uint errorCode = GetLastError();
            if (errorCode != 0)
            {
                DebugEx.Printl($"[Win32错误] GetParent 失败，错误码: {errorCode} (0x{errorCode:X8})");
            }
        }
        return result;
    }

    /// <summary>
    /// 将虚拟键码转换为ASCII字符
    /// </summary>
    /// <param name="uVirtKey">虚拟键码</param>
    /// <param name="uScancode">扫描码</param>
    /// <param name="lpdKeyState">键盘状态</param>
    /// <param name="lpwTransKey">输出缓冲区</param>
    /// <param name="fuState">状态标志</param>
    /// <returns>转换结果</returns>
    [DllImport("user32.dll")]
    public static extern int ToAscii(int uVirtKey, int uScancode, byte[] lpdKeyState, byte[] lpwTransKey, int fuState);

    /// <summary>
    /// 获取活动窗口
    /// </summary>
    /// <returns>活动窗口句柄</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetActiveWindow();

    /// <summary>
    /// 获取窗口线程进程ID
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="lpdwProcessId">进程ID</param>
    /// <returns>线程ID</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern long GetWindowThreadProcessId(IntPtr hwnd, ref int lpdwProcessId);

    /// <summary>
    /// 判断窗口是否最小化
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>是否最小化</returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsIconic(int hWnd);

    /// <summary>
    /// 判断窗口是否启用
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>是否启用</returns>
    [DllImport("user32.dll")]
    public static extern bool IsWindowEnabled(IntPtr hWnd);

    /// <summary>
    /// 获取系统双击时间
    /// </summary>
    /// <returns>双击时间（毫秒）</returns>
    [DllImport("user32.dll")]
    public static extern int GetDoubleClickTime();
    #endregion

    #region 键盘钩子
    /// <summary>
    /// Windows API回调委托 - 使用StdCall调用约定以确保x86/x64兼容性
    /// </summary>
    /// <param name="nCode">钩子代码</param>
    /// <param name="wParam">消息参数</param>
    /// <param name="lParam">消息参数</param>
    /// <returns>回调结果</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr CallBack(int nCode, int wParam, IntPtr lParam);

    /// <summary>
    /// 设置Windows钩子 - 带异常保护包装
    /// </summary>
    /// <param name="idHook">钩子类型</param>
    /// <param name="lpfn">回调函数</param>
    /// <param name="hmod">模块句柄</param>
    /// <param name="dwThreadId">线程ID</param>
    /// <returns>钩子句柄</returns>
    public static IntPtr SetWindowsHookExSafe(HookType idHook, CallBack lpfn, IntPtr hmod, int dwThreadId)
    {
        try
        {
            return SetWindowsHookExInternal(idHook, lpfn, hmod, dwThreadId);
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[SetWindowsHookExSafe] BadImageFormatException: {ex.Message}");
            return IntPtr.Zero;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[SetWindowsHookExSafe] 异常: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// 设置Windows钩子 - 内部实现
    /// </summary>
    [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern IntPtr SetWindowsHookExInternal(HookType idHook, CallBack lpfn, IntPtr hmod, int dwThreadId);

    /// <summary>
    /// 卸载Windows钩子 - 带异常保护包装
    /// </summary>
    /// <param name="hHook">钩子句柄</param>
    /// <returns>卸载是否成功</returns>
    public static bool UnhookWindowsHookExSafe(IntPtr hHook)
    {
        if (hHook == IntPtr.Zero)
            return false;
        try
        {
            return UnhookWindowsHookExInternal(hHook) != IntPtr.Zero;
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[UnhookWindowsHookExSafe] BadImageFormatException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[UnhookWindowsHookExSafe] 异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 卸载Windows钩子 - 内部实现
    /// </summary>
    [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern IntPtr UnhookWindowsHookExInternal(IntPtr hHook);

    /// <summary>
    /// 调用下一个钩子 - 带异常保护包装
    /// </summary>
    /// <param name="hHook">钩子句柄</param>
    /// <param name="ncode">钩子代码</param>
    /// <param name="wParam">消息参数</param>
    /// <param name="lParam">消息参数</param>
    /// <returns>钩子处理结果</returns>
    public static IntPtr CallNextHookExSafe(IntPtr hHook, int ncode, int wParam, IntPtr lParam)
    {
        try
        {
            return CallNextHookExInternal(hHook, ncode, wParam, lParam);
        }
        catch (BadImageFormatException ex)
        {
            DebugEx.Printl($"[CallNextHookExSafe] BadImageFormatException: {ex.Message}");
            return IntPtr.Zero;
        }
        catch (Exception ex)
        {
            DebugEx.Printl($"[CallNextHookExSafe] 异常: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// 调用下一个钩子 - 内部实现
    /// </summary>
    [DllImport("user32.dll", EntryPoint = "CallNextHookEx", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern IntPtr CallNextHookExInternal(IntPtr hHook, int ncode, int wParam, IntPtr lParam);

    // 保留原始方法以保持兼容性，但标记为已过时
    /// <summary>
    /// 设置Windows钩子（原始方法，建议使用SetWindowsHookExSafe）
    /// </summary>
    [Obsolete("请使用 SetWindowsHookExSafe 以获得更好的异常保护", false)]
    [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(HookType idHook, CallBack lpfn, IntPtr hmod, int dwThreadId);

    /// <summary>
    /// 卸载Windows钩子（原始方法，建议使用UnhookWindowsHookExSafe）
    /// </summary>
    [Obsolete("请使用 UnhookWindowsHookExSafe 以获得更好的异常保护", false)]
    [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr UnhookWindowsHookEx(IntPtr hHook);

    /// <summary>
    /// 调用下一个钩子（原始方法，建议使用CallNextHookExSafe）
    /// </summary>
    [Obsolete("请使用 CallNextHookExSafe 以获得更好的异常保护", false)]
    [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
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
        /// <summary>
        /// 键码,该代码必须有一个价值的范围1至254
        /// </summary>
        public int VkCode;

        /// <summary>
        /// 指定的硬件扫描码的关键
        /// </summary>
        public int ScanCode;

        /// <summary>
        /// 键标志
        /// </summary>
        public int Flags;

        /// <summary>
        /// 指定的时间戳记的这个讯息
        /// </summary>
        public int Time;

        /// <summary>
        /// 指定额外信息相关的信息
        /// </summary>
        public int DwExtraInfo;

        /// <summary>
        /// 从IntPtr创建KeyboardHookStruct实例
        /// </summary>
        /// <param name="lParam">包含键盘钩子数据的指针</param>
        /// <returns>KeyboardHookStruct实例</returns>
        public static KeyboardHookStruct Create(IntPtr lParam)
        {
            return (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
        }

        /// <summary>
        /// 将当前实例转换为IntPtr
        /// </summary>
        /// <param name="lParam">目标指针</param>
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

        /// <summary>
        /// 左边界
        /// </summary>
        public int Left => _Left;

        /// <summary>
        /// 上边界
        /// </summary>
        public int Top => _Top;

        /// <summary>
        /// 右边界
        /// </summary>
        public int Right => _Right;

        /// <summary>
        /// 下边界
        /// </summary>
        public int Bottom => _Bottom;

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width => checked(_Right - _Left);

        /// <summary>
        /// 高度
        /// </summary>
        public int Height => checked(_Bottom - _Top);

        /// <summary>
        /// 初始化IntRect结构体
        /// </summary>
        /// <param name="left">左边界</param>
        /// <param name="top">上边界</param>
        /// <param name="right">右边界</param>
        /// <param name="bottom">下边界</param>
        public IntRect(int left, int top, int right, int bottom)
        {
            _Left = left;
            _Top = top;
            _Right = right;
            _Bottom = bottom;
        }

        static readonly IntRect _Zero = new(0, 0, 0, 0);

        /// <summary>
        /// 获取零矩形
        /// </summary>
        public static IntRect Zero => _Zero;

        /// <summary>
        /// 返回矩形的字符串表示
        /// </summary>
        /// <returns>矩形的字符串表示</returns>
        public override string ToString() => $"({_Left},{_Top},{_Right},{_Bottom})";

        #region 重载运算符_比较
        /// <summary>
        /// 判断两个矩形是否相等
        /// </summary>
        /// <param name="other">另一个矩形</param>
        /// <returns>是否相等</returns>
        public bool Equals(IntRect other)
        {
            return
            _Left == other._Left &&
            _Top == other._Top &&
            _Right == other._Right &&
            _Bottom == other._Bottom;
        }

        /// <summary>
        /// 判断两个矩形是否不相等
        /// </summary>
        /// <param name="a">第一个矩形</param>
        /// <param name="b">第二个矩形</param>
        /// <returns>是否不相等</returns>
        public static bool operator !=(IntRect a, IntRect b)
        {
            return !(a == b);
        }

        /// <summary>
        /// 判断两个矩形是否相等
        /// </summary>
        /// <param name="a">第一个矩形</param>
        /// <param name="b">第二个矩形</param>
        /// <returns>是否相等</returns>
        public static bool operator ==(IntRect a, IntRect b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 判断对象是否等于当前矩形
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return obj is IntRect d && Equals(d);
        }

        /// <summary>
        /// 获取哈希代码
        /// </summary>
        /// <returns>哈希代码</returns>
        public override int GetHashCode()
        {
            return ((_Left, _Top).GetHashCode(), _Right).GetHashCode() ^ _Bottom.GetHashCode();
        }

        /// <summary>
        /// 克隆矩形
        /// </summary>
        /// <returns>克隆的矩形</returns>
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
        /// <summary>
        /// 高度
        /// </summary>
        public int Hight;

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width;

        /// <summary>
        /// 初始化IntSize结构体
        /// </summary>
        /// <param name="cx">宽度</param>
        /// <param name="cy">高度</param>
        public IntSize(int cx, int cy)
        {
            Hight = cx;
            Width = cy;
        }

        /// <summary>
        /// 返回尺寸的字符串表示
        /// </summary>
        /// <returns>尺寸的字符串表示</returns>
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

        /// <summary>
        /// X坐标
        /// </summary>
        public double X;

        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y;

        /// <summary>
        /// Z坐标
        /// </summary>
        public double Z;

        /// <summary>
        /// 初始化Point3D结构体
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Point3d到Point3D的隐式转换
        /// </summary>
        /// <param name="pt">Point3d实例</param>
        public static implicit operator Point3D(Point3d pt)
        {
            return new Point3D(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Point3D到Point3d的隐式转换
        /// </summary>
        /// <param name="pt">Point3D实例</param>
        public static implicit operator Point3d(Point3D pt)
        {
            return new Point3d(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// 返回点的字符串表示
        /// </summary>
        /// <returns>点的字符串表示</returns>
        public override string ToString() => $"({X},{Y},{Z})";

        /// <summary>
        /// 从IntPtr创建Point3D实例
        /// </summary>
        /// <param name="lParam">包含点数据的指针</param>
        /// <returns>Point3D实例</returns>
        public static Point3D Create(IntPtr lParam)
        {
            return (Point3D)Marshal.PtrToStructure(lParam, typeof(Point3D));
        }

        /// <summary>
        /// 将当前实例转换为IntPtr
        /// </summary>
        /// <param name="lParam">目标指针</param>
        public void ToPtr(IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }


        #region 重载运算符_比较
        /// <summary>
        /// 判断两个三维点是否相等
        /// </summary>
        /// <param name="other">另一个点</param>
        /// <returns>是否相等</returns>
        public bool Equals(Point3D other)
        {
            return
            X == other.X &&
            Y == other.Y &&
            Z == other.Z;
        }

        /// <summary>
        /// 判断两个三维点是否不相等
        /// </summary>
        /// <param name="a">第一个点</param>
        /// <param name="b">第二个点</param>
        /// <returns>是否不相等</returns>
        public static bool operator !=(Point3D a, Point3D b)
        {
            return !(a == b);
        }

        /// <summary>
        /// 判断两个三维点是否相等
        /// </summary>
        /// <param name="a">第一个点</param>
        /// <param name="b">第二个点</param>
        /// <returns>是否相等</returns>
        public static bool operator ==(Point3D a, Point3D b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 判断对象是否等于当前点
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return obj is Point3D d && Equals(d);
        }

        /// <summary>
        /// 获取哈希代码
        /// </summary>
        /// <returns>哈希代码</returns>
        public override int GetHashCode()
        {
            return (X, Y).GetHashCode() ^ Z.GetHashCode();
        }
        #endregion
    }
}