using System;
using System.Collections.Generic;
using IFoxCAD.Cad;
using Newtonsoft.Json;
using MyFormatting = IFoxCAD.Cad.Formatting;
using NewtonsoftFormatting = Newtonsoft.Json.Formatting;
using MyPreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling;
using MyReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling;
using NewtonsoftPreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling;
using NewtonsoftReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling;

namespace TestConsole;

public class TestCircularReference
{
    public class Node
    {
        public string Name { get; set; } = string.Empty;
        public Node? Parent { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
    }

    public class Person
    {
        public string Name { get; set; } = string.Empty;
        public Person? BestFriend { get; set; }
        public List<Person> Friends { get; set; } = new List<Person>();
    }

    public static void RunTests()
    {
        Console.WriteLine("=== 循环引用测试 ===");
        
        TestSimpleCircularReference();  // 重新启用，测试修复结果
        TestComplexCircularReference();  // 重新启用，修复引用处理后应该没问题
        TestSelfReference();  // 重新启用，修复引用处理后应该没问题
        TestNewtonsoftJsonComparison(); // 重新启用，修复引用处理后应该没问题
        TestDeserializationComparison(); // 重新启用测试
        TestMyJsonPreserveReferences(); // 测试新的引用处理功能
        
        Console.WriteLine("所有循环引用测试完成！");
    }

