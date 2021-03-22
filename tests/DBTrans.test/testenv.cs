using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFoxCAD.Cad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;

namespace test
{
    public class testenv
    {
        [CommandMethod("testenum")]
        public void testenum()
        {
           
            Env.CmdEcho = true;
            
        }
        [CommandMethod("testenum1")]
        public void testenum1()
        {
            
            Env.CmdEcho = false;
           
        }

    }
}
