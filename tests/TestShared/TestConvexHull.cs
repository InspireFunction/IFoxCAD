namespace Test;


public class TestConvexHull
{
    [CommandMethod(nameof(Test_ConvexHull))]
    public void Test_ConvexHull()
    {
        // using DBTrans tr = new();
        // List<Point3d> pts = [];
        // var flag = true;
        // while (flag)
        // {
        //    var pt = tr.Editor.GetPoint("qudian");
        //    if (pt.Status == PromptStatus.OK)
        //    {
        //        pts.Add(pt.Value);
        //        tr.CurrentSpace.AddEntity(new DBPoint(pt.Value));
        //    }
        //    else
        //    {
        //        flag = false;
        //    }

        // }

        // var ptt = ConvexHull.GetConvexHull(pts);

        // Polyline pl = new Polyline();
        // for (int i = 0; i < ptt.Count; i++)
        // {
        //    pl.AddVertexAt(i, ptt[i].Point2d(), 0, 0, 0);
        // }
        //// pl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
        //// pl.AddVertexAt(1, new Point2d(10, 10), 0, 0, 0);
        //// pl.AddVertexAt(2, new Point2d(20, 20), 0, 0, 0);
        //// pl.AddVertexAt(3, new Point2d(30, 30), 0, 0, 0);
        //// pl.AddVertexAt(4, new Point2d(40, 40), 0, 0, 0);
        // pl.Closed = true;
        // pl.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
        // tr.CurrentSpace.AddEntity(pl);

        // var a1 = GeometryEx.GetArea(new Point2d(0, 0), new Point2d(1, 0), new Point2d(1, 1));
        // var a2 = ConvexHull.cross(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(1, 1, 0));
        // tr.Editor.WriteMessage(a1.ToString());
        // tr.Editor.WriteMessage(a2.ToString());


        // var vec1 = new Vector2d(1, 1);
        // var vec2 = new Vector2d(-1, 1);

        // var vec3 = vec1.GetPerpendicularVector();
        // var vec4 = vec2.GetPerpendicularVector();

        // var area1 = vec2.DotProduct(vec1.GetPerpendicularVector());
        // var area2 = vec1.DotProduct(vec2.GetPerpendicularVector());

        // var area3 = vec2.DotProduct(vec1);
        // var area4 = vec1.DotProduct(vec2);

        var area5 = GeometryEx.GetArea(new List<Point2d> { new Point2d(0, 0), new Point2d(1, 1), new Point2d(-1, 1) });

        var area6 = GeometryEx.GetArea(new List<Point2d> { new Point2d(0, 0), new Point2d(-1, 1), new Point2d(1, 1) });
        // Env.Editor.WriteMessage($"vec1 的法向量= {vec3} \n");
        // Env.Editor.WriteMessage($"vec2 的法向量= {vec4} \n");

        // Env.Editor.WriteMessage($"vec2 点乘 vec1的法向量= {area1} \n");
        // Env.Editor.WriteMessage($"vec1 点乘 vec2的法向量= {area2} \n");

        // Env.Editor.WriteMessage($"vec2 点乘 vec1= {area3} \n");
        // Env.Editor.WriteMessage($"vec1 点乘 vec2= {area4} \n");

        Env.Editor.WriteMessage($"点集的有向面积：{area5} \n");
        Env.Editor.WriteMessage($"点集的有向面积：{area6} \n");
    }
}