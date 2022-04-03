namespace test;

public class Test : AutoRegAssem
{
    [CommandMethod("dbtest")]
    public void Dbtest()
    {
        using var tr = new DBTrans();
        tr.Editor.WriteMessage("\n测试 Editor 属性是否工作！");
        tr.Editor.WriteMessage("\n----------开始测试--------------");
        tr.Editor.WriteMessage("\n测试document属性是否工作");
        if (tr.Document == Getdoc())
        {
            tr.Editor.WriteMessage("\ndocument 正常");
        }
        tr.Editor.WriteMessage("\n测试database属性是否工作");
        if (tr.Database == Getdb())
        {
            tr.Editor.WriteMessage("\ndatabase 正常");
        }

        Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        Circle circle = new(new Point3d(0, 0, 0), Vector3d.ZAxis, 2);
        //var lienid = tr.AddEntity(line);
        //var cirid = tr.AddEntity(circle);
        //var linent = tr.GetObject<Line>(lienid);
        //var lineent = tr.GetObject<Circle>(cirid);
        //var linee = tr.GetObject<Line>(cirid); //经测试，类型不匹配，返回null
        //var dd = tr.GetObject<Circle>(lienid);
        //List<DBObject> ds = new() { linee, dd };
        //tr.CurrentSpace.AddEntity(line,tr);
    }

    //add entity test
    [CommandMethod("addent")]
    public void Addent()
    {
        using var tr = new DBTrans();
        Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        tr.CurrentSpace.AddEntity(line);
        Line line1 = new(new Point3d(10, 10, 0), new Point3d(41, 1, 0));
        tr.ModelSpace.AddEntity(line1);
        Line line2 = new(new Point3d(-10, 10, 0), new Point3d(41, 1, 0));
        tr.PaperSpace.AddEntity(line2);
    }

    [CommandMethod("drawarc")]
    public void drawarc()
    {
        using var tr = new DBTrans();
        Arc arc1 = EntityEx.CreateArcSCE(new Point3d(2, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 2, 0));//起点，圆心，终点
        Arc arc2 = EntityEx.CreateArc(new Point3d(4, 0, 0), new Point3d(0, 0, 0), Math.PI / 2);            //起点，圆心，弧度
        Arc arc3 = EntityEx.CreateArc(new Point3d(1, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 1, 0));   //起点，圆上一点，终点
        tr.CurrentSpace.AddEntity(arc1, arc2, arc3);
        tr.CurrentSpace.AddArc(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(2, 0, 0));//起点，圆上一点，终点
    }

    [CommandMethod("drawcircle")]
    public void draCircle()
    {
        using var tr = new DBTrans();
        Circle circle1 = EntityEx.CreateCircle(new Point3d(0, 0, 0), new Point3d(1, 0, 0));                       //起点，终点
        Circle circle2 = EntityEx.CreateCircle(new Point3d(-2, 0, 0), new Point3d(2, 0, 0), new Point3d(0, 2, 0));//三点画圆，成功
        Circle circle3 = EntityEx.CreateCircle(new Point3d(-2, 0, 0), new Point3d(0, 0, 0), new Point3d(2, 0, 0));//起点，圆心，终点，失败
        tr.CurrentSpace.AddEntity(circle1, circle2);
        if (circle3 is not null)
        {
            tr.CurrentSpace.AddEntity(circle3);
        }
        else
        {
            tr.Editor.WriteMessage("三点画圆失败");
        }
        tr.CurrentSpace.AddEntity(circle3);
        tr.CurrentSpace.AddCircle(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(2, 0, 0));//三点画圆，成功
        tr.CurrentSpace.AddCircle(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(2, 2, 0));//起点，圆上一点，终点(共线)
    }

