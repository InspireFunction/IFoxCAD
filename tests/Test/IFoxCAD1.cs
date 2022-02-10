namespace Test
{
    public class IFoxCAD1
    {
        [CommandMethod("testtr")]
        public void Testtr()
        {
            string filename = @"C:\Users\vic\Desktop\test.dwg";
            using var tr = new DBTrans(filename);
            tr.ModelSpace.AddCircle(new Point3d(0, 0, 0), 20);
            tr.Database.SaveAs(filename,DwgVersion.Current);

            
        }
    }
}
