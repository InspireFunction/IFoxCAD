public class MirrorFile
{
    /// <summary>
    /// 测试:后台打开图纸,镜像文字是否存在文字偏移
    /// 答案:不存在
    /// </summary>
    [CommandMethod("CmdTest_MirrorFile")]
    public static void CmdTest_MirrorFile()
    {
        const string file = "D:/JX.dwg";
        const string fileSave = "D:/JX222.dwg";

        using var tr = new DBTrans(file, openMode: FileOpenMode.OpenForReadAndReadShare);

        tr.BlockTable.Change(tr.ModelSpace.ObjectId, ms => {
            foreach (ObjectId entId in ms)
            {
                var text = tr.GetObject<DBText>(entId, OpenMode.ForRead)!;
                if (text is null)
                    continue;

                text.UpgradeOpen();
                var pos = text.Position;
                //text.Move(pos, Point3d.Origin);
                //Y轴
                text.Mirror(Point3d.Origin, new Point3d(0, 1, 0));
                //text.Move(Point3d.Origin, pos);
                text.DowngradeOpen();
            }
        });
        tr.Database.SaveAs(fileSave, DwgVersion.AC1021/*AC1021 AutoCAD 2007/2008/2009.*/);
    }
}