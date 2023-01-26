namespace IFoxCAD.Cad;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using static IFoxCAD.Basal.WindowsAPI;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

// DWORD == uint
// WORD == ushort
// LONG == int

/*
 * Console.WriteLine(Marshal.SizeOf(typeof(PlaceableMetaHeader)));
 * Console.WriteLine(Marshal.SizeOf(typeof(WindowsMetaHeader)));
 * Console.WriteLine(Marshal.SizeOf(typeof(StandardMetaRecord)));
 */


// https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-metafilepict
// http://www.cppblog.com/zwp/archive/2012/02/25/60225.html
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct MetaFilePict
{
    public MappingModes mm;
    public int xExt;
    public int yExt;
    public IntPtr hMF; //内存图元文件的句柄
}

public enum MappingModes : int
{
    MM_TEXT = 1,
    MM_LOMETRIC = 2,
    MM_HIMETRIC = 3,
    MM_LOENGLISH = 4,//逻辑坐标的单位为0.01英寸
    MM_HIENGLISH = 5,
    MM_TWIPS = 6,
    MM_ISOTROPIC = 7,
    MM_ANISOTROPIC = 8,

    //Minimum and Maximum Mapping Mode values
    MM_MIN = MM_TEXT,
    MM_MAX = MM_ANISOTROPIC,
    MM_MAX_FIXEDSCALE = MM_TWIPS,
}