    [CommandMethod("layertest")]
    public void Layertest()
    {
        using var tr = new DBTrans();
        tr.LayerTable.Add("1");
        tr.LayerTable.Add("2", lt =>
        {
            lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 1);
            lt.LineWeight = LineWeight.LineWeight030;

        });
        tr.LayerTable.Remove("3");
        tr.LayerTable.Delete("0");
        tr.LayerTable.Change("4", lt =>
        {
            lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 2);
        });
    }


    //添加图层
    [CommandMethod("layerAdd1")]
    public void Layertest1()
    {
        using var tr = new DBTrans();
        tr.LayerTable.Add("test1", Color.FromColorIndex(ColorMethod.ByColor, 1));
    }

    //添加图层
    [CommandMethod("layerAdd2")]
    public void Layertest2()
    {
        using var tr = new DBTrans();
        tr.LayerTable.Add("test2", 2);
        //tr.LayerTable["3"] = new LayerTableRecord();
    }
    //删除图层
    [CommandMethod("layerdel")]
    public void LayerDel()
    {
        using var tr = new DBTrans();
        Env.Editor.WriteMessage(tr.LayerTable.Delete("0").ToString());        //删除图层 0
        Env.Editor.WriteMessage(tr.LayerTable.Delete("Defpoints").ToString());//删除图层 Defpoints
        Env.Editor.WriteMessage(tr.LayerTable.Delete("1").ToString());        //删除不存在的图层 1
        Env.Editor.WriteMessage(tr.LayerTable.Delete("2").ToString());        //删除有图元的图层 2
        Env.Editor.WriteMessage(tr.LayerTable.Delete("3").ToString());        //删除图层 3

        tr.LayerTable.Remove("2"); //测试是否能强制删除
    }

    //添加直线
    [CommandMethod("linedemo1")]
    public void AddLine1()
    {
        using var tr = new DBTrans();
        //    tr.ModelSpace.AddEnt(line);
        //    tr.ModelSpace.AddEnts(line,circle);

        //    tr.PaperSpace.AddEnt(line);
        //    tr.PaperSpace.AddEnts(line,circle);

        // tr.addent(btr,line);
        // tr.addents(btr,line,circle);


        //    tr.BlockTable.Add(new BlockTableRecord(), line =>
        //    {
        //        line.
        //    });
        Line line1 = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        Line line2 = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        Line line3 = new(new Point3d(1, 1, 0), new Point3d(3, 3, 0));
        Circle circle = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 10);
        tr.CurrentSpace.AddEntity(line1);
        tr.CurrentSpace.AddEntity(line2, line3, circle);
    }

    //增加多段线1
    [CommandMethod("Pldemo1")]
    public void AddPolyline1()
    {
        using var tr = new DBTrans();
        Polyline pl = new Polyline();
        pl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
        pl.AddVertexAt(1, new Point2d(10, 10), 0, 0, 0);
        pl.AddVertexAt(2, new Point2d(20, 20), 0, 0, 0);
        pl.AddVertexAt(3, new Point2d(30, 30), 0, 0, 0);
        pl.AddVertexAt(4, new Point2d(40, 40), 0, 0, 0);
        pl.Closed = true;
        pl.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
        tr.CurrentSpace.AddEntity(pl);
    }

    //增加多段线2
    [CommandMethod("pldemo2")]
    public void Addpl2()
    {
        var pts = new List<(Point3d, double, double, double)>
            {
                (new Point3d(0,0,0),0,0,0),
                (new Point3d(10,0,0),0,0,0),
                (new Point3d(10,10,0),0,0,0),
                (new Point3d(0,10,0),0,0,0),
                (new Point3d(5,5,0),0,0,0)
            };
        using var tr = new DBTrans();
        tr.CurrentSpace.AddPline(pts);
    }






    // 测试扩展数据
    [CommandMethod("addxdata")]
    public void AddXdata()
    {
        using var tr = new DBTrans();
        var appname = "myapp";

        tr.RegAppTable.Add(appname); // add函数会默认的在存在这个名字的时候返回这个名字的regapp的id，不存在就新建
        tr.RegAppTable.Add("myapp2");

        var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0))
        {
            XData = new XDataList()
                {
                    { DxfCode.ExtendedDataRegAppName, appname },  //可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "hahhahah" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, "myapp2" },  //可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "hahhahah" },
                    {1070, 12 }
                }
        };

        tr.CurrentSpace.AddEntity(line);
    }

    [CommandMethod("getxdata")]
    public void GetXdata()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;
        using var tr = new DBTrans();
        tr.RegAppTable.ForEach(id =>
            id.GetObject<RegAppTableRecord>().Name.Print());
        tr.RegAppTable.GetRecords().ForEach(rec => rec.Name.Print());
        tr.RegAppTable.GetRecordNames().ForEach(name => name.Print());
        tr.RegAppTable.ForEach(re => re.Name.Print());

        //var res = ed.GetEntity("\n select the entity:");
        //if (res.Status == PromptStatus.OK)
        //{
        //    using var tr = new DBTrans();
        //    tr.RegAppTable.ForEach(id => id.GetObject<RegAppTableRecord>().Print());
        //    var data = tr.GetObject<Entity>(res.ObjectId).XData;
        //    ed.WriteMessage(data.ToString());
        //}
    }

    [CommandMethod("changexdata")]
    public void Changexdata()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;
        var appname = "myapp";
        var res = ed.GetEntity("\n select the entity:");
        if (res.Status == PromptStatus.OK)
        {
            using var tr = new DBTrans();
            var data = tr.GetObject<Entity>(res.ObjectId);
            data.ChangeXData(appname, DxfCode.ExtendedDataAsciiString, "change");

            ed.WriteMessage(data.XData.ToString());
        }
    }
    [CommandMethod("removexdata")]
    public void Removexdata()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;
        var appname = "myapp";
        var res = ed.GetEntity("\n select the entity:");
        if (res.Status == PromptStatus.OK)
        {
            using var tr = new DBTrans();
            var data = tr.GetObject<Entity>(res.ObjectId);
            data.RemoveXData(appname, DxfCode.ExtendedDataAsciiString);

            ed.WriteMessage(data.XData.ToString());
        }
    }

    [CommandMethod("PrintLayerName")]
    public void PrintLayerName()
    {
        using var tr = new DBTrans();
        foreach (var layerRecord in tr.LayerTable.GetRecords())
        {
            tr.Editor.WriteMessage(layerRecord.Name);
        }

    }




    [CommandMethod("testpt")]
    public void TestPt()
    {
        //var pt = Env.Editor.GetPoint("pick pt:").Value;
        //var pl = Env.Editor.GetEntity("pick pl").ObjectId;
        var tr1 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        using var tr2 = new DBTrans();
        var tr3 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr6 = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr2.Transaction == tr3);
        Env.Print(tr3 == tr6);
        using var tr4 = new DBTrans();
        var tr5 = HostApplicationServices.WorkingDatabase.TransactionManager.TopTransaction;
        var tr7 = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
        Env.Print(tr4.Transaction == tr5);
        Env.Print(tr5 == tr7);
        var trm = HostApplicationServices.WorkingDatabase.TransactionManager;
        //var ptt = tr.GetObject<Polyline>(pl).GetClosestPointTo(pt,false);
        //var pt1 = new Point3d(0, 0.00000000000001, 0);
        //var pt2 = new Point3d(0, 0.00001, 0);
        //Env.Print(Tolerance.Global.EqualPoint);
        //Env.Print(pt1.IsEqualTo(pt2).ToString());
        //Env.Print(pt1.IsEqualTo(pt2,new Tolerance(0.0,1e-6)).ToString());
        //Env.Print((pt1 == pt2).ToString());
        //Env.Print((pt1 != pt2).ToString());



    }
    [CommandMethod("TestBack")]
    public void TestBack()
    {
        using var tr = new DBTrans(@"C:\Users\vic\Desktop\test.dwg");
        tr.ModelSpace.GetEntities<Circle>().ForEach(ent =>
        {
            ent.ForWrite(e => e.ColorIndex = 3);
        });
        tr.Database.SaveAs(@"C:\Users\vic\Desktop\test.dwg", DwgVersion.Current);

        tr.ModelSpace.GetEntities<Circle>().ForEach(ent =>
        {
            ent.ForWrite(e => e.ColorIndex = 4);
        });
        tr.Database.SaveAs(@"C:\Users\vic\Desktop\test.dwg", DwgVersion.Current);

    }

    public Database Getdb()
    {
        var db = Application.DocumentManager.MdiActiveDocument.Database;
        return db;
    }


    public Document Getdoc()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        return doc;
    }

    public override void Initialize()
    {
        //文档管理器将比此接口前创建,因此此句会执行
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nload....");
    }

    public override void Terminate()
    {
        //文档管理器将比此接口前死亡,因此此句不会执行
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\nunload....");
    }
}


