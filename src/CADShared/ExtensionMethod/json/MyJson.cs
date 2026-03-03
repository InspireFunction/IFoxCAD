#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace IFoxCAD.Cad;

/// <summary>
/// 指示在序列化时应忽略属性或字段
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonIgnoreAttribute : Attribute
{
}

/// <summary>
/// 自定义JSON序列化器，功能类似于Newtonsoft.Json
/// </summary>
public class MyJson
{
    /// <summary>
    /// JSON转换器列表
    /// </summary>
    private List<MyJsonConverter> _converters = [];

    /// <summary>
    /// 是否格式化输出（缩进）
    /// </summary>
    private bool _indent;

    /// <summary>
    /// 换行符选项
    /// </summary>
    private LineEnding _lineEnding = LineEnding.Default;

    /// <summary>
    /// 获取或设置换行符选项
    /// </summary>
    public LineEnding LineEnding
    {
        get => _lineEnding;
        set => _lineEnding = value;
    }

    /// <summary>
    /// 对象到ID的映射，用于处理引用
    /// </summary>
    private readonly Dictionary<object, int> _objectReferences = new Dictionary<object, int>();

    /// <summary>
    /// ID到对象的映射，用于处理引用
    /// </summary>
    private readonly Dictionary<int, object> _idToObject = new Dictionary<int, object>();

    /// <summary>
    /// 引用ID计数器
    /// </summary>
    private int _referenceIdCounter = 1;

    /// <summary>
    /// 用于检测循环引用的已处理对象集合
    /// </summary>
    private readonly HashSet<object> _processedObjects = new HashSet<object>();

    /// <summary>
    /// 获取或设置格式化选项
    /// </summary>
    public Formatting Formatting
    {
        get => _indent ? Formatting.Indented : Formatting.None;
        set => _indent = value == Formatting.Indented;
    }

    /// <summary>
    /// 获取或设置类型名称处理选项
    /// </summary>
    public TypeNameHandling TypeNameHandling { get; set; }

    /// <summary>
    /// 获取或设置引用保留处理选项
    /// </summary>
    public PreserveReferencesHandling PreserveReferencesHandling { get; set; }

    /// <summary>
    /// 获取或设置循环引用处理选项
    /// </summary>
    public ReferenceLoopHandling ReferenceLoopHandling { get; set; }

