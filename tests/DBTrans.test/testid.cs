using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using IFoxCAD.Cad;
namespace test
{
    public class Testid
    {
        [CommandMethod("testid")]
        public void TestId()
        {
            using var tr = new DBTrans();
            Line line = new(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            tr.CurrentSpace.AddEntity(line);
            tr.Dispose();

            var res = Env.Editor.GetEntity("\npick ent:");
            if (res.Status == PromptStatus.OK)
            {
                res.ObjectId.Erase();
            }
            //using (var tr = new DBTrans())
            //{
            //    var res = Env.Editor.GetEntity("\npick ent:");
            //    if(res.Status == PromptStatus.OK)
            //    {
            //        res.ObjectId.Erase();
            //    }

            //}

        }
        
    }
}