public class BlockImportClass
{

    [CommandMethod("CBLL")]
    public void cbll()
    {
        string filename = @"C:\Users\vic\Desktop\Drawing1.dwg";
        using var tr = new DBTrans();
        using var tr1 = new DBTrans(filename);
        //tr.BlockTable.GetBlockFrom(filename, true);
        string blkdefname = SymbolUtilityServices.RepairSymbolName(SymbolUtilityServices.GetSymbolNameFromPathName(filename, "dwg"), false);
        tr.Database.Insert(blkdefname, tr1.Database, false); //插入了块定义，未插入块参照

    }


    [CommandMethod("CBL")]
    public void CombineBlocksIntoLibrary()
    {
        Document doc =
            Application.DocumentManager.MdiActiveDocument;
        Editor ed = doc.Editor;
        Database destDb = doc.Database;

        // Get name of folder from which to load and import blocks

        PromptResult pr =
          ed.GetString("\nEnter the folder of source drawings: ");

        if (pr.Status != PromptStatus.OK)
            return;
        string pathName = pr.StringResult;

        // Check the folder exists

        if (!Directory.Exists(pathName))
        {
            ed.WriteMessage(
              "\nDirectory does not exist: {0}", pathName
            );
            return;
        }

        // Get the names of our DWG files in that folder

        string[] fileNames = Directory.GetFiles(pathName, "*.dwg");

        // A counter for the files we've imported

        int imported = 0, failed = 0;

        // For each file in our list

        foreach (string fileName in fileNames)
        {
            // Double-check we have a DWG file (probably unnecessary)

            if (fileName.EndsWith(
                  ".dwg",
                  StringComparison.InvariantCultureIgnoreCase
                )
            )
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

                    using (Database db = new Database(false, true))
                    {
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