    /// <summary>
    /// 将对象序列化为JSON字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="formatting">格式化选项</param>
    /// <returns>JSON字符串</returns>
    public static string SerializeObject<T>(T? obj, Formatting formatting = Formatting.None)
    {
        var serializeSettings = new MyJsonSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = formatting
        };
        return SerializeObject(obj, serializeSettings);
    }

    /// <summary>
    /// 使用指定设置将对象序列化为JSON字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="settings">序列化设置</param>
    /// <returns>JSON字符串</returns>
    public static string SerializeObject<T>(T? obj, MyJsonSettings settings)
    {
        var json = new MyJson();
        json.Formatting = settings.Formatting;
        json.TypeNameHandling = settings.TypeNameHandling;
        json.PreserveReferencesHandling = settings.PreserveReferencesHandling;
        json.ReferenceLoopHandling = settings.ReferenceLoopHandling;
        json.LineEnding = settings.LineEnding;
        if (settings.Converters != null)
        {
            foreach (var converter in settings.Converters)
                json.RegisterConverters(new[] { converter });
        }
        return json.Serialize(obj);
    }

    /// <summary>
    /// 反序列化JSON字符串为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <param name="json">JSON字符串</param>
    /// <returns>反序列化后的对象</returns>
    public static T? DeserializeObject<T>(string json)
    {
        return DeserializeObject<T>(json, new MyJsonSettings());
    }

    /// <summary>
    /// 使用指定设置反序列化JSON字符串为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <param name="json">JSON字符串</param>
    /// <param name="settings">反序列化设置</param>
    /// <returns>反序列化后的对象</returns>
    public static T? DeserializeObject<T>(string json, MyJsonSettings settings)
    {
        var jsonSerializer = new MyJson();
        jsonSerializer.Formatting = settings.Formatting;
        jsonSerializer.TypeNameHandling = settings.TypeNameHandling;
        jsonSerializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
        if (settings.Converters != null)
        {
            foreach (var converter in settings.Converters)
                jsonSerializer.RegisterConverters(new[] { converter });
        }
        return jsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// 注册JSON转换器
    /// </summary>
    /// <param name="converters">转换器集合</param>
    public void RegisterConverters(IEnumerable<MyJsonConverter> converters)
    {
        _converters.AddRange(converters);
    }

    /// <summary>
    /// 序列化对象为JSON字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>JSON字符串</returns>
    public string Serialize<T>(T? obj)
    {
        if (obj == null)
            return "null";

        // 清理引用跟踪状态
        _objectReferences.Clear();
        _idToObject.Clear();
        _referenceIdCounter = 1;
        _processedObjects.Clear();

        var sb = new StringBuilder();
        SerializeValue<T>(obj, sb, 0);
        return sb.ToString();
    }

    /// <summary>
    /// 创建对象实例 - 支持无参构造函数和没有无参构造函数的类
    /// </summary>
    /// <param name="type">要创建的类型</param>
    /// <returns>创建的对象实例，如果失败则返回null</returns>
    //private object? CreateInstance(Type type)
    //{
    //    try
    //    {
    //        // 这种方式会先跑异常,导致我控制台会卡一会.
    //        // 但是1是List必须要的,不可以直接去2
    //        // 1,首先尝试使用 Activator.CreateInstance（需要无参构造函数）
    //        return Activator.CreateInstance(type);
    //    }
    //    catch (MissingMethodException)
    //    {
    //        try
    //        {
    //            // 2,如果没有无参构造函数，使用 FormatterServices 创建未初始化的对象
    //            return FormatterServices.GetUninitializedObject(type);
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }
    //}

    private object? CreateInstance(Type type)
    {
        // 值类型直接创建（包括结构体）
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        // 检查是否有无参构造函数
        var defaultConstructor = type.GetConstructor(Type.EmptyTypes);

        if (defaultConstructor != null)
        {
            // 有无参构造函数，直接创建
            return Activator.CreateInstance(type);
        }

        // 没有无参构造函数，使用 FormatterServices 创建未初始化对象
        try
        {
            return FormatterServices.GetUninitializedObject(type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 判断类型是否应该保留引用
    /// </summary>
    /// <param name="type">要检查的类型</param>
    /// <returns>是否应该保留引用</returns>
    private bool ShouldPreserveReferences(Type type)
    {
        if (PreserveReferencesHandling == PreserveReferencesHandling.None)
            return false;

        if (PreserveReferencesHandling == PreserveReferencesHandling.All)
            return true;

        if (PreserveReferencesHandling == PreserveReferencesHandling.Objects)
            return type.IsClass && type != typeof(string);

        if (PreserveReferencesHandling == PreserveReferencesHandling.Arrays)
            return type.IsArray || (type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(List<>) ||
                 type.GetGenericTypeDefinition() == typeof(IList<>)));

        return false;
    }

    /// <summary>
    /// 获取缩进字符串
    /// </summary>
    /// <param name="indent">缩进级别</param>
    /// <returns>缩进字符串</returns>
    private string GetIndentString(int indent)
    {
        return _indent ? new string(' ', indent * 2) : string.Empty; // 使用2个空格缩进，与Newtonsoft.Json保持一致
    }

    /// <summary>
    /// 获取换行符字符串
    /// </summary>
    /// <returns>换行符字符串</returns>
    private string GetLineEnding()
    {
        return _lineEnding switch
        {
            LineEnding.LF => "\n",
            LineEnding.CRLF => "\r\n",
            _ => Environment.NewLine // Default
        };
    }

    /// <summary>
    /// 追加换行符到 StringBuilder
    /// </summary>
    /// <param name="sb">StringBuilder实例</param>
    private void AppendLine(StringBuilder sb)
    {
        sb.Append(GetLineEnding());
    }

    /// <summary>
    /// 序列化值到StringBuilder
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="obj">要序列化的值</param>
    /// <param name="sb">StringBuilder实例</param>
    /// <param name="indent">缩进级别</param>
    private void SerializeValue<T>(T obj, StringBuilder sb, int indent)
    {
        if (obj == null)
        {
            sb.Append("null");
            return;
        }

        Type type = obj.GetType();

        var converter = _converters.FirstOrDefault(c => c.SupportedTypes.Contains(type));
        if (converter != null)
        {
            var dict = converter.Serialize(obj, this);
            SerializeDictionary(dict, sb, indent);
            return;
        }

        if (obj == null)
        {
            sb.Append("null");
            return;
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime))
        {
            SerializePrimitive(obj, sb);
        }
        else if (type.IsEnum)
        {
            // 默认将枚举序列化为数字，与Newtonsoft.Json保持一致
            sb.Append(Convert.ChangeType(obj, Enum.GetUnderlyingType(type)).ToString());
        }
        else if (obj is Array array)
        {
            SerializeArray(array, sb, indent);
        }
        else if (obj is IDictionary dict)
        {
            SerializeDictionary(dict, sb, indent);
        }
        else if (obj is IEnumerable enumerable && !(obj is string))
        {
            SerializeEnumerable(enumerable, sb, indent);
        }
        else if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
        {
            // 处理结构体（struct），包括自定义结构体
            SerializeObject(obj, sb, indent);
        }
        else if (type.IsClass)
        {
            SerializeObject(obj, sb, indent);
        }
        else
        {
            sb.Append(obj.ToString());
        }
    }

    /// <summary>
    /// 序列化基本类型（字符串、布尔值、数字等）
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="sb">StringBuilder实例</param>
    private void SerializePrimitive(object obj, StringBuilder sb)
    {
        if (obj is string str)
        {
            sb.Append($"\"{EscapeString(str)}\"");
        }
        else if (obj is bool b)
        {
            sb.Append(b ? "true" : "false");
        }
        else if (obj is double d)
        {
            // 保留小数点表示，与Newtonsoft.Json保持一致
            if (d == Math.Floor(d))
                sb.Append(d.ToString("R"));
            else
                sb.Append(d.ToString("R"));
        }
        else if (obj is float f)
        {
            // 保留小数点表示，与Newtonsoft.Json保持一致
            if (f == Math.Floor(f))
                sb.Append(f.ToString("R"));
            else
                sb.Append(f.ToString("R"));
        }
        else if (obj is decimal dec)
        {
            // 保留小数点表示，与Newtonsoft.Json保持一致
            if (dec == Math.Floor(dec))
                sb.Append(dec.ToString());
            else
                sb.Append(dec.ToString());
        }
        else
        {
            sb.Append(obj.ToString());
        }
    }

    /// <summary>
    /// 转义字符串中的特殊字符
    /// </summary>
    /// <param name="str">要转义的字符串</param>
    /// <returns>转义后的字符串</returns>
    private string EscapeString(string str)
    {
        return str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// 序列化数组
    /// </summary>
    /// <param name="array">要序列化的数组</param>
    /// <param name="sb">StringBuilder实例</param>
    /// <param name="indent">缩进级别</param>
    private void SerializeArray(Array array, StringBuilder sb, int indent)
    {
        // 处理数组引用
        if (ShouldPreserveReferences(array.GetType()))
        {
            if (_objectReferences.TryGetValue(array, out int existingId))
            {
                sb.Append($"{{\"$ref\":\"{existingId}\"}}");
                return;
            }

            int newId = _referenceIdCounter++;
            _objectReferences[array] = newId;

            sb.Append($"{{\"$id\":\"{newId}\",\"$values\":[");

            if (array.Length > 0)
            {
                if (_indent)
                {
                    AppendLine(sb);
                    int newIndent = indent + 1;
                    for (int i = 0; i < array.Length; i++)
                    {
                        sb.Append(GetIndentString(newIndent));
                        SerializeValue(array.GetValue(i), sb, newIndent);
                        if (i < array.Length - 1)
                        {
                            sb.Append(',');
                        }
                        AppendLine(sb);
                    }
                    sb.Append(GetIndentString(indent));
                }
                else
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        SerializeValue(array.GetValue(i), sb, indent);
                    }
                }
            }

            sb.Append("]}");
            _objectReferences.Remove(array);
        }
        else
        {
            // 普通数组序列化
            sb.Append('[');
            if (array.Length > 0)
            {
                if (_indent)
                {
                    AppendLine(sb);
                    int newIndent = indent + 1;
                    for (int i = 0; i < array.Length; i++)
                    {
                        sb.Append(GetIndentString(newIndent));
                        SerializeValue(array.GetValue(i), sb, newIndent);
                        if (i < array.Length - 1)
                        {
                            sb.Append(',');
                        }
                        AppendLine(sb);
                    }
                    sb.Append(GetIndentString(indent));
                }
                else
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        SerializeValue(array.GetValue(i), sb, indent);
                    }
                }
            }

            sb.Append(']');
        }
    }

    private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb, int indent)
    {
        // 处理集合引用
        if (ShouldPreserveReferences(enumerable.GetType()))
        {
            if (_objectReferences.TryGetValue(enumerable, out int existingId))
            {
                sb.Append($"{{\"$ref\":\"{existingId}\"}}");
                return;
            }

            int newId = _referenceIdCounter++;
            _objectReferences[enumerable] = newId;

            sb.Append($"{{\"$id\":\"{newId}\",\"$values\":[");

            bool first = true;
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }

            if (items.Count > 0)
            {
                if (_indent)
                {
                    AppendLine(sb);
                    int newIndent = indent + 1;
                    for (int i = 0; i < items.Count; i++)
                    {
                        sb.Append(GetIndentString(newIndent));
                        SerializeValue(items[i], sb, newIndent);
                        if (i < items.Count - 1)
                        {
                            sb.Append(',');
                        }
                        AppendLine(sb);
                    }
                    sb.Append(GetIndentString(indent));
                }
                else
                {
                    foreach (var item in items)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        SerializeValue(item, sb, indent);
                    }
                }
            }

            sb.Append("]}");
            _objectReferences.Remove(enumerable);
        }
        else
        {
            // 普通集合序列化
            sb.Append('[');
            bool first = true;

            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }

            if (items.Count > 0)
            {
                if (_indent)
                {
                    AppendLine(sb);
                    int newIndent = indent + 1;
                    for (int i = 0; i < items.Count; i++)
                    {
                        sb.Append(GetIndentString(newIndent));
                        SerializeValue(items[i], sb, newIndent);
                        if (i < items.Count - 1)
                        {
                            sb.Append(',');
                        }
                        AppendLine(sb);
                    }
                    sb.Append(GetIndentString(indent));
                }
                else
                {
                    foreach (var item in items)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        SerializeValue(item, sb, indent);
                    }
                }
            }
            sb.Append(']');
        }
    }

    private void SerializeDictionary(IDictionary dict, StringBuilder sb, int indent)
    {
        sb.Append('{');
        var keys = dict.Keys.Cast<object>().ToList();
        bool first = true;
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (!first)
            {
                if (_indent)
                {
                    sb.Append(',');
                    AppendLine(sb);
                    sb.Append(GetIndentString(indent + 1));
                }
                else
                {
                    sb.Append(',');
                    sb.Append(' ');
                }
            }
            else if (_indent)
            {
                AppendLine(sb);
                sb.Append(GetIndentString(indent + 1));
            }
            first = false;
            sb.Append(_indent ? "\"" + EscapeString(key.ToString() ?? "") + "\": " : "\"" + EscapeString(key.ToString() ?? "") + "\":");
            SerializeValue(dict[key]!, sb, indent + 1);
        }
        if (keys.Count > 0 && _indent)
        {
            AppendLine(sb);
            sb.Append(GetIndentString(indent));
        }
        sb.Append('}');
    }

    private void SerializeDictionary(IDictionary<string, object> dict, StringBuilder sb, int indent)
    {
        sb.Append('{');
        var keys = dict.Keys.ToList();
        bool first = true;
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (!first)
            {
                if (_indent)
                {
                    sb.Append(',');
                    AppendLine(sb);
                    sb.Append(GetIndentString(indent + 1));
                }
                else
                {
                    sb.Append(',');
                    sb.Append(' ');
                }
            }
            else if (_indent)
            {
                AppendLine(sb);
                sb.Append(GetIndentString(indent + 1));
            }
            first = false;
            sb.Append(_indent ? "\"" + EscapeString(key.ToString() ?? "") + "\": " : "\"" + EscapeString(key.ToString() ?? "") + "\":");
            SerializeValue(dict[key], sb, indent + 1);
        }
        if (keys.Count > 0 && _indent)
        {
            AppendLine(sb);
            sb.Append(GetIndentString(indent));
        }
        sb.Append('}');
    }


    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="sb">StringBuilder实例</param>
    /// <param name="indent">缩进级别</param>
    private void SerializeObject(object obj, StringBuilder sb, int indent)
    {
        Type objType = obj.GetType();

        // 检查是否是循环引用
        if (_processedObjects.Contains(obj))
        {
            // 根据ReferenceLoopHandling设置处理循环引用
            switch (ReferenceLoopHandling)
            {
                case ReferenceLoopHandling.Error:
                throw new InvalidOperationException("发现循环引用");
                case ReferenceLoopHandling.Ignore:
                // 忽略循环引用，输出空对象
                sb.Append("{}");
                return;
                case ReferenceLoopHandling.Serialize:
                // 继续序列化，使用引用机制
                break;
            }
        }

        // 将对象添加到已处理集合
        _processedObjects.Add(obj);

        // 处理引用保留
        if (ShouldPreserveReferences(objType))
        {
            if (_objectReferences.TryGetValue(obj, out int existingId))
            {
                // 已经序列化过，输出引用
                sb.Append($"{{\"$ref\":\"{existingId}\"}}");
                // 从已处理对象集合中移除
                _processedObjects.Remove(obj);
                return;
            }

            // 新对象，分配ID并注册
            int newId = _referenceIdCounter++;
            _objectReferences[obj] = newId;

            // 输出$id并继续序列化
            sb.Append('{');

            if (_indent)
            {
                AppendLine(sb);
                int newIndent = indent + 1;
                sb.Append(GetIndentString(newIndent));
                sb.Append($"\"$id\":\"{newId}\",");
                AppendLine(sb);

                bool first = true;
                SerializeFields(obj, sb, newIndent, ref first);

                AppendLine(sb);
                sb.Append(GetIndentString(indent));
            }
            else
            {
                sb.Append($"\"$id\":\"{newId}\",");

                bool first = true;
                SerializeFields(obj, sb, indent, ref first);
            }

            sb.Append('}');

            // 从已处理对象集合中移除
            _processedObjects.Remove(obj);
            return;
        }

        // 普通对象序列化
        sb.Append('{');

        bool first2 = true;
        int newIndent2 = indent + 1;

        if (TypeNameHandling == TypeNameHandling.Auto)
        {
            if (_indent)
            {
                AppendLine(sb);
                sb.Append(GetIndentString(newIndent2));
                sb.Append($"\"$type\":\"{obj.GetType().FullName}\",");
                AppendLine(sb);
                sb.Append(GetIndentString(newIndent2));
                SerializeFields(obj, sb, newIndent2, ref first2);
                AppendLine(sb);
                sb.Append(GetIndentString(indent));
            }
            else
            {
                sb.Append($"\"$type\":\"{obj.GetType().FullName}\",");
                SerializeFields(obj, sb, indent, ref first2);
            }
        }
        else
        {
            if (_indent)
            {
                AppendLine(sb);
                sb.Append(GetIndentString(newIndent2));
                SerializeFields(obj, sb, newIndent2, ref first2);
                AppendLine(sb);
                sb.Append(GetIndentString(indent));
            }
            else
            {
                SerializeFields(obj, sb, indent, ref first2);
            }
        }
        sb.Append('}');

        // 从已处理对象集合中移除
        _processedObjects.Remove(obj);
    }

    /// <summary>
    /// 序列化对象的字段和属性
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="sb">StringBuilder实例</param>
    /// <param name="indent">缩进级别</param>
    /// <param name="first">是否为第一个元素</param>
    private void SerializeFields(object obj, StringBuilder sb, int indent, ref bool first)
    {
        Type type = obj.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            // 检查是否有JsonIgnore特性
            if (field.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0)
                continue;

            if (!first && _indent)
            {
                sb.Append(',');
                AppendLine(sb);
                sb.Append(GetIndentString(indent));
            }
            else if (!first && !_indent)
            {
                sb.Append(',');
            }
            first = false;
            sb.Append(_indent ? $"\"{field.Name}\": " : $"\"{field.Name}\":");
            object? value = field.GetValue(obj);
            SerializeValue(value, sb, indent);
        }

        foreach (var prop in properties)
        {
            // 检查是否是索引器属性（有参数的属性）
            var indexParams = prop.GetIndexParameters();
            if (indexParams.Length > 0)
            {
                // 跳过索引器属性
                continue;
            }

            // 检查是否有JsonIgnore特性
            if (prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0)
                continue;

            if (!first && _indent)
            {
                sb.Append(',');
                AppendLine(sb);
                sb.Append(GetIndentString(indent));
            }
            else if (!first && !_indent)
            {
                sb.Append(',');
            }
            first = false;
            sb.Append(_indent ? $"\"{prop.Name}\": " : $"\"{prop.Name}\":");
            object? value;
            try
            {
                // obj 类型 { System.Collections.Hashtable.HashtableEnumerator}
                // System.InvalidOperationException:“枚举尚未开始。调用 MoveNext。”
                value = prop.GetValue(obj, null);
            }
            catch
            {
                value = null;
                Debugger.Break();
            }
            SerializeValue(value, sb, indent);
        }
    }

    /// <summary>
    /// 反序列化JSON字符串为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <param name="json">JSON字符串</param>
    /// <returns>反序列化后的对象</returns>
    public T? Deserialize<T>(string json)
    {
        // 清理引用跟踪状态
        _objectReferences.Clear();
        _idToObject.Clear();
        _referenceIdCounter = 1;
        _processedObjects.Clear();

        using var reader = new StringReader(json);
        return DeserializeValue<T>(reader);
    }

    /// <summary>
    /// 从文本读取器反序列化值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">文本读取器</param>
    /// <returns>反序列化后的值</returns>
    private T? DeserializeValue<T>(TextReader reader)
    {
        var token = ReadToken(reader);
        if (token == null)
            return default;

        return (T)DeserializeToken(token, typeof(T))!;
    }

    /// <summary>
    /// 反序列化令牌为指定类型的对象
    /// </summary>
    /// <param name="token">令牌</param>
    /// <param name="targetType">目标类型</param>
    /// <returns>反序列化后的对象</returns>
    private object? DeserializeToken(Token token, Type targetType)
    {
        if (token.Type == TokenType.Null)
            return null;

        // 处理$ref引用
        if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> dict)
        {
            if (dict.TryGetValue("$ref", out var refValue) && refValue != null)
            {
                int refId = Convert.ToInt32(refValue.ToString());
                if (_idToObject.TryGetValue(refId, out var referencedObject))
                {
                    return referencedObject;
                }
                return null; // 找不到引用的对象
            }
        }

        if (targetType == typeof(string))
        {
            return token.Value?.ToString();
        }

        if (targetType == typeof(int))
            return Convert.ToInt32(token.Value);

        if (targetType == typeof(long))
            return Convert.ToInt64(token.Value);

        if (targetType == typeof(float))
            return Convert.ToSingle(token.Value);

        if (targetType == typeof(double))
            return Convert.ToDouble(token.Value);

        if (targetType == typeof(bool))
            return Convert.ToBoolean(token.Value);

        if (targetType == typeof(decimal))
            return Convert.ToDecimal(token.Value);

        if (targetType == typeof(DateTime))
        {
            if (token.Value is string dateString)
                return DateTime.Parse(dateString);
            else
                return DateTime.MinValue;
        }

        if (targetType.IsEnum)
            return Enum.Parse(targetType, token.Value?.ToString() ?? "");

        // 特殊处理：Dictionary<string, object> - 直接返回
        if (targetType == typeof(Dictionary<string, object>))
        {
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object>)
            {
                return token.Value;
            }
            return new Dictionary<string, object>();
        }

        // 处理数组类型
        if (targetType.IsArray)
        {
            if (token.Type == TokenType.Array && token.Value is List<object> arrList)
            {
                return DeserializeArray(arrList, targetType);
            }
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> arrDict)
            {
                if (arrDict.TryGetValue("$values", out var valuesValue) && valuesValue is List<object> valuesList)
                {
                    return DeserializeArray(valuesList, targetType);
                }
            }
            Type elementType = targetType.GetElementType()!;
            return Array.CreateInstance(elementType, 0);
        }

        // 处理 Dictionary<TKey, TValue> - 需要类型转换
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> dictValue)
            {
                return DeserializeDictionary(dictValue, targetType);
            }
            return CreateInstance(targetType);
        }

        // 特殊处理：List<object> - 直接返回
        if (targetType == typeof(List<object>))
        {
            if (token.Type == TokenType.Array && token.Value is List<object>)
            {
                return token.Value;
            }
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> listDict)
            {
                if (listDict.TryGetValue("$values", out var values) && values is List<object> valuesList)
                {
                    return valuesList;
                }
            }
            return new List<object>();
        }

        // 处理 List<T> 等泛型集合 - 需要类型转换
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            if (token.Type == TokenType.Array && token.Value is List<object>)
            {
                return DeserializeArray(token.Value as List<object>, targetType);
            }
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> genericListDict)
            {
                if (genericListDict.TryGetValue("$values", out var genericValues) && genericValues is List<object> genericValuesList)
                {
                    return DeserializeArray(genericValuesList, targetType);
                }
            }
            return CreateInstance(targetType);
        }

        var converter = _converters.FirstOrDefault(c => c.SupportedTypes.Contains(targetType));
        if (converter != null && token.Type == TokenType.Object)
        {
            return converter.Deserialize((Dictionary<string, object>)token.Value!, targetType, this);
        }

        if (token.Type == TokenType.Object)
        {
            // TODO 这样执行为什么有大量的异常
            // 引发的异常:“System.MissingMethodException”(位于 mscorlib.dll 中)
            return DeserializeObject(token.Value as Dictionary<string, object>, targetType);
        }

        if (token.Type == TokenType.Array)
        {
            return DeserializeArray(token.Value as List<object>, targetType);
        }

        // 处理带$id的数组或集合
        if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> objDict)
        {
            if (objDict.TryGetValue("$id", out var idValue) && objDict.TryGetValue("$values", out var valuesValue))
            {
                int arrayId = Convert.ToInt32(idValue.ToString());
                var valuesList = valuesValue as List<object>;

                if (targetType.IsArray)
                {
                    Type elementType = targetType.GetElementType()!;
                    var array = Array.CreateInstance(elementType, valuesList?.Count ?? 0);

                    if (valuesList != null)
                    {
                        for (int i = 0; i < valuesList.Count; i++)
                        {
                            array.SetValue(DeserializeToken(new Token { Type = GetTokenType(valuesList[i]), Value = valuesList[i] }, elementType), i);
                        }
                    }

                    _idToObject[arrayId] = array;
                    return array;
                }
                else if (targetType.IsGenericType)
                {
                    var genericArgs = targetType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        Type elementType = genericArgs[0];
                        var listType = typeof(List<>).MakeGenericType(elementType);
                        var result = CreateInstance(listType);
                        var addMethod = listType.GetMethod("Add");

                        if (valuesList != null && result != null && addMethod != null)
                        {
                            foreach (var item in valuesList)
                            {
                                var token2 = new Token { Type = GetTokenType(item), Value = item };
                                var deserializedItem = DeserializeToken(token2, elementType);
                                var convertedItem = ConvertToType(deserializedItem, elementType);

                                if (convertedItem != null || elementType.IsClass || Nullable.GetUnderlyingType(elementType) != null)
                                {
                                    addMethod.Invoke(result, new[] { convertedItem });
                                }
                            }
                        }

                        _idToObject[arrayId] = result!;
                        return result;
                    }
                }
            }
        }

        return token.Value;
    }

    /// <summary>
    /// 反序列化字典为对象
    /// </summary>
    /// <param name="dict">键值对字典</param>
    /// <param name="type">目标类型</param>
    /// <returns>反序列化后的对象</returns>
    private object? DeserializeObject(Dictionary<string, object>? dict, Type type)
    {
        if (dict == null)
            return null;

        // 处理$id引用
        int objectId = 0;
        if (dict.TryGetValue("$id", out var idValue) && idValue != null)
        {
            objectId = Convert.ToInt32(idValue.ToString());
            dict.Remove("$id");
        }

        if (TypeNameHandling == TypeNameHandling.Auto && dict.TryGetValue("$type", out var typeName))
        {
            var fullName = typeName?.ToString();
            if (!string.IsNullOrEmpty(fullName))
            {
                var actualType = Type.GetType(fullName);
                if (actualType != null)
                {
                    type = actualType;
                }
            }
            dict.Remove("$type");
        }

        var obj = CreateInstance(type);
        if (obj == null)
            return null;

        // 注册对象引用（如果有ID）
        if (objectId > 0)
        {
            _idToObject[objectId] = obj;
        }

        //Console.Error.WriteLine($"[MyJson] 开始反序列化对象，类型: {type.FullName}, 字段数: {dict.Count}");

        foreach (var kvp in dict)
        {
            var field = type.GetField(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                var value = DeserializeToken(new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value }, field.FieldType);
                value = ConvertToType(value, field.FieldType);
                field.SetValue(obj, value);
                //Console.Error.WriteLine($"[MyJson] 设置字段 {kvp.Key} (Field)");
                continue;
            }

            var prop = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                var value = DeserializeToken(new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value }, prop.PropertyType);
                value = ConvertToPropertyType(value, prop.PropertyType);
                prop.SetValue(obj, value, null);
                //Console.Error.WriteLine($"[MyJson] 设置属性 {kvp.Key} (Property)");
                continue;
            }

            // 如果找不到字段或属性，尝试忽略大小写
            prop = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite)
            {
                var value = DeserializeToken(new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value }, prop.PropertyType);
                value = ConvertToPropertyType(value, prop.PropertyType);
                prop.SetValue(obj, value, null);
                //Console.Error.WriteLine($"[MyJson] 设置属性 {kvp.Key} (Property, IgnoreCase)");
                continue;
            }

            // 如果还是找不到，尝试包含所有标志
            prop = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
            if (prop != null && prop.CanWrite)
            {
                var value = DeserializeToken(new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value }, prop.PropertyType);
                value = ConvertToPropertyType(value, prop.PropertyType);
                prop.SetValue(obj, value, null);
                //Console.Error.WriteLine($"[MyJson] 设置属性 {kvp.Key} (Property, IgnoreCase+FlattenHierarchy)");
                continue;
            }

            //Console.Error.WriteLine($"[MyJson] 警告: 找不到字段或属性 {kvp.Key}");
        }

        return obj;
    }

    /// <summary>
    /// 反序列化数组
    /// </summary>
    /// <param name="list">对象列表</param>
    /// <param name="type">目标类型</param>
    /// <returns>反序列化后的数组</returns>
    private object? DeserializeArray(List<object>? list, Type type)
    {
        if (list == null)
            return null;

        Type elementType;
        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            var array = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                array.SetValue(DeserializeToken(new Token { Type = GetTokenType(list[i]), Value = list[i] }, elementType), i);
            }
            return array;
        }
        else
        {
            // 处理List<T>等泛型集合
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                elementType = genericArgs[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var result = CreateInstance(listType);
                if (result == null)
                    return null;
                var addMethod = listType.GetMethod("Add");
                foreach (var item in list)
                {
                    var token = new Token { Type = GetTokenType(item), Value = item };
                    var rawValue = DeserializeToken(token, elementType);
                    var convertedValue = ConvertToType(rawValue, elementType);

                    if (convertedValue != null || elementType.IsClass || Nullable.GetUnderlyingType(elementType) != null)
                    {
                        addMethod?.Invoke(result, new[] { convertedValue });
                    }
                }
                return result;
            }
        }

        return list;
    }

    /// <summary>
    /// 反序列化字典
    /// </summary>
    /// <param name="dict">源字典</param>
    /// <param name="type">目标类型</param>
    /// <returns>反序列化后的字典</returns>
    private object? DeserializeDictionary(Dictionary<string, object>? dict, Type type)
    {
        if (dict == null)
            return null;

        var genericArgs = type.GetGenericArguments();
        if (genericArgs.Length != 2)
            return dict;

        Type keyType = genericArgs[0];
        Type valueType = genericArgs[1];

        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var result = CreateInstance(dictType);
        if (result == null)
            return null;

        var addMethod = dictType.GetMethod("Add");
        if (addMethod == null)
            return result;

        foreach (var kvp in dict)
        {
            var key = ConvertToType(kvp.Key, keyType);
            var valueToken = new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value };
            var value = DeserializeToken(valueToken, valueType);
            value = ConvertToType(value, valueType);

            if (key != null || keyType.IsClass)
            {
                addMethod.Invoke(result, new[] { key, value });
            }
        }

        return result;
    }

    private object? ConvertToType(object? value, Type targetType)
    {
        if (value == null)
        {
            if (targetType.IsClass || Nullable.GetUnderlyingType(targetType) != null)
                return null;
            else
                return CreateInstance(targetType); // 值类型的默认值
        }

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        try
        {
            // 处理字符串到数字类型的转换
            if (value is string strValue)
            {
                if (targetType == typeof(byte))
                    return Convert.ToByte(strValue);
                if (targetType == typeof(short))
                    return Convert.ToInt16(strValue);
                if (targetType == typeof(ushort))
                    return Convert.ToUInt16(strValue);
                if (targetType == typeof(int))
                    return Convert.ToInt32(strValue);
                if (targetType == typeof(uint))
                    return Convert.ToUInt32(strValue);
                if (targetType == typeof(long))
                    return Convert.ToInt64(strValue);
                if (targetType == typeof(ulong))
                    return Convert.ToUInt64(strValue);
                if (targetType == typeof(float))
                    return Convert.ToSingle(strValue);
                if (targetType == typeof(double))
                    return Convert.ToDouble(strValue);
                if (targetType == typeof(decimal))
                    return Convert.ToDecimal(strValue);
                if (targetType == typeof(bool))
                    return Convert.ToBoolean(strValue);
            }

            // 处理数字类型转换 - 使用 Convert 类进行安全转换
            if (IsNumericType(targetType))
            {
                return ConvertToNumericType(value, targetType);
            }

            // 通用转换
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            // 返回目标类型的默认值
            if (targetType.IsValueType)
                return CreateInstance(targetType);
            return null;
        }
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }

    private static object ConvertToNumericType(object value, Type targetType)
    {
        // 先转换为 decimal 作为中间类型，避免精度丢失
        decimal decimalValue;
        if (value is int i) decimalValue = i;
        else if (value is long l) decimalValue = l;
        else if (value is short s) decimalValue = s;
        else if (value is byte b) decimalValue = b;
        else if (value is float f) decimalValue = (decimal)f;
        else if (value is double d) decimalValue = (decimal)d;
        else if (value is decimal dec) decimalValue = dec;
        else if (value is ushort us) decimalValue = us;
        else if (value is uint ui) decimalValue = ui;
        else if (value is ulong ul) decimalValue = ul;
        else if (value is sbyte sb) decimalValue = sb;
        else decimalValue = Convert.ToDecimal(value);

        if (targetType == typeof(byte)) return (byte)decimalValue;
        if (targetType == typeof(sbyte)) return (sbyte)decimalValue;
        if (targetType == typeof(short)) return (short)decimalValue;
        if (targetType == typeof(ushort)) return (ushort)decimalValue;
        if (targetType == typeof(int)) return (int)decimalValue;
        if (targetType == typeof(uint)) return (uint)decimalValue;
        if (targetType == typeof(long)) return (long)decimalValue;
        if (targetType == typeof(ulong)) return (ulong)decimalValue;
        if (targetType == typeof(float)) return (float)decimalValue;
        if (targetType == typeof(double)) return (double)decimalValue;
        if (targetType == typeof(decimal)) return decimalValue;

        return Convert.ChangeType(decimalValue, targetType);
    }


    private object? ConvertToPropertyType(object? value, Type propertyType)
    {
        if (value == null)
            return null;

        if (propertyType.IsAssignableFrom(value.GetType()))
            return value;

        // 处理 List<object> 转换为具体类型的 List<T> 或集合接口
        if (value is List<object> listObj && propertyType.IsGenericType)
        {
            var genericDef = propertyType.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) ||
                genericDef == typeof(IEnumerable<>) || genericDef == typeof(ICollection<>))
            {
                var elementType = propertyType.GetGenericArguments()[0];
                var targetListType = typeof(List<>).MakeGenericType(elementType);
                var targetList = Activator.CreateInstance(targetListType);
                var addMethod = targetListType.GetMethod("Add");

                if (targetList != null && addMethod != null)
                {
                    foreach (var item in listObj)
                    {
                        var convertedItem = ConvertToType(item, elementType);
                        if (convertedItem != null || elementType.IsClass || Nullable.GetUnderlyingType(elementType) != null)
                        {
                            addMethod.Invoke(targetList, new[] { convertedItem });
                        }
                    }
                }
                return targetList;
            }
        }

        // 处理数组类型转换
        if (value is List<object> listObj2 && propertyType.IsArray)
        {
            var elementType = propertyType.GetElementType()!;
            var array = Array.CreateInstance(elementType, listObj2.Count);
            for (int i = 0; i < listObj2.Count; i++)
            {
                array.SetValue(ConvertToType(listObj2[i], elementType), i);
            }
            return array;
        }

        // 尝试使用 ConvertToType 进行普通转换
        return ConvertToType(value, propertyType);
    }


    private TokenType GetTokenType(object value)
    {
        if (value == null)
            return TokenType.Null;
        if (value is Dictionary<string, object>)
            return TokenType.Object;
        if (value is List<object>)
            return TokenType.Array;
        if (value is string)
            return TokenType.String;
        if (value is bool)
            return TokenType.Boolean;
        if (value is int || value is long || value is short || value is byte)
            return TokenType.Number;
        if (value is double || value is float || value is decimal)
            return TokenType.Number;
        return TokenType.String;
    }

    private Token? ReadToken(TextReader reader)
    {
        SkipWhitespace(reader);

        int ch = reader.Peek();
        if (ch == -1)
            return null;

        if (ch == 'n') // null
        {
            ReadString(reader, 4);
            return new Token { Type = TokenType.Null, Value = null };
        }

        if (ch == 't') // true
        {
            ReadString(reader, 4);
            return new Token { Type = TokenType.Boolean, Value = true };
        }

        if (ch == 'f') // false
        {
            ReadString(reader, 5);
            return new Token { Type = TokenType.Boolean, Value = false };
        }

        if (ch == '"') // string
        {
            return new Token { Type = TokenType.String, Value = ReadString(reader) };
        }

        if (ch == '[') // array
        {
            reader.Read();
            var list = new List<object>();
            SkipWhitespace(reader);

            if (reader.Peek() == ']')
            {
                reader.Read();
                return new Token { Type = TokenType.Array, Value = list };
            }

            while (true)
            {
                var token = ReadToken(reader);
                if (token != null)
                    list.Add(token.Value!);

                SkipWhitespace(reader);
                ch = reader.Read();
                if (ch == ']')
                    break;
                if (ch != ',')
                    throw new Exception("MyJsonExpected , or ]");
                SkipWhitespace(reader);
            }
            return new Token { Type = TokenType.Array, Value = list };
        }

        if (ch == '{') // object
        {
            reader.Read();
            var dict = new Dictionary<string, object>();
            SkipWhitespace(reader);

            if (reader.Peek() == '}')
            {
                reader.Read();
                return new Token { Type = TokenType.Object, Value = dict };
            }

            while (true)
            {
                SkipWhitespace(reader);
                if (reader.Peek() != '"')
                    throw new Exception("MyJsonExpected string key");

                var key = ReadString(reader);
                SkipWhitespace(reader);

                if (reader.Read() != ':')
                    throw new Exception("MyJsonExpected :");

                SkipWhitespace(reader);
                var value = ReadToken(reader);
                if (value != null)
                    dict[key] = value.Value!;

                SkipWhitespace(reader);
                ch = reader.Read();
                if (ch == '}')
                    break;
                if (ch != ',')
                    throw new Exception("MyJsonExpected , or }");
            }
            return new Token { Type = TokenType.Object, Value = dict };
        }

        if (ch == '-' || char.IsDigit((char)ch)) // number
        {
            var sb = new StringBuilder();
            if (ch == '-') sb.Append((char)reader.Read());
            while (true)
            {
                ch = reader.Peek();
                if (ch == '.' || ch == 'e' || ch == 'E' || char.IsDigit((char)ch))
                {
                    sb.Append((char)reader.Read());
                }
                else break;
            }
            var numStr = sb.ToString();
            if (numStr.Contains('.') || numStr.Contains('e') || numStr.Contains('E'))
            {
                if (double.TryParse(numStr, out var d))
                    return new Token { Type = TokenType.Number, Value = d };
            }
            else
            {
                if (int.TryParse(numStr, out var i))
                    return new Token { Type = TokenType.Number, Value = i };
                if (long.TryParse(numStr, out var l))
                    return new Token { Type = TokenType.Number, Value = l };
            }
            return new Token { Type = TokenType.String, Value = numStr };
        }

        throw new Exception($"MyJsonUnexpected character: {(char)ch}");
    }

    private void SkipWhitespace(TextReader reader)
    {
        while (true)
        {
            int ch = reader.Peek();
            if (ch != ' ' && ch != '\n' && ch != '\r' && ch != '\t')
                break;
            reader.Read();
        }
    }

    private string ReadString(TextReader reader)
    {
        reader.Read(); // opening quote
        var sb = new StringBuilder();

        while (true)
        {
            int ch = reader.Read();
            if (ch == -1)
                break;

            if (ch == '"')
                break;

            if (ch == '\\')
            {
                int next = reader.Read();
                switch (next)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                    var hex = new char[4];
                    for (int i = 0; i < 4; i++) hex[i] = (char)reader.Read();
                    sb.Append((char)Convert.ToInt32(new string(hex), 16));
                    break;
                    default: sb.Append((char)next); break;
                }
            }
            else
            {
                sb.Append((char)ch);
            }
        }

        return sb.ToString();
    }

    private void ReadString(TextReader reader, int length)
    {
        for (int i = 0; i < length; i++)
            reader.Read();
    }
}

