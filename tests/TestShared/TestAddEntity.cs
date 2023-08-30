namespace Test;

public partial class Test
{
    // add entity test
    [CommandMethod(nameof(Test_Addent))]
    public void Test_Addent()
    {
        using DBTrans tr = new();
        Line line = new(new(0, 0, 0), new(1, 1, 0));
        tr.CurrentSpace.AddEntity(line);
        Line line1 = new(new(10, 10, 0), new(41, 1, 0));
        tr.ModelSpace.AddEntity(line1);
        Line line2 = new(new(-10, 10, 0), new(41, 1, 0));
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
        var pl = pts.CreatePolyline();
        tr.CurrentSpace.AddEntity(pl);
    }

    [CommandMethod(nameof(Test_AddPolyline3))]
    public void Test_AddPolyline3()
    {
        using var tr = new DBTrans();

        var pts = new List<Point3d>
        {
            new(0, 0, 0),
            new(0, 1, 0),
            new(1, 1, 0),
            new(1, 0, 0)
        };
        var pline = pts.CreatePolyline();
        tr.CurrentSpace.AddEntity(pline);

        var pline1 = pts.CreatePolyline(p =>
        {
            p.Closed = true;
            p.ConstantWidth = 0.2;
            p.ColorIndex = 1;
        });
        tr.CurrentSpace.AddEntity(pline1);
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
            bool result = Math.Abs(p12.DotProduct(p23)) < 1e8 &&
                          Math.Abs(p23.DotProduct(p34)) < 1e8 &&
                          Math.Abs(p34.DotProduct(p41)) < 1e8;
        });

        Tools.TestTimes(1000000, "三次垂直", () => {
            bool result = p12.IsParallelTo(p23) &&
                          p23.IsParallelTo(p34) &&
                          p34.IsParallelTo(p41);
        });
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
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

    [CommandMethod(nameof(Test_sleeptrans))]
    public static void Test_sleeptrans()
    {
        using var tr = new DBTrans();
        for (int i = 0; i < 100; i++)
        {
            var cir = CircleEx.CreateCircle(new Point3d(i, i, 0), 0.5);
            cir.ColorIndex = i;
            tr.CurrentSpace.AddEntity(cir);
            tr.Editor?.Redraw(cir);
            System.Threading.Thread.Sleep(10);
        }
    }
}