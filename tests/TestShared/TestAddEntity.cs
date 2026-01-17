namespace Test;

public partial class Test
{



    
   

    


    


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
            if (cir is null)
            {
                return;
            }
            cir.ColorIndex = i;
            tr.CurrentSpace.AddEntity(cir);
            tr.Editor?.Redraw(cir);
            System.Threading.Thread.Sleep(10);
        }
    }
}