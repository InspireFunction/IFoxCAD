using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal;

using IFoxCAD.Cad;
using Autodesk.AutoCAD.Colors;


namespace test
{
    public class Class1
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
            var lienid = tr.AddEntity(line);
            var cirid = tr.AddEntity(circle);
            var linent = tr.GetObject<Line>(lienid); 
            var lineent = tr.GetObject<Circle>(cirid);
            var linee = tr.GetObject<Line>(cirid); //经测试，类型不匹配，返回null
            var dd = tr.GetObject<Circle>(lienid);
            List<DBObject> ds = new() { linee, dd };
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
            tr.LayerTable.Add("test1", Color.FromColorIndex(ColorMethod.ByColor,1));
        }

        //添加图层
        [CommandMethod("layerAdd2")]
        public void Layertest2()
        {
            using var tr = new DBTrans();
            tr.LayerTable.Add("test2", 2);
            //tr.LayerTable["3"] = new LayerTableRecord();
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
            Line line = new(new Point3d(0,0,0),new Point3d(1,1,0));
            tr.AddEntity(line);
        }

        //增加多段线1
        [CommandMethod("Pldemo1")]
        public void AddPolyline1()
        {
            using var tr = new DBTrans();
            Polyline pl = new Polyline();
            pl.AddVertexAt(0, new Point2d(0,0), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(10,10), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(20,20), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(30,30), 0, 0, 0);
            pl.AddVertexAt(4, new Point2d(40,40), 0, 0, 0);
            pl.Closed = true;
            pl.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
            tr.AddEntity(pl);
        }

        //块定义
        [CommandMethod("blockdef")]
        public void BlockDef()
        {
            using var tr = new DBTrans();
            //var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            tr.BlockTable.Add("test", 
                () => //图元
                {
                    return new List<Entity> { new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0))};
                },
                () => //属性定义
                {
                    var id1 = new AttributeDefinition() { Position = new Point3d(0, 0, 0), Tag = "start", Height = 0.2 };
                    var id2 = new AttributeDefinition() { Position = new Point3d(1, 1, 0), Tag = "end", Height = 0.2 };
                    return new List<AttributeDefinition> { id1, id2 };
                }
            );
        }
        //修改块定义
        [CommandMethod("blockdefchange")]
        public void BlockDefChange()
        {
            using var tr = new DBTrans();
            //var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            tr.BlockTable.Change("test", btr =>
            {
                btr.Origin = new Point3d(5, 5, 0);
                tr.AddEntity(new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 2), btr);
                btr.GetEntities<BlockReference>(tr.Trans)
                    .ToList()
                    .ForEach(e => tr.Flush(e)); //刷新块显示
                
            });
        }

        [CommandMethod("insertblockdef")]
        public void InsertBlockDef()
        {
            using var tr = new DBTrans();
            //var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            tr.InsertBlock(new Point3d(4, 4, 0), "test1"); //测试插入不存在的块定义
            tr.InsertBlock(new Point3d(4, 4, 0), "test"); // 测试默认
            tr.InsertBlock(new Point3d(0, 0, 0),"test", new Scale3d(2)); // 测试放大2倍
            tr.InsertBlock(new Point3d(4, 4, 0), "test", new Scale3d(2), Math.PI / 4); // 测试放大2倍,旋转45度
            var def = new Dictionary<string, string>
            {
                { "start", "1" },
                { "end", "2" }
            };
            tr.InsertBlock(new Point3d(4, 4, 0), "test", atts: def);
        }

        // 测试扩展数据
        [CommandMethod("addxdata")]
        public void AddXdata()
        {
            using var tr = new DBTrans();
            var appname = "myapp";
            
            tr.RegAppTable.Add(appname); // add函数会默认的在存在这个名字的时候返回这个名字的regapp的id，不存在就新建
            
            var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));


            line.XData = new XDataList() 
            { 
                { DxfCode.ExtendedDataRegAppName, appname },  //可以用dxfcode和int表示组码
                { DxfCode.ExtendedDataAsciiString, "hahhahah" },
                {1070, 12 }
            };

            tr.AddEntity(line);
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
                using (data.ForWrite())
                {
                    data.XData = new XDataList()
                    {
                        { DxfCode.ExtendedDataRegAppName, appname },  //可以用dxfcode和int表示组码
                        { DxfCode.ExtendedDataAsciiString, "change" },
                        { 1070, 20 },
                        { DxfCode.ExtendedDataLayerName, "0"}
                    };
                }
                
                //tr.AddEntity(data);

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

    }
}
