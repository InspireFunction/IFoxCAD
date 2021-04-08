using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using IFoxCAD.Cad;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace test
{
    public class testConvexHull
    {
        [CommandMethod("testch")]
        public void testch()
        {
            using var tr = new DBTrans();
            //var pts = new List<Point3d>();
            //var flag = true;
            //while (flag)
            //{
            //    var pt = tr.Editor.GetPoint("qudian");
            //    if (pt.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            //    {
            //        pts.Add(pt.Value);
            //        tr.CurrentSpace.AddEntity(new DBPoint(pt.Value));
            //    }
            //    else
            //    {
            //        flag = false;
            //    }
                
            //}

            //var ptt = ConvexHull.GetConvexHull(pts);

            //Polyline pl = new Polyline();
            //for (int i = 0; i < ptt.Count; i++)
            //{
            //    pl.AddVertexAt(i, ptt[i].Point2d(), 0, 0, 0);
            //}
            ////pl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            ////pl.AddVertexAt(1, new Point2d(10, 10), 0, 0, 0);
            ////pl.AddVertexAt(2, new Point2d(20, 20), 0, 0, 0);
            ////pl.AddVertexAt(3, new Point2d(30, 30), 0, 0, 0);
            ////pl.AddVertexAt(4, new Point2d(40, 40), 0, 0, 0);
            //pl.Closed = true;
            //pl.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
            //tr.CurrentSpace.AddEntity(pl);

            var a1 = GeometryEx.GetArea(new Point2d(0, 0), new Point2d(1, 0), new Point2d(1, 1));
            //var a2 = ConvexHull.cross(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(1, 1, 0));
            tr.Editor.WriteMessage(a1.ToString());
            //tr.Editor.WriteMessage(a2.ToString());
        }
    }
}
