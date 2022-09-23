using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace IFoxCAD.Cad;

#if true2
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

public class ClipboardEnv
{
    // 此句将导致剪贴板的key隔离,从而导致cad版本隔离
    // public static string CadVer = $"AutoCAD.r{Acap.Version.Major}"
    // 将r17写死,代表每个cad版本都去找它,实现不隔离cad版本
    public static string CadVer = "AutoCAD.r17";
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct IntRect
{
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }
    public IntRect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public static IntRect Zero => new(0, 0, 0, 0);

    public override string ToString()
    {
        return $"Left:{Left},Top:{Top},Right:{Right},Bottom:{Bottom}";
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct Point3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public static implicit operator Point3D(Point3d pt)
    {
        return new Point3D(pt.X, pt.Y, pt.Z);
    }
    public static implicit operator Point3d(Point3D pt)
    {
        return new Point3d(pt.X, pt.Y, pt.Z);
    }

    public override string ToString()
    {
        return $"X:{X},Y:{Y},Z:{Z}";
    }
}
/// <summary>
/// ARX剪贴板结构
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode/*此参数将导致260*2*/)]
public struct TagClipboardInfo
{
    #region 字段,对应arx结构的,不要改动,本结构也不允许再加字段
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szTempFile;                               // 临时文件夹的dwg文件
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szSourceFile;                             // 文件名从中做出选择..是不是指定块表记录?
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string szSignature;
    public int nFlags;                                      // kbDragGeometry: 从AutoCAD拖动几何图形
    public Point3D dptInsert;                               // 插入点的原始世界坐标'
    public IntRect rectGDI;                                 // GDI coord 选择集的边界矩形
    public IntPtr mpView;                                   // void*  用于验证这个对象是在这个视图中创建的 (HWND*)
    public int dwThreadId;                                  // AutoCAD thread 创建数据对象
    public int nLen;                                        // 下一段的长度的数据,如果有的话,从chData
    public int nType;                                       // 类型的数据,如果有(eExpandedClipDataTypes)
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string chData; // 数据的开始,如果有
    #endregion

    public string File => szTempFile;
    public Point3d Point => dptInsert;

    /// <summary>
    /// cad剪贴板
    /// </summary>
    /// <param name="tmpFile">临时dwg的保存路径</param>
    /// <param name="insert">粘贴点</param>
    public TagClipboardInfo(string tmpFile, Point3d insert)
    {
        szTempFile = tmpFile;
        szSourceFile = string.Empty;
        szSignature = "R15";  //恒定是这个
        nFlags = 0;
        dptInsert = insert;
        rectGDI = IntRect.Zero;
        nLen = 0;
        nType = 0;
        chData = string.Empty;

        mpView = AcedGetAcadDwgview();
        dwThreadId = (int)NativeMethods.GetWindowThreadProcessId(
                          Acap.MainWindow.Handle, out uint processId);
    }

    /// <summary>
    /// 获取视口指针
    /// </summary>
#if NET35
    [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPAVCView@@XZ")] //acad08
#else
    [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPEAVCView@@XZ")]//acad21
#endif
    static extern IntPtr AcedGetAcadDwgview();


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"szTempFile:{szTempFile}");
        sb.AppendLine($"szSourceFile:{szSourceFile}");
        sb.AppendLine($"szSignature:{szSignature}");
        sb.AppendLine($"nFlags:{nFlags}");
        sb.AppendLine($"dptInsert:{dptInsert}");
        sb.AppendLine($"rectGDI:{rectGDI}");
        sb.AppendLine($"mpView:{mpView}");
        sb.AppendLine($"dwThreadId:{dwThreadId}");
        sb.AppendLine($"nLen:{nLen}");
        sb.AppendLine($"nType:{nType}");
        sb.AppendLine($"chData:{chData}");
        return sb.ToString();
    }



    /// <summary>
    /// 获取剪贴板
    /// </summary>
    /// <param name="cadVer">控制不同的cad</param>
    public static TagClipboardInfo? GetClipboard()
    {
        TagClipboardInfo? tag = null;
        bool openFlag = false;
        try
        {
            openFlag = NativeMethods.OpenClipboard(IntPtr.Zero);
            if (!openFlag)
                return null;

            // 读取剪贴板的指定 key 的数据
            var key = NativeMethods.RegisterClipboardFormat(ClipboardEnv.CadVer);
            var data = NativeMethods.GetClipboardData(key);

            // 剪贴板的数据拷贝进去结构体中,会依照数据长度进行拷贝
            bool lockFlag = NativeMethods.GlobalLockTask(data, ptr => {
                // 非托管内存块->托管对象
                tag = (TagClipboardInfo)Marshal.PtrToStructure(ptr, typeof(TagClipboardInfo));
            });
            if (!lockFlag)
                return null;
        }
        finally
        {
            if (openFlag)
                NativeMethods.CloseClipboard();
        }
        return tag;
    }


