using System.Diagnostics;
using System.Web.UI.WebControls;

namespace Test;

public partial class Test
{
    [CommandMethod(nameof(Test_DBTrans))]
    public void Test_DBTrans()
    {
        using DBTrans tr = new();
        if (tr.Editor is null)
            return;
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
        // var lienid = tr.AddEntity(line);
        // var cirid = tr.AddEntity(circle);
        // var linent = tr.GetObject<Line>(lienid);
        // var lineent = tr.GetObject<Circle>(cirid);
        // var linee = tr.GetObject<Line>(cirid); // 经测试，类型不匹配，返回null
        // var dd = tr.GetObject<Circle>(lienid);
        // List<DBObject> ds = new() { linee, dd };
        // tr.CurrentSpace.AddEntity(line,tr);
    }

    // add entity test
    [CommandMethod(nameof(Test_Addent))]
    public void Test_Addent()
    {
        using DBTrans tr = new();
        Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        tr.CurrentSpace.AddEntity(line);
        Line line1 = new(new Point3d(10, 10, 0), new Point3d(41, 1, 0));
        tr.ModelSpace.AddEntity(line1);
        Line line2 = new(new Point3d(-10, 10, 0), new Point3d(41, 1, 0));
        tr.PaperSpace.AddEntity(line2);
    }

    [CommandMethod(nameof(Test_Drawarc))]
    public void Test_Drawarc()
    {
        using DBTrans tr = new();
        Arc arc1 = ArcEx.CreateArcSCE(new Point3d(2, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 2, 0));// 起点，圆心，终点
        Arc arc2 = ArcEx.CreateArc(new Point3d(4, 0, 0), new Point3d(0, 0, 0), Math.PI / 2);            // 起点，圆心，弧度
        Arc arc3 = ArcEx.CreateArc(new Point3d(1, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 1, 0));   // 起点，圆上一点，终点
        tr.CurrentSpace.AddEntity(arc1, arc2, arc3);
        tr.CurrentSpace.AddArc(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(2, 0, 0));// 起点，圆上一点，终点
    }

    [CommandMethod(nameof(Test_DrawCircle))]
    public void Test_DrawCircle()
    {
        using DBTrans tr = new();
        var circle1 = CircleEx.CreateCircle(new Point3d(0, 0, 0), new Point3d(1, 0, 0));                       // 起点，终点
        var circle2 = CircleEx.CreateCircle(new Point3d(-2, 0, 0), new Point3d(2, 0, 0), new Point3d(0, 2, 0));// 三点画圆，成功
        var circle3 = CircleEx.CreateCircle(new Point3d(-2, 0, 0), new Point3d(0, 0, 0), new Point3d(2, 0, 0));// 起点，圆心，终点，失败
        tr.CurrentSpace.AddEntity(circle1, circle2!);
        if (circle3 is not null)
            tr.CurrentSpace.AddEntity(circle3);
        else
            tr.Editor?.WriteMessage("三点画圆失败");
        tr.CurrentSpace.AddEntity(circle3!);
        tr.CurrentSpace.AddCircle(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(2, 0, 0));// 三点画圆，成功
        tr.CurrentSpace.AddCircle(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(2, 2, 0));// 起点，圆上一点，终点(共线)
    }

