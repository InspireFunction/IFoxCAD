using System;
using System.Collections.Generic;
using System.Linq;
using IFoxCAD.Cad;

// 如果项目中有Newtonsoft.Json，可以取消下面这行的注释
// using Newtonsoft.Json;

namespace TestConsole
{
    /// <summary>
    /// 对比 MyJson 和 Newtonsoft.Json 的 PreserveReferencesHandling 功能
    /// </summary>
    public class JsonComparison
    {
        public class Node
        {
            public string Name { get; set; } = "";
            public Node? Parent { get; set; }
            public List<Node> Children { get; set; } = new List<Node>();
        }

        public class Container
        {
            public Node SharedNode { get; set; } = new Node();
            public Node Node1 { get; set; } = new Node();
            public Node Node2 { get; set; } = new Node();
            public List<Node> NodeList { get; set; } = new List<Node>();
            public Node[] NodeArray { get; set; } = new Node[0];
        }

        public static void RunComparison()
        {
            Console.WriteLine("=== MyJson vs Newtonsoft.Json PreserveReferencesHandling 对比 ===\n");

            // 创建包含共享引用的数据结构
            var sharedNode = new Node { Name = "SharedNode" };
            var node1 = new Node { Name = "Node1", Parent = sharedNode };
            var node2 = new Node { Name = "Node2", Parent = sharedNode };

            var container = new Container
            {
                SharedNode = sharedNode,
                Node1 = node1,
                Node2 = node2,
                NodeList = new List<Node> { sharedNode, node1, node2 },
                NodeArray = new Node[] { sharedNode, node1, node2 }
            };

            // 1. 测试 MyJson 的 PreserveReferencesHandling.Objects
            Console.WriteLine("1. MyJson PreserveReferencesHandling.Objects:");
            var myJsonSettingsObjects = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };
            var myJsonResultObjects = MyJson.SerializeObject(container, myJsonSettingsObjects);
            Console.WriteLine(myJsonResultObjects);
            Console.WriteLine();

            // 2. 测试 MyJson 的 PreserveReferencesHandling.Arrays
            Console.WriteLine("2. MyJson PreserveReferencesHandling.Arrays:");
            var myJsonSettingsArrays = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                Formatting = Formatting.Indented
            };
            var myJsonResultArrays = MyJson.SerializeObject(container, myJsonSettingsArrays);
            Console.WriteLine(myJsonResultArrays);
            Console.WriteLine();

            // 3. 测试 MyJson 的 PreserveReferencesHandling.All
            Console.WriteLine("3. MyJson PreserveReferencesHandling.All:");
            var myJsonSettingsAll = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            };
            var myJsonResultAll = MyJson.SerializeObject(container, myJsonSettingsAll);
            Console.WriteLine(myJsonResultAll);
            Console.WriteLine();

            // 4. 测试 MyJson 的 PreserveReferencesHandling.None (默认)
            Console.WriteLine("4. MyJson PreserveReferencesHandling.None (默认):");
            var myJsonSettingsNone = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                Formatting = Formatting.Indented
            };
            var myJsonResultNone = MyJson.SerializeObject(container, myJsonSettingsNone);
            Console.WriteLine(myJsonResultNone);
            Console.WriteLine();

            // 5. 演示如何在MyJson中同时保留对象和数组
            Console.WriteLine("5. MyJson PreserveReferencesHandling.All (保留所有引用 - 对象和数组):");
            var myJsonSettingsCombined = new MyJsonSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            };
            var myJsonResultCombined = MyJson.SerializeObject(container, myJsonSettingsCombined);
            Console.WriteLine(myJsonResultCombined);
            Console.WriteLine();

            // 6. 验证反序列化后引用是否保持
            Console.WriteLine("6. 验证反序列化后引用是否保持:");
            var deserializedContainer = MyJson.DeserializeObject<Container>(myJsonResultCombined, myJsonSettingsCombined);
            if (deserializedContainer != null)
            {
                Console.WriteLine($"NodeList[0] == NodeArray[0]: {ReferenceEquals(deserializedContainer.NodeList[0], deserializedContainer.NodeArray[0])}");
                Console.WriteLine($"Node1.Parent == Node2.Parent: {ReferenceEquals(deserializedContainer.Node1.Parent, deserializedContainer.Node2.Parent)}");
                Console.WriteLine($"SharedNode == NodeList[0]: {ReferenceEquals(deserializedContainer.SharedNode, deserializedContainer.NodeList[0])}");
            }
            Console.WriteLine();

            // 7. Newtonsoft.Json 示例 (如果可用)
            Console.WriteLine("7. Newtonsoft.Json PreserveReferencesHandling 示例:");
            Console.WriteLine("// 注意: Newtonsoft.Json 的 PreserveReferencesHandling 枚举值:");
            Console.WriteLine("// PreserveReferencesHandling.None = 0");
            Console.WriteLine("// PreserveReferencesHandling.Objects = 1");
            Console.WriteLine("// PreserveReferencesHandling.Arrays = 2");
            Console.WriteLine("// PreserveReferencesHandling.All = 3");
            Console.WriteLine("// ");
            Console.WriteLine("// 在Newtonsoft.Json中，使用方式相同:");
            Console.WriteLine("// var settings = new JsonSerializerSettings");
            Console.WriteLine("// {");
            Console.WriteLine("//     PreserveReferencesHandling = PreserveReferencesHandling.All,");
            Console.WriteLine("//     Formatting = Formatting.Indented");
            Console.WriteLine("// };");
            Console.WriteLine("// var result = JsonConvert.SerializeObject(container, settings);");
            Console.WriteLine();

            Console.WriteLine("=== 总结 ===");
            Console.WriteLine("1. MyJson 和 Newtonsoft.Json 的 PreserveReferencesHandling 枚举定义几乎完全相同");
            Console.WriteLine("2. 两者都有四个值：None(0), Objects(1), Arrays(2), All(3)");
            Console.WriteLine("3. 'All' 值可以同时保留对象和数组的引用");
            Console.WriteLine("4. 这是一种组合枚举，通过位标志实现组合效果");
            Console.WriteLine("5. 当设置为 All 时，既保留对象引用也保留数组/集合引用");
        }
    }
}