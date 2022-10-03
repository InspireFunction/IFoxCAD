#define Marshal

namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

public class ClipboardEnv
{
    // 此句将导致剪贴板的key隔离,从而导致cad版本隔离
    // public static string CadVer = $"AutoCAD.r{Acap.Version.Major}"
    // 将r17写死,代表每个cad版本都去找它,实现不隔离cad版本
    public static string CadVer = "AutoCAD.r17";
    public static string CadCurrentVer = $"AutoCAD.r{Acap.Version.Major}";
}

/// <summary>
/// ARX剪贴板结构
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode/*此参数将导致260*2*/)]
public struct TagClipboardInfo : IEquatable<TagClipboardInfo>
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
    public IntPtr mpView;                                   // 用于验证这个对象是在这个视图中创建的 (HWND*)
    public int dwThreadId;                                  // AutoCAD thread 创建数据对象
    public int nLen;                                        // 下一段的长度的数据,如果有的话,从chData
    public int nType;                                       // 类型的数据,如果有(eExpandedClipDataTypes)
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string chData; // 数据的开始,如果有
    #endregion

    #region 属性,可以改动
    public string File => szTempFile;
    public Point3d Point => dptInsert;

#pragma warning disable CA2211 // 非常量字段应当不可见
    public static IntPtr AcadDwgview
        = IntPtr.Zero;
    //= AcedGetAcadDwgview();  // c#需要收集这个函数,我先不写,免得中间版本挂了

    public static int MainWindowThreadId =
        (int)WindowsAPI.GetWindowThreadProcessId(Acap.MainWindow.Handle, out uint processId);
#pragma warning restore CA2211 // 非常量字段应当不可见
    #endregion

    #region 构造
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

        // mpView threadid 可能是用来删除的,用于剪贴板回调清理资源时候判断信息
        mpView = AcadDwgview;
        dwThreadId = MainWindowThreadId;
    }
    #endregion

    #region 方法
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
    #endregion

    #region 测试大小
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
    #endregion

    #region 视口指针
    /*
       [CommandMethod(nameof(testAcedGetAcadDwgview))]
       public void testAcedGetAcadDwgview()
       {
           var dm = Acap.DocumentManager;
           var doc = dm.MdiActiveDocument;
           var ed = doc.Editor;

           var a = AcedGetAcadDwgview().ToString(); //自动执行的时候就存在了
           var b = ed.CurrentViewportObjectId.ToString();
           Debug.WriteLine("a == b:" + a == b);//不对

           var tab = ed.GetCurrentView();
           var c = tab.ObjectId.ToString();
           Debug.WriteLine("a == c:" + a == c);//不对
       }
    */

    /// <summary>
    /// 获取视口指针
    /// </summary>
#if NET35
    [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPAVCView@@XZ")] //acad08
#else
    [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPEAVCView@@XZ")]//acad21
#endif
    static extern IntPtr AcedGetAcadDwgview();
    #endregion

    #region 重载运算符_比较
    public bool Equals(TagClipboardInfo other)
    {
        return
        szTempFile == other.szTempFile &&
        szSourceFile == other.szSourceFile &&
        szSignature == other.szSignature &&
        nFlags == other.nFlags &&
        dptInsert == other.dptInsert &&
        rectGDI == other.rectGDI &&
        mpView == other.mpView &&
        dwThreadId == other.dwThreadId &&
        nLen == other.nLen &&
        nType == other.nType &&
        chData == other.chData;
    }
    public static bool operator !=(TagClipboardInfo a, TagClipboardInfo b)
    {
        return !(a == b);
    }
    public static bool operator ==(TagClipboardInfo a, TagClipboardInfo b)
    {
        return a.Equals(b);
    }
    public override bool Equals(object obj)
    {
        return obj is TagClipboardInfo info && Equals(info);
    }
    public override int GetHashCode()
    {
        return
           szTempFile.GetHashCode() ^
           szSourceFile.GetHashCode() ^
           szSignature.GetHashCode() ^
           nFlags ^
           dptInsert.GetHashCode() ^
           rectGDI.GetHashCode() ^
           mpView.GetHashCode() ^
           dwThreadId ^
           nLen ^
           nType ^
           chData.GetHashCode();
    }
    #endregion
}


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
    public IntRect(int left, int top, int right, int bottom)
    {
        _Left = left;
        _Top = top;
        _Right = right;
        _Bottom = bottom;
    }

    static readonly IntRect _Zero = new(0, 0, 0, 0);
    public static IntRect Zero => _Zero;

    public override string ToString()
    {
        return $"({_Left},{_Top},{_Right},{_Bottom})";
    }

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
    #endregion
}


