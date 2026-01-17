namespace Test;

public class TestXdata
{
    // 测试扩展数据
    private const string Appname = "myapp2";

    // 增
    [CommandMethod(nameof(Test_AddXdata))]
    public void Test_AddXdata()
    {
        using DBTrans tr = new();
       
        tr.RegAppTable.Add("myapp1");
        tr.RegAppTable.Add(Appname); // add函数会默认的在存在这个名字的时候返回这个名字的regapp的id，不存在就新建
        tr.RegAppTable.Add("myapp3");
        tr.RegAppTable.Add("myapp4");

        var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0))
        {
            XData = new XDataList()
                {
                    { DxfCode.ExtendedDataRegAppName, "myapp1" },  // 可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "xxxxxxx" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, Appname },  // 可以用dxfcode和int表示组码,移除中间的测试
                    { DxfCode.ExtendedDataAsciiString, "要移除的我" },
                    { DxfCode.ExtendedDataAsciiString, "要移除的我" },
                    { DxfCode.ExtendedDataAsciiString, "要移除的我" },
                    { DxfCode.ExtendedDataAsciiString, "要移除的我" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, "myapp3" },  // 可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "aaaaaaaaa" },
                    {1070, 12 },
                    { DxfCode.ExtendedDataRegAppName, "myapp4" },  // 可以用dxfcode和int表示组码
                    { DxfCode.ExtendedDataAsciiString, "bbbbbbbbb" },
                    {1070, 12 }
                }
        };

        var line1 = new Line(new(0, 0, 0), new(2, 0, 0));
        line1.XData = new XDataList()
        {
            { DxfCode.ExtendedDataRegAppName, "myapp1" }, // 可以用dxfcode和int表示组码
            { DxfCode.ExtendedDataAsciiString, "xxxxxxx" },
            { 1070, 12 },
            { DxfCode.ExtendedDataRegAppName, Appname }, // 可以用dxfcode和int表示组码,移除中间的测试
            { DxfCode.ExtendedDataAsciiString, "要移除的我" },
            { DxfCode.ExtendedDataAsciiString, "要移除的我" },
            { DxfCode.ExtendedDataAsciiString, "要移除的我" },
            { DxfCode.ExtendedDataAsciiString, "要移除的我" },
            { 1070, 12 },
            { DxfCode.ExtendedDataRegAppName, "myapp3" }, // 可以用dxfcode和int表示组码
            { DxfCode.ExtendedDataAsciiString, "aaaaaaaaa" },
            { DxfCode.ExtendedDataAsciiString, "ccccccccc" },
            { 1070, 12 },
            { DxfCode.ExtendedDataRegAppName, "myapp4" }, // 可以用dxfcode和int表示组码
            { DxfCode.ExtendedDataAsciiString, "bbbbbbbbb" },
            { 1070, 12 }
        };

        tr.CurrentSpace.AddEntity(line,line1);
    }
    // 删
    [CommandMethod(nameof(Test_RemoveXdata))]
    public void Test_RemoveXdata()
    {
        var res = Env.Editor.GetEntity("\n select the entity:");
        if (res.Status != PromptStatus.OK) return;
        
        using DBTrans tr = new();
        var ent = tr.GetObject<Entity>(res.ObjectId);
        if (ent == null || ent.XData == null)
            return;

        Env.Printl("\n移除前:" + ent.XData);

        ent.RemoveXData(Appname, DxfCode.ExtendedDataAsciiString);
        Env.Printl("\n移除成员后:" + ent.XData);

        ent.RemoveXData(Appname);
        Env.Printl("\n移除appName后:" + ent.XData);
    }
    // 查
    [CommandMethod(nameof(Test_GetXdata))]
    public void Test_GetXdata()
    {
        using DBTrans tr = new();
        tr.RegAppTable.ForEach(id =>
            id.GetObject<RegAppTableRecord>()?.Name.Print());
        tr.RegAppTable.GetRecords().ForEach(rec => rec.Name.Print());
        tr.RegAppTable.GetRecordNames().ForEach(name => name.Print());
        tr.RegAppTable.ForEach(reg => reg.Name.Print(), checkIdOk: false);
        
        // 查询appName里面是否含有某个

        var res = Env.Editor.GetEntity("\n select the entity:");
        if (res.Status != PromptStatus.OK) return;
        
        var ent = tr.GetObject<Entity>(res.ObjectId);
        if (ent == null || ent.XData == null)
            return;

        XDataList data = ent.XData;
        if (data.Contains(Appname))
            Env.Printl("含有appName:" + Appname);
        else
            Env.Printl("不含有appName:" + Appname);

        const string str = "要移除的我";
        if (data.Contains(Appname, str))
            Env.Printl("含有内容:" + str);
        else
            Env.Printl("不含有内容:" + str);
    }
    // 改
    [CommandMethod(nameof(Test_ChangeXdata))]
    public void Test_ChangeXdata()
    {
        var res = Env.Editor.GetEntity("\n select the entity:");
        if (res.Status != PromptStatus.OK) return;
        
        using DBTrans tr = new();
        var data = tr.GetObject<Entity>(res.ObjectId)!;
        data.ChangeXData(Appname, DxfCode.ExtendedDataAsciiString, "change");

        if (data.XData == null)
            return;
        Env.Printl(data.XData.ToString());
    }

}