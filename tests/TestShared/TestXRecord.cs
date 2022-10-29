#if NewtonsoftJson
namespace Test_XRecord;

using IFoxCAD.Cad;
using Newtonsoft.Json;
using System.Collections.Generic;
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
                        BBB = $"{i}",
                        CCCC = i * 0.5,
                        DDDD = i % 2 != 0,
                        EEEE = new(0, i, 0)
                    });
                }

                using (pl.ForWrite())
                    pl.SerializeXRecord(datas);
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
        Tools.TestTimes(1, nameof(TestDeserializeGetXRecord), () => {
            var pl = prs.ObjectId.GetObject<Entity>();
            if (pl == null)
                return;
            var data = pl.DeserializeXRecord<TestABCList>();
            if (data == null)
                return;
            Env.Printl(data);
        });
    }
}

public class TestABCList : List<TestABC>
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Count; i++)
            sb.Append(this[i]);
        return sb.ToString();
    }
}

public class TestABC
{
    public int AAA { get; set; }
    public string? BBB { get; set; }
    public double CCCC { get; set; }
    public bool DDDD { get; set; }
    public Point3D EEEE { get; set; }

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
    static string[] _newl = new string[] { Environment.NewLine };

    /// <summary>
    /// 设定信息
    /// </summary>
    /// <param name="dbo">储存对象</param>
    /// <param name="data">储存数据</param>
    public static void SerializeXRecord<T>(this DBObject dbo, T data)
    {
        var xd = dbo.GetXDictionary();
        if (xd == null)
            return;
        var json = JsonConvert.SerializeObject(data, _sset);
        var arrStr = json.Split(_newl, StringSplitOptions.None);
        XRecordDataList datas = new();
        for (int i = 0; i < arrStr.Length; i++)
            datas.Add(DxfCode.XTextString, arrStr[i]);
        xd.SetXRecord(typeof(T).FullName, datas);
    }


    internal static JsonSerializerSettings _sset2 = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
    };
    /// <summary>
    /// 提取信息
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="dbo">储存对象</param>
    /// <returns>提取数据生成的对象</returns>
    public static T? DeserializeXRecord<T>(this DBObject dbo)
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
        return JsonConvert.DeserializeObject<T>(sb.ToString(), _sset2);
    }
}
#endif