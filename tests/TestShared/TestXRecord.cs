//#define ExtendedDataBinaryChunk
#define XTextString

#if NewtonsoftJson
using System.Diagnostics;
using Newtonsoft.Json;
using static IFoxCAD.Cad.WindowsAPI;

namespace Test_XRecord;

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
#if XTextString
            datas = pl.DeserializeToXRecord<TestABCList>();
#endif

#if ExtendedDataBinaryChunk
            // 这里有数据容量限制,而且很小
            var xd = pl.GetXDictionary();
            if (xd == null)
                return;
            if (xd.XData == null)
                return;

            XDataList data = xd.XData;
            var sb = new StringBuilder();
            data.ForEach(a => {
                if (a.TypeCode == (short)DxfCode.ExtendedDataBinaryChunk)
                    if (a.Value is byte[] bytes)
                        sb.Append(Encoding.UTF8.GetString(bytes));
            });
            datas = JsonConvert.DeserializeObject<TestABCList>(sb.ToString(), XRecordHelper._sset);
#endif
        });
        if (datas == null)
        {
            Env.Printl("没有反序列的东西");
            return;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < datas.Count; i++)
            sb.Append(datas[i]);
        Env.Printl(sb);
    }
}

public static class XRecordHelper
{
    #region 序列化方式
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
        const int GigaByte2 = 2147483647;
        // 单条只能 16KiBit => 2048 * 16 == 32768
        const int KiBit16 = (2048 * 16) - 1;

        // 就算这个写法支持,计算机也不一定有那么多内存,所以遇到此情况最好换成内存拷贝
        var json = JsonConvert.SerializeObject(data, _sset);// 此时内存占用了2G
        var buffer = Encoding.UTF8.GetBytes(json);          // 此时内存又占用了2G..

        BytesTask(buffer, GigaByte2, bts => {
#if XTextString
            XRecordDataList datas = new();
            BytesTask(buffer, KiBit16, bts => {
                datas.Add(DxfCode.XTextString, Encoding.UTF8.GetString(bts)); // 这对的
                // datas.Add(DxfCode.XTextString, bts);//这样 bts 变成 "System.Byte[]"
            });
            xd.SetXRecord(typeof(T).FullName, datas);
#endif

#if ExtendedDataBinaryChunk
            // 这里有数据容量限制,而且很小
            var appname = typeof(T).FullName;
            DBTrans.Top.RegAppTable.Add(appname);

            XDataList datas = new();
            datas.Add(DxfCode.ExtendedDataRegAppName, appname);
            BytesTask(buffer, KiBit16, bts => {
                datas.Add(DxfCode.ExtendedDataBinaryChunk, bts);
            });
            using (xd.ForWrite())
                xd.XData = datas; // Autodesk.AutoCAD.Runtime.Exception:“eXdataSizeExceeded”
#endif
        });
    }

    [DebuggerHidden]
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
    #endregion


    /// <summary>
    /// 设置描述(容量无限)
    /// </summary>
    /// <param name="db"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetSummaryInfoAtt(this Database db, string key, object value)
    {
        var info = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
        if (!info.CustomPropertyTable.Contains(key))
            info.CustomPropertyTable.Add(key, value);
        else
            info.CustomPropertyTable[key] = value;
        db.SummaryInfo = info.ToDatabaseSummaryInfo();
    }
    /// <summary>
    /// 获取描述
    /// </summary>
    /// <param name="db"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static object? GetSummaryInfoAtt(this Database db, string key)
    {
        var info = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
        if (info.CustomPropertyTable.Contains(key))
            return info.CustomPropertyTable[key];
        return null;
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