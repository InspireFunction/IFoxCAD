#if NewtonsoftJson

namespace Test_XRecord;

using System.Diagnostics;
using Newtonsoft.Json;
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
                for (int i = 0; i < 1000; i++)
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

        // XRecordDataList 不能超过2G大小
        const int G2 = 2147483647;

        var json = JsonConvert.SerializeObject(data, _sset);
        var buffer = Encoding.UTF8.GetBytes(json);

        if (buffer.Length < G2)
        {
            Set16<T>(xd, json, buffer);
            return;
        }
        // 大于2G
        BytesTask(buffer, G2, bts => {
            Set16<T>(xd, json, bts);
        });
    }

    static void BytesTask(byte[] buffer, int max, Action<byte[]> action)
    {
        int index = 0;
        while (index < buffer.Length)
        {
            // 每次 max,然后末尾剩余就单独
            byte[] bts;
            if (buffer.Length - index > max)
                bts = new byte[max];
            else
                bts = new byte[buffer.Length - index];

            for (int i = 0; i < bts.Length; i++)
                bts[i] = buffer[index++];

            action.Invoke(bts);
        }
    }

    private static void Set16<T>(DBDictionary xd, string json, byte[] buffer)
    {
        // 单条只能 16KiBit => 2048 * 16 == 32768
        const int KiBit16 = 2048 * 16;

        XRecordDataList datas = new();
        if (buffer.Length < KiBit16)
        {
            datas.Add(DxfCode.XTextString, json);
        }
        else
        {
            BytesTask(buffer, KiBit16, bts => {
                datas.Add(DxfCode.XTextString, Encoding.UTF8.GetString(bts));
            });
        }
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
#endif