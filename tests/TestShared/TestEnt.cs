namespace Test;

public class TestEnt
{
    [CommandMethod(nameof(Test_EntRoration))]
    public void Test_EntRoration()
    {
        var line = new Line(new(0, 0, 0), new(100, 0, 0));

        using DBTrans tr = new();
        tr.CurrentSpace.AddEntity(line);
        var line2 = (Line)line.Clone();
        tr.CurrentSpace.AddEntity(line2);
        line2.Rotation(new(100, 0, 0), Math.PI / 2);
    }


    [CommandMethod(nameof(Test_TypeSpeed))]
    public void Test_TypeSpeed()
    {
        var line = new Line();
        var line1 = line as Entity;
        Tools.TestTimes(100000, "is 匹配：", () => {
            var t = line1 is Line;
        });
        Tools.TestTimes(100000, "name 匹配：", () => {
            // var t = line.GetType().Name;
            var tt = line1.GetType().Name == nameof(Line);
        });
        Tools.TestTimes(100000, "dxfname 匹配：", () => {
            // var t = line.GetType().Name;
            var tt = line1.GetRXClass().DxfName == nameof(Line);
        });
    }
}