    [CommandMethod(nameof(Test_LayerAdd0))]
    public void Test_LayerAdd0()
    {
        using DBTrans tr = new();
        tr.LayerTable.Add("1");
        tr.LayerTable.Add("2", lt => {
            lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 1);
            lt.LineWeight = LineWeight.LineWeight030;
        });
        tr.LayerTable.Remove("3");
        tr.LayerTable.Delete("0");
        tr.LayerTable.Change("4", lt => {
            lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 2);
        });
    }


    // 添加图层
    [CommandMethod(nameof(Test_LayerAdd1))]
    public void Test_LayerAdd1()
    {
        using DBTrans tr = new();
        tr.LayerTable.Add("test1", Color.FromColorIndex(ColorMethod.ByColor, 1));
    }

    // 添加图层
    [CommandMethod(nameof(Test_LayerAdd2))]
    public void Test_LayerAdd2()
    {
        using DBTrans tr = new();
        tr.LayerTable.Add("test2", 2);
        // tr.LayerTable["3"] = new LayerTableRecord();
    }
    // 删除图层
    [CommandMethod(nameof(Test_LayerDel))]
    public void Test_LayerDel()
    {
        using DBTrans tr = new();
        Env.Editor.WriteMessage(tr.LayerTable.Delete("0").ToString());        // 删除图层 0
        Env.Editor.WriteMessage(tr.LayerTable.Delete("Defpoints").ToString());// 删除图层 Defpoints
        Env.Editor.WriteMessage(tr.LayerTable.Delete("1").ToString());        // 删除不存在的图层 1
        Env.Editor.WriteMessage(tr.LayerTable.Delete("2").ToString());        // 删除有图元的图层 2
        Env.Editor.WriteMessage(tr.LayerTable.Delete("3").ToString());        // 删除图层 3

        tr.LayerTable.Remove("2"); // 测试是否能强制删除
    }

    // 添加直线
    [CommandMethod(nameof(Test_AddLine1))]
    public void Test_AddLine1()
    {
        using DBTrans tr = new();
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
        Circle circle = new(new Point3d(0, 0, 0), Vector3d.ZAxis, 10);
        tr.CurrentSpace.AddEntity(line1);
        tr.CurrentSpace.AddEntity(line2, line3, circle);
    }

    // 增加多段线1
    [CommandMethod(nameof(Test_AddPolyline1))]
    public void Test_AddPolyline1()
    {
        using DBTrans tr = new();
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        pl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
        pl.AddVertexAt(1, new Point2d(10, 10), 0, 0, 0);
        pl.AddVertexAt(2, new Point2d(20, 20), 0, 0, 0);
        pl.AddVertexAt(3, new Point2d(30, 30), 0, 0, 0);
        pl.AddVertexAt(4, new Point2d(40, 40), 0, 0, 0);
        pl.Closed = true;
        pl.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
        tr.CurrentSpace.AddEntity(pl);
    }

    // 增加多段线2
    [CommandMethod(nameof(Test_AddPolyline2))]
    public void Test_AddPolyline2()
    {
        var pts = new List<(Point3d, double, double, double)>
            {
                (new Point3d(0,0,0),0,0,0),
                (new Point3d(10,0,0),0,0,0),
                (new Point3d(10,10,0),0,0,0),
                (new Point3d(0,10,0),0,0,0),
                (new Point3d(5,5,0),0,0,0)
            };
        using DBTrans tr = new();
        tr.CurrentSpace.AddPline(pts);
    }




    // 测试扩展数据
    static readonly string _appname = "myapp2";
    // 增
    [CommandMethod(nameof(Test_AddXdata))]
    public void Test_AddXdata()
    {
        using DBTrans tr = new();
        var appname = "myapp2";

        tr.RegAppTable.Add("myapp1");
        tr.RegAppTable.Add(appname); // add函数会默认的在存在这个名字的时候返回这个名字的regapp的id，不存在就新建
        tr.RegAppTable.Add("myapp3");
        tr.RegAppTable.Add("myapp4");

        var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0))
        {
            XData = new XDataList()
                {
                    { DxfCode.ExtendedDataRegAppName, "myapp1" },  // 可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "xxxxxxx" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, appname },  // 可以用dxfcode和int表示组码,移除中间的测试
                    { DxfCode.ExtendedDataAsciiString, "要移除的我" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, "myapp3" },  // 可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "aaaaaaaaa" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, "myapp4" },  // 可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "bbbbbbbbb" },
                    {1070, 12 }
                }
        };

        tr.CurrentSpace.AddEntity(line);
    }
    // 删
    [CommandMethod(nameof(Test_RemoveXdata))]
    public void Test_RemoveXdata()
    {
        var res = Env.Editor.GetEntity("\n select the entity:");
        if (res.Status == PromptStatus.OK)
        {
            using DBTrans tr = new();
            var ent = tr.GetObject<Entity>(res.ObjectId);
            if (ent == null || ent.XData == null)
                return;

            Env.Printl("\n移除前:" + ent.XData.ToString());

            ent.RemoveXData(_appname, DxfCode.ExtendedDataAsciiString);
            Env.Printl("\n移除成员后:" + ent.XData.ToString());

            ent.RemoveXData(_appname);
            Env.Printl("\n移除appName后:" + ent.XData.ToString());
        }
    }
    // 查
    [CommandMethod(nameof(Test_GetXdata))]
    public void Test_GetXdata()
    {
        using DBTrans tr = new();
        tr.RegAppTable.ForEach(id =>
            id.GetObject<RegAppTableRecord>()?.Name.Print());
        tr.RegAppTable.GetRecords().ForEach(rec => rec.Name.Print());
        tr.RegAppTable.GetRecordNames().ForEach(name => name.Print());
        tr.RegAppTable.ForEach(reg => reg.Name.Print(), checkIdOk: false);

        // var res = ed.GetEntity("\n select the entity:");
        // if (res.Status == PromptStatus.OK)
        // {
        //    using DBTrans tr = new();
        //    tr.RegAppTable.ForEach(id => id.GetObject<RegAppTableRecord>().Print());
        //    var data = tr.GetObject<Entity>(res.ObjectId).XData;
        //    ed.WriteMessage(data.ToString());
        // }

        // 查询appName里面是否含有某个

        var res = Env.Editor.GetEntity("\n select the entity:");
        if (res.Status == PromptStatus.OK)
        {
            var ent = tr.GetObject<Entity>(res.ObjectId);
            if (ent == null || ent.XData == null)
                return;

            XDataList data = ent.XData;
            if (data.Contains(_appname))
                Env.Printl("含有appName:" + _appname);
            else
                Env.Printl("不含有appName:" + _appname);

            var str = "要移除的我";
            if (data.Contains(_appname, str))
                Env.Printl("含有内容:" + str);
            else
                Env.Printl("不含有内容:" + str);
        }
    }
    // 改
    [CommandMethod(nameof(Test_ChangeXdata))]
    public void Test_ChangeXdata()
    {
        var res = Env.Editor.GetEntity("\n select the entity:");
        if (res.Status != PromptStatus.OK)
            return;
        using DBTrans tr = new();
        var data = tr.GetObject<Entity>(res.ObjectId)!;
        data.ChangeXData(_appname, DxfCode.ExtendedDataAsciiString, "change");

        if (data.XData == null)
            return;
        Env.Printl(data.XData.ToString());
    }



    [CommandMethod(nameof(Test_PrintLayerName))]
    public void Test_PrintLayerName()
    {
        using DBTrans tr = new();
        foreach (var layerRecord in tr.LayerTable.GetRecords())
        {
            tr.Editor?.WriteMessage(layerRecord.Name);
        }
        foreach (var layerRecord in tr.LayerTable.GetRecords())
        {
            tr.Editor?.WriteMessage(layerRecord.Name);
            break;
        }
    }


    [CommandMethod(nameof(Test_Rec))]
    public void Test_Rec()
    {
        Point2d p1 = new(10000.2, 100000.5);
        Point2d p2 = new(15000.9, 100000.5);
        Point2d p3 = new(15000.9, 105000.7);
        Point2d p4 = new(10000.2, 105000.7);

        var p12 = p2 - p1;
        var p23 = p3 - p2;
        var p34 = p4 - p3;
        var p41 = p1 - p4;
        var p13 = p3 - p1;
        var p24 = p4 - p2;


        const double pi90 = Math.PI / 2;
        pi90.Print();

        Tools.TestTimes(1000000, "对角线", () => {
            var result = false;
            if (Math.Abs(p13.Length - p24.Length) <= 1e8)
            {
                result = p41.IsParallelTo(p12);
            }
        });

#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
        Tools.TestTimes(1000000, "三次点乘", () => {
            var result = false;
            if (Math.Abs(p12.DotProduct(p23)) < 1e8 &&
                Math.Abs(p23.DotProduct(p34)) < 1e8 &&
                Math.Abs(p34.DotProduct(p41)) < 1e8)
                result = true;
        });

        Tools.TestTimes(1000000, "三次垂直", () => {
            var result = false;
            if (p12.IsParallelTo(p23) &&
                p23.IsParallelTo(p34) &&
                p34.IsParallelTo(p41))
                result = true;
        });
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
    }

    public Database Getdb()
    {
        var db = Acap.DocumentManager.MdiActiveDocument.Database;
        return db;
    }

    public Document Getdoc()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        return doc;
    }


    [CommandMethod(nameof(Test_EntRoration))]
    public void Test_EntRoration()
    {
        var line = new Line(new(0, 0, 0), new(100, 0, 0));

        using DBTrans tr = new();
        tr.CurrentSpace.AddEntity(line);
        var line2 = (Line)line.Clone();
        tr.CurrentSpace.AddEntity(line2);
        line2.Rotation(new(100, 0, 0), Math.PI / 2);
    }

    [CommandMethod(nameof(Test_TypeSpeed))]
    public void Test_TypeSpeed()
    {
        var line = new Line();
        var line1 = line as Entity;
        Tools.TestTimes(100000, "is 匹配：", () => {
            var t = line1 is Line;
        });
        Tools.TestTimes(100000, "name 匹配：", () => {
            // var t = line.GetType().Name;
            var tt = line1.GetType().Name == nameof(Line);
        });
        Tools.TestTimes(100000, "dxfname 匹配：", () => {
            // var t = line.GetType().Name;
            var tt = line1.GetRXClass().DxfName == nameof(Line);
        });
    }
}