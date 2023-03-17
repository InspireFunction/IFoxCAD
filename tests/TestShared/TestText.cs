
namespace TestShared;

public class TestText
{

    [CommandMethod(nameof(TestDBText))]
    public void TestDBText()
    {
        using var tr = new DBTrans();
        tr.CurrentSpace.AddDBText(new(-1, -1, 0), "123", 2.5, t=> t.ColorIndex = 1);

        tr.CurrentSpace.AddDBText(new(0, 0, 0), "123", 2.5, t => {
            t.Justify = AttachmentPoint.BaseCenter;
            t.AlignmentPoint = new(1, 1, 0);
            t.ColorIndex = 2;
        });
    }

    [CommandMethod(nameof(TestMText))]
    public void TestMText()
    {
        using var tr = new DBTrans();
        tr.CurrentSpace.AddMText(new(5, 5, 0), "123", 2.5, t => t.ColorIndex = 1);

        tr.CurrentSpace.AddMText(new(10, 10, 0), "123", 2.5, t => {
            t.Attachment = AttachmentPoint.TopCenter;
            t.ColorIndex = 2;
        });
    }

}
