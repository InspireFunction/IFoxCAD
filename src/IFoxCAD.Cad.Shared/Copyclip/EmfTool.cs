namespace IFoxCAD.Cad;

using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

public static class EmfTool
{
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
    /// <param name="hdcRef">参考设备环境，NULL时表示以屏幕为参考</param>
    /// <param name="szFilename">指定文件名时，创建磁盘文件(.EMF)。为NULL时创建内存图元文件</param>
    /// <param name="lpRect">用于描述图元文件的大小和位置（以0.01mm为单位）,可用它精确定义图元文件的物理尺寸</param>
    /// <param name="lpDescription">对图元文件的一段说明。包括创建应用程序的名字、一个NULL字符、对图元文件的一段说明以及两个NULL字符。</param>
    /// <returns>增强型图元文件DC(注意不是图元文件的句柄,要获得实际的图元文件句柄,得调用 CloseEnhMetaFile 函数)</returns>
    [DllImport("gdi32.dll")]
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
    public static byte[]? ToByteArray(this System.Drawing.Image image)
    {
        return ToByteArray((Metafile)image);
    }

    /// <summary>
    /// 矢量图 转换 byte[]
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static byte[]? ToByteArray(this Metafile mf)
    {
        byte[]? dataArray = null;
        IntPtr handle = mf.GetHenhmetafile();

        uint size = GetEnhMetaFileBits(handle, 0, null!);
        if (handle != IntPtr.Zero)
        {
            dataArray = new byte[size];
            GetEnhMetaFileBits(handle, size, dataArray);
        }
        DeleteEnhMetaFile(handle);
        return dataArray;
    }

    /// <summary>
    /// byte[] 转换 矢量图
    /// </summary>
    /// <param name="data"></param>
    /// <param name="task">返回值true删除句柄</param>
    /// <returns></returns>
    public static void ToMetafile(byte[] data, Func<System.Drawing.Image, bool> task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        IntPtr hemf = SetEnhMetaFileBits((uint)data.Length, data);
        using var mf = new Metafile(hemf, true);
        if (task.Invoke(mf)) // 对图像进行操作,就不能进行删除句柄
            DeleteEnhMetaFile(hemf);
    }



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
}

// DWORD == uint
// WORD == ushort
// LONG == int

/*
 * Console.WriteLine(Marshal.SizeOf(typeof(PlaceableMetaHeader)));
 * Console.WriteLine(Marshal.SizeOf(typeof(WindowsMetaHeader)));
 * Console.WriteLine(Marshal.SizeOf(typeof(StandardMetaRecord)));
 */

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct WmfStr
{
    public PlaceableMetaHeader Placeable;
    public WindowsMetaHeader Wmfhead;
    public StandardMetaRecord Wmfrecord;
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
    public ushort Checksum;      /* Checksum value for previous 10 WORDs */
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


public class WMF_java
{
    /// <summary>
    /// 判断此文件是否为活动式wmf文件
    /// https://blog.51cto.com/chenyanxi/803247
    /// </summary>
    /// <param name="wmf">文件路径</param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public static byte[] OpinionHead(string wmf)
    {
        using FileStream wmfFile = new(wmf, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // FileShare才能进c盘
        if (wmfFile.Length == 0)
            throw new IOException("文件字节0长:" + wmfFile);

        var fileByte = new byte[wmfFile.Length];
        wmfFile.Read(fileByte, 0, fileByte.Length);
        wmfFile.Close();

        // 对头22个字节进行判断
        Debug.WriteLine((fileByte[0] & 0xff)
               + "\t" + (fileByte[1] & 0xff)
               + "\t" + (fileByte[2] & 0xff)
               + "\t" + (fileByte[3] & 0xff));

        // 微软的wmf文件分为两种一种是标准的图元文件,
        // 一种是活动式图元文件,活动式图元文件 与 标准的图元文件 的主要区别是,
        // 活动式图元文件包含了图像的原始大小和缩放信息.
        if (fileByte.Length < 5)
            throw new IOException("无法校验文件签名:" + wmfFile);

        // 对开始的四个字节进行校验 d7 cd c6 9a (WMF活动式图元文件签名)
        if (((fileByte[0] & 0xff) != 215) ||
            ((fileByte[1] & 0xff) != 205) ||
            ((fileByte[2] & 0xff) != 198) ||
            ((fileByte[3] & 0xff) != 154))
        {
            fileByte = null;
            throw new IOException("此文件不为活动式wmf文件:" + wmfFile);
        }

        // 验证文件校验位
        int index = 0;
        for (int i = 0; i < 20; i += 2)
        {
            if (i == 0)
                index = ((fileByte[i + 1] & 0xff) << 8) | (fileByte[i] & 0xff);
            else if (i < 19)
                index ^= ((fileByte[i + 1] & 0xff) << 8) | (fileByte[i] & 0xff);
        }
        if (index != (((fileByte[21] & 0xff) << 8) | (fileByte[20] & 0xff)))
        {
            fileByte = null;
            throw new IOException("此文件已损坏!" + wmfFile);
        }
        return fileByte;
    }
}