using System;
using System.Collections.Generic;
using IFoxCAD.Cad;

namespace TestConsole;

public class TestJson
{
    public static void RunTests()
    {
        Console.WriteLine("=== JSON序列化测试 ===");
        Console.WriteLine();

        // 测试1：基本对象序列化
        TestBasicObject();
        Console.WriteLine();

        // 测试2：包含数组的对象（用户提供的格式）
        TestIMEConfig();
        Console.WriteLine();

        // 测试3：嵌套对象和数组
        TestComplexObject();
    }

    static void TestBasicObject()
    {
        Console.WriteLine("1. 基本对象序列化：");
        
        var person = new {
            Name = "张三",
            Age = 30,
            IsActive = true,
            Salary = 5000.50
        };

        // 未格式化输出
        var settingsNone = new MyJsonSettings { Formatting = Formatting.None };
        var jsonNone = MyJson.SerializeObject(person, settingsNone);
        Console.WriteLine("未格式化：");
        Console.WriteLine(jsonNone);

        // 格式化输出
        var settingsIndented = new MyJsonSettings { Formatting = Formatting.Indented };
        var jsonIndented = MyJson.SerializeObject(person, settingsIndented);
        Console.WriteLine("格式化：");
        Console.WriteLine(jsonIndented);
    }

    static void TestIMEConfig()
    {
        Console.WriteLine("2. IME配置对象（包含空数组）：");
        
        var imeConfig = new {
            AutoEn2Cn = new string[0],
            AutoCn2En = new string[0],
            IMEHookStyle = "Global",
            IMEInputSwitch = "Shift"
        };

        // 未格式化输出
        var settingsNone = new MyJsonSettings { Formatting = Formatting.None };
        var jsonNone = MyJson.SerializeObject(imeConfig, settingsNone);
        Console.WriteLine("未格式化：");
        Console.WriteLine(jsonNone);

        // 格式化输出
        var settingsIndented = new MyJsonSettings { Formatting = Formatting.Indented };
        var jsonIndented = MyJson.SerializeObject(imeConfig, settingsIndented);
        Console.WriteLine("格式化：");
        Console.WriteLine(jsonIndented);
    }

    static void TestComplexObject()
    {
        Console.WriteLine("3. 复杂对象（嵌套对象和数组）：");
        
        var complexObj = new {
            Id = 1,
            Name = "复杂对象",
            Tags = new[] { "JSON", "测试", "格式化" },
            Nested = new {
                Value = 123,
                Items = new List<int> { 1, 2, 3, 4, 5 }
            },
            NullValue = (string)null
        };

        // 未格式化输出
        var settingsNone = new MyJsonSettings { Formatting = Formatting.None };
        var jsonNone = MyJson.SerializeObject(complexObj, settingsNone);
        Console.WriteLine("未格式化：");
        Console.WriteLine(jsonNone);

        // 格式化输出
        var settingsIndented = new MyJsonSettings { Formatting = Formatting.Indented };
        var jsonIndented = MyJson.SerializeObject(complexObj, settingsIndented);
        Console.WriteLine("格式化：");
        Console.WriteLine(jsonIndented);
    }
}