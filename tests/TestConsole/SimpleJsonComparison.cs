using System;
using System.Collections.Generic;
using System.Reflection;
using IFoxCAD.Cad;

namespace TestConsole
{
    public class SimpleJsonComparison
    {
        public class SimpleNode
        {
            public string Name { get; set; } = "";
        }

        public class SimpleContainer
        {
            public SimpleNode SharedNode { get; set; } = new SimpleNode();
            public List<SimpleNode> NodeList { get; set; } = new List<SimpleNode>();
            public SimpleNode[] NodeArray { get; set; } = new SimpleNode[0];
        }

        public static void RunSimpleTest()
        {
            Console.WriteLine("=== 简化版 MyJson PreserveReferencesHandling 测试 ===\n");

            // 创建包含共享引用的数据结构
            var sharedNode = new SimpleNode { Name = "SharedNode" };
            var node1 = new SimpleNode { Name = "Node1" };
            var node2 = new SimpleNode { Name = "Node2" };

            var container = new SimpleContainer
            {
                SharedNode = sharedNode,
                NodeList = new List<SimpleNode> { sharedNode, node1, node2 },
                NodeArray = new SimpleNode[] { sharedNode, node1, node2 }
            };

            // 测试 PreserveReferencesHandling.Objects
            Console.WriteLine("1. PreserveReferencesHandling.Objects:");
            var settingsObjects = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };
            try
            {
                var resultObjects = MyJson.SerializeObject(container, settingsObjects);
                Console.WriteLine(resultObjects);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}\n");
            }

            // 测试 PreserveReferencesHandling.Arrays
            Console.WriteLine("2. PreserveReferencesHandling.Arrays:");
            var settingsArrays = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                Formatting = Formatting.Indented
            };
            try
            {
                var resultArrays = MyJson.SerializeObject(container, settingsArrays);
                Console.WriteLine(resultArrays);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}\n");
            }

            // 测试 PreserveReferencesHandling.All
            Console.WriteLine("3. PreserveReferencesHandling.All:");
            var settingsAll = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            };
            try
            {
                var resultAll = MyJson.SerializeObject(container, settingsAll);
                Console.WriteLine(resultAll);
                Console.WriteLine();
                
                // 验证反序列化后引用是否保持
                Console.WriteLine("4. 验证反序列化后引用是否保持:");
                var deserializedContainer = MyJson.DeserializeObject<SimpleContainer>(resultAll, settingsAll);
                if (deserializedContainer != null)
                {
                    Console.WriteLine($"SharedNode == NodeList[0]: {ReferenceEquals(deserializedContainer.SharedNode, deserializedContainer.NodeList[0])}");
                    Console.WriteLine($"NodeList[0] == NodeArray[0]: {ReferenceEquals(deserializedContainer.NodeList[0], deserializedContainer.NodeArray[0])}");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}\n");
            }

            // 测试 PreserveReferencesHandling.None
            Console.WriteLine("5. PreserveReferencesHandling.None (默认):");
            var settingsNone = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                Formatting = Formatting.Indented
            };
            try
            {
                var resultNone = MyJson.SerializeObject(container, settingsNone);
                Console.WriteLine(resultNone);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}\n");
            }

            Console.WriteLine("=== PreserveReferencesHandling 枚举值说明 ===");
            Console.WriteLine("PreserveReferencesHandling.None = 0    (不保留任何引用)");
            Console.WriteLine("PreserveReferencesHandling.Objects = 1 (仅保留对象引用)");
            Console.WriteLine("PreserveReferencesHandling.Arrays = 2  (仅保留数组/集合引用)");
            Console.WriteLine("PreserveReferencesHandling.All = 3     (保留所有引用 - 对象和数组)");
            Console.WriteLine();
            Console.WriteLine("当设置为 'All' 时，可以同时保留对象和数组的引用，");
            Console.WriteLine("这相当于一种组合枚举，通过位标志实现组合效果。");
        }
    }
}