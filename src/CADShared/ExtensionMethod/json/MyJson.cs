#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace IFoxCAD.Cad;

public class MyJson
{
    private List<MyJsonConverter> _converters = new();
    private bool _indent;

    public Formatting Formatting
    {
        get => _indent ? Formatting.Indented : Formatting.None;
        set => _indent = value == Formatting.Indented;
    }

    public TypeNameHandling TypeNameHandling { get; set; }

    public static string SerializeObject(object? obj)
    {
        return SerializeObject(obj, new MyJsonSettings());
    }

    public static string SerializeObject(object? obj, MyJsonSettings settings)
    {
        var json = new MyJson();
        json.Formatting = settings.Formatting;
        json.TypeNameHandling = settings.TypeNameHandling;
        if (settings.Converters != null)
        {
            foreach (var converter in settings.Converters)
                json.RegisterConverters(new[] { converter });
        }
        return json.Serialize(obj);
    }

    public static T? DeserializeObject<T>(string json)
    {
        return DeserializeObject<T>(json, new MyJsonSettings());
    }

    public static T? DeserializeObject<T>(string json, MyJsonSettings settings)
    {
        var jsonSerializer = new MyJson();
        jsonSerializer.Formatting = settings.Formatting;
        jsonSerializer.TypeNameHandling = settings.TypeNameHandling;
        if (settings.Converters != null)
        {
            foreach (var converter in settings.Converters)
                jsonSerializer.RegisterConverters(new[] { converter });
        }
        return jsonSerializer.Deserialize<T>(json);
    }

    public void RegisterConverters(IEnumerable<MyJsonConverter> converters)
    {
        _converters.AddRange(converters);
    }

    public string Serialize(object? obj)
    {
        if (obj == null)
            return "null";

        var sb = new StringBuilder();
        SerializeValue(obj, sb, 0);
        return sb.ToString();
    }

    private string GetIndentString(int indent)
    {
        return _indent ? new string(' ', indent * 4) : string.Empty;
    }

