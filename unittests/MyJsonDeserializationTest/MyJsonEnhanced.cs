using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace JsonDeserializationTest
{
    /// <summary>
    /// 增强版 MyJson，支持无参构造函数和更好的类型转换
    /// </summary>
    public class MyJsonEnhanced
    {
        private List<MyJsonConverter> _converters = [];
        private bool _indent;
        private LineEnding _lineEnding = LineEnding.Default;
        private readonly Dictionary<object, int> _objectReferences = new();
        private readonly Dictionary<int, object> _idToObject = new();
        private int _referenceIdCounter = 1;
        private readonly HashSet<object> _processedObjects = new();

        public Formatting Formatting
        {
            get => _indent ? Formatting.Indented : Formatting.None;
            set => _indent = value == Formatting.Indented;
        }

        public TypeNameHandling TypeNameHandling { get; set; }
        public PreserveReferencesHandling PreserveReferencesHandling { get; set; }
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; }

        public static string SerializeObject<T>(T? obj, Formatting formatting = Formatting.None)
        {
            var json = new MyJsonEnhanced();
            json.Formatting = formatting;
            return json.Serialize(obj);
        }

        public static T? DeserializeObject<T>(string json)
        {
            var jsonSerializer = new MyJsonEnhanced();
            return jsonSerializer.Deserialize<T>(json);
        }

        public string Serialize<T>(T? obj)
        {
            if (obj == null) return "null";

            _objectReferences.Clear();
            _idToObject.Clear();
            _referenceIdCounter = 1;
            _processedObjects.Clear();

            var sb = new StringBuilder();
            SerializeValue(obj, sb, 0);
            return sb.ToString();
        }

        public T? Deserialize<T>(string json)
        {
            _objectReferences.Clear();
            _idToObject.Clear();
            _referenceIdCounter = 1;
            _processedObjects.Clear();

            using var reader = new StringReader(json);
            return DeserializeValue<T>(reader);
        }

        private T? DeserializeValue<T>(TextReader reader)
        {
            var token = ReadToken(reader);
            if (token == null) return default;
            return (T?)DeserializeToken(token, typeof(T));
        }

        /// <summary>
        /// 创建对象实例 - 关键改进：支持无参构造函数
        /// </summary>
        private object? CreateInstance(Type type)
        {
            try
            {
                // 首先尝试使用 Activator.CreateInstance（需要无参构造函数）
                return Activator.CreateInstance(type);
            }
            catch (MissingMethodException)
            {
                // 如果没有无参构造函数，使用 FormatterServices 创建未初始化的对象
                try
                {
                    return FormatterServices.GetUninitializedObject(type);
                }
                catch
                {
                    // 如果上述方法也失败，返回 null
                    return null;
                }
            }
        }

        private object? DeserializeToken(Token token, Type targetType)
        {
            if (token.Type == TokenType.Null) return null;

            // 处理 $ref 引用
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue("$ref", out var refValue) && refValue != null)
                {
                    int refId = Convert.ToInt32(refValue.ToString());
                    if (_idToObject.TryGetValue(refId, out var referencedObject))
                        return referencedObject;
                    return null;
                }
            }

            // 基本类型处理
            if (targetType == typeof(string)) return token.Value?.ToString();
            if (targetType == typeof(int)) return Convert.ToInt32(token.Value);
            if (targetType == typeof(long)) return Convert.ToInt64(token.Value);
            if (targetType == typeof(float)) return Convert.ToSingle(token.Value);
            if (targetType == typeof(double)) return Convert.ToDouble(token.Value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(token.Value);
            if (targetType == typeof(decimal)) return Convert.ToDecimal(token.Value);
            if (targetType == typeof(DateTime))
            {
                if (token.Value is string dateString)
                    return DateTime.Parse(dateString);
                return DateTime.MinValue;
            }
            if (targetType.IsEnum)
                return Enum.Parse(targetType, token.Value?.ToString() ?? "");

            // 数组类型
            if (targetType.IsArray)
            {
                if (token.Type == TokenType.Array && token.Value is List<object> arrList)
                    return DeserializeArray(arrList, targetType);
                return Array.CreateInstance(targetType.GetElementType()!, 0);
            }

            // List<T>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (token.Type == TokenType.Array && token.Value is List<object> list)
                    return DeserializeList(list, targetType);
                return Activator.CreateInstance(targetType);
            }

            // 对象类型
            if (token.Type == TokenType.Object && token.Value is Dictionary<string, object> objDict)
            {
                return DeserializeObject(objDict, targetType);
            }

            return token.Value;
        }

        private object? DeserializeObject(Dictionary<string, object>? dict, Type type)
        {
            if (dict == null) return null;

            // 处理 $id
            int objectId = 0;
            if (dict.TryGetValue("$id", out var idValue) && idValue != null)
            {
                objectId = Convert.ToInt32(idValue.ToString());
                dict.Remove("$id");
            }

            // 处理 $type
            if (TypeNameHandling == TypeNameHandling.Auto && dict.TryGetValue("$type", out var typeName))
            {
                var fullName = typeName?.ToString();
                if (!string.IsNullOrEmpty(fullName))
                {
                    var actualType = Type.GetType(fullName);
                    if (actualType != null) type = actualType;
                }
                dict.Remove("$type");
            }

            // 关键改进：使用 CreateInstance 方法创建对象
            var obj = CreateInstance(type);
            if (obj == null) return null;

            if (objectId > 0)
                _idToObject[objectId] = obj;

            // 设置字段值
            foreach (var kvp in dict)
            {
                var field = type.GetField(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    var value = DeserializeToken(
                        new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value },
                        field.FieldType);
                    // 改进：使用 ConvertToType 进行类型转换
                    value = ConvertToType(value, field.FieldType);
                    field.SetValue(obj, value);
                    continue;
                }

                var prop = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    var value = DeserializeToken(
                        new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value },
                        prop.PropertyType);
                    value = ConvertToType(value, prop.PropertyType);
                    prop.SetValue(obj, value, null);
                }
            }

            return obj;
        }

        /// <summary>
        /// 改进的类型转换方法
        /// </summary>
        private object? ConvertToType(object? value, Type targetType)
        {
            if (value == null)
            {
                if (targetType.IsClass || Nullable.GetUnderlyingType(targetType) != null)
                    return null;
                return Activator.CreateInstance(targetType);
            }

            var valueType = value.GetType();
            if (targetType.IsAssignableFrom(valueType))
                return value;

            try
            {
                // 处理字符串到数值类型的转换
                if (value is string strValue)
                {
                    if (targetType == typeof(int)) return int.Parse(strValue);
                    if (targetType == typeof(short)) return short.Parse(strValue);
                    if (targetType == typeof(long)) return long.Parse(strValue);
                    if (targetType == typeof(byte)) return byte.Parse(strValue);
                    if (targetType == typeof(ushort)) return ushort.Parse(strValue);
                    if (targetType == typeof(uint)) return uint.Parse(strValue);
                    if (targetType == typeof(ulong)) return ulong.Parse(strValue);
                    if (targetType == typeof(float)) return float.Parse(strValue);
                    if (targetType == typeof(double)) return double.Parse(strValue);
                    if (targetType == typeof(decimal)) return decimal.Parse(strValue);
                    if (targetType == typeof(bool)) return bool.Parse(strValue);
                }

                // 处理 int 到 short/byte 等的转换
                if (value is int intValue)
                {
                    if (targetType == typeof(short)) return (short)intValue;
                    if (targetType == typeof(ushort)) return (ushort)intValue;
                    if (targetType == typeof(byte)) return (byte)intValue;
                    if (targetType == typeof(long)) return (long)intValue;
                    if (targetType == typeof(float)) return (float)intValue;
                    if (targetType == typeof(double)) return (double)intValue;
                    if (targetType == typeof(decimal)) return (decimal)intValue;
                }

                // 通用转换
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);
                return null;
            }
        }

        private object? DeserializeArray(List<object>? list, Type type)
        {
            if (list == null) return null;

            Type elementType = type.GetElementType()!;
            var array = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var value = DeserializeToken(
                    new Token { Type = GetTokenType(list[i]), Value = list[i] },
                    elementType);
                array.SetValue(ConvertToType(value, elementType), i);
            }
            return array;
        }

        private object? DeserializeList(List<object>? list, Type type)
        {
            if (list == null) return null;

            var elementType = type.GetGenericArguments()[0];
            var result = Activator.CreateInstance(type);
            if (result == null) return null;

            var addMethod = type.GetMethod("Add");
            foreach (var item in list)
            {
                var value = DeserializeToken(
                    new Token { Type = GetTokenType(item), Value = item },
                    elementType);
                value = ConvertToType(value, elementType);
                addMethod?.Invoke(result, new[] { value });
            }
            return result;
        }

        #region 序列化和 Token 相关方法（简化版）

        private void SerializeValue<T>(T obj, StringBuilder sb, int indent)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            Type type = obj.GetType();

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime))
            {
                SerializePrimitive(obj, sb);
            }
            else if (type.IsEnum)
            {
                sb.Append(Convert.ChangeType(obj, Enum.GetUnderlyingType(type)).ToString());
            }
            else if (obj is IEnumerable enumerable && !(obj is string))
            {
                SerializeEnumerable(enumerable, sb, indent);
            }
            else
            {
                SerializeObject(obj, sb, indent);
            }
        }

        private void SerializePrimitive(object obj, StringBuilder sb)
        {
            if (obj is string str)
                sb.Append($"\"{EscapeString(str)}\"");
            else if (obj is bool b)
                sb.Append(b ? "true" : "false");
            else
                sb.Append(obj.ToString());
        }

        private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb, int indent)
        {
            sb.Append('[');
            bool first = true;
            foreach (var item in enumerable)
            {
                if (!first) sb.Append(',');
                first = false;
                SerializeValue(item, sb, indent);
            }
            sb.Append(']');
        }

        private void SerializeObject(object obj, StringBuilder sb, int indent)
        {
            sb.Append('{');
            Type type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            bool first = true;

            foreach (var field in fields)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append($"\"{field.Name}\":");
                var value = field.GetValue(obj);
                SerializeValue(value, sb, indent);
            }
            sb.Append('}');
        }

        private string EscapeString(string str)
        {
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private Token? ReadToken(TextReader reader)
        {
            SkipWhitespace(reader);
            int ch = reader.Read();
            if (ch == -1) return null;

            char c = (char)ch;

            if (c == '{')
            {
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
                    var keyToken = ReadToken(reader);
                    if (keyToken?.Type != TokenType.String) break;
                    string key = (string)keyToken.Value!;

                    SkipWhitespace(reader);
                    if (reader.Read() != ':') break;

                    SkipWhitespace(reader);
                    var valueToken = ReadToken(reader);
                    if (valueToken == null) break;

                    dict[key] = valueToken.Value!;

                    SkipWhitespace(reader);
                    int next = reader.Read();
                    if (next == '}') break;
                    if (next != ',') break;
                }
                return new Token { Type = TokenType.Object, Value = dict };
            }

            if (c == '[')
            {
                var list = new List<object>();
                SkipWhitespace(reader);
                if (reader.Peek() == ']')
                {
                    reader.Read();
                    return new Token { Type = TokenType.Array, Value = list };
                }

                while (true)
                {
                    SkipWhitespace(reader);
                    var item = ReadToken(reader);
                    if (item == null) break;
                    list.Add(item.Value!);

                    SkipWhitespace(reader);
                    int next = reader.Read();
                    if (next == ']') break;
                    if (next != ',') break;
                }
                return new Token { Type = TokenType.Array, Value = list };
            }

            if (c == '"')
            {
                var sb = new StringBuilder();
                while (true)
                {
                    int next = reader.Read();
                    if (next == -1 || next == '"') break;
                    if (next == '\\')
                    {
                        int escape = reader.Read();
                        if (escape == -1) break;
                        sb.Append((char)escape switch
                        {
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            '\\' => '\\',
                            '"' => '"',
                            _ => (char)escape
                        });
                    }
                    else
                    {
                        sb.Append((char)next);
                    }
                }
                return new Token { Type = TokenType.String, Value = sb.ToString() };
            }

            if (c == 't' || c == 'f' || c == 'n')
            {
                var sb = new StringBuilder();
                sb.Append(c);
                while (char.IsLetter((char)reader.Peek()))
                    sb.Append((char)reader.Read());

                string word = sb.ToString();
                if (word == "true") return new Token { Type = TokenType.Boolean, Value = true };
                if (word == "false") return new Token { Type = TokenType.Boolean, Value = false };
                if (word == "null") return new Token { Type = TokenType.Null, Value = null };
            }

            if (char.IsDigit(c) || c == '-')
            {
                var sb = new StringBuilder();
                sb.Append(c);
                while (char.IsDigit((char)reader.Peek()) || (char)reader.Peek() == '.' || (char)reader.Peek() == 'e' || (char)reader.Peek() == 'E' || (char)reader.Peek() == '-' || (char)reader.Peek() == '+')
                    sb.Append((char)reader.Read());

                string numStr = sb.ToString();
                if (numStr.Contains('.') || numStr.Contains('e') || numStr.Contains('E'))
                {
                    if (double.TryParse(numStr, out double d))
                        return new Token { Type = TokenType.Number, Value = d };
                }
                else
                {
                    if (int.TryParse(numStr, out int i))
                        return new Token { Type = TokenType.Number, Value = i };
                    if (long.TryParse(numStr, out long l))
                        return new Token { Type = TokenType.Number, Value = l };
                }
            }

            return null;
        }

        private void SkipWhitespace(TextReader reader)
        {
            while (char.IsWhiteSpace((char)reader.Peek()))
                reader.Read();
        }

        private TokenType GetTokenType(object value)
        {
            if (value == null) return TokenType.Null;
            if (value is string) return TokenType.String;
            if (value is bool) return TokenType.Boolean;
            if (value is int || value is long || value is double || value is float || value is decimal)
                return TokenType.Number;
            if (value is Dictionary<string, object>) return TokenType.Object;
            if (value is List<object>) return TokenType.Array;
            return TokenType.String;
        }

        #endregion
    }

    #region 辅助类和枚举

    public class Token
    {
        public TokenType Type { get; set; }
        public object? Value { get; set; }
    }

    public enum TokenType
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null
    }

    public enum Formatting
    {
        None,
        Indented
    }

    public enum TypeNameHandling
    {
        None,
        Auto
    }

    public enum PreserveReferencesHandling
    {
        None,
        Objects,
        Arrays,
        All
    }

    public enum ReferenceLoopHandling
    {
        Error,
        Ignore,
        Serialize
    }

    public enum LineEnding
    {
        Default,
        LF,
        CRLF
    }

    public class MyJsonSettings
    {
        public Formatting Formatting { get; set; }
        public TypeNameHandling TypeNameHandling { get; set; }
        public PreserveReferencesHandling PreserveReferencesHandling { get; set; }
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; }
        public LineEnding LineEnding { get; set; }
        public List<MyJsonConverter>? Converters { get; set; }
    }

    public abstract class MyJsonConverter
    {
        public abstract Type[] SupportedTypes { get; }
        public abstract Dictionary<string, object> Serialize(object obj, MyJsonEnhanced json);
        public abstract object? Deserialize(Dictionary<string, object> dict, Type targetType, MyJsonEnhanced json);
    }

    #endregion
}
