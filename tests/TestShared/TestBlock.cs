using IFoxCAD.Cad;
using System.Data.Common;

namespace Test;

public class TestBlock
{
    // 一个命令就把块编辑搞定,减少用户记忆命令
    [CommandMethod(nameof(Test_Refedit), CommandFlags.Redraw | CommandFlags.Session)]
    public void Test_Refedit()
    {
        Env.Printl($"{nameof(Test_Refedit)}-在位编辑块/在位保存块");

        // 全部用lisp发送命令是为了空格还是本命令
        // 打开了块编辑器,就关闭掉,保存提示
        if ((short)Env.GetVar("BlockEditor") == 1)
        {
            Env.Editor.RunLisp("(command \"_.bclose\")");
            return;
        }
        // 0x01 非在位编辑状态: 先选择块参照,然后在位编辑
        // 0x02 在位编辑状态:   关闭并保存
        if (Env.GetVar("RefEditName").ToString() == "")//显示正在编辑的参照名称
            Env.Editor.RunLisp("(command \"_.refedit\")");//直接点选可以有嵌套层次
        else
            Env.Editor.RunLisp("(command \"_.refclose\" \"s\")");
    }

    [CommandMethod(nameof(Test_GetBoundingBoxEx))]
    public void Test_GetBoundingBoxEx()
    {
        using DBTrans tr = new();
        var ents = Env.Editor.SSGet().Value?.GetEntities<Entity>();
        if (ents == null)
            return;
        foreach (var item in ents)
        {
            if (item is null)
                continue;
            var box = item.GetBoundingBoxEx();
            Env.Print("min:" + box.Min + ";max" + box.Max);
        }
    }

