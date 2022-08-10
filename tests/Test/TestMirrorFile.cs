public class MirrorFile
{
    const string file = "D:/JX.dwg";
    const string fileSave = "D:/JX222.dwg";

    /// <summary>
    /// 测试:后台打开图纸,镜像文字是否存在文字偏移
    /// 答案:不存在
    /// </summary>
    [CommandMethod("CmdTest_MirrorFile")]
    public static void CmdTest_MirrorFile()
    {
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


    [CommandMethod("CmdTest_MirrorFile2")]
    public static void CmdTest_MirrorFile2()
    {
        using var tr = new DBTrans(file, openMode: FileOpenMode.OpenForReadAndReadShare);

        tr.Database.DBTextDeviation(() => {
            tr.BlockTable.Change(tr.ModelSpace.ObjectId, ms => {
                foreach (ObjectId entId in ms)
                {
                    var entity = tr.GetObject<Entity>(entId, OpenMode.ForWrite)!;
                    if (entity is DBText text)
                    {
                        text.Mirror(Point3d.Origin, new Point3d(0, 1, 0));
                        text.IsMirroredInX = true;   //这句将导致文字偏移

                        if (text.VerticalMode == TextVerticalMode.TextBase)
                            text.VerticalMode = TextVerticalMode.TextBottom;

                        text.HorizontalMode = text.HorizontalMode switch
                        {
                            TextHorizontalMode.TextLeft => TextHorizontalMode.TextRight,
                            TextHorizontalMode.TextRight => TextHorizontalMode.TextLeft,
                            _ => text.HorizontalMode
                        };
                        //Point3d pos = text.GeometricExtents.MidMidPoint();
                        //text.Mirror(pos, pos.Polar(text.Rotation+PI/2, 100));
                        text.AdjustAlignment(tr.Database);
                        continue;
                    }
                    entity.Mirror(Point3d.Origin, new Point3d(0, 1, 0));
                }
            });
        });
        tr.Database.SaveAs(fileSave, DwgVersion.AC1021/*AC1021 AutoCAD 2007/2008/2009.*/);
    }
}