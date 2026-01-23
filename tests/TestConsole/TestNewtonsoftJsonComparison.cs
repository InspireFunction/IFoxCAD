using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using IFoxCAD.Cad;

using MyFormatting = IFoxCAD.Cad.Formatting;
using NewFormatting = Newtonsoft.Json.Formatting;

namespace TestConsole
{
    public class TestNewtonsoftJsonComparison
    {
        public static void RunTests()
        {
            Console.WriteLine("=== Newtonsoft.Json 与 MyJson 对比测试 ===");
            Console.WriteLine();

            // 测试1：基本对象序列化
            TestBasicObject();
            Console.WriteLine();

            // 测试2：复杂对象序列化
            TestComplexObject();
            Console.WriteLine();

            // 测试3：数组序列化
            TestArraySerialization();
            Console.WriteLine();

            // 测试4：枚举序列化
            TestEnumSerialization();
            Console.WriteLine();

            // 测试5：自定义对象序列化
            TestCustomObject();
            Console.WriteLine();

            // 测试6：反序列化功能
            TestDeserialization();
            Console.WriteLine();

            // 测试7：点结构体序列化对比
            TestPointStruct();
            Console.WriteLine();

            // 测试8：字典序列化对比
            TestDictionarySerialization();
            Console.WriteLine();
        }