    // 前台块定义
    [CommandMethod(nameof(Test_BlockDef))]
    public void Test_BlockDef()
    {
        using DBTrans tr = new();
        // var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        tr.BlockTable.Add("test",
            btr => {
                btr.Origin = new Point3d(0, 0, 0);
            },
            () => // 图元
                new List<Entity> { new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0)) },
            () => // 属性定义
            {
                var id1 = new AttributeDefinition() { Position = new Point3d(0, 0, 0), Tag = "start", Height = 0.2 };
                var id2 = new AttributeDefinition() { Position = new Point3d(1, 1, 0), Tag = "end", Height = 0.2 };
                return new List<AttributeDefinition> { id1, id2 };
            }
        );
        // ObjectId objectId = tr.BlockTable.Add("a");// 新建块
        // objectId.GetObject<BlockTableRecord>().AddEntity();// 测试添加空实体
        tr.BlockTable.Add("test1",
        btr => {
            btr.Origin = new Point3d(0, 0, 0);
        },
        () => {
            var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            var acText = DBTextEx.CreateDBText(Point3d.Origin, "123", 2.5);
            return new List<Entity> { line, acText };
        });
    }

    // 后台块定义
    [CommandMethod(nameof(Test_BlockDefbehind))]
    public void Test_BlockDefbehind()
    {
        using DBTrans tr = new(@"C:\Users\vic\Desktop\test.dwg");
        // var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        tr.BlockTable.Add("test",
            btr => {
                btr.Origin = new Point3d(0, 0, 0);
            },
            () => // 图元
            {
                return new List<Entity> { new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0)) };
            },
            () => // 属性定义
            {
                var id1 = new AttributeDefinition() { Position = new Point3d(0, 0, 0), Tag = "start", Height = 0.2 };
                var id2 = new AttributeDefinition() { Position = new Point3d(1, 1, 0), Tag = "end", Height = 0.2 };
                return new List<AttributeDefinition> { id1, id2 };
            }
        );
        // ObjectId objectId = tr.BlockTable.Add("a");// 新建块
        // objectId.GetObject<BlockTableRecord>().AddEntity();// 测试添加空实体
        tr.BlockTable.Add("test1",
        btr => {
            btr.Origin = new Point3d(0, 0, 0);
        },
        () => {
            var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            var acText = DBTextEx.CreateDBText(Point3d.Origin, "12345", 2.5);
            
            return new List<Entity> { line, acText };
        });
        tr.SaveDwgFile();
    }



    // 修改块定义
    [CommandMethod(nameof(Test_BlockDefChange))]
    public void Test_BlockDefChange()
    {
        using DBTrans tr = new();
        // var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        // tr.BlockTable.Change("test", btr =>
        // {
        //    btr.Origin = new Point3d(5, 5, 0);
        //    btr.AddEntity(new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 2));
        //    btr.GetEntities<BlockReference>()
        //        .ToList()
        //        .ForEach(e => e.Flush()); // 刷新块显示

        // });


        tr.BlockTable.Change("test", btr => {
            foreach (var id in btr)
            {
                var ent = tr.GetObject<Entity>(id);
                using (ent!.ForWrite())
                {
                    switch (ent)
                    {
                        case Dimension dBText:
                            dBText.DimensionText = "234";
                            dBText.RecomputeDimensionBlock(true);
                            break;
                        case Hatch hatch:
                            hatch.ColorIndex = 0;
                            break;
                    }
                }
            }
        });
        tr.Editor?.Regen();
    }

    [CommandMethod(nameof(Test_InsertBlockDef))]
    public void Test_InsertBlockDef()
    {
        using DBTrans tr = new();
        var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        var line2 = new Line(new Point3d(0, 0, 0), new Point3d(-1, 1, 0));
        var att1 = new AttributeDefinition() { Position = new Point3d(10, 10, 0), Tag = "tagTest1", Height = 1, TextString = "valueTest1" };
        var att2 = new AttributeDefinition() { Position = new Point3d(10, 12, 0), Tag = "tagTest2", Height = 1, TextString = "valueTest2" };
        tr.BlockTable.Add("test1", line1, line2, att1, att2);


        var ents = new List<Entity>();
        var line5 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        var line6 = new Line(new Point3d(0, 0, 0), new Point3d(-1, 1, 0));
        ents.Add(line5);
        ents.Add(line6);
        tr.BlockTable.Add("test44", ents);


        var line3 = new Line(new Point3d(5, 5, 0), new Point3d(6, 6, 0));
        var line4 = new Line(new Point3d(5, 5, 0), new Point3d(-6, 6, 0));
        var att3 = new AttributeDefinition() { Position = new Point3d(10, 14, 0), Tag = "tagTest3", Height = 1, TextString = "valueTest3" };
        var att4 = new AttributeDefinition() { Position = new Point3d(10, 16, 0), Tag = "tagTest4", Height = 1, TextString = "valueTest4" };
        tr.BlockTable.Add("test2", new List<Entity> { line3, line4 }, new List<AttributeDefinition> { att3, att4 });
        // tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test1"); // 测试默认
        // tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test2");
        // tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test3"); // 测试插入不存在的块定义
        // tr.CurrentSpace.InsertBlock(new Point3d(0, 0, 0), "test1", new Scale3d(2)); // 测试放大2倍
        // tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test1", new Scale3d(2), Math.PI / 4); // 测试放大2倍,旋转45度

        var def1 = new Dictionary<string, string>
        {
            { "tagTest1", "1" },
            { "tagTest2", "2" }
        };
        tr.CurrentSpace.InsertBlock(new Point3d(0, 0, 0), "test1", atts: def1);
        var def2 = new Dictionary<string, string>
        {
            { "tagTest3", "1" },
            { "tagTest4", "" }
        };
        tr.CurrentSpace.InsertBlock(new Point3d(10, 10, 0), "test2", atts: def2);
        tr.CurrentSpace.InsertBlock(new Point3d(-10, 0, 0), "test44");
    }
    [CommandMethod(nameof(Test_InsertBlockWithDoubleDatabase))]
    public void Test_InsertBlockWithDoubleDatabase()
    {
        using var tr = new DBTrans(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.dwg"));
        using var trans = new DBTrans();

        tr.BlockTable.Add("test456",
            btr => {
                btr.Origin = new(0, 0, 0);
            },
            () => {
                var line = new Line(new(0, 0, 0), new(1, 1, 0));
                var actext = DBTextEx.CreateDBText(Point3d.Origin, "123", 2.5, tr.Database);

                return new List<Entity> { line,actext };

            });
        tr.CurrentSpace.InsertBlock(Point3d.Origin, "test456");
        tr.SaveDwgFile();
    }



    [CommandMethod(nameof(Test_AddAttsDef))]
    public void Test_AddAttsDef()
    {
        using DBTrans tr = new();
        var blockid = Env.Editor.GetEntity("pick block:").ObjectId;
        var btf = tr.GetObject<BlockReference>(blockid);
        if (btf is null)
            return;
        var att1 = new AttributeDefinition() { Position = new Point3d(20, 20, 0), Tag = "addtagTest1", Height = 1, TextString = "valueTest1" };
        var att2 = new AttributeDefinition() { Position = new Point3d(10, 12, 0), Tag = "tagTest2", Height = 1, TextString = "valueTest2" };
        tr.BlockTable.AddAttsToBlocks(btf.BlockTableRecord, new() { att1, att2 });
    }

    [CommandMethod(nameof(Test_BlockNullBug))]
    public void Test_BlockNullBug()
    {
        using DBTrans tr = new();

        var ents = new List<Entity>();
        var line5 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        var line6 = new Line(new Point3d(0, 0, 0), new Point3d(-1, 1, 0));
        ents.Add(line5);
        ents.Add(line6);
        tr.BlockTable.Add("test44", ents);
        tr.CurrentSpace.InsertBlock(new Point3d(0, 0, 0), "test44");
    }

    [CommandMethod(nameof(Test_BlockFile))]
    public void Test_BlockFile()
    {
        using DBTrans tr = new();
        var id = tr.BlockTable.GetBlockFrom(@"C:\Users\vic\Desktop\test.dwg", false);
        tr.CurrentSpace.InsertBlock(Point3d.Origin, id);
    }


    [CommandMethod(nameof(Test_ClipBlock))]
    public void Test_ClipBlock()
    {
        using DBTrans tr = new();
        tr.BlockTable.Add("test1", btr => {
            btr.Origin = new Point3d(0, 0, 0);
            btr.AddEntity(new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0)),
                          new Line(new Point3d(10, 10, 0), new Point3d(10, 0, 0)));
        });
        // tr.BlockTable.Add("hah");
        var id = tr.CurrentSpace.InsertBlock(new Point3d(0, 0, 0), "test1");
        var brf1 = tr.GetObject<BlockReference>(id)!;
        var pts = new List<Point3d> { new Point3d(3, 3, 0), new Point3d(7, 3, 0), new Point3d(7, 7, 0), new Point3d(3, 7, 0) };
        brf1.ClipBlockRef(pts);

        var id1 = tr.CurrentSpace.InsertBlock(new Point3d(20, 20, 0), "test1");
        var brf2 = tr.GetObject<BlockReference>(id);
        brf2?.ClipBlockRef(new Point3d(13, 13, 0), new Point3d(17, 17, 0));
    }

    // 给用户的测试程序，不知道对错
    [CommandMethod(nameof(Test_Block_ej))]
    public void Test_Block_ej()
    {
        using (DBTrans tr = new())
        {
            // Point3d.Origin.AddBellowToModelSpace(100, 100, 5, 3, 30);// 画波纹管

            // Database db2 = new Database(false, true);
            // string fullFileName = @".\MyBlockDwgFile\001.dwg";
            // db2.ReadDwgFile(fullFileName, System.IO.FileShare.Read, true, null);
            // db2.CloseInput(true);
            // string blockName = "test";
            // if (!tr.BlockTable.Has(blockName))
            // {
            //    // tr.Database.Insert(blockName, db2, false);// 插入块
            //    db.Insert(blockName, db2, false);

            // }

            string fullFileName = @"C:\Users\vic\Desktop\001.dwg";
            var blockdef = tr.BlockTable.GetBlockFrom(fullFileName, false);

            tr.Database.Clayer = tr.LayerTable["0"];// 当前图层切换为0图层
            tr.LayerTable.Change(tr.Database.Clayer, ltr => {
                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 2); // ColorMethod.ByAci可以让我们使用AutoCAD ACI颜色索引……这里为2（表示黄色）
            });

            var id = tr.ModelSpace.InsertBlock(Point3d.Origin, blockdef);// 插入块参照
            var brf = tr.GetObject<BlockReference>(id);
            brf?.Draw();
        }

        using DBTrans tr2 = new();
        PromptEntityOptions peo = new("\n请选择一个块");
        peo.SetRejectMessage("\n对象必须是块");
        peo.AddAllowedClass(typeof(BlockReference), true);

        var per = Env.Editor.GetEntity(peo);
        if (per.Status != PromptStatus.OK)
            return;

        var brf2 = tr2.GetObject<BlockReference>(per.ObjectId)!;
        // var BTR = tr.GetObject<BlockTableRecord>(Bref.BlockTableRecord, OpenMode.ForWrite);
        //// 如果知道块名字BTRName
        // BlockTableRecord BTR = tr.GetObject<BlockTableRecord>(tr.BlockTable[blockName], OpenMode.ForWrite);

        var btr = tr2.BlockTable[brf2.Name];

        tr2.BlockTable.Change(btr, ltr => {
            foreach (ObjectId oid in ltr)
            {
                var ent = tr2.GetObject<Entity>(oid);
                if (ent is MText mText)
                {
                    using (ent.ForWrite())
                        switch (mText.Text)
                        {
                            case "$$A":
                            mText.Contents = "hahaha";
                            break;
                            case "$$B":
                            break;
                            default:
                            break;
                        }
                }
                else if (ent is DBText dBText)
                {
                    using (ent.ForWrite())
                        dBText.TextString = "haha";
                }
                else if (ent is Dimension dimension)
                {
                    using (ent.ForWrite())
                        switch (dimension.DimensionText)
                        {
                            case "$$pipeLen":
                            dimension.DimensionText = "350";
                            dimension.RecomputeDimensionBlock(true);
                            break;
                            default:
                            break;
                        }
                }
            }
        });
        tr2.Editor?.Regen();
    }

    [CommandMethod(nameof(Test_QuickBlockDef2))]
    public void Test_QuickBlockDef2()
    {
        // Database db = HostApplicationServices.WorkingDatabase;
        Editor ed = Acap.DocumentManager.MdiActiveDocument.Editor;
        PromptSelectionOptions promptOpt = new()
        {
            MessageForAdding = "请选择需要快速制作块的对象"
        };
        string blockName = "W_BLOCK_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        // var rss = ed.GetSelection(promptOpt);
        var rss = Env.Editor.GetSelection(promptOpt);
        using DBTrans tr = new();
        if (rss.Status == PromptStatus.OK)
        {
            // SelectionSet ss = rss.Value;
            // ObjectId[] ids = ss.GetObjectIds();
            // var ents = new List<KeyValuePair<Entity, long>>();
            // var extents = new Extents3d();
            // foreach (var id in ids)
            // {
            //    Entity ent = tr.GetObject<Entity>(id);
            //    if (ent is null)
            //        continue;
            //    try
            //    {
            //        extents.AddExtents(ent.GeometricExtents);
            //        var order = id.Handle.Value;
            //        var newEnt = ent.Clone() as Entity;
            //        ents.Add(new KeyValuePair<Entity, long>(newEnt, order));
            //        ent.UpgradeOpen();
            //        ent.Erase();
            //        ent.DowngradeOpen();
            //    }
            //    catch (System.Exception exc)
            //    {
            //        ed.WriteMessage(exc.Message);
            //    }
            // }
            // ents = ents.OrderBy(x => x.Value).ToList();
            var ents = rss.Value.GetEntities<Entity>();
            // ents.ForEach(ent => extents.AddExtents(ent.GeometricExtents));
            var extents = ents!.GetExtents();
            Point3d pt = extents.MinPoint;
            Matrix3d matrix = Matrix3d.Displacement(Point3d.Origin - pt);
            // var newEnts = new List<Entity>();
            // foreach (var ent in ents)
            // {
            //    var newEnt = ent.Key;
            //    newEnt.TransformBy(matrix);
            //    newEnts.Add(newEnt);
            // }
            // if (tr.BlockTable.Has(blockName))
            // {
            //    Acap.ShowAlertDialog(Environment.NewLine + "块名重复，程序退出！");
            //    return;
            // }
            ents.ForEach(ent =>
                ent?.ForWrite(e => e?.TransformBy(matrix)));
            // var newents = ents.Select(ent =>
            // {
            //    var maping = new IdMapping();
            //    return ent.DeepClone(ent, maping, true) as Entity;
            // });
            var newents = ents.Select(ent => ent?.Clone() as Entity);

            // ents.ForEach(ent => ent.ForWrite(e => e.Erase(true))); // 删除实体就会卡死，比较奇怪，估计是Clone()函数的问题
            // 经过测试不是删除的问题
            var btrId = tr.BlockTable.Add(blockName, newents!);
            ents.ForEach(ent => ent?.ForWrite(e => e?.Erase(true)));
            var bId = tr.CurrentSpace.InsertBlock(pt, blockName);
            // tr.GetObject<Entity>(bId, OpenMode.ForWrite).Move(Point3d.Origin, Point3d.Origin);
            // var ed = Acap.DocumentManager.MdiActiveDocument.Editor;
            // ed.Regen();
            // tr.Editor.Regen();
            // 调用regen() 卡死
        }
        // tr.Editor.Regen();
        // ed.Regen();
        // using (var tr = new DBTrans())
        // {
        //    tr.CurrentSpace.InsertBlock(Point3d.Origin, blockName);
        //    tr.Editor.Regen();
        // }
    }

    [CommandMethod(nameof(Test_QuickBlockDef1))]
    public void Test_QuickBlockDef1()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        PromptSelectionOptions promptOpt = new()
        {
            MessageForAdding = "请选择需要快速制作块的对象"
        };
        string blockName = "W_BLOCK_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var rss = Env.Editor.GetSelection(promptOpt);
        if (rss.Status != PromptStatus.OK)
            return;

        using var tr = db.TransactionManager.StartTransaction();
        var ids = rss.Value.GetObjectIds();
        var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr = new BlockTableRecord
        {
            Name = blockName
        };
        foreach (var item in ids)
        {
            var ent = tr.GetObject(item, OpenMode.ForRead) as Entity;
            btr.AppendEntity(ent!.Clone() as Entity);
            ent.ForWrite(e => e.Erase(true));
        }
        bt!.UpgradeOpen();
        bt.Add(btr);
        tr.AddNewlyCreatedDBObject(btr, true);
        bt.DowngradeOpen();
        //    tr.Commit();
        // }

        // using (var tr1 = db.TransactionManager.StartTransaction())
        // {
        // var bt = tr1.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr1 = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
        var brf = new BlockReference(Point3d.Origin, bt[blockName])
        {
            ScaleFactors = default
        };
        btr1!.AppendEntity(brf);
        tr.AddNewlyCreatedDBObject(brf, true);
        btr1.DowngradeOpen();
        ed.Regen();
        tr.Commit();
        // ed.Regen();
    }

    void Wblock()
    {
        var curdb = HostApplicationServices.WorkingDatabase;
        PromptSelectionOptions opts = new()
        {
            MessageForAdding = "选择对象"
        };
        var ss = Env.Editor.GetSelection(opts).Value;
        using ObjectIdCollection ids = new(ss.GetObjectIds());
        var db = curdb.Wblock(ids, Point3d.Origin);
        db.SaveAs(@"c:\test.dwg", DwgVersion.Current);
    }
    [CommandMethod(nameof(ChangeDynameicBlock))]
    public void ChangeDynameicBlock()
    {
        var pro = new Dictionary<string, object>
        {
            { "haha", 1 }
        };
        var blockid = Env.Editor.GetEntity("选择个块").ObjectId;
        using DBTrans tr = new();
        var brf = tr.GetObject<BlockReference>(blockid)!;
        brf.ChangeBlockProperty(pro);
        // 这是第一个函数的用法
    }
    [CommandMethod(nameof(ChangeBlockProperty))]
    public void ChangeBlockProperty()
    {
        Dictionary<string, string>? pro = new Dictionary<string, string>
        {
            { "haha", "1" }
        };
        var blockid = Env.Editor.GetEntity("选择个块").ObjectId;
        using DBTrans tr = new();
        var brf = tr.GetObject<BlockReference>(blockid)!;
        brf.ChangeBlockProperty(pro);
        // 这是第一个函数的用法
    }

    [CommandMethod(nameof(Test_Back))]
    public void Test_Back()
    {
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string dwg = dir + "\\test.dwg";
        if (!File.Exists(dwg))
        {
            System.Windows.Forms.MessageBox.Show(dwg, "你还没有创建此文件");
            return;
        }

        using DBTrans tr = new(dwg);
        tr.ModelSpace.GetEntities<Circle>().ForEach(ent => {
            ent.ForWrite(e => e.ColorIndex = 3);
        });
        tr.Database.SaveAs(dwg, DwgVersion.Current);

        tr.ModelSpace.GetEntities<Circle>().ForEach(ent => {
            ent.ForWrite(e => e.ColorIndex = 4);
        });
        tr.Database.SaveAs(dwg, DwgVersion.Current);
    }
}

