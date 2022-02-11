/**************************************************************
*作者：Leon
*创建时间：2022/2/11 9:55:32
**************************************************************/
namespace Test
{
    public class TestFileDatabase
    {
        [CommandMethod("Test_FileDatabaseInit")]
        public void TestDatabase()
        {
            try
            {
                var fileName = @"C:\Users\Administrator\Desktop\合并详图测试BUG.dwg";

                DBTrans trans = new(fileName);
                trans.ModelSpace.AddEntity(new Line(new(0, 0, 0), new(1000, 1000, 0)));
                trans.SaveAs(fileName, DwgVersion.AC1021);
                trans.Dispose();
            }
            catch (System.Exception e)
            {
               System.Windows.MessageBox.Show(e.Message);
            }

        }
    }
}