    /// <summary>
    /// 设置剪贴板
    /// </summary>
    /// <returns>true成功拷贝;false可能重复粘贴对象导致</returns>
    public static bool SetClipboard(TagClipboardInfo tag)
    {
        IntPtr data = IntPtr.Zero;
        bool openFlag = false;
        try
        {
            openFlag = NativeMethods.OpenClipboard(IntPtr.Zero);
            if (!openFlag)
                return false;
            const uint GMEM_MOVEABLE = 0x0002;
            int size = Marshal.SizeOf(typeof(TagClipboardInfo));
            data = NativeMethods.GlobalAlloc(GMEM_MOVEABLE, size);

            bool lockFlag = NativeMethods.GlobalLockTask(data, ptr => {
                NativeMethods.EmptyClipboard();
                // 托管对象->非托管内存块
                Marshal.StructureToPtr(tag, ptr, true);
            });
            if (!lockFlag)
                return false;

            var key = NativeMethods.RegisterClipboardFormat(ClipboardEnv.CadVer);
            NativeMethods.SetClipboardData(key, data);
        }
        finally
        {
            if (data != IntPtr.Zero)
                NativeMethods.GlobalFree(data);
            if (openFlag)
                NativeMethods.CloseClipboard();
        }
        return true;
    }

#if true2
        // 这种写入方式是失败的
        /// <summary>
        /// 写入剪贴板
        /// </summary>
        public void SetClipboard()
        {
            using var data = new MemoryStream();
            var bytes = Encoding.Unicode.GetBytes(ArxTagClipboardInfo.ToString());
            data.Write(bytes, 0, bytes.Length);
            Clipboard.SetData($"AutoCAD.r{Acap.Version.Major}", data);
        }

        /// <summary>
        /// 获取剪贴板
        /// </summary>
        /// <param name="pasteclipStr">控制不同的cad:"AutoCAD"</param>
        /// <returns>转为结构输出</returns>
        public static TagClipboardInfo? GetClipboard(string pasteclipStr = "AutoCAD")
        {
            /// 粘贴基点字符串
            const string PointStr = "BasePoint:";

            // 获取剪贴板上面的保存路径
            var format = Clipboard.GetDataObject()
                                  .GetFormats()
                                  .FirstOrDefault(f => f.StartsWith(pasteclipStr));
            if (string.IsNullOrEmpty(format))
                return null;

            using var data = (MemoryStream)Clipboard.GetData(format);
            using var reader = new StreamReader(data);

            // 获取剪贴板设置的:临时文件夹中的dwg文件路径
            var str = reader.ReadToEnd();
            str = str.Replace("\0", string.Empty);
            var file = str.Substring(0, str.IndexOf(".DWG") + 4);
            if (string.IsNullOrEmpty(Path.GetFileName(file)))
                return null;

            // 获取剪贴板设置的:dwg基点
            var bpstr = str.IndexOf(PointStr);
            if (bpstr == -1)
                return null;

            int start = bpstr + PointStr.Length + 1;//1是(括号
            int end = str.IndexOf(")", start) - start;
            var ptstr = str.Substring(start, end);
            var ptArr = ptstr.Split(',');

            double x = 0;
            double y = 0;
            double z = 0;
            if (ptArr.Length > 0)
                x = double.Parse(ptArr[0]);
            if (ptArr.Length > 1)
                y = double.Parse(ptArr[1]);
            if (ptArr.Length > 2)
                z = double.Parse(ptArr[2]);

            return new(file, new(x, y, z));
        }
#endif
    void GetSize()
    {
        var v_1 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.szTempFile)).ToInt32();
        var v_2 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.szSourceFile)).ToInt32();
        var v_3 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.szSignature)).ToInt32();
        var v_4 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.nFlags)).ToInt32();
        var v_5 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.dptInsert)).ToInt32();
        var v_6 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.rectGDI)).ToInt32();
        var v_7 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.mpView)).ToInt32();
        var v_8 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.dwThreadId)).ToInt32();
        var v_9 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.nLen)).ToInt32();
        var v_10 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.nType)).ToInt32();
        var v_11 = Marshal.OffsetOf(typeof(TagClipboardInfo), nameof(TagClipboardInfo.chData)).ToInt32();
        var v_12 = Marshal.SizeOf(typeof(TagClipboardInfo)); //1120

        var v_a = Marshal.SizeOf(typeof(Point3D));//24
        var v_b = Marshal.SizeOf(typeof(IntRect));//16
    }
}
class NativeMethods
{
    /// <summary>
    /// 锁定内存
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern IntPtr GlobalLock(IntPtr hMem);
    /// <summary>
    /// 解锁内存
    /// </summary>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern bool GlobalUnlock(IntPtr hMem);
    /// <summary>
    /// 开启剪贴板
    /// </summary>
    /// <param name="hWndNewOwner"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool OpenClipboard(IntPtr hWndNewOwner);
    /// <summary>
    /// 关闭剪贴板
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool CloseClipboard();
    /// <summary>
    /// 根据数据格式获取剪贴板
    /// </summary>
    /// <param name="lpszFormat">数据格式名称</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern uint RegisterClipboardFormat(string lpszFormat);
    /// <summary>
    /// 获取剪贴板
    /// </summary>
    /// <param name="uFormat"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetClipboardData(uint uFormat);
    /// <summary>
    /// 设置剪贴板
    /// </summary>
    /// <param name="uFormat"></param>
    /// <param name="hMem"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
    /// <summary>
    /// 清空剪切板并释放剪切板内数据的句柄
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool EmptyClipboard();
    /// <summary>
    /// 从堆中分配一定数目的字节数
    /// </summary>
    /// <param name="uFlags">分配方式</param>
    /// <param name="dwBytes">分配的字节数</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalAlloc(uint uFlags, int dwBytes);
    /// <summary>
    /// 释放堆内存
    /// </summary>
    /// <param name="hMem">由<see cref="GlobalAlloc"/>产生的句柄</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalFree(IntPtr hMem);

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
        finally
        {
            GlobalUnlock(data);
        }
        return true;
    }
}