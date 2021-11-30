using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

using IFoxCAD.Cad;
using test.wpf;

namespace test
{
    public class Test
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
            tr.LayerTable.Add("2", lt => {
                lt.Color      = Color.FromColorIndex(ColorMethod.ByColor, 1);
                lt.LineWeight = LineWeight.LineWeight030;

            });
            tr.LayerTable.Remove("3");
            tr.LayerTable.Delete("0");
            tr.LayerTable.Change("4", lt => {
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

        //块定义
        [CommandMethod("blockdef")]
        public void BlockDef()
        {
            using var tr = new DBTrans();
            //var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            tr.BlockTable.Add("test",
                btr => {
                    btr.Origin = new Point3d(0, 0, 0);
                },
                () => //图元
                {
                    return new List<Entity> { new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0)) };
                },
                () => //属性定义
                {
                    var id1 = new AttributeDefinition() { Position = new Point3d(0, 0, 0), Tag = "start", Height = 0.2 };
                    var id2 = new AttributeDefinition() { Position = new Point3d(1, 1, 0), Tag = "end", Height = 0.2 };
                    return new List<AttributeDefinition> { id1, id2 };
                }
            );
            //ObjectId objectId = tr.BlockTable.Add("a");//新建块
            //objectId.GetObject<BlockTableRecord>().AddEntity();//测试添加空实体
        }
        //修改块定义
        [CommandMethod("blockdefchange")]
        public void BlockDefChange()
        {
            using var tr = new DBTrans();
            //var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            tr.BlockTable.Change("test", btr => {
                btr.Origin = new Point3d(5, 5, 0);
                btr.AddEntity(new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 2));
                btr.GetEntities<BlockReference>()
                    .ToList()
                    .ForEach(e => e.Flush()); //刷新块显示

            });
            tr.Editor.Regen();
        }

        [CommandMethod("insertblockdef")]
        public void InsertBlockDef()
        {
            using var tr = new DBTrans();
            var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            var line2 = new Line(new Point3d(0, 0, 0), new Point3d(-1, 1, 0));
            var att1 = new AttributeDefinition() { Position = new Point3d(10, 10, 0), Tag = "tagTest1", Height = 1, TextString = "valueTest1" };
            var att2 = new AttributeDefinition() { Position = new Point3d(10, 12, 0), Tag = "tagTest2", Height = 1, TextString = "valueTest2" };
            tr.BlockTable.Add("test1", line1, line2, att1, att2);
            var line3 = new Line(new Point3d(5, 5, 0), new Point3d(6, 6, 0));
            var line4 = new Line(new Point3d(5, 5, 0), new Point3d(-6, 6, 0));
            var att3 = new AttributeDefinition() { Position = new Point3d(10, 14, 0), Tag = "tagTest3", Height = 1, TextString = "valueTest3" };
            var att4 = new AttributeDefinition() { Position = new Point3d(10, 16, 0), Tag = "tagTest4", Height = 1, TextString = "valueTest4" };
            tr.BlockTable.Add("test2", new List<Entity> { line3, line4 }, new List<AttributeDefinition> { att3, att4 });
            //tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test1"); // 测试默认
            //tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test2");
            //tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test3"); //测试插入不存在的块定义
            //tr.CurrentSpace.InsertBlock(new Point3d(0, 0, 0), "test1", new Scale3d(2)); // 测试放大2倍
            //tr.CurrentSpace.InsertBlock(new Point3d(4, 4, 0), "test1", new Scale3d(2), Math.PI / 4); // 测试放大2倍,旋转45度

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
        }


        [CommandMethod("testclip")]
        public void TestClipBlock()
        {
            using var tr = new DBTrans();
            tr.BlockTable.Add("test1",
                btr => {
                    btr.Origin = new Point3d(0, 0, 0);
                    btr.AddEntity(new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0)),
                        new Line(new Point3d(10, 10, 0), new Point3d(10, 0, 0))
                        );
                }
                );
            //tr.BlockTable.Add("hah");
            var id = tr.CurrentSpace.InsertBlock(new Point3d(0, 0, 0), "test1");
            var bref = tr.GetObject<BlockReference>(id);
            var pts = new List<Point3d> { new Point3d(3, 3, 0), new Point3d(7, 3, 0), new Point3d(7, 7, 0), new Point3d(3, 7, 0) };
            bref.ClipBlockRef(pts);

            var id1 = tr.CurrentSpace.InsertBlock(new Point3d(20, 20, 0), "test1");
            var bref1 = tr.GetObject<BlockReference>(id);

            bref1.ClipBlockRef(new Point3d(13, 13, 0), new Point3d(17, 17, 0));
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

            var res = ed.GetEntity("\n select the entity:");
            if (res.Status == PromptStatus.OK)
            {
                using var tr = new DBTrans();
                var data = tr.GetObject<Entity>(res.ObjectId).XData;
                ed.WriteMessage(data.ToString());
            }
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


        [CommandMethod("testwpf")]
        public void TestWPf()
        {
            var test = new TestView();
            Application.ShowModalWindow(test);
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

        //public override void Initialize()
        //{
        //    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nload....");
        //}

        //public override void Terminate()
        //{
        //    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nunload....");
        //}
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

}
