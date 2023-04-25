namespace Test;
using IFoxCAD.Cad;
public class TestDwgMark {
    [CommandMethod(nameof(DwgMarktest))]
    public void DwgMarktest() {
        FileInfo file = new FileInfo(@"D:\TEST\1.dwg");
        DwgMark.AddMark(file, 0x4D);
        DwgMark.RemoveMark(file);
        int A = DwgMark.GetMark(file);
    }
}