/// <summary>
/// MyJson序列化器的配置设置
/// </summary>
public class MyJsonSettings
{
    /// <summary>
    /// 格式化选项
    /// </summary>
    public Formatting Formatting { get; set; }

    /// <summary>
    /// 类型名称处理选项
    /// </summary>
    public TypeNameHandling TypeNameHandling { get; set; }

    /// <summary>
    /// 引用保留处理选项
    /// </summary>
    public PreserveReferencesHandling PreserveReferencesHandling { get; set; }

    /// <summary>
    /// 循环引用处理选项
    /// </summary>
    public ReferenceLoopHandling ReferenceLoopHandling { get; set; }

    /// <summary>
    /// 换行符选项
    /// </summary>
    public LineEnding LineEnding { get; set; } = LineEnding.Default;

    /// <summary>
    /// 转换器列表
    /// </summary>
    public List<MyJsonConverter>? Converters { get; set; }
}

/// <summary>
/// 格式化选项枚举
/// </summary>
public enum Formatting
{
    /// <summary>
    /// 无格式化
    /// </summary>
    None = 0,

    /// <summary>
    /// 缩进格式化
    /// </summary>
    Indented = 1
}

/// <summary>
/// 类型名称处理选项枚举
/// </summary>
public enum TypeNameHandling
{
    /// <summary>
    /// 不处理类型名称
    /// </summary>
    None = 0,

