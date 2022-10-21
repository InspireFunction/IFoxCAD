namespace IFoxCAD.Cad;

#if NewtonsoftJson
/*
 * 参考 https://www.cnblogs.com/fps2tao/p/14798710.html
 * json类型转换器,使用方法:
 * 在类上面增加此特性:  [JsonConverter(typeof(ObjectIdConverter))]
 */
/// <summary>
/// json转换器
/// </summary>
public class ObjectIdConverter : JsonConverter
{
    /// <summary>
    /// 约束类型
    /// </summary>
    public override bool CanConvert(Type objectType)
    {
        return typeof(ObjectId) == objectType;
    }

    /// <summary>
    /// 反序列化_把字符串生成对象
    /// </summary>
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
            return ObjectId.Null;
        try
        {
            using DBTrans tr = new();
            var id = tr.GetObjectId(reader.Value.ToString());
            return id;
        }
        catch { return ObjectId.Null; }
    }

    /// <summary>
    /// 序列化_写入json
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is ObjectId id)
            writer.WriteValue(id == ObjectId.Null ? 0 : id.Handle.Value);
    }
}
#else
/*
 *  参考 https://developer.aliyun.com/article/51053
 *  json类型转换器,使用方法:
 *  public static string SerializeToJson(object obj)
 *  {
 *      JavaScriptSerializer serializer = new();
 *      serializer.RegisterConverters(new[] { new ObjectIdConverter() });
 *      return serializer.Serialize(obj);
 *  }
 *
 *  public static T DeserializeJson<T>(string jsonString)
 *  {
 *      JavaScriptSerializer serializer = new();
 *      serializer.RegisterConverters(new[] { new ObjectIdConverter() });
 *      return serializer.Deserialize<T>(jsonString);
 *  }
 */
/// <summary>
/// json转换器
/// </summary>
public class ObjectIdConverter : JavaScriptConverter
{
    const string _id = nameof(ObjectId);

    /// <summary>
    /// 约束类型
    /// </summary>
    public override IEnumerable<Type> SupportedTypes => new Type[] { typeof(ObjectId) };

    /// <summary>
    /// 序列化_写入json
    /// </summary>
    public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
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
    public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (type != typeof(ObjectId))
            return null!;

        ObjectId id = new();
        try
        {
            if (dictionary.TryGetValue(_id, out object value))
            {
                using DBTrans tr = new();
                id = tr.GetObjectId(value.ToString());
            }
        }
        catch { return ObjectId.Null; }
        return id;
    }
}
#endif