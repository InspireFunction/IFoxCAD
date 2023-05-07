
namespace TestShared;

public class TestText
{

    [CommandMethod(nameof(TestDBText))]
    public void TestDBText()
    {
        using var tr = new DBTrans();
        tr.CurrentSpace.AddEntity(DBTextEx.CreateDBText(new(-1, -1, 0), "123", 2.5, action:t=> t.ColorIndex = 1));

        tr.CurrentSpace.AddEntity(DBTextEx.CreateDBText(new(-1, -1, 0), "123", 2.5, action: t => {
            t.Justify = AttachmentPoint.BaseCenter;
            t.AlignmentPoint = new(1, 1, 0);
            t.ColorIndex = 2;
        }));
    }

    [CommandMethod(nameof(TestBackDBText))]
    public void TestBackDBText()
    {
        using var tr = new DBTrans(@"C:\Users\vic\Desktop\test.dwg");
        tr.CurrentSpace.AddEntity(DBTextEx.CreateDBText(new(-1, -1, 0), "123", 2.5, action: t => t.ColorIndex = 1));

        tr.CurrentSpace.AddEntity(DBTextEx.CreateDBText(new(-1, -1, 0), "123", 2.5, action: t => 
        {
            t.Justify = AttachmentPoint.BaseCenter; 
            t.AlignmentPoint = new(1, 1, 0); 
            t.ColorIndex = 2; 
        }));
        tr.Database.SaveDwgFile();
    }



    [CommandMethod(nameof(TestMText))]
    public void TestMText()
    {
        using var tr = new DBTrans();
        tr.CurrentSpace.AddEntity(MTextEx.CreateMText(new(5, 5, 0), "123", 2.5, action: t => t.ColorIndex = 1));

        tr.CurrentSpace.AddEntity(MTextEx.CreateMText(new(5, 5, 0), "123", 2.5, action: t => {
            t.Attachment = AttachmentPoint.TopCenter;
            t.ColorIndex = 2;
        }));
    }

}