[StructLayout(LayoutKind.Sequential)]
public struct IntSize
{
    public int Hight;
    public int Width;

    public IntSize(int cx, int cy)
    {
        this.Hight = cx;
        this.Width = cy;
    }

    public override string ToString()
    {
        return $"({Hight},{Width})";
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 8)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(Point3D))]
public struct Point3D : IEquatable<Point3D>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"(X:{_X},Y:{_Y},Z:{_Z})";

    double _X;
    double _Y;
    double _Z;
    public double X => _X;
    public double Y => _Y;
    public double Z => _Z;
    public Point3D(double x, double y, double z)
    {
        _X = x;
        _Y = y;
        _Z = z;
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
        return $"({X},{Y},{Z})";
    }

    #region 重载运算符_比较
    public bool Equals(Point3D other)
    {
        return
        _X == other._X &&
        _Y == other._Y &&
        _Z == other._Z;
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
        return (_X, _Y).GetHashCode() ^ _Z.GetHashCode();
    }
    #endregion
}

/*
 * OLE 剪贴板说明
 * https://blog.csdn.net/chinabinlang/article/details/9815495
 *
 * 感觉如果桌子真的是这样做,那么粘贴链接可能还真没法做成.
 * 1,不知道桌子如何发送wmf文件,是结构体传送,还是文件路径传送.
 * 2,不知道桌子如何接收剪贴板数据,是延迟接收还是一次性写入全局变量或者文件.
 */