    private static void TestSimpleCircularReference()
    {
        Console.WriteLine("\n1. 简单循环引用测试：");
        
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child" };
        
        parent.Children.Add(child);
        child.Parent = parent; // 创建循环引用
        
        try
        {
            // 使用Ignore来匹配Newtonsoft.Json的行为
            var settings = new MyJsonSettings 
            { 
                Formatting = MyFormatting.Indented,
                ReferenceLoopHandling = MyReferenceLoopHandling.Ignore
            };
            
            var json = MyJson.SerializeObject(parent, settings);
            Console.WriteLine("序列化结果：");
            Console.WriteLine(json);
            
            // 验证反序列化
            var deserialized = MyJson.DeserializeObject<Node>(json);
            Console.WriteLine($"反序列化成功: {deserialized?.Name ?? "null"}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }

    private static void TestComplexCircularReference()
    {
        Console.WriteLine("\n2. 复杂循环引用测试：");
        
        var person1 = new Person { Name = "张三" };
        var person2 = new Person { Name = "李四" };
        var person3 = new Person { Name = "王五" };
        
        // 创建复杂的循环引用关系
        person1.BestFriend = person2;
        person2.BestFriend = person3;
        person3.BestFriend = person1; // 循环引用
        
        person1.Friends.Add(person2);
        person1.Friends.Add(person3);
        person2.Friends.Add(person1);
        person3.Friends.Add(person1);
        
        try
        {
            var settings = new MyJsonSettings 
            { 
                Formatting = MyFormatting.Indented,
                ReferenceLoopHandling = MyReferenceLoopHandling.Ignore
            };
            var json = MyJson.SerializeObject(person1, settings);
            Console.WriteLine("序列化结果：");
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }

    private static void TestSelfReference()
    {
        Console.WriteLine("\n3. 自引用测试：");
        
        var node = new Node { Name = "SelfReferencing" };
        node.Parent = node; // 自引用
        
        try
        {
            var settings = new MyJsonSettings 
            { 
                Formatting = MyFormatting.Indented,
                ReferenceLoopHandling = MyReferenceLoopHandling.Ignore
            };
            var json = MyJson.SerializeObject(node, settings);
            Console.WriteLine("序列化结果：");
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }

    private static void TestNewtonsoftJsonComparison()
    {
        Console.WriteLine("\n4. Newtonsoft.Json vs MyJson 循环引用处理对比：");
        
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child" };
        parent.Children.Add(child);
        child.Parent = parent; // 创建循环引用
        
        Console.WriteLine("MyJson 序列化结果：");
        try
        {
            var settings = new MyJsonSettings 
            { 
                Formatting = MyFormatting.Indented,
                ReferenceLoopHandling = MyReferenceLoopHandling.Ignore // 与Newtonsoft.Json保持一致
            };
            var myJsonResult = MyJson.SerializeObject(parent, settings);
            Console.WriteLine(myJsonResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MyJson 错误: {ex.Message}");
        }
        
        Console.WriteLine("\nNewtonsoft.Json 序列化结果：");
        try
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = NewtonsoftFormatting.Indented,
                ReferenceLoopHandling = NewtonsoftReferenceLoopHandling.Ignore // 忽略循环引用
            };
            var newtonsoftResult = Newtonsoft.Json.JsonConvert.SerializeObject(parent, settings);
            Console.WriteLine(newtonsoftResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Newtonsoft.Json 错误: {ex.Message}");
        }
    }

    private static void TestDeserializationComparison()
    {
        Console.WriteLine("\n5. 循环引用反序列化对比测试：");
        
        // 创建一个包含循环引用的JSON字符串（MyJson格式）
        string circularJson = """
        {
          "Name": "Parent",
          "Parent": null,
          "Children": [
            {
              "Name": "Child",
              "Parent": null,
              "Children": []
            }
          ]
        }
        """;
        
        Console.WriteLine("原始JSON（注意：循环引用被转换为null）：");
        Console.WriteLine(circularJson);
        
        Console.WriteLine("\nMyJson 反序列化：");
        try
        {
            var myDeserialized = MyJson.DeserializeObject<Node>(circularJson);
            Console.WriteLine($"反序列化成功: {myDeserialized?.Name ?? "null"}");
            Console.WriteLine($"子节点数量: {myDeserialized?.Children?.Count ?? 0}");
            if (myDeserialized?.Children?.Count > 0)
            {
                Console.WriteLine($"第一个子节点: {myDeserialized.Children[0].Name}");
                Console.WriteLine($"子节点的父节点: {myDeserialized.Children[0].Parent?.Name ?? "null"}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MyJson 反序列化错误: {ex.Message}");
        }
        
        // 创建一个Newtonsoft.Json格式的循环引用JSON（使用引用ID）
        string newtonsoftCircularJson = """
        {
          "$id": "1",
          "Name": "Parent",
          "Parent": null,
          "Children": [
            {
              "$id": "2",
              "Name": "Child",
              "Parent": {
                "$ref": "1"
              },
              "Children": []
            }
          ]
        }
        """;
        
        Console.WriteLine("\nNewtonsoft.Json 反序列化（使用$ref引用）：");
        try
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = NewtonsoftPreserveReferencesHandling.Objects
            };
            var newtonsoftDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Node>(newtonsoftCircularJson, settings);
            Console.WriteLine($"反序列化成功: {newtonsoftDeserialized?.Name ?? "null"}");
            Console.WriteLine($"子节点数量: {newtonsoftDeserialized?.Children?.Count ?? 0}");
            if (newtonsoftDeserialized?.Children?.Count > 0)
            {
                Console.WriteLine($"第一个子节点: {newtonsoftDeserialized.Children[0].Name}");
                Console.WriteLine($"子节点的父节点: {newtonsoftDeserialized.Children[0].Parent?.Name ?? "null"}");
                Console.WriteLine($"是否恢复循环引用: {newtonsoftDeserialized.Children[0].Parent == newtonsoftDeserialized}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Newtonsoft.Json 反序列化错误: {ex.Message}");
        }
    }

    private static void TestMyJsonPreserveReferences()
    {
        Console.WriteLine("\n6. MyJson 新引用处理功能测试：");
        
        // 先测试简单的对象引用（非循环引用）
        var sharedNode = new Node { Name = "SharedNode" };
        var container = new { Node1 = sharedNode, Node2 = sharedNode }; // 同一个对象被两个属性引用
        
        // 启用引用保留
        var settings = new MyJsonSettings 
        { 
            Formatting = MyFormatting.Indented,
            PreserveReferencesHandling = MyPreserveReferencesHandling.Objects
        };
        
        Console.WriteLine("启用PreserveReferencesHandling.Objects的简单引用测试：");
        try
        {
            var json = MyJson.SerializeObject(container, settings);
            Console.WriteLine("序列化结果：");
            Console.WriteLine(json);
            
            // 测试反序列化
            Console.WriteLine("\n反序列化结果：");
            var deserialized = MyJson.DeserializeObject<object>(json);
            Console.WriteLine($"反序列化成功: {deserialized != null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
            Console.WriteLine($"堆栈: {ex.StackTrace}");
        }
        
        // 测试数组引用
        Console.WriteLine("\n7. 数组引用处理测试：");
        var list1 = new List<string> { "item1", "item2" };
        var arrayContainer = new { List1 = list1, List2 = list1 }; // list1被两个属性引用
        
        var arraySettings = new MyJsonSettings 
        { 
            Formatting = MyFormatting.Indented,
            PreserveReferencesHandling = MyPreserveReferencesHandling.All
        };
        
        try
        {
            var arrayJson = MyJson.SerializeObject(arrayContainer, arraySettings);
            Console.WriteLine("数组引用序列化结果：");
            Console.WriteLine(arrayJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数组引用测试错误: {ex.Message}");
        }
        
        // 测试循环引用处理（使用Ignore）
        Console.WriteLine("\n8. 循环引用处理（使用Ignore）测试：");
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child" };
        parent.Children.Add(child);
        child.Parent = parent; // 创建循环引用
        
        var ignoreSettings = new MyJsonSettings 
        { 
            Formatting = MyFormatting.Indented,
            ReferenceLoopHandling = MyReferenceLoopHandling.Ignore
        };
        
        try
        {
            var json = MyJson.SerializeObject(parent, ignoreSettings);
            Console.WriteLine("使用ReferenceLoopHandling.Ignore的序列化结果：");
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }
}