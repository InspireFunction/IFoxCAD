namespace Test;

/*
 * 这里可以把 Xrecord 像json一样实现一条语句就序列化,
 * 只需要属性上面添加XrecordPropertyAttribute特性
 * 
 * 把历史写入主字典,
 * 但是遇到对象删除事件不能开无撤事务,所以纯内存记录历史更好.
 * 因此放弃本文件.
 * 
 */


/// <summary>
/// 用于标记属性在 Xrecord 中存储时使用的 DxfCode 类型
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class XrecordPropertyAttribute : Attribute
{
    /// <summary>
    /// 获取属性的 DxfCode 类型
    /// </summary>
    public DxfCode DxfCode { get; }

    /// <summary>
    /// 获取或设置是否跳过空值（对于 ObjectId，跳过 ObjectId.Null）
    /// </summary>
    public bool SkipNull { get; set; } = true;

    /// <summary>
    /// 初始化 XrecordPropertyAttribute
    /// </summary>
    /// <param name="dxfCode">属性的 DxfCode 类型</param>
    public XrecordPropertyAttribute(DxfCode dxfCode)
    {
        DxfCode = dxfCode;
    }
}

/// <summary>
/// Xrecord 反射序列化帮助类，支持像 JSON 一样简单的序列化/反序列化
/// </summary>
public static class XrecordSerializer
{
    private const string HeaderMarker = "__XRECORD_HEADER__";

