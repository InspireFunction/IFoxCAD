#define Marshal
namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.Threading;

public class WindowsAPI
{
    // https://blog.csdn.net/haelang/article/details/45147121
    [DllImport("kernel32.dll")]
    public extern static uint GetLastError();

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
    /// <param name="result">返回的结构体</param>
    /// <param name="typeSize">返回的结构大小</param>
    /// <returns>转换后的结构体</returns>
    public static bool BytesToStruct<T>(byte[] bytes, out T? result, out int typeSize)
    {
        result = default;
        var structType = typeof(T);
        // 得到结构体的大小
        typeSize = Marshal.SizeOf(structType);
        if (typeSize > bytes.Length)
            return false;

        // 分配结构体大小的内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(typeSize);
        // 将byte数组拷到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, typeSize);
        // 将内存空间转换为目标结构体
        result = (T)Marshal.PtrToStructure(structPtr, structType);
        // 释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return true;
    }

    /// <summary>
    /// 结构体转byte数组
    /// </summary>
    /// <param name="structObj">要转换的结构体</param>
    public static byte[] StructToBytes(object? structObj)
    {
        // 得到结构体的大小
        int typeSize = Marshal.SizeOf(structObj);
        // 从内存空间拷到byte数组
        byte[] bytes = new byte[typeSize];

        StructToPtr(structObj, structPtr => {
            Marshal.Copy(structPtr, bytes, 0, typeSize);
        });
        return bytes;
    }

    /// <summary>
    /// 结构体转指针
    /// </summary>
    /// <param name="structObj">要转换的结构体</param>
    /// <param name="task">输出指针</param>
    /// <param name="freeHGlobal">释放申请的内存</param>
    /// <param name="lockPrt">是否锁定内存</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void StructToPtr(object? structObj,
                                   Action<IntPtr>? task = null,
                                   bool freeHGlobal = true,
                                   bool lockPrt = true)
    {
        if (structObj == null)
            throw new ArgumentNullException(nameof(structObj));
        IntPtr newPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structObj));
        if (newPtr == IntPtr.Zero)
            throw new ArgumentException(nameof(newPtr));
        try
        {
            // 剪贴板写入的时候不允许锁定内存,否则在频繁触发剪贴板将导致卡死程序
            if (lockPrt)
                GlobalLockTask(newPtr, ptr => {
                    ToPtr(structObj, task, ptr);
                });
            else
                ToPtr(structObj, task, newPtr);
        }
        catch (Exception e)
        {
            Debugger.Break();
            Debug.WriteLine(e.Message);
        }
        finally
        {
            if (freeHGlobal && newPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(newPtr);
        }

        // 将结构体拷到分配好的内存空间
        static void ToPtr(object? structObj, Action<IntPtr>? task, IntPtr newPtr)
        {
            Marshal.StructureToPtr(structObj, newPtr, true);
            task?.Invoke(newPtr);
        }
    }


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
    /// <returns>非0值打开，0值关闭</returns>
    [DllImport("imm32.dll")]
    public static extern bool ImmGetOpenStatus(IntPtr hwnd);
    #endregion

    #region user32
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
    [DllImport("user32.dll")]
    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
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
    /// <returns>按键状态值，高位为1表示按下（<0），0表示弹起（>0）</returns>
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
    #endregion

    [DllImport("kernel32.dll")]
    public static extern int GetCurrentThreadId();
}


#if true2
// arx剪贴板头文件的枚举
enum eClipInfoFlags
{
    kbDragGeometry = 0x01,
};

enum eXrefType
{
    kXrefTypeAttach = 1,
    kXrefTypeOverlay = 2
};

enum eExpandedClipDataTypes
{
    kDcPlotStyles = 1,
    kDcXrefs = 2,
    kDcLayouts = 3,
    kDcBlocks = 4,
    kDcLayers = 5,
    kDcDrawings = 6,
    kDcLinetypes = 7,
    kDcTextStyles = 8,
    kDcDimStyles = 9,
    kDcBlocksWithAttdef = 10,
    //#ifdef ADCHATCH
    kDcHatches = 11,
    //#endif
    kTpXrefs = 12,
    kTpImages = 13,
    kTpTable = 14,
    kDcTableStyles = 15,
    kDcMultileaderStyles = 16,
    kDcVisualStyles = 17,
    kDcSectionViewStyles = 18,
    kDcDetailViewStyles = 19,
};
#endif