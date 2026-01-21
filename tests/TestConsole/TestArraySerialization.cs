using System;
using System.Collections.Generic;

namespace IFoxCAD.Cad.Tests
{
    class TestArraySerialization
    {
        static void Main(string[] args)
        {
            // 测试对象1：包含空数组
            var testObj1 = new TestData
            {
                Name = "Test1",
                EmptyArray = new string[0],
                EmptyList = new List<int>(),
                NormalArray = new int[] { 1, 2, 3 },
                NormalList = new List<string> { "a", "b", "c" },
                aaa = new HashSet<string>() { "a", "b", "c" }
            };

            // 测试对象2：用户提供的JSON格式
            var testObj2 = new IMEConfig
            {
                AutoEn2Cn = new string[0],
                AutoCn2En = new string[0],
                IMEHookStyle = "Global",
                IMEInputSwitch = "Shift",
                aaa = new HashSet<string>() { "a", "b", "c" }
            };

            Console.WriteLine("=== 测试1：包含各种数组的对象 ===");
            TestSerialization(testObj1);
            Console.WriteLine();

            Console.WriteLine("=== 测试2：用户提供的IME配置对象 ===");
            TestSerialization(testObj2);

            Console.ReadKey();
        }

        static void TestSerialization(object obj)
        {
            // 未格式化输出
            var settingsNone = new MyJsonSettings { Formatting = Formatting.None };
            var jsonNone = MyJson.SerializeObject(obj, settingsNone);
            Console.WriteLine("未格式化JSON:");
            Console.WriteLine(jsonNone);
            Console.WriteLine();

            // 格式化输出
            var settingsIndented = new MyJsonSettings { Formatting = Formatting.Indented };
            var jsonIndented = MyJson.SerializeObject(obj, settingsIndented);
            Console.WriteLine("格式化JSON:");
            Console.WriteLine(jsonIndented);
        }
    }

    class TestData
    {
        public string Name { get; set; }
        public string[] EmptyArray { get; set; }
        public HashSet<string> aaa { get; set; }
        public List<int> EmptyList { get; set; }
        public int[] NormalArray { get; set; }
        public List<string> NormalList { get; set; }
    }

    class IMEConfig
    {
        public HashSet<string> aaa { get; set; }
        public string[] AutoEn2Cn { get; set; }
        public string[] AutoCn2En { get; set; }
        public string IMEHookStyle { get; set; }
        public string IMEInputSwitch { get; set; }
    }
}