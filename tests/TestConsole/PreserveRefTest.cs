using System;
using System.Collections.Generic;
using IFoxCAD.Cad;

namespace TestConsole
{
    public class PreserveRefTest
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

        public static void RunTest()
        {
            Console.WriteLine("=== PreserveReferencesHandling 详细测试 ===\n");

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

            Console.WriteLine("原始对象引用关系:");
            Console.WriteLine($"sharedNode == container.NodeList[0]: {ReferenceEquals(sharedNode, container.NodeList[0])}");
            Console.WriteLine($"sharedNode == container.NodeArray[0]: {ReferenceEquals(sharedNode, container.NodeArray[0])}");
            Console.WriteLine($"container.NodeList[0] == container.NodeArray[0]: {ReferenceEquals(container.NodeList[0], container.NodeArray[0])}");
            Console.WriteLine();

            // 1. 测试 PreserveReferencesHandling.None
            Console.WriteLine("1. PreserveReferencesHandling.None:");
            var settingsNone = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                Formatting = Formatting.Indented
            };
            var resultNone = MyJson.SerializeObject(container, settingsNone);
            Console.WriteLine(resultNone);
            Console.WriteLine();

            // 2. 测试 PreserveReferencesHandling.Objects
            Console.WriteLine("2. PreserveReferencesHandling.Objects:");
            var settingsObjects = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };
            var resultObjects = MyJson.SerializeObject(container, settingsObjects);
            Console.WriteLine(resultObjects);
            Console.WriteLine();

            // 3. 测试 PreserveReferencesHandling.Arrays
            Console.WriteLine("3. PreserveReferencesHandling.Arrays:");
            var settingsArrays = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                Formatting = Formatting.Indented
            };
            var resultArrays = MyJson.SerializeObject(container, settingsArrays);
            Console.WriteLine(resultArrays);
            Console.WriteLine();

            // 4. 测试 PreserveReferencesHandling.All
            Console.WriteLine("4. PreserveReferencesHandling.All:");
            var settingsAll = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            };
            var resultAll = MyJson.SerializeObject(container, settingsAll);
            Console.WriteLine(resultAll);
            Console.WriteLine();

            // 5. 测试反序列化（小心处理异常）
            Console.WriteLine("5. 反序列化测试 (PreserveReferencesHandling.All):");
            try
            {
                var deserializedContainer = MyJson.DeserializeObject<SimpleContainer>(resultAll, settingsAll);
                if (deserializedContainer != null)
                {
                    Console.WriteLine($"反序列化成功!");
                    Console.WriteLine($"SharedNode == NodeList[0]: {ReferenceEquals(deserializedContainer.SharedNode, deserializedContainer.NodeList[0])}");
                    Console.WriteLine($"NodeList[0] == NodeArray[0]: {ReferenceEquals(deserializedContainer.NodeList[0], deserializedContainer.NodeArray[0])}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"反序列化失败: {ex.GetType().Name}: {ex.Message}");
            }
            Console.WriteLine();

            Console.WriteLine("=== PreserveReferencesHandling 枚举详解 ===");
            Console.WriteLine("PreserveReferencesHandling 是一个位标志枚举，定义如下：");
            Console.WriteLine("  None = 0     (0000) - 不保留任何引用");
            Console.WriteLine("  Objects = 1  (0001) - 仅保留对象引用");
            Console.WriteLine("  Arrays = 2   (0010) - 仅保留数组/集合引用");
            Console.WriteLine("  All = 3      (0011) - 保留所有引用 (Objects | Arrays)");
            Console.WriteLine();
            Console.WriteLine("这确实是组合枚举，通过位运算实现组合效果：");
            Console.WriteLine("  Objects | Arrays = 1 | 2 = 3 = All");
            Console.WriteLine("  当设置为 'All' 时，同时保留对象和数组的引用。");
        }
    }
}