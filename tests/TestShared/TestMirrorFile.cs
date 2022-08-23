namespace Test;

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

        tr.BlockTable.Change(tr.ModelSpace.ObjectId, modelSpace => {
            foreach (ObjectId entId in modelSpace)
            {
                var dbText = tr.GetObject<DBText>(entId, OpenMode.ForRead)!;
                if (dbText is null)
                    continue;

                dbText.UpgradeOpen();
                var pos = dbText.Position;
                //text.Move(pos, Point3d.Origin);
                //Y轴
                dbText.Mirror(Point3d.Origin, new Point3d(0, 1, 0));
                //text.Move(Point3d.Origin, pos);
                dbText.DowngradeOpen();
            }
        });
        var ver = (DwgVersion)27;/*AC1021 AutoCAD 2007/2008/2009.*/
        tr.Database.SaveAs(fileSave, ver);
    }

    /// <summary>
    /// 测试:后台设置 dbText.IsMirroredInX 属性会令文字偏移
    /// 答案:存在,并提出解决方案
    /// </summary>
    [CommandMethod("CmdTest_MirrorFile2")]
    public static void CmdTest_MirrorFile2()
    {
        using var tr = new DBTrans(file);

        tr.Task(() => {
            var yaxis = new Point3d(0, 1, 0);
            tr.BlockTable.Change(tr.ModelSpace.ObjectId, modelSpace => {
                foreach (ObjectId entId in modelSpace)
                {
                    var entity = tr.GetObject<Entity>(entId, OpenMode.ForWrite)!;
                    if (entity is DBText dbText)
                    {
                        dbText.Mirror(Point3d.Origin, yaxis);
                        dbText.IsMirroredInX = true;   //这句将导致文字偏移

                        //指定文字的垂直对齐方式
                        if (dbText.VerticalMode == TextVerticalMode.TextBase)
                            dbText.VerticalMode = TextVerticalMode.TextBottom;

                        //指定文字的水平对齐方式
                        dbText.HorizontalMode = dbText.HorizontalMode switch
                        {
                            TextHorizontalMode.TextLeft => TextHorizontalMode.TextRight,
                            TextHorizontalMode.TextRight => TextHorizontalMode.TextLeft,
                            _ => dbText.HorizontalMode
                        };
                        dbText.AdjustAlignment(tr.Database);
                    }
                }
            });
        });
        tr.Database.SaveAs(fileSave, (DwgVersion)27 /*AC1021 AutoCAD 2007/2008/2009.*/);
    }
}