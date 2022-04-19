using System.Runtime.Remoting.Metadata;

namespace Test
{
    public class TestTrans
    {
        [CommandMethod("testtr")]
        public void Testtr()
        {
            string filename = @"C:\Users\vic\Desktop\test.dwg";
            using var tr = new DBTrans(filename);
            tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
            tr.Database.SaveAs(filename,DwgVersion.Current);
        }
        [CommandMethod("testifoxcommit")]
        public void Testifoxcommit()
        {
            
            using var tr = new DBTrans();
            tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
            tr.Abort();
            //tr.Commit();
        }

        // AOP 应用 预计示例：
        // 1. 无参数
        //[AOP]
        //[CommandMethod("TESTAOP")]
        //public void testaop()
        //{
        //    // 不用 using var tr = new DBTrans();
        //    var tr = DBTrans.Top;
        //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
        //}

        // 2. 有参数
        //[AOP("file")]
        //[CommandMethod("TESTAOP")]
        //public void testaop()
        //{
        //    // 不用 using var tr = new DBTrans(file);
        //    var tr = DBTrans.Top;
        //    tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
        //}



    }
}
