namespace Test;

public class Testid
{
    [CommandMethod("testid")]
    public void TestId()
    {
        using var tr = new DBTrans();
        Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        tr.CurrentSpace.AddEntity(line);
        tr.Dispose();

        var res = Env.Editor.GetEntity("\npick ent:");
        if (res.Status == PromptStatus.OK)
        {
            res.ObjectId.Erase();
        }
        //using (var tr = new DBTrans())
        //{
        //    var res = Env.Editor.GetEntity("\npick ent:");
        //    if(res.Status == PromptStatus.OK)
        //    {
        //        res.ObjectId.Erase();
        //    }

        //}
    }
    
    [CommandMethod("testmycommand")]
    public void TestMyCommand()
    {
        using var dbtrans = new DBTrans(Env.Document, true, false);
        using var trans = Env.Database.TransactionManager.StartTransaction();

        var l1 = new Line(new Point3d(0, 0, 0), new Point3d(100, 100, 0));
        var blkred = trans.GetObject(Env.Database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
        blkred?.AppendEntity(l1);
        trans.AddNewlyCreatedDBObject(l1, true);
        trans.Commit();
        //dbtrans.Dispose();
    }
    [CommandMethod("testtextstyle")]
    public void TestTextStyle()
    {
        using var tr = new DBTrans();
        tr.TextStyleTable.Add("宋体", "宋体.ttf", 0.8);

        tr.TextStyleTable.Add("宋体1", FontTTF.宋体, 0.8);
        tr.TextStyleTable.Add("仿宋体", FontTTF.仿宋, 0.8);
        tr.TextStyleTable.Add("fsgb2312", FontTTF.仿宋GB2312, 0.8);
        tr.TextStyleTable.Add("arial", FontTTF.Arial, 0.8);
        tr.TextStyleTable.Add("romas", FontTTF.Romans, 0.8);



        tr.TextStyleTable.Add("daziti", ttr =>
        {
            ttr.FileName = "ascii.shx";
            ttr.BigFontFileName = "gbcbig.shx";
        });
    }

    [CommandMethod("testtextstylechange")]
    public void TestTextStyleChange()
    {
        using var tr = new DBTrans();
        

        tr.TextStyleTable.AddWithChange("宋体1", "simfang.ttf", height: 5);
        tr.TextStyleTable.AddWithChange("仿宋体", "宋体.ttf");
        tr.TextStyleTable.AddWithChange("fsgb2312", "Romans", "gbcbig");
    }
}
