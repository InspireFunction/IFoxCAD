using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using IFoxCAD.Cad;
using Autodesk.AutoCAD.EditorInput;

namespace Test
{
    public class testselectfilter
    {
        [CommandMethod("testfilter")]
        public void testfilter()
        {
            
            var p = new Point3d(10, 10, 0);
            var f = OpFilter.Bulid(
            e =>!(e.Dxf(0) == "line" & e.Dxf(8) == "0")
                    | e.Dxf(0) != "circle" & e.Dxf(8) == "2" & e.Dxf(10) >= p);
            
           
            var f2 = OpFilter.Bulid(
            e => e.Or(
            !e.And(e.Dxf(0) == "line", e.Dxf(8) == "0"),
            e.And(e.Dxf(0) != "circle", e.Dxf(8) == "2",
            e.Dxf(10) <= new Point3d(10, 10, 0))));

            SelectionFilter f3 = f;
            SelectionFilter f4 = f2;

            Env.Editor.WriteMessage("");
        }

        [CommandMethod("testselectanpoint")]
        public void testselectanpoint()
        {

            
            var sel2 = Env.Editor.SelectAtPoint(new Point3d(0, 0, 0));

            Env.Editor.WriteMessage("");
        }
    }
}
