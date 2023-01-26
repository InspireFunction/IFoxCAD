namespace IFoxCAD.Cad;

using System;
/// <summary>
/// bitmap工具类
/// </summary>
public class BitmapTool
{
    //  https://blog.csdn.net/shellching/article/details/18405185
    /// Windows不允许程序员直接访问硬件,
    /// 它对屏幕的操作是通过环境设备,也就是DC来完成的
    /// 屏幕上的每一个窗口都对应一个DC,可以把DC想象成一个视频缓冲区,
    /// 对这这个缓冲区的操作,会表现在这个缓冲区对应的屏幕窗口上.
    /// 在窗口的DC之外,可以建立自己的DC,就是说它不对应窗口,
    /// 这个方法就是 CreateCompatibleDC,这个DC就是一个内存缓冲区,
    /// 通过这个DC你可以把和它兼容的窗口DC保存到这个DC中,
    /// 就是说你可以通过它在不同的DC之间拷贝数据.
    /// 例如:你先在这个DC中建立好数据,然后在拷贝到窗口的DC就是完成了这个窗口的刷新

    /// <summary>
    /// 检索指定窗口的工作区的显示设备上下文(DC)的句柄<br/>
    /// 显示设备上下文可以在随后的图形显示界面(GDI)函数中使用,<br/>
    /// 以在窗口的工作区中绘制<br/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="hDC"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    /// <summary>
    /// 创建DC
    /// </summary>
    /// <param name="hdc"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

    /// <summary>
    /// Creates a bitmap compatible with the device that is associated with the specified device context.
    /// </summary>
    /// <param name="hdc">A handle to a device context.</param>
    /// <param name="nWidth">The bitmap width, in pixels.</param>
    /// <param name="nHeight">The bitmap height, in pixels.</param>
    /// <returns>If the function succeeds, the return value is a handle to the compatible bitmap (DDB). If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.</returns>
    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
    public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

    /// <summary>Selects an object into the specified device context (DC). The new object replaces the previous object of the same type.</summary>
    /// <param name="hdc">A handle to the DC.</param>
    /// <param name="hgdiobj">A handle to the object to be selected.</param>
    /// <returns>
    ///   <para>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced. If the selected object is a region and the function succeeds, the return value is one of the following values.</para>
    ///   <para>SIMPLEREGION - Region consists of a single rectangle.</para>
    ///   <para>COMPLEXREGION - Region consists of more than one rectangle.</para>
    ///   <para>NULLREGION - Region is empty.</para>
    ///   <para>If an error occurs and the selected object is not a region, the return value is <c>NULL</c>. Otherwise, it is <c>HGDI_ERROR</c>.</para>
    /// </returns>
    /// <remarks>
    ///   <para>This function returns the previously selected object of the specified type. An application should always replace a new object with the original, default object after it has finished drawing with the new object.</para>
    ///   <para>An application cannot select a single bitmap into more than one DC at a time.</para>
    ///   <para>ICM: If the object being selected is a brush or a pen, color management is performed.</para>
    /// </remarks>
    [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
    public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

    /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
    /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
    /// <returns>
    ///   <para>If the function succeeds, the return value is nonzero.</para>
    ///   <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
    /// </returns>
    /// <remarks>
    ///   <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
    ///   <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
    /// </remarks>
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);

    /// <summary>
    /// 指定的源设备环境区域中的像素进行位块转换,以传送到目标设备环境
    /// </summary>
    /// <param name="hdc">Handle to the destination device context.</param>
    /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
    /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
    /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
    /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
    /// <param name="hdcSrc">Handle to the source device context.</param>
    /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
    /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
    /// <param name="dwRop">A raster-operation code.</param>
    /// <returns>
    ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
    /// </returns>
    [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
    /// <summary>
    /// A raster-operation code enum
    /// </summary>
    public enum TernaryRasterOperations : uint
    {

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        SRCCOPY = 0x00CC0020,
        SRCPAINT = 0x00EE0086,
        SRCAND = 0x008800C6,
        SRCINVERT = 0x00660046,
        SRCERASE = 0x00440328,
        NOTSRCCOPY = 0x00330008,
        NOTSRCERASE = 0x001100A6,
        MERGECOPY = 0x00C000CA,
        MERGEPAINT = 0x00BB0226,
        PATCOPY = 0x00F00021,
        PATPAINT = 0x00FB0A09,
        PATINVERT = 0x005A0049,
        DSTINVERT = 0x00550009,
        BLACKNESS = 0x00000042,
        WHITENESS = 0x00FF0062,
        CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }

    /// <summary>
    /// 截图成为BMP
    /// </summary>
    /// <param name="hWnd">截图的窗口</param>
    /// <param name="action">扔出BMP执行任务</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void CaptureWndImage(IntPtr hWnd, Action<IntPtr> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var hDC = GetDC(hWnd);
        var hMemDC = CreateCompatibleDC(hDC);
        if (hMemDC == IntPtr.Zero)
            return;

        WindowsAPI.GetClientRect(hWnd, out WindowsAPI.IntRect rcClient);
        int width = rcClient.Right - rcClient.Left;
        int height = rcClient.Bottom - rcClient.Top;

        var hBitmap = CreateCompatibleBitmap(hDC, width, height);
        if (hBitmap != IntPtr.Zero)
        {
            SelectObject(hMemDC, hBitmap);
            if (BitBlt(hMemDC, 0, 0, width, height,
                       hDC, 0, 0, TernaryRasterOperations.SRCCOPY))
            {
                action.Invoke(hBitmap);
            }
            DeleteObject(hBitmap);
        }
        DeleteObject(hMemDC);
    }
}