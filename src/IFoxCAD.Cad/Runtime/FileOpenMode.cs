#if ac2008 //NET35
namespace Autodesk.AutoCAD.DatabaseServices
{
    [Wrapper("AcDbDatabase::OpenMode")]
    public enum FileOpenMode
    {
        OpenTryForReadShare = 4,
        OpenForReadAndAllShare = 3,
        OpenForReadAndWriteNoShare = 2,
        OpenForReadAndReadShare = 1
    }
}
#endif