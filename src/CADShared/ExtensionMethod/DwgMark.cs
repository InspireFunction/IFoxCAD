namespace IFoxCAD.Cad;

/// <summary>
/// 为dwg文件添加标记
/// </summary>
public static class DwgMark
{
    private const int kFreeSpace = 0x15;
    private const int kFreeSpaceDefault = 0x00;

    /// <summary>
    /// 为dwg文件添加标识
    /// </summary>
    /// <param name="file">DWG文件</param>
    /// <param name="bite">ASCII标识字节0X00~0X7F</param>
    /// <exception cref="ArgumentException">非dwg文件会报错，给定bite超界限也报错</exception>
    public static void AddMark(FileInfo file, int bite)
    {
        if (file.Extension.ToLower() != ".dwg")
        {
            throw new ArgumentException("必须是dwg文件！");
        }

        if (bite > 0x7F || bite < 0x00)
        {
            throw new ArgumentException("字符必须在ASCII范围！");
        }

        using var bw = new BinaryWriter(File.Open(file.FullName, FileMode.Open));
        bw.BaseStream.Position = kFreeSpace; //文件头第21个字节
        bw.Write(bite); //写入数据，仅一个字节
    }

    /// <summary>
    /// 将dwg文件标记恢复为默认值
    /// </summary>
    /// <param name="file">文件</param>
    /// <exception cref="ArgumentException">非dwg文件会报错</exception>
    public static void RemoveMark(FileInfo file)
    {
        if (file.Extension.ToLower() != ".dwg")
        {
            throw new ArgumentException("必须是dwg文件！");
        }

        using var bw = new BinaryWriter(File.Open(file.FullName, FileMode.Open));
        bw.BaseStream.Position = kFreeSpace; //文件头第21个字节
        bw.Write(kFreeSpaceDefault); //写入数据，仅一个字节
    }

    /// <summary>
    /// 获取设置的dwg文件标记
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">非dwg文件会报错</exception>
    public static int GetMark(FileInfo file)
    {
        if (file.Extension.ToLower() != ".dwg")
        {
            throw new ArgumentException("必须是dwg文件！");
        }

        using var fs = File.OpenRead(file.FullName);
        fs.Seek(kFreeSpace, SeekOrigin.Begin);
        var mark = new byte[1];
        _ = fs.Read(mark, 0, mark.Length);
        return mark[0];
    }
}