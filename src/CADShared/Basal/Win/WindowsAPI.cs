#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#define Marshal
#if !NET8_0_OR_GREATER
using ArgumentNullException = IFoxCAD.Basal.ArgumentNullEx;
#endif
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
    public static extern IntPtr GetModuleHandle(string moduleName);

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
    private static extern IntPtr GlobalLock(IntPtr hMem);

    /// <summary>
    /// 解锁内存
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(IntPtr hMem);
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
    /// <exception cref="System.ArgumentNullException"></exception>
    public static bool GlobalLockTask(IntPtr data, Action<IntPtr> task)
    {
        //if (task == null)
        //    throw new ArgumentNullException(nameof(task));
        ArgumentNullException.ThrowIfNull(task);
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
        finally
        {
            GlobalUnlock(data);
        }

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
        var structPtr = Marshal.AllocHGlobal(typeSize);

        // 将byte数组拷到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, typeSize);
        // 将内存空间转换为目标结构体;
        // 转类型的时候会拷贝一次,看它们地址验证 &result != &structPtr
        var result = (T)Marshal.PtrToStructure(structPtr, structType)!;

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
        T? result;
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
    public static byte[] StructToBytes<T>(T structObj) where T : unmanaged /*非托管的T从来不为空*/
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
    #region 键盘钩子

    public delegate IntPtr CallBack(int nCode, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowsHookEx(HookType idHook, CallBack lpfn, IntPtr hmod,
        int dwThreadId);

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
        public int VkCode; // 键码,该代码必须有一个价值的范围1至254
        public int ScanCode; // 指定的硬件扫描码的关键
        public int Flags; // 键标志
        public int Time; // 指定的时间戳记的这个讯息
        public int DwExtraInfo; // 指定额外信息相关的信息

        public static KeyboardHookStruct Create(IntPtr lParam)
        {
            return (KeyboardHookStruct)(Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct)) ?? throw new InvalidOperationException());
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
        if (registryKey is not null && (int)registryKey.GetValue(llh, 0) < setLowLevel)
        {
            registryKey.SetValue(llh, setLowLevel, RegistryValueKind.DWord);
        }
    }

    #endregion
}

public partial class WindowsAPI
{
    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="hWnd"></param>
    public static void CloseWindow(IntPtr hWnd)
    {
        PInvokeUser32.SendMessage(hWnd, WmMessage.Close, IntPtr.Zero, IntPtr.Zero);
    }

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
        private string DebuggerDisplay =>
            $"(Left:{_Left},Top:{_Top},Right:{_Right},Bottom:{_Bottom})";

        private int _Left;
        private int _Top;
        private int _Right;
        private int _Bottom;
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

        private static readonly IntRect _Zero = new(0, 0, 0, 0);
        public static IntRect Zero => _Zero;

        public override string ToString() => $"({_Left},{_Top},{_Right},{_Bottom})";

        #region 重载运算符_比较

        public bool Equals(IntRect other)
        {
            return _Left == other._Left && _Top == other._Top && _Right == other._Right &&
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

        public override bool Equals(object? obj)
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
            return (Point3D)(Marshal.PtrToStructure(lParam, typeof(Point3D)) ?? throw new InvalidOperationException());
        }

        public void ToPtr(IntPtr lParam)
        {
            Marshal.StructureToPtr(this, lParam, true);
        }

        #region 重载运算符_比较

        public bool Equals(Point3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public static bool operator !=(Point3D a, Point3D b)
        {
            return !(a == b);
        }

        public static bool operator ==(Point3D a, Point3D b)
        {
            return a.Equals(b);
        }

        public override bool Equals(object? obj)
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

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释