namespace Test;

public class TestEnt
{
    [CommandMethod("TestEntRoration")]
    public void TestEntRoration()
    {
        var line = new Line(new(0,0,0),new(100,0,0));

        using var tr = new DBTrans();
        tr.CurrentSpace.AddEntity(line);
        var line2 = line.Clone() as Line;
        tr.CurrentSpace.AddEntity(line2);
        line2.Rotation(new(100, 0, 0), Math.PI / 2);


    }


    [CommandMethod("Testtypespeed")]
    public void TestTypeSpeed()
    {
        var line = new Line();
        var line1 = line as Entity;
        Tools.TestTimes(100000, "is 匹配：", () =>
         {
             var t = line1 is Line;
         });
        Tools.TestTimes(100000, "name 匹配：", () =>
        {
            //var t = line.GetType().Name;
            var tt = line1.GetType().Name == nameof(Line);
        });
        Tools.TestTimes(100000, "dxfname 匹配：", () =>
        {
            //var t = line.GetType().Name;
            var tt = line1.GetRXClass().DxfName == nameof(Line);
        });
    }
}