public partial class ClipTool
{
    /// <summary>
    /// 侦听剪贴板
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AddClipboardFormatListener(IntPtr hWnd);
    /// <summary>
    /// 移除侦听剪贴板
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RemoveClipboardFormatListener(IntPtr hWnd);
    /// <summary>
    /// 将CWnd加入一个窗口链
    /// 每当剪贴板的内容发生变化时,就会通知这些窗口
    /// </summary>
    /// <param name="hWndNewViewer">句柄</param>
    /// <returns>返回剪贴板观察器链中下一个窗口的句柄</returns>
    [DllImport("User32.dll")]
    public static extern int SetClipboardViewer(IntPtr hWndNewViewer);
    /// <summary>
    /// 从剪贴板链中移出的窗口句柄
    /// </summary>
    /// <param name="hWndRemove">从剪贴板链中移出的窗口句柄</param>
    /// <param name="hWndNewNext">hWndRemove的下一个在剪贴板链中的窗口句柄</param>
    /// <returns>如果成功,非零;否则为0。</returns>
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);


    /// <summary>
    /// 开启剪贴板<br/>
    /// 如果另一个窗口已经打开剪贴板,函数会失败.每次成功调用后都应有<see cref="CloseClipboard"/>调用.
    /// </summary>
    /// <param name="hWndNewOwner"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool OpenClipboard(IntPtr hWndNewOwner);
    /// <summary>
    /// 关闭剪贴板
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool CloseClipboard();
    /// <summary>
    /// 根据数据格式获取剪贴板
    /// </summary>
    /// <param name="lpszFormat">数据格式名称</param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint RegisterClipboardFormat(string lpszFormat);
    /// <summary>
    /// 获取剪贴板
    /// </summary>
    /// <param name="uFormat">通常为<see cref="ClipboardFormat"/>但是cad有自己的位码</param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetClipboardData(uint uFormat);
    /// <summary>
    /// 设置剪贴板
    /// </summary>
    /// <param name="uFormat">通常为<see cref="ClipboardFormat"/>但是cad有自己的位码</param>
    /// <param name="hMem">指定具有指定格式的数据的句柄,<br/>
    /// 该参数为空则为延迟提交:<br/>
    /// 有其他程序对剪切板中的数据进行请求时,该程序才会将指定格式的数据写入到剪切板中.
    /// </param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
    /// <summary>
    /// 清空剪切板并释放剪切板内数据的句柄
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EmptyClipboard();
    /// <summary>
    /// 枚举剪贴板内数据类型
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint EnumClipboardFormats(uint format);


    /*  写入时候注意:
     *  0x01 c#自带的是com剪贴板
     *  最好不要使用,它不能在已经打开的剪贴板中使用,
     *  也无法写入多个cf对象,也就是复制bitmap的时候会覆盖cad图元
     *  Clipboard.SetImage(bitmap);
     *  0x02
     *  剪贴板写入各种类型 https://blog.csdn.net/glt3953/article/details/8808262
     */

    /// <summary>
    /// 打开剪贴板<br/>
    /// 写入之前必须清空,<br/>
    /// 否则将导致发送 WM_DESTROYCLIPBOARD 消息到上一次剪贴板拥有者释放资源<br/>
    /// 所以写入的时候必须一次性写入多个cf<br/>
    /// </summary>
    /// <param name="action">接收返回的栈空间指针用于释放</param>
    /// <param name="isWrite">true写入,false读取</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool OpenClipboardTask(bool isWrite, Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        bool openFlag = false;
        try
        {
            openFlag = OpenClipboard(IntPtr.Zero);
            if (!openFlag)
                return false;
            if (isWrite)
                EmptyClipboard();
            action.Invoke();
        }
        catch (Exception e)
        {
            Debugger.Break();
            Debug.WriteLine(e.Message);
        }
        finally
        {
            if (openFlag)
                CloseClipboard();
        }
        return openFlag;
    }

    /// <summary>
    /// 获取剪贴板
    /// </summary>
    /// <param name="clipKey">剪贴板的索引名</param>
    public static bool GetClipboard<T>(string clipKey, out T? tag)
    {
        bool locked = false;
        T? result = default;

        ClipTool.OpenClipboardTask(false, () => {
            // 读取剪贴板的指定数据
            var clipKeyFormat = RegisterClipboardFormat(clipKey);//ClipboardEnv.CadVer
            var clipTypeData = GetClipboardData(clipKeyFormat);

            // 剪贴板的数据拷贝进去结构体中,会依照数据长度进行拷贝
            locked = WindowsAPI.GlobalLockTask(clipTypeData, ptr => {
                // 非托管内存块->托管对象
                result = (T)Marshal.PtrToStructure(ptr, typeof(T));
            });
        });

        tag = result;
        return locked;
    }
}

#if true2
// 无法备份emf内容
public static class ClipEx
{
    // https://blog.csdn.net/vencon_s/article/details/46345083

    /// <summary>
    /// 剪贴板数据保存目标数据列表
    /// </summary>
    static readonly List<byte[]> _bytes = new();
    /// <summary>
    /// 剪贴板数据类型列表
    /// </summary>
    static readonly List<uint> _formats = new();

    /// <summary>
    /// 遍历剪贴板保存内容
    /// </summary>
    /// <returns>true成功,false失败</returns>
    public static bool SaveClip()
    {
        bool result = ClipTool.OpenClipboardTask(false, free => {
            _bytes.Clear();
            _formats.Clear();

            uint cf = 0;
            while (true)
            {
                cf = ClipTool.EnumClipboardFormats(cf);// 枚举剪贴板所有数据类型
                if (cf == 0)
                    break;

                _formats.Add(cf);
                IntPtr clipTypeData = ClipTool.GetClipboardData(cf);
                var locked = WindowsAPI.GlobalLockTask(clipTypeData, prt => {
                    uint size = WindowsAPI.GlobalSize(clipTypeData);
                    if (size > 0)
                    {
                        var buffer = new byte[size];
                        Marshal.Copy(prt, buffer, 0, buffer.Length);// 将剪贴板数据保存到自定义字节数组
                        _bytes.Add(buffer);
                    }
                });
            }
        });
        if (result)
            result = _formats.Count > 0;
        return result;
    }