    private void SerializeValue(object obj, StringBuilder sb, int indent)
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

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
        {
            SerializePrimitive(obj, sb);
        }
        else if (type.IsEnum)
        {
            sb.Append($"\"{obj}\"");
        }
        else if (type.IsArray)
        {
            SerializeArray((Array)obj, sb, indent);
        }
        else if (obj is IDictionary dict)
        {
            SerializeDictionary(dict, sb, indent);
        }
        else if (obj is IEnumerable enumerable)
        {
            SerializeEnumerable(enumerable, sb, indent);
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
        else
        {
            sb.Append(obj.ToString());
        }
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

    private void SerializeArray(Array array, StringBuilder sb, int indent)
    {
        sb.Append("[");
        if (array.Length > 0)
        {
            if (_indent)
            {
                sb.AppendLine();
                int newIndent = indent + 1;
                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(GetIndentString(newIndent));
                    SerializeValue(array.GetValue(i), sb, newIndent);
                    if (i < array.Length - 1)
                    {
                        sb.Append(",");
                    }
                    sb.AppendLine();
                }
                sb.Append(GetIndentString(indent));
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    SerializeValue(array.GetValue(i), sb, indent);
                }
            }
        }
        sb.Append("]");
    }

    private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb, int indent)
    {
        sb.Append("[");
        bool first = true;
        var items = enumerable.Cast<object>().ToList();
        if (items.Count > 0)
        {
            if (_indent)
            {
                sb.AppendLine();
                int newIndent = indent + 1;
                for (int i = 0; i < items.Count; i++)
                {
                    sb.Append(GetIndentString(newIndent));
                    SerializeValue(items[i], sb, newIndent);
                    if (i < items.Count - 1)
                    {
                        sb.Append(",");
                    }
                    sb.AppendLine();
                }
                sb.Append(GetIndentString(indent));
            }
            else
            {
                foreach (var item in items)
                {
                    if (!first) sb.Append(", ");
                    first = false;
                    SerializeValue(item, sb, indent);
                }
            }
        }
        sb.Append("]");
    }

    private void SerializeDictionary(IDictionary dict, StringBuilder sb, int indent)
    {
        sb.Append("{");
        var keys = dict.Keys.Cast<object>().ToList();
        bool first = true;
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (!first)
            {
                if (_indent)
                {
                    sb.Append(",");
                    sb.AppendLine();
                    sb.Append(GetIndentString(indent + 1));
                }
                else
                {
                    sb.Append(", ");
                }
            }
            else if (_indent)
            {
                sb.AppendLine();
                sb.Append(GetIndentString(indent + 1));
            }
            first = false;
            sb.Append("\"" + EscapeString(key.ToString() ?? "") + "\": ");
            SerializeValue(dict[key]!, sb, indent + 1);
        }
        if (keys.Count > 0 && _indent)
        {
            sb.AppendLine();
            sb.Append(GetIndentString(indent));
        }
        sb.Append("}");
    }

    private void SerializeDictionary(IDictionary<string, object> dict, StringBuilder sb, int indent)
    {
        sb.Append("{");
        var keys = dict.Keys.ToList();
        bool first = true;
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (!first)
            {
                if (_indent)
                {
                    sb.Append(",");
                    sb.AppendLine();
                    sb.Append(GetIndentString(indent + 1));
                }
                else
                {
                    sb.Append(", ");
                }
            }
            else if (_indent)
            {
                sb.AppendLine();
                sb.Append(GetIndentString(indent + 1));
            }
            first = false;
            sb.Append("\"" + EscapeString(key.ToString() ?? "") + "\": ");
            SerializeValue(dict[key], sb, indent + 1);
        }
        if (keys.Count > 0 && _indent)
        {
            sb.AppendLine();
            sb.Append(GetIndentString(indent));
        }
        sb.Append("}");
    }

    private void SerializeObject(object obj, StringBuilder sb, int indent)
    {
        sb.Append("{");
        if (TypeNameHandling == TypeNameHandling.Auto)
        {
            if (_indent)
            {
                sb.AppendLine();
                int newIndent = indent + 1;
                sb.Append(GetIndentString(newIndent));
                sb.Append($"\"$type\": \"{obj.GetType().FullName}\",");
                sb.AppendLine();
                sb.Append(GetIndentString(newIndent));
                SerializeFields(obj, sb, newIndent);
                sb.AppendLine();
                sb.Append(GetIndentString(indent));
            }
            else
            {
                sb.Append($"\"$type\": \"{obj.GetType().FullName}\", ");
                SerializeFields(obj, sb, indent);
            }
        }
        else
        {
            if (_indent)
            {
                sb.AppendLine();
                SerializeFields(obj, sb, indent + 1);
                sb.AppendLine();
                sb.Append(GetIndentString(indent));
            }
            else
            {
                SerializeFields(obj, sb, indent);
            }
        }
        sb.Append("}");
    }

    private void SerializeFields(object obj, StringBuilder sb, int indent)
    {
        Type type = obj.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        bool first = true;
        int fieldCount = fields.Length + properties.Length;
        int currentIndex = 0;

        foreach (var field in fields)
        {
            if (!first && _indent)
            {
                sb.Append(",");
                sb.AppendLine();
                sb.Append(GetIndentString(indent));
            }
            else if (!first && !_indent)
            {
                sb.Append(", ");
            }
            first = false;
            sb.Append($"\"{field.Name}\": ");
            object? value = field.GetValue(obj);
            SerializeValue(value, sb, indent);
            currentIndex++;
        }

        foreach (var prop in properties)
        {
            if (!first && _indent)
            {
                sb.Append(",");
                sb.AppendLine();
                sb.Append(GetIndentString(indent));
            }
            else if (!first && !_indent)
            {
                sb.Append(", ");
            }
            first = false;
            sb.Append($"\"{prop.Name}\": ");
            object? value;
            try { value = prop.GetValue(obj, null); }
            catch { value = null; }
            SerializeValue(value!, sb, indent);
            currentIndex++;
        }
    }

    public T? Deserialize<T>(string json)
    {
        using var reader = new StringReader(json);
        return DeserializeValue<T>(reader);
    }

    private T? DeserializeValue<T>(TextReader reader)
    {
        var token = ReadToken(reader);
        if (token == null)
            return default;

        return (T)DeserializeToken(token, typeof(T))!;
    }

    private object? DeserializeToken(Token token, Type targetType)
    {
        if (token.Type == TokenType.Null)
            return null;

        if (targetType == typeof(string))
        {
            return token.Value?.ToString();
        }

        if (targetType == typeof(int))
            return Convert.ToInt32(token.Value);

        if (targetType == typeof(double))
            return Convert.ToDouble(token.Value);

        if (targetType == typeof(bool))
            return Convert.ToBoolean(token.Value);

        if (targetType == typeof(decimal))
            return Convert.ToDecimal(token.Value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, token.Value?.ToString() ?? "");

        var converter = _converters.FirstOrDefault(c => c.SupportedTypes.Contains(targetType));
        if (converter != null && token.Type == TokenType.Object)
        {
            return converter.Deserialize((Dictionary<string, object>)token.Value!, targetType, this);
        }

        if (token.Type == TokenType.Object)
        {
            return DeserializeObject(token.Value as Dictionary<string, object>, targetType);
        }

        if (token.Type == TokenType.Array)
        {
            return DeserializeArray(token.Value as List<object>, targetType);
        }

        return token.Value;
    }

    private object? DeserializeObject(Dictionary<string, object>? dict, Type type)
    {
        if (dict == null)
            return null;

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

        var obj = Activator.CreateInstance(type);
        if (obj == null)
            return null;

        foreach (var kvp in dict)
        {
            var field = type.GetField(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                var value = DeserializeToken(new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value }, field.FieldType);
                field.SetValue(obj, value);
            }

            var prop = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                var value = DeserializeToken(new Token { Type = GetTokenType(kvp.Value), Value = kvp.Value }, prop.PropertyType);
                try { prop.SetValue(obj, value, null); }
                catch { }
            }
        }

        return obj;
    }

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
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                elementType = genericArgs[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var result = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");
                foreach (var item in list)
                {
                    var value = DeserializeToken(new Token { Type = GetTokenType(item), Value = item }, elementType);
                    addMethod?.Invoke(result, new[] { value });
                }
                return result;
            }
        }

        return list;
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
                    throw new Exception("Expected , or ]");
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
                    throw new Exception("Expected string key");

                var key = ReadString(reader);
                SkipWhitespace(reader);

                if (reader.Read() != ':')
                    throw new Exception("Expected :");

                SkipWhitespace(reader);
                var value = ReadToken(reader);
                if (value != null)
                    dict[key] = value.Value!;

                SkipWhitespace(reader);
                ch = reader.Read();
                if (ch == '}')
                    break;
                if (ch != ',')
                    throw new Exception("Expected , or }");
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

        throw new Exception($"Unexpected character: {(char)ch}");
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

public class MyJsonSettings
{
    public Formatting Formatting { get; set; }
    public TypeNameHandling TypeNameHandling { get; set; }
    public List<MyJsonConverter>? Converters { get; set; }
}

public enum Formatting
{
    None = 0,
    Indented = 1
}

public enum TypeNameHandling
{
    None = 0,
    Auto = 1
}

public class Token
{
    public TokenType Type { get; set; }
    public object? Value { get; set; }
}

public enum TokenType
{
    None,
    Object,
    Array,
    String,
    Number,
    Boolean,
    Null
}

public abstract class MyJsonConverter
{
    public abstract IEnumerable<Type> SupportedTypes { get; }
    public abstract IDictionary<string, object> Serialize(object obj, MyJson serializer);
    public abstract object Deserialize(IDictionary<string, object> dictionary, Type type, MyJson serializer);
}
