namespace Test;

public class TestAddEntity
{
#region 直线
    [CommandMethod(nameof(Test_AddLinetoCurrentSpace))]
    public void Test_AddLinetoCurrentSpace()
    {
        using DBTrans tr = new(); // 开启事务

        Line line = new(new(0, 0, 0), new(1, 1, 0)); // 定义一个直线
        tr.CurrentSpace.AddEntity(line); // 将直线添加到当前空间
    }

    [CommandMethod(nameof(Test_AddLinetoModelSpace))]
    public void Test_AddLinetoModelSpace()
    {
        using DBTrans tr = new(); // 开启事务

        Line line = new(new(0, 0, 0), new(1, 1, 0)); // 定义一个直线
        tr.ModelSpace.AddEntity(line); // 将直线添加到模型空间
    }

    [CommandMethod(nameof(Test_AddLinetoPaperSpace))]
    public void Test_AddLinetoPaperSpace()
    {
        using DBTrans tr = new(); // 开启事务

        Line line = new(new(0, 0, 0), new(1, 1, 0)); // 定义一个直线
        tr.PaperSpace.AddEntity(line); // 将直线添加到图纸空间
    }

    [CommandMethod(nameof(Test_AddEntities))]
    public void Test_AddEntities()
    {
        // 开启事务
        using DBTrans tr = new();
        // 定义三条直线
        Line line1 = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        Line line2 = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
        Line line3 = new(new Point3d(1, 1, 0), new Point3d(3, 3, 0));
        Circle circle = new();
        // 一次性添加到当前空间
        tr.CurrentSpace.AddEntity(line2, line2, line3, circle);
        // 或者可以传入个列表
        List<Line> lines = [line1, line2, line3];
        tr.CurrentSpace.AddEntity(lines);
        // 或者可以传入个数组
        Line[] lines1 = [line1, line2, line3];
        tr.CurrentSpace.AddEntity(lines1);
        // 图元数组
        Entity[] lines2 = [line1, line2, line3, circle];
        tr.CurrentSpace.AddEntity(lines2);
        // c#12 新语法，集合表达式
        tr.CurrentSpace.AddEntity([line1, line2, circle]);
    }
#endregion

#region 圆
    [CommandMethod(nameof(Test_AddCircle))]
    public void Test_AddCircle()
    {
        var cir = CircleEx.CreateCircle(Point3d.Origin, new(1,0,0)); // 两点创建圆
        var cir1 = CircleEx.CreateCircle(Point3d.Origin, new(1,1,0), new(2,0,0)); //三点创建圆
        var cir2 = CircleEx.CreateCircle(Point3d.Origin, 5); // 圆心半径创建圆

        using DBTrans tr = new();
        tr.CurrentSpace.AddEntity(cir, cir2);
        
        // 由于三点不一定能成功创建一个圆，因此返回值是可空的，需要判空
        if (cir1 is not null)
        {
            tr.CurrentSpace.AddEntity(cir1);
        }
    }
#endregion

#region 圆弧
    [CommandMethod(nameof(Test_AddArc))]
    public void Test_AddArc()
    {
        using DBTrans tr = new();
        Arc arc1 = ArcEx.CreateArcSCE(new Point3d(2, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 2, 0));// 起点，圆心，终点
        Arc arc2 = ArcEx.CreateArc(new Point3d(4, 0, 0), new Point3d(0, 0, 0), Math.PI / 2);            // 起点，圆心，弧度
        Arc arc3 = ArcEx.CreateArc(new Point3d(1, 0, 0), new Point3d(0, 0, 0), new Point3d(0, 1, 0));   // 起点，圆上一点，终点
        tr.CurrentSpace.AddEntity(arc1, arc2, arc3);
    }

#endregion





#region 多段线
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

    [CommandMethod(nameof(Test_AddPolyline2))]
    public void Test_AddPolyline2()
    {
        // 集合表达式
        List<(Point3d, double, double, double)> pts =
        [
            (new Point3d(0,0,0),0,0,0),
            (new Point3d(10,0,0),0,0,0),
            (new Point3d(10,10,0),0,0,0),
            (new Point3d(0,10,0),0,0,0),
            (new Point3d(5,5,0),0,0,0)
        ];
        
        using DBTrans tr = new();
        var pl = pts.CreatePolyline();
        tr.CurrentSpace.AddEntity(pl);
    }

    [CommandMethod(nameof(Test_AddPolyline3))]
    public void Test_AddPolyline3()
    {
        using var tr = new DBTrans();

        List<Point3d> pts =
        [
            new(0, 0, 0),
            new(0, 1, 0),
            new(1, 1, 0),
            new(1, 0, 0)
        ];
        var pline = pts.CreatePolyline();
        tr.CurrentSpace.AddEntity(pline);

        // 可以通过委托，一次性的创建多段线并设置属性
        var pline1 = pts.CreatePolyline(p =>
        {
            p.Closed = true;
            p.ConstantWidth = 0.2;
            p.ColorIndex = 1;
        });
        tr.CurrentSpace.AddEntity(pline1);
    }

#endregion

}
