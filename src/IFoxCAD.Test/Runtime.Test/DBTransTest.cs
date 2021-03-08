using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using IFoxCAD;
using IFoxCAD.Cad;

namespace IFoxCAD.Test.Runtime.Test
{
    public class DBTransTest :IExtensionApplication
    {
        public void Initialize()
        {
            using (DBTrans db=new DBTrans())
            {
                var editor = db.Editor;
                editor.WriteMessage("IFoxCAD.test start");
            }
        }

        public void Terminate()
        {
           
        }

        [CommandMethod("he")]
        public void CreataEntity()
        {

            using (DBTrans db = new DBTrans())
            {
                var circle = new Circle();
                circle.ForWrite(it =>
                {
                    it.Center = new Autodesk.AutoCAD.Geometry.Point3d(1, 2, 3);
                    it.Radius = 10;
                    it.ColorIndex = 2;
                });
                db.Commit();
            }            
        }
    
    }
}
