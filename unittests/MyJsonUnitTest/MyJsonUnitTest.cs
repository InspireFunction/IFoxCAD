using System;
using System.Collections.Generic;
using IFoxCAD.Cad;
using Xunit;

namespace MyJsonUnityTest;

/// <summary>
/// MyJson 单元测试类
/// </summary>
public class MyJsonTests
{
    #region 基础类型序列化测试

    [Fact]
    public void Serialize_Null_ReturnsNullString()
    {
        string? value = null;
        var result = MyJson.SerializeObject(value);
        Assert.Equal("null", result);
    }

    [Theory]
    [InlineData("hello", "\"hello\"")]
    [InlineData("", "\"\"")]
    [InlineData("with quotes", "\"with quotes\"")]
    [InlineData("line\nbreak", "\"line\\nbreak\"")]
    [InlineData("tab\there", "\"tab\\there\"")]
    [InlineData("back\\slash", "\"back\\\\slash\"")]
    public void Serialize_String_ReturnsCorrectJson(string input, string expected)
    {
        var result = MyJson.SerializeObject(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(42, "42")]
    [InlineData(0, "0")]
    [InlineData(-100, "-100")]
    public void Serialize_Int_ReturnsCorrectJson(int input, string expected)
    {
        var result = MyJson.SerializeObject(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Serialize_Bool_ReturnsCorrectJson(bool input, string expected)
    {
        var result = MyJson.SerializeObject(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Serialize_Double_ReturnsCorrectJson()
    {
        var value = 3.14159;
        var result = MyJson.SerializeObject(value);
        Assert.Contains("3.14", result);
    }

    [Fact]
    public void Serialize_Decimal_ReturnsCorrectJson()
    {
        decimal value = 5000.50m;
        var result = MyJson.SerializeObject(value);
        Assert.Contains("5000", result);
    }

    #endregion

    #region 数组和集合序列化测试（无引用保留）

    [Fact]
    public void Serialize_IntArray_ReturnsCorrectJson()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(array, settings);
        Assert.Equal("[1,2,3,4,5]", result);
    }

    [Fact]
    public void Serialize_StringArray_ReturnsCorrectJson()
    {
        var array = new[] { "a", "b", "c" };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(array, settings);
        Assert.Equal("[\"a\",\"b\",\"c\"]", result);
    }

    [Fact]
    public void Serialize_EmptyArray_ReturnsCorrectJson()
    {
        var array = new int[0];
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(array, settings);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void Serialize_EmptyStringArray_ReturnsCorrectJson()
    {
        var array = new string[0];
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(array, settings);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void Serialize_List_ReturnsCorrectJson()
    {
        var list = new List<int> { 0, 1, 2, 3 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(list, settings);
        Assert.Equal("[0,1,2,3]", result);
    }

    #endregion

    #region 数组和集合序列化测试（带引用保留）

    [Fact]
    public void Serialize_Array_WithPreserveReferences_ReturnsReferenceFormat()
    {
        var array = new[] { 1, 2, 3 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.Arrays };
        var result = MyJson.SerializeObject(array, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$values\"", result);
    }

    [Fact]
    public void Serialize_List_WithPreserveReferences_ReturnsReferenceFormat()
    {
        var list = new List<int> { 0, 1, 2, 3 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
        var result = MyJson.SerializeObject(list, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$values\"", result);
    }

    #endregion

    #region 对象序列化测试

    [Fact]
    public void Serialize_SimpleObject_ReturnsCorrectJson()
    {
        var person = new TestPerson
        {
            Name = "张三",
            Age = 30
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(person, settings);
        Assert.Contains("\"Name\":\"张三\"", result);
        Assert.Contains("\"Age\":30", result);
    }

    [Fact]
    public void Serialize_ObjectWithNull_ReturnsCorrectJson()
    {
        var person = new TestPerson
        {
            Name = null,
            Age = 25
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(person, settings);
        Assert.Contains("\"Name\":null", result);
        Assert.Contains("\"Age\":25", result);
    }

    [Fact]
    public void Serialize_NestedObject_ReturnsCorrectJson()
    {
        var obj = new TestNestedObject
        {
            Id = 1,
            Name = "测试",
            Child = new TestPerson { Name = "子对象", Age = 10 }
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Contains("\"Id\":1", result);
        Assert.Contains("\"Name\":\"测试\"", result);
        Assert.Contains("\"Child\":", result);
        Assert.Contains("\"Name\":\"子对象\"", result);
    }

    [Fact]
    public void Serialize_ObjectWithArray_ReturnsCorrectJson()
    {
        var obj = new TestObjectWithArray
        {
            Name = "测试对象",
            Items = new[] { "a", "b", "c" }
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Contains("\"Name\":\"测试对象\"", result);
        Assert.Contains("\"Items\":[\"a\",\"b\",\"c\"]", result);
    }

    [Fact]
    public void Serialize_ObjectWithMultipleTypes_ReturnsCorrectJson()
    {
        var obj = new
        {
            Name = "张三",
            Age = 30,
            IsActive = true,
            Salary = 5000.50
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Contains("\"Name\":\"张三\"", result);
        Assert.Contains("\"Age\":30", result);
        Assert.Contains("\"IsActive\":true", result);
        Assert.Contains("\"Salary\":", result);
    }

    [Fact]
    public void Serialize_IMEConfig_ReturnsCorrectJson()
    {
        var imeConfig = new
        {
            AutoEn2Cn = new string[0],
            AutoCn2En = new string[0],
            IMEHookStyle = "Global",
            IMEInputSwitch = "Shift"
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(imeConfig, settings);
        Assert.Contains("\"AutoEn2Cn\":[]", result);
        Assert.Contains("\"AutoCn2En\":[]", result);
        Assert.Contains("\"IMEHookStyle\":\"Global\"", result);
        Assert.Contains("\"IMEInputSwitch\":\"Shift\"", result);
    }

    [Fact]
    public void Serialize_ComplexObject_ReturnsCorrectJson()
    {
        var complexObj = new
        {
            Id = 1,
            Name = "复杂对象",
            Tags = new[] { "JSON", "测试", "格式化" },
            Nested = new
            {
                Value = 123,
                Items = new List<int> { 1, 2, 3, 4, 5 }
            },
            NullValue = (string?)null
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(complexObj, settings);
        Assert.Contains("\"Id\":1", result);
        Assert.Contains("\"Name\":\"复杂对象\"", result);
        Assert.Contains("\"Tags\":[\"JSON\",\"测试\",\"格式化\"]", result);
        Assert.Contains("\"Nested\":", result);
        Assert.Contains("\"NullValue\":null", result);
    }

    #endregion

    #region 格式化输出测试

    [Fact]
    public void Serialize_WithFormattingNone_ReturnsCompactJson()
    {
        var obj = new { Name = "Test", Value = 123 };
        var settings = new MyJsonSettings { Formatting = Formatting.None, PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.DoesNotContain("\n", result);
        Assert.DoesNotContain("  ", result);
    }

    [Fact]
    public void Serialize_WithFormattingIndented_ReturnsFormattedJson()
    {
        var obj = new { Name = "Test", Value = 123 };
        var settings = new MyJsonSettings { Formatting = Formatting.Indented, PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Contains("\n", result);
        Assert.Contains("  ", result);
    }

    #endregion

    #region 换行符测试

    [Fact]
    public void Serialize_WithLFLineEnding_ReturnsLFOnly()
    {
        var testObj = new { Name = "测试对象", Value = 123 };
        var settings = new MyJsonSettings
        {
            Formatting = Formatting.Indented,
            LineEnding = LineEnding.LF,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };
        var json = MyJson.SerializeObject(testObj, settings);
        Assert.Contains("\n", json);
        Assert.DoesNotContain("\r\n", json);
    }

    [Fact]
    public void Serialize_WithCRLFLineEnding_ReturnsCRLF()
    {
        var testObj = new { Name = "测试对象", Value = 123 };
        var settings = new MyJsonSettings
        {
            Formatting = Formatting.Indented,
            LineEnding = LineEnding.CRLF,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };
        var json = MyJson.SerializeObject(testObj, settings);
        Assert.Contains("\r\n", json);
    }

    [Fact]
    public void Serialize_WithDefaultLineEnding_ReturnsSystemDefault()
    {
        var testObj = new { Name = "测试对象", Value = 123 };
        var settings = new MyJsonSettings
        {
            Formatting = Formatting.Indented,
            LineEnding = LineEnding.Default,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };
        var json = MyJson.SerializeObject(testObj, settings);
        Assert.Contains(Environment.NewLine, json);
    }

    #endregion

    #region 反序列化测试

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        var json = "null";
        var result = MyJson.DeserializeObject<string>(json);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"\"", "")]
    public void Deserialize_String_ReturnsCorrectValue(string json, string expected)
    {
        var result = MyJson.DeserializeObject<string>(json);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("0", 0)]
    [InlineData("-100", -100)]
    public void Deserialize_Int_ReturnsCorrectValue(string json, int expected)
    {
        var result = MyJson.DeserializeObject<int>(json);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Deserialize_Bool_ReturnsCorrectValue(string json, bool expected)
    {
        var result = MyJson.DeserializeObject<bool>(json);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Deserialize_IntArray_ReturnsCorrectArray()
    {
        var json = "[1,2,3,4,5]";
        var result = MyJson.DeserializeObject<int[]>(json);
        Assert.NotNull(result);
        Assert.Equal(5, result.Length);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public void Deserialize_StringArray_ReturnsCorrectArray()
    {
        var json = "[\"a\",\"b\",\"c\"]";
        var result = MyJson.DeserializeObject<string[]>(json);
        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Equal(new[] { "a", "b", "c" }, result);
    }

    [Fact]
    public void Deserialize_EmptyArray_ReturnsEmptyArray()
    {
        var json = "[]";
        var result = MyJson.DeserializeObject<int[]>(json);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Deserialize_List_ReturnsCorrectList()
    {
        var json = "[0,1,2,3]";
        var result = MyJson.DeserializeObject<List<int>>(json);
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(new List<int> { 0, 1, 2, 3 }, result);
    }

    [Fact]
    public void Deserialize_StringList_ReturnsCorrectList()
    {
        var json = "[\"a\",\"b\",\"c\"]";
        var result = MyJson.DeserializeObject<List<string>>(json);
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(new List<string> { "a", "b", "c" }, result);
    }

    [Fact]
    public void Deserialize_SimpleObject_ReturnsCorrectObject()
    {
        var json = "{\"Name\":\"张三\",\"Age\":30}";
        var result = MyJson.DeserializeObject<TestPerson>(json);
        Assert.NotNull(result);
        Assert.Equal("张三", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Deserialize_ObjectWithNull_ReturnsCorrectObject()
    {
        var json = "{\"Name\":null,\"Age\":25}";
        var result = MyJson.DeserializeObject<TestPerson>(json);
        Assert.NotNull(result);
        Assert.Null(result.Name);
        Assert.Equal(25, result.Age);
    }

    [Fact]
    public void Deserialize_NestedObject_ReturnsCorrectObject()
    {
        var json = "{\"Id\":1,\"Name\":\"测试\",\"Child\":{\"Name\":\"子对象\",\"Age\":10}}";
        var result = MyJson.DeserializeObject<TestNestedObject>(json);
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("测试", result.Name);
        Assert.NotNull(result.Child);
        Assert.Equal("子对象", result.Child.Name);
        Assert.Equal(10, result.Child.Age);
    }

    [Fact]
    public void Deserialize_ObjectWithArray_ReturnsCorrectObject()
    {
        var json = "{\"Name\":\"测试对象\",\"Items\":[\"a\",\"b\",\"c\"]}";
        var result = MyJson.DeserializeObject<TestObjectWithArray>(json);
        Assert.NotNull(result);
        Assert.Equal("测试对象", result.Name);
        Assert.NotNull(result.Items);
        Assert.Equal(new[] { "a", "b", "c" }, result.Items);
    }

    [Fact]
    public void Deserialize_Dictionary_ReturnsCorrectDictionary()
    {
        var json = "{\"one\":1,\"two\":2,\"three\":3}";
        var result = MyJson.DeserializeObject<Dictionary<string, int>>(json);
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result["one"]);
        Assert.Equal(2, result["two"]);
        Assert.Equal(3, result["three"]);
    }

    [Fact]
    public void Deserialize_DictionaryWithObjectValue_ReturnsCorrectDictionary()
    {
        var json = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
        var result = MyJson.DeserializeObject<Dictionary<string, string>>(json);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    #endregion

    #region 序列化与反序列化一致性测试

    [Fact]
    public void SerializeDeserialize_SimpleObject_ReturnsOriginal()
    {
        var original = new TestPerson { Name = "李四", Age = 28 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<TestPerson>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Age, deserialized.Age);
    }

    [Fact]
    public void SerializeDeserialize_ComplexObject_ReturnsOriginal()
    {
        var original = new TestNestedObject
        {
            Id = 100,
            Name = "复杂对象",
            Child = new TestPerson { Name = "子对象", Age = 5 }
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<TestNestedObject>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.NotNull(deserialized.Child);
        Assert.Equal(original.Child.Name, deserialized.Child.Name);
        Assert.Equal(original.Child.Age, deserialized.Child.Age);
    }

    [Fact]
    public void SerializeDeserialize_Array_ReturnsOriginal()
    {
        var original = new[] { 1, 2, 3, 4, 5 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<int[]>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original, deserialized);
    }

    // 注意：MyJson 反序列化 List<T> 时返回 List<object>，这是已知限制
    [Fact]
    public void SerializeDeserialize_IntList_ReturnsOriginal()
    {
        var original = new List<int> { 1, 2, 3, 4, 5 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<List<int>>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void SerializeDeserialize_StringList_ReturnsOriginal()
    {
        var original = new List<string> { "apple", "banana", "cherry" };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<List<string>>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original, deserialized);
    }

    #endregion

    #region 特殊场景测试

    [Fact]
    public void Serialize_ObjectWithSpecialCharacters_ReturnsCorrectJson()
    {
        var obj = new { Text = "Hello\nWorld\t!\"Quote\"" };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Contains("\\n", result);
        Assert.Contains("\\t", result);
        Assert.Contains("\\\"", result);
    }

    [Fact]
    public void Deserialize_ObjectWithSpecialCharacters_ReturnsCorrectValue()
    {
        var json = "{\"Text\":\"Hello\\nWorld\\t!\\\"Quote\\\"\"}";
        var result = MyJson.DeserializeObject<Dictionary<string, object>>(json);
        Assert.NotNull(result);
        Assert.Equal("Hello\nWorld\t!\"Quote\"", result["Text"]);
    }

    [Fact]
    public void Serialize_EmptyObject_ReturnsCorrectJson()
    {
        var obj = new TestEmptyObject();
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Equal("{}", result);
    }

    [Fact]
    public void Deserialize_EmptyObject_ReturnsCorrectObject()
    {
        var json = "{}";
        var result = MyJson.DeserializeObject<TestEmptyObject>(json);
        Assert.NotNull(result);
    }

    [Fact]
    public void SerializeDeserialize_Dictionary_ReturnsOriginal()
    {
        var original = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<Dictionary<string, int>>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.Count);
        Assert.Equal(1, deserialized["one"]);
        Assert.Equal(2, deserialized["two"]);
        Assert.Equal(3, deserialized["three"]);
    }

    [Fact]
    public void SerializeDeserialize_DictionaryWithMultipleTypes_ReturnsOriginal()
    {
        var original = new Dictionary<string, object>
        {
            { "name", "小明" },
            { "age", 20 },
            { "score", 95.5 },
            { "isActive", true }
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var json = MyJson.SerializeObject(original, settings);
        var deserialized = MyJson.DeserializeObject<Dictionary<string, object>>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(4, deserialized.Count);
        Assert.Equal("小明", deserialized["name"]);
        Assert.Equal(20, deserialized["age"]);
        Assert.Equal(95.5, deserialized["score"]);
        Assert.Equal(true, deserialized["isActive"]);
    }

    #endregion

    #region 测试辅助类

    public class TestPerson
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class TestNestedObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public TestPerson? Child { get; set; }
    }

    public class TestObjectWithArray
    {
        public string? Name { get; set; }
        public string[]? Items { get; set; }
    }

    public class TestEmptyObject
    {
    }

    #endregion

    #region 引用保持测试 (来自 JsonComparison)

    public class RefTestNode
    {
        public string Name { get; set; } = "";
        public RefTestNode? Parent { get; set; }
        public List<RefTestNode> Children { get; set; } = new List<RefTestNode>();
    }

    public class RefTestContainer
    {
        public RefTestNode SharedNode { get; set; } = new RefTestNode();
        public RefTestNode Node1 { get; set; } = new RefTestNode();
        public RefTestNode Node2 { get; set; } = new RefTestNode();
        public List<RefTestNode> NodeList { get; set; } = new List<RefTestNode>();
        public RefTestNode[] NodeArray { get; set; } = new RefTestNode[0];
    }

    [Fact]
    public void Serialize_PreserveReferencesObjects_ReturnsReferenceFormat()
    {
        var sharedNode = new RefTestNode { Name = "SharedNode" };
        var container = new RefTestContainer
        {
            SharedNode = sharedNode,
            Node1 = new RefTestNode { Name = "Node1", Parent = sharedNode },
            Node2 = new RefTestNode { Name = "Node2", Parent = sharedNode }
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(container, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$ref\"", result);
    }

    [Fact]
    public void Serialize_PreserveReferencesArrays_ReturnsReferenceFormat()
    {
        var sharedNode = new RefTestNode { Name = "SharedNode" };
        var container = new RefTestContainer
        {
            SharedNode = sharedNode,
            NodeList = new List<RefTestNode> { sharedNode }
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(container, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$values\"", result);
    }

    [Fact]
    public void Serialize_PreserveReferencesAll_ReturnsFullReferenceFormat()
    {
        var sharedNode = new RefTestNode { Name = "SharedNode" };
        var container = new RefTestContainer
        {
            SharedNode = sharedNode,
            Node1 = new RefTestNode { Name = "Node1", Parent = sharedNode },
            Node2 = new RefTestNode { Name = "Node2", Parent = sharedNode },
            NodeList = new List<RefTestNode> { sharedNode },
            NodeArray = new RefTestNode[] { sharedNode }
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.Indented
        };
        var result = MyJson.SerializeObject(container, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$ref\"", result);
        Assert.Contains("\"$values\"", result);
    }

    [Fact]
    public void Deserialize_PreserveReferencesAll_MaintainsObjectReferences()
    {
        var sharedNode = new RefTestNode { Name = "SharedNode" };
        var container = new RefTestContainer
        {
            SharedNode = sharedNode,
            Node1 = new RefTestNode { Name = "Node1", Parent = sharedNode },
            Node2 = new RefTestNode { Name = "Node2", Parent = sharedNode },
            NodeList = new List<RefTestNode> { sharedNode }
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.Indented
        };
        var json = MyJson.SerializeObject(container, settings);
        Console.WriteLine($"JSON:\n{json}");
        var deserialized = MyJson.DeserializeObject<RefTestContainer>(json, settings);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.NodeList);
        Assert.True(deserialized.NodeList.Count > 0, $"NodeList.Count = {deserialized.NodeList.Count}");
        Assert.True(ReferenceEquals(deserialized.Node1.Parent, deserialized.Node2.Parent),
            "Node1.Parent 和 Node2.Parent 应该是同一个引用");
        Assert.True(ReferenceEquals(deserialized.SharedNode, deserialized.NodeList[0]),
            "SharedNode 和 NodeList[0] 应该是同一个引用");
    }

    [Fact]
    public void Serialize_PreserveReferencesNone_NoReferenceFormat()
    {
        var sharedNode = new RefTestNode { Name = "SharedNode" };
        var container = new RefTestContainer
        {
            SharedNode = sharedNode,
            Node1 = new RefTestNode { Name = "Node1", Parent = sharedNode }
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(container, settings);
        Assert.DoesNotContain("\"$id\"", result);
        Assert.DoesNotContain("\"$ref\"", result);
    }

    [Fact]
    public void Serialize_SelfReference_ReturnsReferenceFormat()
    {
        var node = new RefTestNode { Name = "SelfRef" };
        node.Parent = node;
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(node, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$ref\"", result);
    }

    [Fact]
    public void Deserialize_SelfReference_MaintainsReference()
    {
        var node = new RefTestNode { Name = "SelfRef" };
        node.Parent = node;
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.None
        };
        var json = MyJson.SerializeObject(node, settings);
        var deserialized = MyJson.DeserializeObject<RefTestNode>(json, settings);

        Assert.NotNull(deserialized);
        Assert.True(ReferenceEquals(deserialized, deserialized.Parent),
            "反序列化后自引用应该保持");
        Assert.Equal("SelfRef", deserialized.Name);
    }

    [Fact]
    public void Serialize_CircularReference_ReturnsReferenceFormat()
    {
        var parent = new RefTestNode { Name = "Parent" };
        var child = new RefTestNode { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(parent, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$ref\"", result);
    }

    [Fact]
    public void Deserialize_CircularReference_MaintainsReferences()
    {
        var parent = new RefTestNode { Name = "Parent" };
        var child = new RefTestNode { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.None
        };
        var json = MyJson.SerializeObject(parent, settings);
        var deserialized = MyJson.DeserializeObject<RefTestNode>(json, settings);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized.Children);
        Assert.True(ReferenceEquals(deserialized, deserialized.Children[0].Parent),
            "反序列化后循环引用应该保持");
        Assert.Equal("Parent", deserialized.Name);
        Assert.Equal("Child", deserialized.Children[0].Name);
    }

    [Fact]
    public void Serialize_DiamondReference_ReturnsReferenceFormat()
    {
        var shared = new RefTestNode { Name = "Shared" };
        var parent = new RefTestNode { Name = "Parent" };
        var child1 = new RefTestNode { Name = "Child1", Parent = parent };
        var child2 = new RefTestNode { Name = "Child2", Parent = parent };
        parent.Children.Add(child1);
        parent.Children.Add(child2);
        var container = new RefTestContainer
        {
            SharedNode = shared,
            Node1 = child1,
            Node2 = child2
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(container, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$ref\"", result);
    }

    [Fact]
    public void Deserialize_DiamondReference_MaintainsReferences()
    {
        var shared = new RefTestNode { Name = "Shared" };
        var parent = new RefTestNode { Name = "Parent" };
        var child1 = new RefTestNode { Name = "Child1", Parent = parent };
        var child2 = new RefTestNode { Name = "Child2", Parent = parent };
        parent.Children.Add(child1);
        parent.Children.Add(child2);
        var container = new RefTestContainer
        {
            SharedNode = shared,
            Node1 = child1,
            Node2 = child2
        };
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.None
        };
        var json = MyJson.SerializeObject(container, settings);
        var deserialized = MyJson.DeserializeObject<RefTestContainer>(json, settings);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Node1);
        Assert.NotNull(deserialized.Node2);
        Assert.True(ReferenceEquals(deserialized.Node1.Parent, deserialized.Node2.Parent),
            "菱形引用：Node1.Parent 和 Node2.Parent 应该是同一个对象");
        Assert.Equal("Parent", deserialized.Node1.Parent?.Name);
    }



    //1. 菱形引用 + PreserveReferencesHandling.All
    //   → 有 $id 和 $ref（共享引用用 $ref 表示）
    //2. 非菱形引用 + PreserveReferencesHandling.Objects
    //   → 有 $id 但没有 $ref（每个对象只出现一次）
    //3. 非菱形引用 + PreserveReferencesHandling.None
    //   → 没有 $id 和 $ref（纯JSON，无引用追踪）

    // 一般情况,非菱形引用,没有$ref
    [Fact]
    public void Serialize_NoSharedReference_NoRefInJson()
    {
        var root = new RefTestNode { Name = "Root" };
        var child1 = new RefTestNode { Name = "Child1" };
        var child2 = new RefTestNode { Name = "Child2" };
        root.Children.Add(child1);
        root.Children.Add(child2);
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(root, settings);
        Assert.Contains("\"$id\"", result);
        Assert.DoesNotContain("\"$ref\"", result);
    }

    [Fact]
    public void Deserialize_NoSharedReference_MaintainsStructure()
    {
        var root = new RefTestNode { Name = "Root" };
        var child1 = new RefTestNode { Name = "Child1" };
        var child2 = new RefTestNode { Name = "Child2" };
        root.Children.Add(child1);
        root.Children.Add(child2);
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.None
        };
        var json = MyJson.SerializeObject(root, settings);
        var deserialized = MyJson.DeserializeObject<RefTestNode>(json, settings);

        Assert.NotNull(deserialized);
        Assert.Equal("Root", deserialized.Name);
        Assert.Equal(2, deserialized.Children.Count);
        Assert.Equal("Child1", deserialized.Children[0].Name);
        Assert.Equal("Child2", deserialized.Children[1].Name);
    }

    // 完全不使用引用保留，没有$id和$ref
    [Fact]
    public void Serialize_NoPreserveReferences_NoIdOrRef()
    {
        var root = new RefTestNode { Name = "Root" };
        var child1 = new RefTestNode { Name = "Child1" };
        var child2 = new RefTestNode { Name = "Child2" };
        root.Children.Add(child1);
        root.Children.Add(child2);
        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            Formatting = Formatting.None
        };
        var result = MyJson.SerializeObject(root, settings);
        Assert.DoesNotContain("\"$id\"", result);
        Assert.DoesNotContain("\"$ref\"", result);
        Assert.Contains("\"Name\":\"Root\"", result);
        Assert.Contains("\"Name\":\"Child1\"", result);
        Assert.Contains("\"Name\":\"Child2\"", result);
    }

    #endregion

    #region HashSet 序列化测试

    [Fact]
    public void Serialize_HashSet_ReturnsCorrectJson()
    {
        var hashSet = new HashSet<string> { "a", "b", "c" };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(hashSet, settings);
        Assert.Contains("\"a\"", result);
        Assert.Contains("\"b\"", result);
        Assert.Contains("\"c\"", result);
    }

    [Fact]
    public void Serialize_EmptyHashSet_ReturnsEmptyArray()
    {
        var hashSet = new HashSet<string>();
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(hashSet, settings);
        Assert.Equal("[]", result);
    }

    [Fact]
    public void Serialize_HashSetInt_ReturnsCorrectJson()
    {
        var hashSet = new HashSet<int> { 1, 2, 3 };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(hashSet, settings);
        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.Contains("3", result);
    }

    [Fact]
    public void Serialize_ObjectWithHashSet_ReturnsCorrectJson()
    {
        var obj = new TestObjectWithHashSet
        {
            Name = "Test",
            Items = new HashSet<string> { "a", "b", "c" }
        };
        var settings = new MyJsonSettings { PreserveReferencesHandling = PreserveReferencesHandling.None };
        var result = MyJson.SerializeObject(obj, settings);
        Assert.Contains("\"Name\":\"Test\"", result);
        Assert.Contains("\"Items\":", result);
    }

    #endregion

    #region HashtableEnumerator 序列化测试

    [Fact]
    public void Serialize_HashtableEnumerator_ProducesValidJson()
    {
        var hashtable = new System.Collections.Hashtable();
        hashtable["key1"] = "value1";
        hashtable["key2"] = "value2";
        var enumerator = hashtable.GetEnumerator();
        enumerator.MoveNext();

        var result = MyJson.SerializeObject(enumerator);
        Assert.NotNull(result);
        Assert.NotEqual("null", result);
    }

    [Fact]
    public void Serialize_ContainerWithHashtableEnumerator_ProducesValidJson()
    {
        var hashtable = new System.Collections.Hashtable();
        hashtable["key1"] = "value1";
        var enumerator = hashtable.GetEnumerator();
        enumerator.MoveNext();

        var container = new Dictionary<string, object>
        {
            {"normal_data", "test"},
            {"enumerator", enumerator}
        };

        var result = MyJson.SerializeObject(container);
        Assert.NotNull(result);
        Assert.Contains("normal_data", result);
    }

    [Fact]
    public void Serialize_ListEnumerator_ProducesValidJson()
    {
        var list = new List<int> { 1, 2, 3 };
        var listEnumerator = list.GetEnumerator();
        listEnumerator.MoveNext();

        var result = MyJson.SerializeObject(listEnumerator);
        Assert.NotNull(result);
        Assert.NotEqual("null", result);
    }

    #endregion

    #region Newtonsoft.Json 对比测试

    [Fact]
    public void CompareWithNewtonsoft_BasicObject_ProducesSameResult()
    {
        var basicObj = new
        {
            Name = "张三",
            Age = 30,
            IsActive = true,
            Score = 95.5
        };

        var myJsonResult = MyJson.SerializeObject(basicObj, new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(basicObj);

        Assert.Equal(newtonsoftResult, myJsonResult);
    }

    [Fact]
    public void CompareWithNewtonsoft_IntArray_ProducesSameResult()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };

        var myJsonResult = MyJson.SerializeObject(array, new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(array);

        Assert.Equal(newtonsoftResult, myJsonResult);
    }

    [Fact]
    public void CompareWithNewtonsoft_StringArray_ProducesSameResult()
    {
        var array = new string[] { "a", "b", "c" };

        var myJsonResult = MyJson.SerializeObject(array, new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(array);

        Assert.Equal(newtonsoftResult, myJsonResult);
    }

    [Fact]
    public void CompareWithNewtonsoft_EmptyArray_ProducesSameResult()
    {
        var array = new int[0];

        var myJsonResult = MyJson.SerializeObject(array, new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(array);

        Assert.Equal(newtonsoftResult, myJsonResult);
    }

    [Fact]
    public void CompareWithNewtonsoft_Dictionary_ProducesSameResult()
    {
        var dict = new Dictionary<string, object>
        {
            { "name", "张三" },
            { "age", 30 },
            { "isActive", true }
        };

        var myJsonResult = MyJson.SerializeObject(dict, new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(dict);

        Assert.Contains("\"name\"", myJsonResult);
        Assert.Contains("\"age\":30", myJsonResult);
        Assert.Contains("\"isActive\":true", myJsonResult);
        Assert.Contains("\"name\"", newtonsoftResult);
        Assert.Contains("\"age\":30", newtonsoftResult);
        Assert.Contains("\"isActive\":true", newtonsoftResult);
    }

    [Fact]
    public void CompareWithNewtonsoft_NestedObject_ProducesSameResult()
    {
        var nestedObj = new
        {
            Id = 1,
            Name = "测试",
            Nested = new
            {
                Value = 123,
                Items = new List<int> { 1, 2, 3 }
            }
        };

        var myJsonResult = MyJson.SerializeObject(nestedObj, new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(nestedObj);

        Assert.Equal(newtonsoftResult, myJsonResult);
    }

    [Fact]
    public void CompareWithNewtonsoft_CircularReference_BothIgnore()
    {
        var parent = new CircularRefNode { Name = "Parent" };
        var child = new CircularRefNode { Name = "Child" };
        parent.Children.Add(child);
        child.Parent = parent;

        var myJsonSettings = new MyJsonSettings
        {
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        var newtonsoftSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        };

        var myJsonResult = MyJson.SerializeObject(parent, myJsonSettings);
        var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(parent, newtonsoftSettings);

        Assert.Contains("\"Name\":\"Parent\"", myJsonResult);
        Assert.Contains("\"Name\":\"Child\"", myJsonResult);
        Assert.Contains("\"Name\":\"Parent\"", newtonsoftResult);
        Assert.Contains("\"Name\":\"Child\"", newtonsoftResult);
    }

    #endregion

    #region 详细循环引用场景测试

    [Fact]
    public void CircularReference_SimpleParentChild_HandlesCorrectly()
    {
        var parent = new CircularRefNode { Name = "Parent" };
        var child = new CircularRefNode { Name = "Child" };
        parent.Children.Add(child);
        child.Parent = parent;

        var settings = new MyJsonSettings
        {
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var json = MyJson.SerializeObject(parent, settings);
        Assert.Contains("\"Name\":\"Parent\"", json);
        Assert.Contains("\"Name\":\"Child\"", json);
    }

    [Fact]
    public void CircularReference_ComplexFriendNetwork_HandlesCorrectly()
    {
        var person1 = new CircularRefPerson { Name = "张三" };
        var person2 = new CircularRefPerson { Name = "李四" };
        var person3 = new CircularRefPerson { Name = "王五" };

        person1.BestFriend = person2;
        person2.BestFriend = person3;
        person3.BestFriend = person1;

        person1.Friends.Add(person2);
        person1.Friends.Add(person3);
        person2.Friends.Add(person1);
        person3.Friends.Add(person1);

        var settings = new MyJsonSettings
        {
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var json = MyJson.SerializeObject(person1, settings);
        Assert.Contains("\"Name\":\"张三\"", json);
        Assert.Contains("\"Name\":\"李四\"", json);
        Assert.Contains("\"Name\":\"王五\"", json);
    }

    [Fact]
    public void CircularReference_SelfReference_HandlesCorrectly()
    {
        var node = new CircularRefNode { Name = "SelfReferencing" };
        node.Parent = node;

        var settings = new MyJsonSettings
        {
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var json = MyJson.SerializeObject(node, settings);
        Assert.Contains("\"Name\":\"SelfReferencing\"", json);
    }

    [Fact]
    public void CircularReference_WithPreserveReferences_MaintainsReferences()
    {
        var parent = new CircularRefNode { Name = "Parent" };
        var child = new CircularRefNode { Name = "Child" };
        parent.Children.Add(child);
        child.Parent = parent;

        var settings = new MyJsonSettings
        {
            Formatting = Formatting.None,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };

        var json = MyJson.SerializeObject(parent, settings);
        Assert.Contains("\"$id\"", json);
        Assert.Contains("\"$ref\"", json);
    }

    [Fact]
    public void CircularReference_Error_ThrowsException()
    {
        var parent = new CircularRefNode { Name = "Parent" };
        var child = new CircularRefNode { Name = "Child" };
        parent.Children.Add(child);
        child.Parent = parent;

        var settings = new MyJsonSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Error
        };

        Assert.Throws<System.InvalidOperationException>(() => MyJson.SerializeObject(parent, settings));
    }

    #endregion

    #region PreserveReferencesHandling 详细测试

    [Fact]
    public void PreserveReferences_None_NoIdOrRef()
    {
        var sharedNode = new SharedRefNode { Name = "SharedNode" };
        var container = new SharedRefContainer
        {
            SharedNode = sharedNode,
            NodeList = new List<SharedRefNode> { sharedNode },
            NodeArray = new SharedRefNode[] { sharedNode }
        };

        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            Formatting = Formatting.None
        };

        var result = MyJson.SerializeObject(container, settings);
        Assert.DoesNotContain("\"$id\"", result);
        Assert.DoesNotContain("\"$ref\"", result);
    }

    [Fact]
    public void PreserveReferences_Objects_HasIdAndRef()
    {
        var sharedNode = new SharedRefNode { Name = "SharedNode" };
        var container = new SharedRefContainer
        {
            SharedNode = sharedNode,
            NodeList = new List<SharedRefNode> { sharedNode },
            NodeArray = new SharedRefNode[] { sharedNode }
        };

        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.None
        };

        var result = MyJson.SerializeObject(container, settings);
        Assert.Contains("\"$id\"", result);
        Assert.Contains("\"$ref\"", result);
    }

    [Fact]
    public void PreserveReferences_Arrays_HasIdAndRef()
    {
        var sharedNode = new SharedRefNode { Name = "SharedNode" };
        var container = new SharedRefContainer
        {
            SharedNode = sharedNode,
            NodeList = new List<SharedRefNode> { sharedNode },
            NodeArray = new SharedRefNode[] { sharedNode }
        };

        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
            Formatting = Formatting.None
        };

        var result = MyJson.SerializeObject(container, settings);
        Assert.Contains("\"$id\"", result);
    }

    [Fact]
    public void PreserveReferences_All_MaintainsReferencesAfterDeserialization()
    {
        var sharedNode = new SharedRefNode { Name = "SharedNode" };
        var node1 = new SharedRefNode { Name = "Node1" };
        var node2 = new SharedRefNode { Name = "Node2" };

        var container = new SharedRefContainer
        {
            SharedNode = sharedNode,
            NodeList = new List<SharedRefNode> { sharedNode, node1, node2 },
            NodeArray = new SharedRefNode[] { sharedNode, node1, node2 }
        };

        var settings = new MyJsonSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.None
        };

        var json = MyJson.SerializeObject(container, settings);
        var deserialized = MyJson.DeserializeObject<SharedRefContainer>(json, settings);

        Assert.NotNull(deserialized);
        Assert.True(ReferenceEquals(deserialized.SharedNode, deserialized.NodeList[0]),
            "SharedNode 和 NodeList[0] 应该是同一个引用");
        Assert.True(ReferenceEquals(deserialized.NodeList[0], deserialized.NodeArray[0]),
            "NodeList[0] 和 NodeArray[0] 应该是同一个引用");
    }

    [Fact]
    public void PreserveReferences_EnumValues_AreCorrect()
    {
        Assert.Equal(0, (int)PreserveReferencesHandling.None);
        Assert.Equal(1, (int)PreserveReferencesHandling.Objects);
        Assert.Equal(2, (int)PreserveReferencesHandling.Arrays);
        Assert.Equal(3, (int)PreserveReferencesHandling.All);
        Assert.Equal(PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays, PreserveReferencesHandling.All);
    }

    #endregion
}

public class TestObjectWithHashSet
{
    public string Name { get; set; } = string.Empty;
    public HashSet<string> Items { get; set; } = new HashSet<string>();
}

public class CircularRefNode
{
    public string Name { get; set; } = string.Empty;
    public CircularRefNode? Parent { get; set; }
    public List<CircularRefNode> Children { get; set; } = new List<CircularRefNode>();
}

public class CircularRefPerson
{
    public string Name { get; set; } = string.Empty;
    public CircularRefPerson? BestFriend { get; set; }
    public List<CircularRefPerson> Friends { get; set; } = new List<CircularRefPerson>();
}

public class SharedRefNode
{
    public string Name { get; set; } = string.Empty;
}

public class SharedRefContainer
{
    public SharedRefNode SharedNode { get; set; } = new SharedRefNode();
    public List<SharedRefNode> NodeList { get; set; } = new List<SharedRefNode>();
    public SharedRefNode[] NodeArray { get; set; } = new SharedRefNode[0];
}
