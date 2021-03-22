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

        [CommandMethod("testdimblk")]
        public void testdimblk()
        {

            Env.Dimblk = Env.DimblkType.Dot;
            Env.Dimblk = Env.DimblkType.Defult;

        }
        [CommandMethod("testdimblk1")]
        public void testdimblk1()
        {
            var dim = Env.Dimblk;
            Env.Editor.WriteMessage(dim.ToString());

        }

        [CommandMethod("testosmode")]
        public void testosmode()
        {
            // 设置osmode变量，多个值用逻辑或
            Env.OSMode = Env.OSModeType.End | Env.OSModeType.Middle;
            // 也可以直接写数值，进行强转
            Env.OSMode = (Env.OSModeType)5179;
            // 追加模式
            Env.OSMode |= Env.OSModeType.Center;
            //检查是否有某个模式
            var os = Env.OSMode.Check(Env.OSModeType.Center);
            // 取消某个模式
            Env.OSMode ^= Env.OSModeType.Center;
            Env.Editor.WriteMessage(Env.OSMode.ToString());
        }
        [CommandMethod("testosmode1")]
        public void testosmode1()
        {
            var dim = Env.OSMode;
            Env.Editor.WriteMessage(dim.ToString());

        }

    }
}
