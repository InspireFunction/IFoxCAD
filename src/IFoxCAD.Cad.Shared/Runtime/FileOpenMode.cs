#if ac2008 // NET35
namespace Autodesk.AutoCAD.DatabaseServices
{
    [Wrapper("AcDbDatabase::OpenMode")]
    public enum FileOpenMode
    {
        /// <summary>
        /// 只读模式打开
        /// </summary>
        OpenForReadAndReadShare = 1,
        OpenForReadAndWriteNoShare = 2,
        OpenForReadAndAllShare = 3,
        OpenTryForReadShare = 4,
    }

    public static class FileOpenModeHelper
    {
        /*
         *  这个开图方式会致命错误,不清楚怎么用的文件句柄开图
         *  using FileStream fileStream = new(_fileName, FileMode.Open, fileAccess, GetFileShare(fileOpenMode));
         *  Database.ReadDwgFile(fileStream.SafeFileHandle.DangerousGetHandle(), true, password);
         */
        public static FileShare GetFileShare(FileOpenMode fileOpenMode)
        {
            // FileAccess fileAccess = FileAccess.Read;
            FileShare fileShare = FileShare.Read;
            switch (fileOpenMode)
            {
                // 不完美匹配
                case FileOpenMode.OpenTryForReadShare:
                // fileAccess = FileAccess.ReadWrite;
                fileShare = FileShare.ReadWrite;
                break;
                // 完美匹配
                case FileOpenMode.OpenForReadAndAllShare:
                // fileAccess = FileAccess.ReadWrite;
                fileShare = FileShare.ReadWrite;
                break;
                // 完美匹配
                case FileOpenMode.OpenForReadAndWriteNoShare:
                // fileAccess = FileAccess.ReadWrite;
                fileShare = FileShare.None;
                break;
                // 完美匹配
                case FileOpenMode.OpenForReadAndReadShare:
                // fileAccess = FileAccess.Read;
                fileShare = FileShare.Read;
                break;
            }
            return fileShare;
        }
    }
}

#endif