//WMF 文件格式：
//https://blog.51cto.com/chenyanxi/803247
//文件缩放信息：22字节
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct PlaceableMetaHeader
{
    public uint Key;             /* 固定大小以相反顺序出现 9AC6CDD7h */
    public ushort Handle;        /* Metafile HANDLE number (always 0) */

    public short Left;           /* Left coordinate in metafile units */
    public short Top;            /* Top coordinate in metafile units */
    public short Right;          /* Right coordinate in metafile units */
    public short Bottom;         /* Bottom coordinate in metafile units */

    public ushort Inch;          /* Number of metafile units per inch */
    public uint Reserved;        /* Reserved (always 0) */
    public ushort Checksum;      /* Checksum value for previous 10 WORDs */

    // 微软的wmf文件分为两种一种是标准的图元文件,
    // 一种是活动式图元文件,活动式图元文件 与 标准的图元文件 的主要区别是,
    // 活动式图元文件包含了图像的原始大小和缩放信息.
    /// <summary>
    /// 是活动式图元文件
    /// </summary>
    public bool IsActivity => Key == 0x9AC6CDD7;

    /// <summary>
    /// wmf转为emf<br/>
    /// </summary>
    /// <param name="wmfFile">文件路径</param>
    /// <returns>
    /// 错误: <see cref="IntPtr.Zero"/>;<br/>
    /// 成功: 返回一个增强型图元 emf文件句柄 (位于内存中)
    /// </returns>
    /// <exception cref="IOException"></exception>
    public static IntPtr Wmf2Emf(string wmfFile)
    {
        using FileStream file = new(wmfFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // FileShare才能进c盘
        if (file.Length == 0)
            throw new IOException("文件字节0长:" + file);
        if (file.Length < 5)
            throw new IOException("无法校验文件签名:" + file);

        var fileByte = new byte[file.Length];
        file.Read(fileByte, 0, fileByte.Length);
        file.Close();

        var sWMF = BytesToStruct<PlaceableMetaHeader>(fileByte);
        // 转为emf的地址
        IntPtr hEMF = IntPtr.Zero;

        // 控制输出的时候跟cad一样带有一个矩形框边界,而不是所有图元的包围盒作为边界
        var mpType = new MetaFilePict
        {
            mm = MappingModes.MM_ANISOTROPIC,
            xExt = sWMF.Right - sWMF.Left,
            yExt = sWMF.Bottom - sWMF.Top,
            hMF = IntPtr.Zero
        };

        // byte[] 指针偏移
        int iOffset = 0;
        if (sWMF.IsActivity)
            iOffset = Marshal.SizeOf(typeof(PlaceableMetaHeader));

        unsafe
        {
            // 安全指针方法
            //IntPtr fileIntPtr = Marshal.UnsafeAddrOfPinnedArrayElement(fileByte, iOffset);
            // 不安全指针方法
            fixed (byte* fileIntPtr = &fileByte[iOffset])
                hEMF = EmfTool.SetWinMetaFileBits(
                    (uint)fileByte.Length, new IntPtr(fileIntPtr), IntPtr.Zero, new IntPtr(&mpType));
        }
        return hEMF;
    }
}

public class Emf
{
    public IntPtr EmfHandle;

    /// <summary>
    /// 转换wmf到emf
    /// </summary>
    /// <param name="wmfFile"></param>
    public void Wmf2Emf(string wmfFile)
    {
        EmfHandle = PlaceableMetaHeader.Wmf2Emf(wmfFile);//emf文件句柄
    }

    /// <summary>
    /// 获取emf结构
    /// </summary>
    /// <returns></returns>
    public EnhMetaHeader CreateEnhMetaHeader()
    {
        if (EmfHandle == IntPtr.Zero)
            throw new ArgumentException(nameof(EmfHandle) + "== IntPtr.Zero");
        return EnhMetaHeader.Create(EmfHandle);
    }
}

//紧接文件缩放信息的是 WMFHEAD, 18字节
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct WindowsMetaHeader
{
    public ushort FileType;       /* Type of metafile (0=memory, 1=disk) */
    public ushort HeaderSize;     /* Size of header in WORDS (always 9) */
    public ushort Version;        /* Version of Microsoft Windows used */
    public uint FileSize;         /* Total size of the metafile in WORDs */
    public ushort NumOfObjects;   /* Number of objects in the file */
    public uint MaxRecordSize;    /* The size of largest record in WORDs */
    public ushort NumOfParams;    /* Not Used (always 0) */
}

//紧接 WMFHEAD 的是 WMFRECORD, 14字节
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct StandardMetaRecord
{
    public uint Size;             /* Total size of the record in WORDs */
    public ushort Function;       /* Function number (defined in WINDOWS.H) */
    public ushort[] Parameters;   /* Parameter values passed to function */
}

// 文件结构:头记录(ENHMETAHEADER),各记录(ENHMETARECORD),文件结尾(EMR_EOF)
// https://www.cnblogs.com/5iedu/p/4706327.html
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct EnhMetaHeader
{
    [Description("记录类型")]
    public uint iType;
    [Description("结构大小")]
    public int nSize; //注意这个大小是含描述字符串的长度,即等于sizeof(ENHMETAHEADER)+nDescription*2
    [Description("外接矩形(单位是像素)")]
    public IntRect rclBounds;
    [Description("图片矩形(单位是 0.1 毫米)")]
    public IntRect rclFrame;
    [Description("文件签名")]
    public uint dSignature;
    [Description("文件版本")]
    public uint nVersion;
    [Description("文件尺寸")]
    public uint nBytes;
    [Description("记录数")]
    public uint nRecords;
    [Description("句柄数")]
    public ushort nHandles;
    [Description("保留")]
    public ushort sReserved;
    [Description("说明文本的长度")]
    public uint nDescription;
    [Description("说明文本的偏移量")]
    public uint offDescription;
    [Description("调色板的元素数")]
    public uint nPalEntries;
    [Description("分辨率(像素)")]
    public IntSize szlDevice;
    [Description("分辨率(毫米)")]
    public IntSize szlMillimeters;
    [Description("像素格式的尺寸")]
    public uint cbPixelFormat;
    [Description("像素格式的起始偏移位置")]
    public uint offPixelFormat;
    [Description("在不含OpenGL记录时,该值为FALSE")]
    public uint bOpenGL;
    [Description("参考设备的尺寸(微米)")]
    public IntSize szlMicrometers;

    public override string ToString()
    {
        //var tp = GetType();
        //var sb = new StringBuilder();
        //sb.AppendLine(EnumEx.GetDesc(tp, nameof(iType)) + ":" + iType);

        // 输出json
        // NET472 System.Text.Json
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string jsonString = serializer.Serialize(this);
        return jsonString;
    }

    /// <summary>
    /// 通过wmf创建
    /// </summary>
    /// <param name="wmf"></param>
    /// <returns></returns>
    public static EnhMetaHeader Create(string wmf)
    {
        var emf = PlaceableMetaHeader.Wmf2Emf(wmf);
        if (emf == IntPtr.Zero)
            throw new ArgumentException(nameof(emf));
        return Create(emf);
    }

    /// <summary>
    /// 通过emf指针创建
    /// </summary>
    /// <param name="emf"><see cref="EmfTool.GetEnhMetaFileHeader"/>参数1的结构体首地址<br/>
    /// 也就是<see cref="EmfTool.SetWinMetaFileBits"/>的返回值
    /// </param>
    /// <returns></returns>
    public static EnhMetaHeader Create(IntPtr emf)
    {
        var len = EmfTool.GetEnhMetaFileHeader(emf, 0, IntPtr.Zero);
        if (len == 0)
            throw new ArgumentException(nameof(len));

        IntPtr header = Marshal.AllocHGlobal((int)len);
        EmfTool.GetEnhMetaFileHeader(emf, len, header);//这里是切割获取内部的bytes,存放在header

        var result = (EnhMetaHeader)Marshal.PtrToStructure(header, typeof(EnhMetaHeader));

        Marshal.FreeHGlobal(header);
        return result;
    }
}



public static class EmfTool
{
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="clipTypeData">GetEnhMetaFileBits 参数1的结构体首地址</param>
    /// <param name="file">保存路径</param>
    /// <returns></returns>
    public static void Save(IntPtr clipTypeData, string file)
    {
        // 保存emf文件
        // https://blog.csdn.net/tigertianx/article/details/7098490
        var len = EmfTool.GetEnhMetaFileBits(clipTypeData, 0, null!);
        if (len != 0)
        {
            var bytes = new byte[len];
            _ = EmfTool.GetEnhMetaFileBits(clipTypeData, len, bytes);

            using MemoryStream ms1 = new(bytes);
            using var bm = Image.FromStream(ms1);//此方法emf保存成任何版本都会变成png
            bm.Save(file);
        }
    }

    /// <summary>
    /// 返回对一个增强型图元文件的说明
    /// </summary>
    /// <param name="hemf">目标增强型图元文件的句柄</param>
    /// <param name="cchBuffer">lpszDescription缓冲区的长度</param>
    /// <param name="lpDescription">指定一个预先初始化好的字串缓冲区,准备随同图元文件说明载入;
    /// 参考 CreateEnhMetaFile 函数,了解增强型图元文件说明字串的具体格式</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    static extern uint GetEnhMetaFileDescription(IntPtr hemf, uint cchBuffer, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpDescription);

    /// <summary>
    /// 获取emf描述
    /// </summary>
    /// <param name="clipTypeData">文件句柄</param>
    /// <returns>描述的内容</returns>
    [System.Diagnostics.DebuggerStepThrough]
    [System.CodeDom.Compiler.GeneratedCode("InteropSignatureToolkit", "0.9 Beta1")]//初始化时指定生成代码的工具的名称和版本
    public static string? GetEnhMetaFileDescriptionEx(IntPtr clipTypeData)
    {
        var len = GetEnhMetaFileDescription(clipTypeData, 0, null!);
        if (len != 0)
        {
            StringBuilder desc = new((int)len);
            GetEnhMetaFileDescription(clipTypeData, (uint)desc.Capacity, desc);
            return desc.ToString();
        }
        return null;
    }

    public enum DeviceCap : int
    {
        /// <summary>
        /// Device driver version
        /// </summary>
        DRIVERVERSION = 0,
        /// <summary>
        /// Device classification
        /// </summary>
        TECHNOLOGY = 2,
        /// <summary>
        /// Horizontal size in millimeters
        /// </summary>
        HORZSIZE = 4,
        /// <summary>
        /// Vertical size in millimeters
        /// </summary>
        VERTSIZE = 6,
        /// <summary>
        /// Horizontal width in pixels
        /// </summary>
        HORZRES = 8,
        /// <summary>
        /// Vertical height in pixels
        /// </summary>
        VERTRES = 10,
        /// <summary>
        /// Number of bits per pixel
        /// </summary>
        BITSPIXEL = 12,
        /// <summary>
        /// Number of planes
        /// </summary>
        PLANES = 14,
        /// <summary>
        /// Number of brushes the device has
        /// </summary>
        NUMBRUSHES = 16,
        /// <summary>
        /// Number of pens the device has
        /// </summary>
        NUMPENS = 18,
        /// <summary>
        /// Number of markers the device has
        /// </summary>
        NUMMARKERS = 20,
        /// <summary>
        /// Number of fonts the device has
        /// </summary>
        NUMFONTS = 22,
        /// <summary>
        /// Number of colors the device supports
        /// </summary>
        NUMCOLORS = 24,
        /// <summary>
        /// Size required for device descriptor
        /// </summary>
        PDEVICESIZE = 26,
        /// <summary>
        /// Curve capabilities
        /// </summary>
        CURVECAPS = 28,
        /// <summary>
        /// Line capabilities
        /// </summary>
        LINECAPS = 30,
        /// <summary>
        /// Polygonal capabilities
        /// </summary>
        POLYGONALCAPS = 32,
        /// <summary>
        /// Text capabilities
        /// </summary>
        TEXTCAPS = 34,
        /// <summary>
        /// Clipping capabilities
        /// </summary>
        CLIPCAPS = 36,
        /// <summary>
        /// Bitblt capabilities
        /// </summary>
        RASTERCAPS = 38,
        /// <summary>
        /// Length of the X leg
        /// </summary>
        ASPECTX = 40,
        /// <summary>
        /// Length of the Y leg
        /// </summary>
        ASPECTY = 42,
        /// <summary>
        /// Length of the hypotenuse
        /// </summary>
        ASPECTXY = 44,
        /// <summary>
        /// Shading and Blending caps
        /// </summary>
        SHADEBLENDCAPS = 45,

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        LOGPIXELSX = 88,
        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        LOGPIXELSY = 90,

        /// <summary>
        /// Number of entries in physical palette
        /// </summary>
        SIZEPALETTE = 104,
        /// <summary>
        /// Number of reserved entries in palette
        /// </summary>
        NUMRESERVED = 106,
        /// <summary>
        /// Actual color resolution
        /// </summary>
        COLORRES = 108,

        // Printing related DeviceCaps. These replace the appropriate Escapes
        /// <summary>
        /// Physical Width in device units
        /// </summary>
        PHYSICALWIDTH = 110,
        /// <summary>
        /// Physical Height in device units
        /// </summary>
        PHYSICALHEIGHT = 111,
        /// <summary>
        /// Physical Printable Area x margin
        /// </summary>
        PHYSICALOFFSETX = 112,
        /// <summary>
        /// Physical Printable Area y margin
        /// </summary>
        PHYSICALOFFSETY = 113,
        /// <summary>
        /// Scaling factor x
        /// </summary>
        SCALINGFACTORX = 114,
        /// <summary>
        /// Scaling factor y
        /// </summary>
        SCALINGFACTORY = 115,

        /// <summary>
        /// Current vertical refresh rate of the display device (for displays only) in Hz
        /// </summary>
        VREFRESH = 116,
        /// <summary>
        /// Vertical height of entire desktop in pixels
        /// </summary>
        DESKTOPVERTRES = 117,
        /// <summary>
        /// Horizontal width of entire desktop in pixels
        /// </summary>
        DESKTOPHORZRES = 118,
        /// <summary>
        /// Preferred blt alignment
        /// </summary>
        BLTALIGNMENT = 119
    }

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hDC, DeviceCap nIndex);

    [DllImport("gdi32.dll")]
    static extern int SetMapMode(IntPtr hDC, MappingModes fnMapMode);

    [DllImport("gdi32.dll")]
    static extern bool SetViewportOrgEx(IntPtr hDC, int x, int y, Point[] prevPoint);

    [DllImport("gdi32.dll")]
    static extern bool SetWindowOrgEx(IntPtr hDC, int x, int y, Point[] prevPoint);

    [DllImport("gdi32.dll")]
    static extern bool SetViewportExtEx(IntPtr hDC, int nExtentX, int nExtentY, Size[] prevSize);

    [DllImport("gdi32.dll")]
    static extern bool SetWindowExtEx(IntPtr hDC, int nExtentX, int nExtentY, Size[] prevSize);

    [DllImport("Gdi32.dll")]
    public static extern int CreatePen(int nPenStyle, int nWidth, int nColor);

    [DllImport("Gdi32.dll")]
    public static extern int GetStockObject(int nStockBrush);

    [DllImport("Gdi32.dll")]
    public static extern int SelectObject(IntPtr hDC, int hGdiObject);

    [DllImport("Gdi32.dll")]
    public static extern int DeleteObject(int hBitmap);

    [DllImport("Gdi32.dll")]
    public static extern int MoveToEx(IntPtr hDC, int x, int y, int nPreviousPoint);

    [DllImport("Gdi32.dll")]
    public static extern int LineTo(IntPtr hDC, int x, int y);

    [DllImport("Gdi32.dll")]
    public static extern int Rectangle(IntPtr hDC, int nLeft, int nTop, int nRight, int nBottom);

    [DllImport("Gdi32.dll")]
    public static extern bool DPtoLP(IntPtr hdc, [In, Out] Point[] lpPoints, int nCount);


    /// <summary>
    /// 设置emf描述
    /// </summary>
    /// <param name="hMetaFile">emf文件句柄</param>
    /// <param name="desc">设置描述</param>
    /// <returns>新的emf指针</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetEnhMetaFileDescriptionEx(ref IntPtr hMetaFile, string desc)
    {
        if (hMetaFile == IntPtr.Zero)
            throw new ArgumentNullException(nameof(hMetaFile));

        var emh = EnhMetaHeader.Create(hMetaFile);//emf结构 GetEnhMetaFileHeader
        // 创建画布句柄
        IntRect intRect = emh.rclFrame; //new(0, 0, 0, 0);
        var hMetaDC = EmfTool.CreateEnhMetaFile(IntPtr.Zero, null!, ref intRect, desc);
        if (hMetaDC == IntPtr.Zero)
            return;
        //SetMapMode(hMetaDC, MappingModes.MM_ANISOTROPIC); // 默认的就是这个模式
        //SetMapMode(hMetaDC, MappingModes.MM_HIMETRIC);//逻辑单位：0.01mm

        // 设置单位
        //var size = new IntSize(0, 0);
        //EmfTool.SetWindowExtEx(hMetaDC, 0, 0, ref size);
        //EmfTool.SetViewportExtEx(hMetaDC, 0, 0, ref size);
        //EmfTool.GetEnhMetaFilePaletteEntries() 统一调色
        //SetViewportOrgEx(hMetaDC, 0, 0, null!);//将视口原点设在左下角

        // 旧的克隆到新的
        /*
         * 第18章 图元文件_18.2 增强型图元文件(emf）（2）
         * https://blog.51cto.com/u_15082403/3724715
         * 方案2——利用图像的物理尺寸
         * 通过rclFrame字段（是设备单位：0.01mm）显示出来的刻度尺，这样不管在视频显示器
         * 打印机上,显示出来的刻度尺都较为真实
         */
        //目标设备信息
        int cxMms = GetDeviceCaps(hMetaDC, DeviceCap.HORZSIZE);//宽度（单位：mm)
        int cyMms = GetDeviceCaps(hMetaDC, DeviceCap.VERTSIZE);//高度（单位：mm)
        var cxArea = cxMms;
        var cyArea = cyMms;
#if true2
        int cxPix = GetDeviceCaps(hMetaDC, DeviceCap.HORZRES);//宽度（单位：像素）
        int cyPix = GetDeviceCaps(hMetaDC, DeviceCap.VERTRES);//高度（单位：像素）
        int cxImage = emh.rclFrame.Right - emh.rclFrame.Left; //单位:0.01mm
        int cyImage = emh.rclFrame.Bottom - emh.rclFrame.Top;

        // 设置之后图像就没有拉伸了,但是跑偏了
        //将图元文件大小（0.01mm为单位）转换为像素大小
        cxImage = cxImage * cxPix / cxMms / 100;
        cyImage = cyImage * cyPix / cyMms / 100;

        //在指定的矩形区内，水平和垂直居中显示图元文件，同时保证了区域的大小为cxImage和cyImage
        int left = (cxArea - cxImage) / 2;
        int right = (cxArea + cxImage) / 2;
        int top = (cyArea - cyImage) / 2;
        int bottom = (cyArea + cyImage) / 2;
#else
        cxArea = 0;
        cyArea = 0;
        SetMapMode(hMetaDC, MappingModes.MM_HIMETRIC);//逻辑单位：0.01mm
        SetViewportOrgEx(hMetaDC, 0, cyArea, null!);//将视口原点设在左下角
        var pt = new Point(cxArea, 0);

        int cxImage = emh.rclFrame.Right - emh.rclFrame.Left; //单位:0.01mm
        int cyImage = emh.rclFrame.Bottom - emh.rclFrame.Top;

        //在指定的矩形区内，水平和垂直居中显示图元文件，同时保证了区域的大小为cxImage和cyImage
        int left = (pt.X - cxImage) / 2;
        int right = (pt.X + cxImage) / 2;
        int top = (pt.Y + cyImage) / 2;     //注意，这里与前面例子不同
        int bottom = (pt.Y - cyImage) / 2;  //注意，这里与前面例子不同
#endif
        var rect = new IntRect(left, top, right, bottom);

        // 图像拉伸了
        //bool pef = EmfTool.EnumEnhMetaFile(hMetaDC, hMetaFile, IntPtr.Zero, IntPtr.Zero, ref emhValue.rclFrame);// 这个失败
        bool pef = EmfTool.PlayEnhMetaFile(hMetaDC, hMetaFile, ref rect);
        if (!pef)
        {
            DeleteObject(hMetaDC);
            Debugger.Break();
            return;
        }
        // 删除旧的图元文件句柄,返回新的
        var del = EmfTool.DeleteEnhMetaFile(hMetaFile);
        if (del)
            hMetaFile = EmfTool.CloseEnhMetaFile(hMetaDC);
    }

    [DllImport("gdi32.dll", EntryPoint = "GetEnhMetaFileHeader")]
    public static extern uint GetEnhMetaFileHeader(IntPtr hemf, uint cbBuffer, IntPtr /*ENHMETAHEADER*/ lpemh);


    /// <summary>
    /// 将一个标准Windows图元文件转换成增强型图元文件
    /// </summary>
    /// <param name="nSize"><paramref name="lpMeta16Data"/>数组的长度</param>
    /// <param name="lpMeta16Data">
    /// 数组包含了标准图元文件数据.<br/>
    /// 常用 GetMetaFileBitsEx 或 GetWinMetaFileBits 函数获得
    /// </param>
    /// <param name="hdcRef">
    /// 用于决定原始格式及图元文件分辨率的一个参考设备场景;<br/>
    /// 采用显示器分辨率为:<see cref="IntPtr.Zero"/>
    /// </param>
    /// <param name="lpMFP">
    /// 定义一个图元文件附加参考信息的结构<br/>
    /// 为null时,会假定使用当前显示器的 MM_ANISOTROPIC 映射模式
    /// </param>
    /// <returns>
    /// 错误: <see cref="IntPtr.Zero"/>;<br/>
    /// 成功: 返回一个增强型图元emf文件的指针(位于内存中)
    /// </returns>
    [DllImport("gdi32.dll", EntryPoint = "SetWinMetaFileBits")]
    public static extern IntPtr SetWinMetaFileBits(uint nSize, IntPtr lpMeta16Data, IntPtr hdcRef, IntPtr lpMFP);
    /// <summary>
    /// 获取矢量图的byte
    /// </summary>
    /// <param name="hemf"></param>
    /// <param name="cbBuffer"></param>
    /// <param name="lpbBuffer"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    public static extern uint GetEnhMetaFileBits(IntPtr hemf, uint cbBuffer, byte[] lpbBuffer);
    /// <summary>
    /// byte转换矢量图
    /// </summary>
    /// <param name="cbBuffer"></param>
    /// <param name="lpBuffer"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    public static extern IntPtr SetEnhMetaFileBits(uint cbBuffer, byte[] lpBuffer);
    /// <summary>
    /// 删除矢量图
    /// </summary>
    /// <param name="hemf"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    public static extern bool DeleteEnhMetaFile(IntPtr hemf);

    /// <summary>
    /// 创建emf<br/>
    /// https://www.cnblogs.com/5iedu/p/4706327.html
    /// </summary>
    /// <param name="hdcRef">参考设备环境,null以整个屏幕为参考</param>
    /// <param name="szFilename">指定文件名时,创建磁盘文件(.EMF),为null时创建内存图元文件</param>
    /// <param name="lpRect">用于描述图元文件的大小和位置(以0.01mm为单位),可用它精确定义图元文件的物理尺寸</param>
    /// <param name="lpDescription">对图元文件的一段说明.包括创建应用程序的名字、一个NULL字符、对图元文件的一段说明以及两个NULL字符.</param>
    /// <returns>返回画布句柄DC(图元文件句柄得调用 CloseEnhMetaFile 函数)</returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateEnhMetaFile(IntPtr hdcRef, string szFilename, ref IntRect lpRect, string lpDescription);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(IntPtr hdcRef);

    /// <summary>
    /// 在指定的设备场景中画一个增强型图元文件;<br/>
    /// 与标准图元文件不同,完成回放后,增强型图元文件会恢复设备场景以前的状态
    /// </summary>
    /// <param name="hdcRef">画布句柄</param>
    /// <param name="hemf">欲描绘的emf的图元文件句柄</param>
    /// <param name="lpRect">指定显示区域(逻辑单位)GDI会缩放图像以适应该矩形范围</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    public static extern bool PlayEnhMetaFile(IntPtr hdcRef, IntPtr hemf, ref IntRect lpRect);

    // https://blog.csdn.net/hongke457546235/article/details/17404715
    /// <summary>
    /// 逻辑单位设置窗口单位
    /// {只能在 MM_ISOTROPIC 或 MM_ANISOTROPIC 模式下使用下面两个函数}
    /// </summary>
    /// <param name="hdcRef">画布句柄</param>
    /// <param name="nHeight">以逻辑单位表示的新窗口区域的高度</param>
    /// <param name="nWidth">以逻辑单位表示的新窗口区域的宽度</param>
    /// <param name="lpSize">保存函数调用前窗口区域尺寸的SIZE结构地址,NULL则表示忽略调用前的尺寸</param>
    [DllImport("gdi32.dll")]
    public static extern bool SetWindowExtEx(IntPtr hdcRef, int nHeight, int nWidth, ref IntSize lpSize);

    /// <summary>
    /// 视口区域的定义
    /// {只能在 MM_ISOTROPIC 或 MM_ANISOTROPIC 模式下使用下面两个函数}
    /// </summary>
    /// <param name="hdcRef"></param>
    /// <param name="nHeight"></param>
    /// <param name="nWidth"></param>
    /// <param name="lpSize"></param>
    [DllImport("gdi32.dll")]
    public static extern bool SetViewportExtEx(IntPtr hdcRef, int nHeight, int nWidth, ref IntSize lpSize);

    /// <summary>
    /// 旧emf绘制新的hdcEMF中(即回放)
    /// </summary>
    /// <param name="hdcRef">画布句柄</param>
    /// <param name="hmf">图元文件句柄</param>
    /// <param name="proc">回调函数</param>
    /// <param name="procParam">传给回调函数的额外参数</param>
    /// <param name="lpRect">在指定的矩形区内显示图元文件</param>
    /// <returns></returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool EnumEnhMetaFile(IntPtr hdcRef, IntPtr hmf, IntPtr proc, IntPtr procParam, ref IntRect lpRect);

    /// <summary>
    /// 返回图元文件句柄
    /// </summary>
    /// <param name="hdcRef">画布句柄</param>
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CloseEnhMetaFile(IntPtr hdcRef);

    // https://zhidao.baidu.com/question/646739770512964165/answer/1616737219.html?qq-pf-to=pcqq.c2c
    //16位的函数
    [DllImport("gdi32.dll")]
    public static extern IntPtr GetMetaFile(string path);
    //32位的函数
    [DllImport("gdi32.dll")]
    public static extern IntPtr GetEnhMetaFile(string path);



    /// <summary>
    /// EMF保存到文件或者路径
    /// </summary>
    /// <param name="hemfSrc">EMF要复制的增强型图元文件的句柄</param>
    /// <param name="lpszFile">指向目标文件名称的指针,为NULL则将源图元文件复制到内存中</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    public static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string? lpszFile);

    /// <summary>
    /// 矢量图保存
    /// </summary>
    /// <param name="file"></param>
    /// <param name="emfName"></param>
    public static void SaveMetaFile(this Metafile file, string emfName)
    {
        //MetafileHeader metafileHeader = file.GetMetafileHeader(); //这句话可要可不要
        IntPtr h = file.GetHenhmetafile();
        CopyEnhMetaFile(h, emfName);
        DeleteEnhMetaFile(h);
    }

    /// <summary>
    /// 矢量图 转换 byte[]
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static byte[]? ToByteArray(this Image image)
    {
        return ToByteArray((Metafile)image);
    }

    // https://www.pinvoke.net/default.aspx/gdi32.getenhmetafile
    /// <summary>
    /// 矢量图 转换 byte[]
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static byte[]? ToByteArray(this Metafile mf)
    {
        byte[]? arr = null;
        IntPtr handle = mf.GetHenhmetafile();
        if (handle != IntPtr.Zero)
        {
            var size = GetEnhMetaFileBits(handle, 0, null!);
            if (size != 0)
            {
                arr = new byte[size];
                _ = GetEnhMetaFileBits(handle, size, arr);
            }
            DeleteEnhMetaFile(handle);
        }
        return arr;
    }

    /// <summary>
    /// byte[] 转换 矢量图
    /// </summary>
    /// <param name="data"></param>
    /// <param name="task">返回值true删除句柄</param>
    /// <returns></returns>
    public static void ToMetafile(byte[] data, Func<Image, bool> task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        IntPtr hemf = SetEnhMetaFileBits((uint)data.Length, data);
        using var mf = new Metafile(hemf, true);
        if (task.Invoke(mf)) // 对图像进行操作,就不能进行删除句柄
            DeleteEnhMetaFile(hemf);
    }


#if false
    /// <summary>
    /// c#获取wmf方式
    /// </summary>
    /// <param name="wmfFile"></param>
    /// <returns></returns>
    public static IntPtr GetMetafile(string wmfFile)
    {
        using FileStream file = new(wmfFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // FileShare才能进c盘
        var hEMF2 = IntPtr.Zero;

        using Metafile mf = new(file);
        var hEMF = mf.GetHenhmetafile();
        if (hEMF != IntPtr.Zero)
            hEMF2 = CopyEnhMetaFile(hEMF, null);// 这句: 句柄无效..cad的wmf文件不识别
                                                //EmfTool.DeleteEnhMetaFile(hEMF);//托管类应该是封装好的
        return hEMF2;
    }


    /*
    *   // 这是c#写入wmf流程
    *  // c#画的wmf格式是可以的...用这样方式生成的就是可以写剪贴板
    *  WindowsAPI.GetClientRect(doc.Window.Handle, out IntRect rcClient);
    *  int width = rcClient.Right - rcClient.Left;
    *  int height = rcClient.Bottom - rcClient.Top;
    *  EmfTool.Export(wmf, width, height);//cad的命令wmfin:不能导入c#自绘的
    *
    *  //c#方法,但是它读取不了cad的wmf
    *  wmfMeta = EmfTool.GetMetafile(wmf);
    */

    /// <summary>
    /// 导出为 Emf 或 Wmf 文件
    /// <a href="https://blog.csdn.net/zztfj/article/details/5785709">相关链接</a>
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="width">窗口宽度</param>
    /// <param name="height">窗口高度</param>
    /// <returns>是否成功</returns>
    public static bool Export(string filePath, int width, int height)
    {
        try
        {
            using Bitmap bmp = new(width, height);
            using Graphics gs = Graphics.FromImage(bmp);
            using Metafile mf = new(filePath, gs.GetHdc());
            using Graphics g = Graphics.FromImage(mf);
            Draw(g);
            g.Save();
            return true;
        }
        catch { return false; }
    }


    /// <summary>
    /// 绘制图形
    /// </summary>
    /// <param name="g">用于绘图的Graphics对象</param>
    static void Draw(Graphics g)
    {
        HatchBrush hb = new(HatchStyle.LightUpwardDiagonal, Color.Black, Color.White);

        g.FillEllipse(Brushes.Gray, 10f, 10f, 200, 200);
        g.DrawEllipse(new Pen(Color.Black, 1f), 10f, 10f, 200, 200);

        g.FillEllipse(hb, 30f, 95f, 30, 30);
        g.DrawEllipse(new Pen(Color.Black, 1f), 30f, 95f, 30, 30);

        g.FillEllipse(hb, 160f, 95f, 30, 30);
        g.DrawEllipse(new Pen(Color.Black, 1f), 160f, 95f, 30, 30);

        g.FillEllipse(hb, 95f, 30f, 30, 30);
        g.DrawEllipse(new Pen(Color.Black, 1f), 95f, 30f, 30, 30);

        g.FillEllipse(hb, 95f, 160f, 30, 30);
        g.DrawEllipse(new Pen(Color.Black, 1f), 95f, 160f, 30, 30);

        g.FillEllipse(Brushes.Blue, 60f, 60f, 100, 100);
        g.DrawEllipse(new Pen(Color.Black, 1f), 60f, 60f, 100, 100);

        g.FillEllipse(Brushes.BlanchedAlmond, 95f, 95f, 30, 30);
        g.DrawEllipse(new Pen(Color.Black, 1f), 95f, 95f, 30, 30);

        g.DrawRectangle(new Pen(Brushes.Blue, 0.1f), 6, 6, 208, 208);

        g.DrawLine(new Pen(Color.Black, 0.1f), 110f, 110f, 220f, 25f);
        g.DrawString("剖面图", new Font("宋体", 9f), Brushes.Green, 220f, 20f);
    }
#endif
}