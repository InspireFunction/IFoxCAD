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
}