        static void TestBasicObject()
        {
            Console.WriteLine("1. 基本对象序列化对比：");

            var basicObj = new
            {
                Name = "张三",
                Age = 30,
                IsActive = true,
                Score = 95.5,
                Description = "这是一个测试对象"
            };

            // MyJson 序列化
            var myJsonResult = MyJson.SerializeObject(basicObj, new MyJsonSettings { Formatting = MyFormatting.Indented });
            Console.WriteLine("MyJson 结果：");
            Console.WriteLine(myJsonResult);

            // Newtonsoft.Json 序列化
            var newtonsoftResult = JsonConvert.SerializeObject(basicObj, NewFormatting.Indented);
            Console.WriteLine("Newtonsoft.Json 结果：");
            Console.WriteLine(newtonsoftResult);

            Console.WriteLine($"结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
        }

        static void TestComplexObject()
        {
            Console.WriteLine("2. 复杂对象序列化对比：");

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
                Metadata = new Dictionary<string, object>
                {
                    { "Created", DateTime.Now.ToString() },
                    { "Version", 1.0 }
                },
                NullValue = (string?)null
            };

            // MyJson 序列化
            var myJsonResult = MyJson.SerializeObject(complexObj, new MyJsonSettings { Formatting = MyFormatting.Indented });
            Console.WriteLine("MyJson 结果：");
            Console.WriteLine(myJsonResult);

            // Newtonsoft.Json 序列化
            var newtonsoftResult = JsonConvert.SerializeObject(complexObj, NewFormatting.Indented);
            Console.WriteLine("Newtonsoft.Json 结果：");
            Console.WriteLine(newtonsoftResult);

            Console.WriteLine($"结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
        }

        static void TestArraySerialization()
        {
            Console.WriteLine("3. 数组序列化对比：");

            var arrays = new object[]
            {
                new int[] { 1, 2, 3, 4, 5 },
                new string[] { "a", "b", "c" },
                new List<double> { 1.1, 2.2, 3.3 },
                new object[0] // 空数组
            };

            for (int i = 0; i < arrays.Length; i++)
            {
                Console.WriteLine($"  数组 {i + 1}:");

                // MyJson 序列化
                var myJsonResult = MyJson.SerializeObject(arrays[i], new MyJsonSettings { Formatting = MyFormatting.None });
                Console.WriteLine($"    MyJson: {myJsonResult}");

                // Newtonsoft.Json 序列化
                var newtonsoftResult = JsonConvert.SerializeObject(arrays[i]);
                Console.WriteLine($"    Newtonsoft.Json: {newtonsoftResult}");

                Console.WriteLine($"    结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
            }
        }

        static void TestEnumSerialization()
        {
            Console.WriteLine("4. 枚举序列化对比：");

            var enumObj = new
            {
                Status = MyStatus.Active,
                Priority = MyPriority.High,
                Level = MyLevel.Normal
            };

            // MyJson 序列化
            var myJsonResult = MyJson.SerializeObject(enumObj, new MyJsonSettings { Formatting = MyFormatting.None });
            Console.WriteLine("MyJson 结果：");
            Console.WriteLine(myJsonResult);

            // Newtonsoft.Json 序列化
            var newtonsoftResult = JsonConvert.SerializeObject(enumObj);
            Console.WriteLine("Newtonsoft.Json 结果：");
            Console.WriteLine(newtonsoftResult);

            Console.WriteLine($"结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
        }

        static void TestCustomObject()
        {
            Console.WriteLine("5. 自定义对象序列化对比：");

            var customObj = new Person
            {
                Id = 1001,
                Name = "李四",
                Age = 28,
                Email = "lisi@example.com",
                Addresses = new List<Address>
                {
                    new Address { Street = "中山路123号", City = "北京", ZipCode = "100000" },
                    new Address { Street = "解放路456号", City = "上海", ZipCode = "200000" }
                },
                Skills = new string[] { "C#", "JavaScript", "Python" }
            };

            // MyJson 序列化
            var myJsonResult = MyJson.SerializeObject(customObj, new MyJsonSettings { Formatting = MyFormatting.Indented });
            Console.WriteLine("MyJson 结果：");
            Console.WriteLine(myJsonResult);

            // Newtonsoft.Json 序列化
            var newtonsoftResult = JsonConvert.SerializeObject(customObj, NewFormatting.Indented);
            Console.WriteLine("Newtonsoft.Json 结果：");
            Console.WriteLine(newtonsoftResult);

            Console.WriteLine($"结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
        }

        static void TestDeserialization()
        {
            Console.WriteLine("6. 反序列化功能对比：");

            var jsonString = @"{
                ""Id"": 1001,
                ""Name"": ""王五"",
                ""Age"": 35,
                ""Email"": ""wangwu@example.com"",
                ""Skills"": [""Java"", ""Python"", ""Go""]
            }";

            Console.WriteLine("原始JSON字符串：");
            Console.WriteLine(jsonString);

            try
            {
                // MyJson 反序列化
                var myJsonResult = MyJson.DeserializeObject<Person>(jsonString, new MyJsonSettings());
                if (myJsonResult != null)
                {
                    Console.WriteLine("MyJson 反序列化结果：");
                    Console.WriteLine($"  Id: {myJsonResult.Id}, Name: {myJsonResult.Name}, Age: {myJsonResult.Age}, Email: {myJsonResult.Email}");
                    Console.WriteLine($"  技能: [{string.Join(", ", myJsonResult.Skills ?? new string[0])}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MyJson 反序列化异常: {ex.Message}");
            }

            try
            {
                // Newtonsoft.Json 反序列化
                var newtonsoftResult = JsonConvert.DeserializeObject<Person>(jsonString);
                if (newtonsoftResult != null)
                {
                    Console.WriteLine("Newtonsoft.Json 反序列化结果：");
                    Console.WriteLine($"  Id: {newtonsoftResult.Id}, Name: {newtonsoftResult.Name}, Age: {newtonsoftResult.Age}, Email: {newtonsoftResult.Email}");
                    Console.WriteLine($"  技能: [{string.Join(", ", newtonsoftResult.Skills ?? new string[0])}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Newtonsoft.Json 反序列化异常: {ex.Message}");
            }
        }
    
        static void TestPointStruct()
        {
            Console.WriteLine("7. 点结构体序列化对比：");

            var point = new Point(3.14159, 2.71828);

            // MyJson 序列化
            var myJsonResult = MyJson.SerializeObject(point, new MyJsonSettings { Formatting = MyFormatting.Indented });
            Console.WriteLine("MyJson 结果：");
            Console.WriteLine(myJsonResult);

            // Newtonsoft.Json 序列化
            var newtonsoftResult = JsonConvert.SerializeObject(point, NewFormatting.Indented);
            Console.WriteLine("Newtonsoft.Json 结果：");
            Console.WriteLine(newtonsoftResult);

            Console.WriteLine($"结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
        }
        
        static void TestDictionarySerialization()
        {
            Console.WriteLine("8. 字典序列化对比：");

            var dict = new Dictionary<string, object>
            {
                { "name", "张三" },
                { "age", 30 },
                { "isActive", true },
                { "score", 95.5 },
                { "address", new Dictionary<string, string> { { "city", "北京" }, { "street", "中关村" } } }
            };

            // MyJson 序列化
            var myJsonResult = MyJson.SerializeObject(dict, new MyJsonSettings { Formatting = MyFormatting.Indented });
            Console.WriteLine("MyJson 结果：");
            Console.WriteLine(myJsonResult);

            // Newtonsoft.Json 序列化
            var newtonsoftResult = JsonConvert.SerializeObject(dict, NewFormatting.Indented);
            Console.WriteLine("Newtonsoft.Json 结果：");
            Console.WriteLine(newtonsoftResult);

            Console.WriteLine($"结果是否相同: {myJsonResult.Equals(newtonsoftResult)}");
        }
    }
    
    public enum MyStatus
    {
        Inactive,
        Active,
        Pending
    }

    public enum MyPriority
    {
        Low,
        Medium,
        High
    }

    public enum MyLevel
    {
        Low,
        Normal,
        High
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<Address> Addresses { get; set; } = new List<Address>();
        public string[] Skills { get; set; } = new string[0];
    }

    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
    
    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        
        public override string ToString()
        {
            return $"Point({X}, {Y})";
        }
    }
}