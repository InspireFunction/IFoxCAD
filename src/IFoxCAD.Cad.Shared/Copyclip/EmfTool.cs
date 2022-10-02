namespace IFoxCAD.Cad;

using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows;
using System.Security.Policy;
using System.Security.Cryptography;


// DWORD == uint
// WORD == ushort
// LONG == int

/*
 * Console.WriteLine(Marshal.SizeOf(typeof(PlaceableMetaHeader)));
 * Console.WriteLine(Marshal.SizeOf(typeof(WindowsMetaHeader)));
 * Console.WriteLine(Marshal.SizeOf(typeof(StandardMetaRecord)));
 */

//[Serializable]
//[StructLayout(LayoutKind.Sequential, Pack = 2)]
//public struct WmfStr
//{
//    public PlaceableMetaHeader Placeable;
//    public WindowsMetaHeader Wmfhead;
//    public StandardMetaRecord Wmfrecord;
//}

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
    /// <returns>成功emf指针,可以直接写入剪贴板;失败0</returns>
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

        bool bts = WindowsAPI.BytesToStruct(fileByte, out PlaceableMetaHeader sWMF, out int sWMFsize);
        if (!bts)
            throw new IOException("失败:类型转换,路径:" + file);

        var mpType = new MetaFilePict
        {
            mm = MappingModes.MM_ANISOTROPIC,
            xExt = sWMF.Right - sWMF.Left,
            yExt = sWMF.Bottom - sWMF.Top,
            hMF = IntPtr.Zero
        };

        IntPtr hEMF = IntPtr.Zero;
        int iOffset = 0;
        if (sWMF.IsActivity)
            iOffset = sWMFsize;

        // byte[] 指针偏移
        var arr = fileByte.Skip(iOffset).ToArray();
        GCHandle arrHandle = GCHandle.Alloc(arr, GCHandleType.Pinned);

        WindowsAPI.StructToPtr(mpType, mpPtr => {
            hEMF = EmfTool.SetWinMetaFileBits(
                (uint)fileByte.Length, arrHandle.AddrOfPinnedObject(), IntPtr.Zero, mpPtr);
        });

        if (arrHandle.IsAllocated)
            arrHandle.Free();

        return hEMF;
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

public enum MappingModes
{
    MM_TEXT = 1,
    MM_LOMETRIC = 2,
    MM_HIMETRIC = 3,
    MM_LOENGLISH = 4,
    MM_HIENGLISH = 5,
    MM_TWIPS = 6,
    MM_ISOTROPIC = 7,
    MM_ANISOTROPIC = 8,
}


[StructLayout(LayoutKind.Sequential)]
public struct ENHMETAHEADER
{
    public uint iType;
    public int nSize;
    public IntRect rclBounds;
    public IntRect rclFrame;
    public uint dSignature;
    public uint nVersion;
    public uint nBytes;
    public uint nRecords;
    public ushort nHandles;
    public ushort sReserved;
    public uint nDescription;
    public uint offDescription;
    public uint nPalEntries;
    public IntSize szlDevice;
    public IntSize szlMillimeters;
    public uint cbPixelFormat;
    public uint offPixelFormat;
    public uint bOpenGL;
    public IntSize szlMicrometers;
}

public static class EmfTool
{
    // https://zhidao.baidu.com/question/646739770512964165/answer/1616737219.html?qq-pf-to=pcqq.c2c
    //16位的函数
    [DllImport("gdi32.dll")]
    public static extern uint GetMetaFile(StringBuilder path);
    //32位的函数
    [DllImport("gdi32.dll")]
    public static extern uint GetEnhMetaFile(StringBuilder path);


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
    /*
      [DllImport("gdi32.dll", EntryPoint = "SetWinMetaFileBits")]
      public static extern int SetWinMetaFileBits(uint nSize, ref byte lpbBuffer, IntPtr hdcRef, ref METAFILEPICT lpmfp);
    */

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
    /// 创建矢量图
    /// https://www.cnblogs.com/5iedu/p/4706327.html
    /// </summary>
    /// <param name="hdcRef">参考设备环境,NULL时表示以屏幕为参考</param>
    /// <param name="szFilename">指定文件名时,创建磁盘文件(.EMF).为NULL时创建内存图元文件</param>
    /// <param name="lpRect">用于描述图元文件的大小和位置（以0.01mm为单位）,可用它精确定义图元文件的物理尺寸</param>
    /// <param name="lpDescription">对图元文件的一段说明.包括创建应用程序的名字、一个NULL字符、对图元文件的一段说明以及两个NULL字符.</param>
    /// <returns>增强型图元文件DC(注意不是图元文件的句柄,要获得实际的图元文件句柄,得调用 CloseEnhMetaFile 函数)</returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateEnhMetaFile(IntPtr hdcRef, string szFilename, IntRect lpRect, string lpDescription);

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
            uint size = GetEnhMetaFileBits(handle, 0, null!);
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

    /// <summary>
    /// 返回对一个增强型图元文件的说明
    /// </summary>
    /// <param name="hemf">目标增强型图元文件的句柄</param>
    /// <param name="cchBuffer">lpszDescription缓冲区的长度</param>
    /// <param name="lpszDescription">指定一个预先初始化好的字串缓冲区，准备随同图元文件说明载入。
    /// 参考CreateEnhMetaFile函数，了解增强型图元文件说明字串的具体格式</param>
    /// <returns></returns>
    [DllImport("gdi32", EntryPoint = "GetEnhMetaFileDescription")]
    public static extern uint GetEnhMetaFileDescription(
        uint hemf,
        uint cchBuffer,
        [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszDescription
    );


    [DllImport("gdi32", EntryPoint = "GetEnhMetaFileHeader")]
    public static extern uint GetEnhMetaFileHeader(uint hemf, uint cbBuffer, IntPtr /*ENHMETAHEADER*/ lpemh);
}