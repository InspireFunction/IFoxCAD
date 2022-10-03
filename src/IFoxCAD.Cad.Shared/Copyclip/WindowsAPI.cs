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
#if Marshal
        IntPtr structPtr = Marshal.AllocHGlobal(typeSize);
#else
        const int GMEM_MOVEABLE = 0x0002;
        IntPtr structPtr = WindowsAPI.GlobalAlloc(GMEM_MOVEABLE, typeSize);
#endif
        // 将byte数组拷到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, typeSize);
        // 将内存空间转换为目标结构体
        result = (T)Marshal.PtrToStructure(structPtr, structType);
        // 释放内存空间
#if Marshal
        Marshal.FreeHGlobal(structPtr);
#else
        WindowsAPI.GlobalFree(structPtr);
#endif
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
#if Marshal
        IntPtr structPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structObj));
#else
        const int GMEM_MOVEABLE = 0x0002;
        IntPtr structPtr = WindowsAPI.GlobalAlloc(GMEM_MOVEABLE, Marshal.SizeOf(structObj));
#endif
        if (structPtr == IntPtr.Zero)
            return;
        try
        {
            if (lockPrt)
                GlobalLockTask(structPtr, ptr => {
                    ToPtr(structObj, task, ptr);
                });
            else
                ToPtr(structObj, task, structPtr);
        }
        catch (Exception e)
        {
            Debugger.Break();
            Debug.WriteLine(e.Message);
        }
        finally
        {
            if (freeHGlobal && structPtr != IntPtr.Zero)
            {
#if Marshal
                Marshal.FreeHGlobal(structPtr);
#else
                WindowsAPI.GlobalFree(structPtr);
#endif
            }
        }

        // 将结构体拷到分配好的内存空间
        static void ToPtr(object? structObj, Action<IntPtr>? task, IntPtr structPtr)
        {
            Marshal.StructureToPtr(structObj, structPtr, true);
            task?.Invoke(structPtr);
        }
    }
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