    /// <summary>
    /// 自动处理类型名称
    /// </summary>
    Auto = 1
}

/// <summary>
/// 引用保留处理选项枚举
/// </summary>
public enum PreserveReferencesHandling
{
    /// <summary>
    /// 不保留引用
    /// </summary>
    None = 0,

    /// <summary>
    /// 仅保留对象引用
    /// </summary>
    Objects = 1,

    /// <summary>
    /// 仅保留数组引用
    /// </summary>
    Arrays = 2,

    /// <summary>
    /// 保留所有引用
    /// </summary>
    All = 3
}

/// <summary>
/// 循环引用处理选项枚举
/// </summary>
public enum ReferenceLoopHandling
{
    /// <summary>
    /// 抛出错误
    /// </summary>
    Error = 0,

    /// <summary>
    /// 忽略循环引用
    /// </summary>
    Ignore = 1,

    /// <summary>
    /// 序列化循环引用
    /// </summary>
    Serialize = 2
}

/// <summary>
/// 换行符选项枚举
/// </summary>
public enum LineEnding
{
    /// <summary>
    /// 使用系统默认换行符（Windows: \r\n, Linux/macOS: \n）
    /// </summary>
    Default = 0,

    /// <summary>
    /// 使用 LF (\n)
    /// </summary>
    LF = 1,