public class BlockImportClass
{
    [CommandMethod(nameof(Test_Cbll))]
    public void Test_Cbll()
    {
        string filename = @"C:\Users\vic\Desktop\Drawing1.dwg";
        using DBTrans tr = new();
        using DBTrans tr1 = new(filename);
        // tr.BlockTable.GetBlockFrom(filename, true);
        string blkdefname = SymbolUtilityServices.RepairSymbolName(SymbolUtilityServices.GetSymbolNameFromPathName(filename, "dwg"), false);
        tr.Database.Insert(blkdefname, tr1.Database, false); // 插入了块定义，未插入块参照
    }


    [CommandMethod(nameof(Test_CombineBlocksIntoLibrary))]
    public void Test_CombineBlocksIntoLibrary()
    {
        Document doc = Acap.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;
        Database destDb = doc.Database;

        PromptResult pr = ed.GetString("\nEnter the folder of source drawings: ");

        if (pr.Status != PromptStatus.OK)
            return;
        string pathName = pr.StringResult;
        if (!Directory.Exists(pathName))
        {
            ed.WriteMessage("\nDirectory does not exist: {0}", pathName);
            return;
        }
        string[] fileNames = Directory.GetFiles(pathName, "*.dwg");
        int imported = 0, failed = 0;
        foreach (string fileName in fileNames)
        {
            if (fileName.EndsWith(".dwg",
                StringComparison.InvariantCultureIgnoreCase))
            {
                // Catch exceptions at the file level to allow skipping
                try
                {
                    // Suggestion from Thorsten Meinecke...

                    string destName =
                      SymbolUtilityServices.GetSymbolNameFromPathName(
                        fileName, "dwg"
                      );

                    // And from Dan Glassman...

                    destName =
                      SymbolUtilityServices.RepairSymbolName(
                        destName, false
                      );

                    // Create a source database to load the DWG into

                    using Database db = new(false, true);
                    // Read the DWG into our side database

                    db.ReadDwgFile(fileName, FileShare.Read, true, "");
                    bool isAnno = db.AnnotativeDwg;

                    // Insert it into the destination database as
                    // a named block definition

                    ObjectId btrId = destDb.Insert(
                      destName,
                      db,
                      false
                    );

                    if (isAnno)
                    {
                        // If an annotative block, open the resultant BTR
                        // and set its annotative definition status

                        Transaction tr =
                          destDb.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            BlockTableRecord btr =
                              (BlockTableRecord)tr.GetObject(
                                btrId,
                                OpenMode.ForWrite
                              );
                            btr.Annotative = AnnotativeStates.True;
                            tr.Commit();
                        }
                    }

                    // Print message and increment imported block counter

                    ed.WriteMessage("\nImported from \"{0}\".", fileName);
                    imported++;
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage(
                      "\nProblem importing \"{0}\": {1} - file skipped.",
                      fileName, ex.Message
                    );
                    failed++;
                }
            }
        }

        ed.WriteMessage(
          "\nImported block definitions from {0} files{1} in " +
          "\"{2}\" into the current drawing.",
          imported,
          failed > 0 ? " (" + failed + " failed)" : "",
          pathName
        );
    }

}