    /// <summary>
    /// 恢复保存的数据
    /// </summary>
    /// <returns>true成功,false失败</returns>
    public static bool RestoreClip()
    {
        if (_formats.Count <= 0)
            return false;

        bool result = ClipTool.OpenClipboardTask(true, free => {
            for (int i = 0; i < _formats.Count; i++)
            {
                int size = _bytes[i].Length;
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                if (size > 0)
                {
                    Marshal.Copy(_bytes[i], 0, structPtr, size);
                    ClipTool.SetClipboardData(_formats[i], structPtr);
                }
            }
        });

        if (result)
            result = _formats.Count > 0;
        return result;
    }
}
#endif



/// <summary>
/// 剪贴板的CF,也就是它的key
/// </summary>
public enum ClipboardFormat : uint
{
    /// <summary>
    /// Text format. Each line ends with a carriage return/linefeed (CR-LF) combination. A null character signals
    /// the end of the data. Use this format for ANSI text.
    /// </summary>
    CF_TEXT = 1,

    /// <summary>
    /// A handle to a bitmap (<c>HBITMAP</c>).
    /// </summary>
    CF_BITMAP = 2,

    /// <summary>
    /// Handle to a metafile picture format as defined by the <c>METAFILEPICT</c> structure. When passing a
    /// <c>CF_METAFILEPICT</c> handle by means of DDE, the application responsible for deleting <c>hMem</c> should
    /// also free the metafile referred to by the <c>CF_METAFILEPICT</c> handle.
    /// </summary>
    CF_METAFILEPICT = 3,

    /// <summary>
    /// Microsoft Symbolic Link (SYLK) format.
    /// </summary>
    CF_SYLK = 4,

    /// <summary>
    /// Software Arts' Data Interchange Format.
    /// </summary>
    CF_DIF = 5,

    /// <summary>
    /// Tagged-image file format.
    /// </summary>
    CF_TIFF = 6,

    /// <summary>
    /// Text format containing characters in the OEM character set. Each line ends with a carriage return/linefeed
    /// (CR-LF) combination. A null character signals the end of the data.
    /// </summary>
    CF_OEMTEXT = 7,

    /// <summary>
    /// A memory object containing a <c>BITMAPINFO</c> structure followed by the bitmap bits.
    /// </summary>
    CF_DIB = 8,

    /// <summary>
    /// Handle to a color palette. Whenever an application places data in the clipboard that depends on or assumes
    /// a color palette, it should place the palette on the clipboard as well. If the clipboard contains data in
    /// the <see cref="CF_PALETTE"/> (logical color palette) format, the application should use the
    /// <c>SelectPalette</c> and <c>RealizePalette</c> functions to realize (compare) any other data in the
    /// clipboard against that logical palette. When displaying clipboard data, the clipboard always uses as its
    /// current palette any object on the clipboard that is in the <c>CF_PALETTE</c> format.
    /// </summary>
    CF_PALETTE = 9,

    /// <summary>
    /// Data for the pen extensions to the Microsoft Windows for Pen Computing.
    /// </summary>
    CF_PENDATA = 10,

    /// <summary>
    /// Represents audio data more complex than can be represented in a CF_WAVE standard wave format.
    /// </summary>
    CF_RIFF = 11,

    /// <summary>
    /// Represents audio data in one of the standard wave formats, such as 11 kHz or 22 kHz PCM.
    /// </summary>
    CF_WAVE = 12,

    /// <summary>
    /// Unicode text format. Each line ends with a carriage return/linefeed (CR-LF) combination. A null character
    /// signals the end of the data.
    /// </summary>
    CF_UNICODETEXT = 13,

    /// <summary>
    /// A handle to an enhanced metafile (<c>HENHMETAFILE</c>).
    /// </summary>
    CF_ENHMETAFILE = 14,

    /// <summary>
    /// A handle to type <c>HDROP</c> that identifies a list of files. An application can retrieve information
    /// about the files by passing the handle to the <c>DragQueryFile</c> function.
    /// </summary>
    CF_HDROP = 15,

