namespace IFoxCAD.Cad;

/// <summary>
/// json转换器
/// </summary>
public class ObjectIdConverter : MyJsonConverter
{
    const string _id = nameof(ObjectId);

    /// <summary>
    /// 约束类型
    /// </summary>
    public override IEnumerable<Type> SupportedTypes => new[] { typeof(ObjectId) };

    /// <summary>
    /// 序列化_写入json
    /// </summary>
    public override IDictionary<string, object> Serialize(object obj, MyJson serializer)
    {
        if (obj is not ObjectId id)
            return null!;

        Dictionary<string, object> result = new()
        {
            { _id, id == ObjectId.Null ? 0 : id.Handle.Value }
        };
        return result;
    }

    /// <summary>
    /// 反序列化_把字符串生成对象
    /// </summary>
    public override object Deserialize(IDictionary<string, object> dictionary, Type type, MyJson serializer)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (type != typeof(ObjectId))
            return null!;

        ObjectId id = ObjectId.Null;
        try
        {
            if (dictionary.TryGetValue(_id, out object va))
            {
                using var tr = DBTrans.Create();
                id = tr.GetObjectId(va.ToString());
            }
        }
        catch { }
        return id;
    }
}
