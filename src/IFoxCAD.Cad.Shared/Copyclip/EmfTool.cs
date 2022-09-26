namespace IFoxCAD.Cad;

using System;
using System.Drawing.Imaging;

public class EmfTool
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
    /// Copy EMF to file
    /// </summary>
    /// <param name="hemfSrc">Handle to EMF</param>
    /// <param name="lpszFile">File</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string lpszFile);

    /// <summary>
    /// 矢量图保存
    /// </summary>
    /// <param name="file"></param>
    /// <param name="emfName"></param>
    public static void SaveMetaFile(Metafile file, string emfName)
    {
        //MetafileHeader metafileHeader = file.GetMetafileHeader(); //这句话可要可不要
        IntPtr iptrMetafileHandle = file.GetHenhmetafile();
        CopyEnhMetaFile(iptrMetafileHandle, emfName);
        DeleteEnhMetaFile(iptrMetafileHandle);
    }

    /// <summary>
    /// 矢量图 转换 byte[]
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public byte[]? ConvertMetaFileToByteArray(System.Drawing.Image image)
    {
        return ToByteArray((Metafile)image);
    }

    /// <summary>
    /// 矢量图 转换 byte[]
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public byte[]? ToByteArray(Metafile mf)
    {
        byte[]? dataArray = null;
        IntPtr handle = mf.GetHenhmetafile();

        uint bufferSize = GetEnhMetaFileBits(handle, 0, null!);
        if (handle != IntPtr.Zero)
        {
            dataArray = new byte[bufferSize];
            GetEnhMetaFileBits(handle, bufferSize, dataArray);
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
}