    /// <summary>
    /// The data is a handle to the locale identifier associated with text in the clipboard. When you close the
    /// clipboard, if it contains <c>CF_TEXT</c> data but no <c>CF_LOCALE</c> data, the system automatically sets
    /// the <c>CF_LOCALE</c> format to the current input language. You can use the <c>CF_LOCALE</c> format to
    /// associate a different locale with the clipboard text.
    /// An application that pastes text from the clipboard can retrieve this format to determine which character
    /// set was used to generate the text.
    /// Note that the clipboard does not support plain text in multiple character sets. To achieve this, use a
    /// formatted text data type such as RTF instead.
    /// The system uses the code page associated with <c>CF_LOCALE</c> to implicitly convert from
    /// <see cref="CF_TEXT"/> to <see cref="CF_UNICODETEXT"/>. Therefore, the correct code page table is used for
    /// the conversion.
    /// </summary>
    CF_LOCALE = 16,

    /// <summary>
    /// A memory object containing a <c>BITMAPV5HEADER</c> structure followed by the bitmap color space
    /// information and the bitmap bits.
    /// </summary>
    CF_DIBV5 = 17,

    /// <summary>
    /// Owner-display format. The clipboard owner must display and update the clipboard viewer window, and receive
    /// the <see cref="ClipboardMessages.WM_ASKCBFORMATNAME"/>, <see cref="ClipboardMessages.WM_HSCROLLCLIPBOARD"/>,
    /// <see cref="ClipboardMessages.WM_PAINTCLIPBOARD"/>, <see cref="ClipboardMessages.WM_SIZECLIPBOARD"/>, and
    /// <see cref="ClipboardMessages.WM_VSCROLLCLIPBOARD"/> messages. The <c>hMem</c> parameter must be <c>null</c>.
    /// </summary>
    CF_OWNERDISPLAY = 0x0080,

    /// <summary>
    /// Text display format associated with a private format. The <c>hMem</c> parameter must be a handle to data
    /// that can be displayed in text format in lieu of the privately formatted data.
    /// </summary>
    CF_DSPTEXT = 0x0081,

    /// <summary>
    /// Bitmap display format associated with a private format. The <c>hMem</c> parameter must be a handle to
    /// data that can be displayed in bitmap format in lieu of the privately formatted data.
    /// </summary>
    CF_DSPBITMAP = 0x0082,

    /// <summary>
    /// Metafile-picture display format associated with a private format. The <c>hMem</c> parameter must be a
    /// handle to data that can be displayed in metafile-picture format in lieu of the privately formatted data.
    /// </summary>
    CF_DSPMETAFILEPICT = 0x0083,

    /// <summary>
    /// Enhanced metafile display format associated with a private format. The <c>hMem</c> parameter must be a
    /// handle to data that can be displayed in enhanced metafile format in lieu of the privately formatted data.
    /// </summary>
    CF_DSPENHMETAFILE = 0x008E,

    /// <summary>
    /// Start of a range of integer values for application-defined GDI object clipboard formats. The end of the
    /// range is <see cref="CF_GDIOBJLAST"/>. Handles associated with clipboard formats in this range are not
    /// automatically deleted using the <c>GlobalFree</c> function when the clipboard is emptied. Also, when using
    /// values in this range, the <c>hMem</c> parameter is not a handle to a GDI object, but is a handle allocated
    /// by the <c>GlobalAlloc</c> function with the <c>GMEM_MOVEABLE</c> flag.
    /// </summary>
    CF_GDIOBJFIRST = 0x0300,

    /// <summary>
    /// See <see cref="CF_GDIOBJFIRST"/>.
    /// </summary>
    CF_GDIOBJLAST = 0x03FF,

    /// <summary>
    /// Start of a range of integer values for private clipboard formats. The range ends with
    /// <see cref="CF_PRIVATELAST"/>. Handles associated with private clipboard formats are not freed
    /// automatically, the clipboard owner must free such handles, typically in response to the
    /// <see cref="ClipboardMessages.WM_DESTROYCLIPBOARD"/> message.
    /// </summary>
    CF_PRIVATEFIRST = 0x0200,

    /// <summary>
    /// See <see cref="CF_PRIVATEFIRST"/>.
    /// </summary>
    CF_PRIVATELAST = 0x02FF,
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