    /// <summary>
    /// 将对象序列化到 ResultBuffer，自动处理所有标记和类型转换
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>包含序列化数据的 ResultBuffer</returns>
    public static ResultBuffer Serialize(object obj)
    {
        var rb = new ResultBuffer
        {
            new TypedValue((int)DxfCode.ExtendedDataAsciiString, HeaderMarker)
        };

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attr = Attribute.GetCustomAttribute(prop, typeof(XrecordPropertyAttribute)) as XrecordPropertyAttribute;
            if (attr == null)
                continue;

            var value = prop.GetValue(obj, null);

            if (ShouldSkip(attr, value))
                continue;

            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, prop.Name));
            AppendValue(rb, value, attr.DxfCode);
        }

        return rb;
    }

    /// <summary>
    /// 从 ResultBuffer 反序列化数据到对象，自动过滤头部标记
    /// </summary>
    /// <param name="rb">包含序列化数据的 ResultBuffer</param>
    /// <param name="obj">目标对象</param>
    public static void Deserialize(ResultBuffer? rb, object obj)
    {
        if (rb == null)
            return;

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propDict = new Dictionary<string, (PropertyInfo prop, XrecordPropertyAttribute attr)>();

        foreach (var prop in properties)
        {
            var attrs = prop.GetCustomAttributes(typeof(XrecordPropertyAttribute), false);
            if (attrs != null && attrs.Length > 0)
                propDict[prop.Name] = (prop, (XrecordPropertyAttribute)attrs[0]);
        }

        List<ObjectId> objectIdBuffer = [];
        (PropertyInfo prop, XrecordPropertyAttribute attr)? currentInfo = null;

        ParseResultBuffer(rb, (propName, tv, buffer) => {
            if (!propDict.TryGetValue(propName, out var info))
                return;
            currentInfo = info;
            ProcessValue(tv, obj, info.prop, info.attr, buffer);

        }, (propName, buffer) => {
            if (currentInfo != null)
                FlushObjectIdBuffer(obj, currentInfo.Value.prop, buffer);
        });
    }

    /// <summary>
    /// 从 ResultBuffer 反序列化数据到字典（用于调试显示）
    /// </summary>
    public static Dictionary<string, object?> DeserializeToDictionary(ResultBuffer? rb)
    {
        var result = new Dictionary<string, object?>();
        var objectIdBuffer = new List<ObjectId>();

        ParseResultBuffer(rb, (propName, tv, buffer) => {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataInteger16)
                result[propName] = (short)tv.Value == 1;
            else if (tv.TypeCode == (int)DxfCode.ExtendedDataInteger32)
                result[propName] = (int)tv.Value;
            else if (tv.TypeCode == (int)DxfCode.ExtendedDataReal)
                result[propName] = (double)tv.Value;
            else if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                result[propName] = tv.Value as string;
            else if (tv.TypeCode == (int)DxfCode.SoftPointerId)
                buffer.Add((ObjectId)tv.Value);
        }, (propName, buffer) => {
            if (buffer.Count > 0)
                result[propName] = buffer.Count == 1 ? buffer[0] : buffer.Count;
        });

        return result;
    }

    /// <summary>
    /// 解析 ResultBuffer 的核心方法，通过回调处理每个属性值
    /// </summary>
    /// <param name="rb">要解析的 ResultBuffer</param>
    /// <param name="onValue">处理值的回调，参数为：属性名、TypedValue、ObjectId缓冲区</param>
    /// <param name="onFlush">刷新 ObjectId 缓冲区的回调，参数为：属性名、ObjectId缓冲区</param>
    private static void ParseResultBuffer(
        ResultBuffer? rb,
        Action<string, TypedValue, List<ObjectId>> onValue,
        Action<string, List<ObjectId>> onFlush)
    {
        if (rb == null)
            return;

        string? currentPropName = null;
        List<ObjectId> objectIdBuffer = [];

        foreach (var tv in rb)
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataAsciiString)
            {
                if (currentPropName != null)
                    onFlush(currentPropName, objectIdBuffer);

                string marker = tv.Value as string ?? "";
                if (marker == HeaderMarker)
                {
                    currentPropName = null;
                    objectIdBuffer.Clear();
                    continue;
                }

                currentPropName = marker;
                objectIdBuffer.Clear();
            }
            else if (currentPropName != null)
            {
                onValue(currentPropName, tv, objectIdBuffer);
            }
        }

        if (currentPropName != null)
            onFlush(currentPropName, objectIdBuffer);
    }

    private static bool ShouldSkip(XrecordPropertyAttribute attr, object? value)
    {
        if (!attr.SkipNull)
            return false;

        if (value == null)
            return true;

        if (value is ObjectId oid && oid.IsNull)
            return true;

        return false;
    }

    private static void AppendValue(ResultBuffer rb, object? value, DxfCode dxfCode)
    {
        switch (value)
        {
            case IEnumerable<ObjectId> objectIds:
            foreach (var id in objectIds)
                rb.Add(new TypedValue((int)dxfCode, id));
            break;
            case bool boolValue:
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, boolValue ? (short)1 : (short)0));
            break;
            case int intValue:
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, intValue));
            break;
            case short shortValue:
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataInteger16, shortValue));
            break;
            case string strValue:
            // 使用 ExtendedDataRegAppName 来区分属性名和字符串值
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, strValue));
            break;
            case double doubleValue:
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, doubleValue));
            break;
            case ObjectId objectId:
            rb.Add(new TypedValue((int)dxfCode, objectId));
            break;
            default:
            rb.Add(new TypedValue((int)dxfCode, value));
            break;
        }
    }

    private static void ProcessValue(TypedValue tv, object obj, PropertyInfo prop,
        XrecordPropertyAttribute attr, List<ObjectId> objectIdBuffer)
    {
        var propType = prop.PropertyType;

        if (typeof(IEnumerable<ObjectId>).IsAssignableFrom(propType) && propType != typeof(ObjectId))
        {
            if (tv.TypeCode == (int)attr.DxfCode)
                objectIdBuffer.Add((ObjectId)tv.Value);
        }
        else if (propType == typeof(bool))
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataInteger16)
                prop.SetValue(obj, (short)tv.Value == 1, null);
        }
        else if (propType == typeof(int))
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataInteger32)
                prop.SetValue(obj, (int)tv.Value, null);
        }
        else if (propType == typeof(short))
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataInteger16)
                prop.SetValue(obj, (short)tv.Value, null);
        }
        else if (propType == typeof(string))
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                prop.SetValue(obj, tv.Value as string, null);
        }
        else if (propType == typeof(double))
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataReal)
                prop.SetValue(obj, (double)tv.Value, null);
        }
        else if (propType == typeof(ObjectId))
        {
            if (tv.TypeCode == (int)attr.DxfCode)
                prop.SetValue(obj, (ObjectId)tv.Value, null);
        }
    }

    private static void FlushObjectIdBuffer(object obj, PropertyInfo? prop, List<ObjectId> buffer)
    {
        if (prop == null || buffer.Count == 0)
            return;

        var existingSet = prop.GetValue(obj, null);
        if (existingSet is HashSet<ObjectId> hashSet)
        {
            hashSet.Clear();
            foreach (var id in buffer)
                hashSet.Add(id);
        }
        buffer.Clear();
    }
}
