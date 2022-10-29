#if NewtonsoftJson
namespace Test_XRecord;

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using IFoxCAD.Cad;
using static IFoxCAD.Cad.WindowsAPI;

public class TestCmd_XRecord
{
    [CommandMethod(nameof(TestSerializeSetXRecord))]
    public void TestSerializeSetXRecord()
    {
        var prs = Env.Editor.SSGet("\n 序列化,选择多段线:");
        if (prs.Status != PromptStatus.OK)
            return;

        using var tr = new DBTrans();
        var pls = prs.Value.GetEntities<Polyline>();
        Tools.TestTimes(1, nameof(TestSerializeSetXRecord), () => {
            foreach (var pl in pls)
            {
                if (pl == null)
                    continue;

                TestABCList datas = new();
                for (int i = 0; i < 5; i++)
                {
                    datas.Add(new()
                    {
                        AAA = i,
                        BBB = i.ToString(),
                        CCCC = i * 0.5,
                        DDDD = i % 2 != 0,
                        EEEE = new(0, i, 0)
                    });
                }

                using (pl.ForWrite())
                    pl.SerializeToXRecord(datas);
            }
        });
    }


    [CommandMethod(nameof(TestDeserializeGetXRecord))]
    public void TestDeserializeGetXRecord()
    {
        var prs = Env.Editor.GetEntity("\n 反序列化,选择多段线:");
        if (prs.Status != PromptStatus.OK)
            return;

        using var tr = new DBTrans();
        TestABCList? datas = null;
        Tools.TestTimes(1, nameof(TestDeserializeGetXRecord), () => {
            var pl = prs.ObjectId.GetObject<Entity>();
            if (pl == null)
                return;
            datas = pl.DeserializeToXRecord<TestABCList>();
        });
        if (datas == null)
            return;
        for (int i = 0; i < datas.Count; i++)
            Env.Printl(datas[i]);
    }
}

public class TestABCList : List<TestABC>
{
}

[ComVisible(true)]
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(TestABC))]
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class TestABC
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    public int AAA;
    public string? BBB;
    public double CCCC;
    public bool DDDD;
    public Point3D EEEE;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, XRecordHelper._sset);
    }
}


public static class XRecordHelper
{
    internal static JsonSerializerSettings _sset = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Auto
    };

    /// <summary>
    /// 设定信息
    /// </summary>
    /// <param name="dbo">储存对象</param>
    /// <param name="data">储存数据</param>
    public static void SerializeToXRecord<T>(this DBObject dbo, T data)
    {
        var xd = dbo.GetXDictionary();
        if (xd == null)
            return;

        var json = JsonConvert.SerializeObject(data, _sset);
        XRecordDataList datas = new()
        {
            { DxfCode.XTextString, json }
        };
        xd.SetXRecord(typeof(T).FullName, datas);
    }

    /// <summary>
    /// 提取信息
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="dbo">储存对象</param>
    /// <returns>提取数据生成的对象</returns>
    public static T? DeserializeToXRecord<T>(this DBObject dbo)
    {
        var xd = dbo.GetXDictionary();
        if (xd == null)
            return default;

        var datas = xd.GetXRecord(typeof(T).FullName);
        if (datas == null)
            return default;

        var sb = new StringBuilder();
        for (int i = 0; i < datas.Count; i++)
            sb.Append(datas[i].Value);
        return JsonConvert.DeserializeObject<T>(sb.ToString(), _sset);
    }
}
#endif