    /// <summary>
    /// 使用 CRLF (\r\n)
    /// </summary>
    CRLF = 2
}

/// <summary>
/// 令牌类，表示JSON中的一个语法单元
/// </summary>
public class Token
{
    /// <summary>
    /// 令牌类型
    /// </summary>
    public TokenType Type { get; set; }

    /// <summary>
    /// 令牌值
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// 令牌类型枚举
/// </summary>
public enum TokenType
{
    /// <summary>
    /// 无类型
    /// </summary>
    None,

    /// <summary>
    /// 对象类型
    /// </summary>
    Object,

    /// <summary>
    /// 数组类型
    /// </summary>
    Array,

    /// <summary>
    /// 字符串类型
    /// </summary>
    String,

    /// <summary>
    /// 数字类型
    /// </summary>
    Number,

    /// <summary>
    /// 布尔类型
    /// </summary>
    Boolean,

    /// <summary>
    /// 空值类型
    /// </summary>
    Null
}

/// <summary>
/// JSON转换器抽象基类
/// </summary>
public abstract class MyJsonConverter
{
    /// <summary>
    /// 支持的类型集合
    /// </summary>
    public abstract IEnumerable<Type> SupportedTypes { get; }

    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <param name="serializer">序列化器实例</param>
    /// <returns>序列化后的字典</returns>
    public abstract IDictionary<string, object> Serialize(object obj, MyJson serializer);

    /// <summary>
    /// 反序列化对象
    /// </summary>
    /// <param name="dictionary">包含数据的字典</param>
    /// <param name="type">目标类型</param>
    /// <param name="serializer">序列化器实例</param>
    /// <returns>反序列化后的对象</returns>
    public abstract object Deserialize(IDictionary<string, object> dictionary, Type type, MyJson serializer);
}
