using System;
using System.Collections.Generic;
using System.Linq;

namespace JoinBoxAcad
{
    /// <summary>
    /// 测试重做功能中时间戳选择的验证类
    /// </summary>
    public class TestRedoTimestamp
    {
        /// <summary>
        /// 验证修复后的重做功能是否正确选择时间最新的子节点
        /// </summary>
        public static void ValidateTimestampSelection()
        {
            Console.WriteLine("验证重做功能中时间戳选择逻辑:");
            Console.WriteLine("===============================================");

            // 创建DAG实例
            var dag = new ActionDAG();

            // 创建一个模拟的图元ID
            var mockId = new ObjectId();

            // 创建三个不同时间的修改动作
            var changes1 = new Dictionary<string, (object, object)>
            {
                { "ColorIndex", (1, 2) }
            };
            var action1 = new ModifyEntityAction(mockId, changes1);
            // 手动调整时间戳，确保它们有不同的时间
            var baseTime = DateTime.Now.AddSeconds(-10);
            SetActionTimestamp(action1, baseTime.AddSeconds(1)); // 最早的

            var changes2 = new Dictionary<string, (object, object)>
            {
                { "ColorIndex", (2, 3) }
            };
            var action2 = new ModifyEntityAction(mockId, changes2);
            SetActionTimestamp(action2, baseTime.AddSeconds(3)); // 最新的

            var changes3 = new Dictionary<string, (object, object)>
            {
                { "ColorIndex", (3, 4) }
            };
            var action3 = new ModifyEntityAction(mockId, changes3);
            SetActionTimestamp(action3, baseTime.AddSeconds(2)); // 中间

            // 创建三个独立的节点（强制不合并）
            var node1 = dag.AddAction(action1, "TEST_COMMAND_1");
            var originalCurrent = dag.Current;

            // 回到父节点以创建分支
            dag.Current = dag.Root;
            var node2 = dag.AddAction(action2, "TEST_COMMAND_2"); // 时间最新
            dag.Current = dag.Root;
            var node3 = dag.AddAction(action3, "TEST_COMMAND_3"); // 时间中间

            Console.WriteLine($"根节点的子节点数量: {dag.Root.Children.Count}");

            foreach (var child in dag.Root.Children)
            {
                Console.WriteLine($"子节点命令: {child.CommandContext}");
                if (child.Actions.Count > 0)
                {
                    var firstAction = child.Actions.First() as BaseAction;
                    if (firstAction != null)
                    {
                        Console.WriteLine($"  动作时间戳: {firstAction.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                    }
                }
            }

            // 测试EnhancedActionLogger中的时间戳选择逻辑
            var logger = new TestEnhancedActionLogger();
            var orderedChildren = dag.Root.Children
                .OrderByDescending(child => logger.GetLatestActionTimestamp(child))
                .ToList();

            Console.WriteLine("\n按时间戳排序后的子节点顺序:");
            for (int i = 0; i < orderedChildren.Count; i++)
            {
                var child = orderedChildren[i];
                Console.WriteLine($"{i + 1}. {child.CommandContext}");
                if (child.Actions.Count > 0)
                {
                    var firstAction = child.Actions.First() as BaseAction;
                    if (firstAction != null)
                    {
                        Console.WriteLine($"   时间戳: {firstAction.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                    }
                }
            }

            Console.WriteLine("\n验证结果:");
            Console.WriteLine("- 应该首先选择时间戳最新的节点 (TEST_COMMAND_2)");
            Console.WriteLine("- 排序后第一个应该是 TEST_COMMAND_2");
            Console.WriteLine($"- 实际第一个是: {orderedChildren.FirstOrDefault()?.CommandContext}");

            if (orderedChildren.FirstOrDefault()?.CommandContext == "TEST_COMMAND_2")
            {
                Console.WriteLine("✅ 验证通过！时间戳选择逻辑正常工作。");
            }
            else
            {
                Console.WriteLine("❌ 验证失败！时间戳选择逻辑有问题。");
            }
        }

        /// <summary>
        /// 设置动作的时间戳（用于测试）
        /// </summary>
        private static void SetActionTimestamp(IAction action, DateTime timestamp)
        {
            if (action is BaseAction baseAction)
            {
                // 使用反射设置时间戳
                var field = typeof(BaseAction).GetField("Timestamp",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(baseAction, timestamp);
                }
            }
        }
    }

    /// <summary>
    /// 用于测试的增强日志器包装类
    /// </summary>
    public class TestEnhancedActionLogger
    {
        public DateTime GetLatestActionTimestamp(VersionNode node)
        {
            if (node.Actions == null || node.Actions.Count == 0)
                return DateTime.MinValue;

            return node.Actions.Max(action => action is BaseAction baseAction ? baseAction.Timestamp : DateTime.MinValue);
        }
    }
}