namespace Test;

public class TestLayer
{
    [CommandMethod(nameof(Test_LayerAdd0))]
    public void Test_LayerAdd0()
    {
        using DBTrans tr = new();
        tr.LayerTable.Add("1");
        tr.LayerTable.Add("2", lt => {
            lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 1);
            lt.LineWeight = LineWeight.LineWeight030;
        });
        tr.LayerTable.Remove("3");
        tr.LayerTable.Delete("0");
        tr.LayerTable.Change("4", lt => {
            lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 2);
        });
    }


    // 添加图层
    [CommandMethod(nameof(Test_LayerAdd1))]
    public void Test_LayerAdd1()
    {
        using DBTrans tr = new();
        tr.LayerTable.Add("test1", Color.FromColorIndex(ColorMethod.ByColor, 1));
    }

    // 添加图层
    [CommandMethod(nameof(Test_LayerAdd2))]
    public void Test_LayerAdd2()
    {
        using DBTrans tr = new();
        tr.LayerTable.Add("test2", 2);
        // tr.LayerTable["3"] = new LayerTableRecord();
    }
    // 删除图层
    [CommandMethod(nameof(Test_LayerDel))]
    public void Test_LayerDel()
    {
        using DBTrans tr = new();
        tr.LayerTable.Remove("0");        // 删除图层 0
        tr.LayerTable.Remove("Defpoints");// 删除图层 Defpoints
        tr.LayerTable.Remove("1");        // 删除不存在的图层 1
        tr.LayerTable.Remove("2");        // 删除有图元的图层 2
        tr.LayerTable.Remove("3");        // 删除图层 3  // 删除图层 3

        tr.LayerTable.Remove("2"); // 测试是否能强制删除
    }
    
    [CommandMethod(nameof(Test_PrintLayerName))]
    public void Test_PrintLayerName()
    {
        using DBTrans tr = new();
        foreach (var layerRecord in tr.LayerTable.GetRecords())
        {
            Env.Printl(layerRecord.Name);
        }
        foreach (var layerRecord in tr.LayerTable.GetRecords())
        {
            Env.Printl(layerRecord.Name);
            break;
        }
    }
}