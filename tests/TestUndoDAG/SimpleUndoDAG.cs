using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace TestUndoDAG
{
    // 简化的动作类型枚举
    public enum ActionType
    {
        // 系统操作(根节点使用)
        OtherOperation,

        // 命令相关
        CommandExecution,

        // 数据库操作
        CreateEntity,      // 创建图元
        DeleteEntity,      // 删除图元
        ModifyEntity,      // 修改图元
    }

    // 动作接口
    public interface IAction
    {
        string Id { get; }
        ActionType Type { get; }
        string Description { get; }
        DateTime Timestamp { get; }

        // 执行动作
        void Execute();

        // 获取逆动作
        IAction GetInverseAction();

        // 克隆动作
        IAction Clone();

        // 合并动作（如果可能）
        bool CanMergeWith(IAction otherAction);
        IAction MergeWith(IAction otherAction);
    }

    // 基础动作实现
    public abstract class BaseAction : IAction
    {
        private static int _globalActionId = 0;
        public string Id { get; } = "#" + Interlocked.Increment(ref _globalActionId).ToString();
        public abstract ActionType Type { get; }
        public abstract string Description { get; }
        public DateTime Timestamp { get; } = DateTime.Now;

        public abstract void Execute();
        public abstract IAction GetInverseAction();
        public abstract IAction Clone();
        public abstract bool CanMergeWith(IAction otherAction);
        public abstract IAction MergeWith(IAction otherAction);

        // 序列化支持
        public virtual string Serialize() =>
            $"{{\"Id\":\"{Id}\",\"Type\":\"{Type}\",\"Description\":\"{Description}\",\"Timestamp\":\"{Timestamp}\"}}";

        public static IAction Deserialize(string json)
        {
            // 简单的反序列化实现
            return null;
        }
    }

    // 版本节点
    public class VersionNode
    {
        private static int _globalNodeId = 0;
        public string Id { get; } = "@" + Interlocked.Increment(ref _globalNodeId).ToString();
        public IAction Action { get; set; }
        public VersionNode Parent { get; set; }
        public List<VersionNode> Children { get; } = new List<VersionNode>();
        public int Depth { get; set; }
        public string Hash { get; private set; }

        public VersionNode(IAction action, VersionNode parent = null)
        {
            Action = action;
            Parent = parent;
            Depth = (parent != null) ? parent.Depth + 1 : 0;
            CalculateHash();
        }

        public void RecalculateHash()
        {
            CalculateHash();
        }

        public void CalculateHash()
        {
            var data = $"{Action.Id}-{Action.Type}-{Action.Timestamp.Ticks}-{Depth}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public bool IsAncestorOf(VersionNode node)
        {
            var current = node;
            while (current != null)
            {
                if (current == this) return true;
                current = current.Parent;
            }
            return false;
        }

        public bool IsDescendantOf(VersionNode node) => node.IsAncestorOf(this);
    }

    // DAG版本图
    public class ActionDAG
    {
        public VersionNode Root { get; }
        public VersionNode Current { get; private set; }
        public List<VersionNode> Branches { get; } = new List<VersionNode>();
        public List<VersionNode> History => GetAllNodes();

        public ActionDAG()
        {
            Root = new VersionNode(new RootAction());
            Current = Root;
        }

        // 添加动作
        public VersionNode AddAction(IAction action)
        {
            // 检查是否可以合并到当前节点
            if (Current.Action.CanMergeWith(action))
            {
                Current.Action = Current.Action.MergeWith(action);
                Current.CalculateHash();
                return Current;
            }

            var node = new VersionNode(action, Current);
            Current.Children.Add(node);
            Current = node;

            return node;
        }

        // 添加分支
        public VersionNode AddBranch(IAction action, VersionNode fromNode)
        {
            if (fromNode == null || !History.Contains(fromNode))
                throw new ArgumentException("无效的源节点");

            var node = new VersionNode(action, fromNode);
            fromNode.Children.Add(node);

            if (!Branches.Contains(fromNode))
                Branches.Add(fromNode);

            return node;
        }

        // 切换到指定节点
        public void SwitchTo(VersionNode targetNode)
        {
            if (targetNode == null || !History.Contains(targetNode))
                throw new ArgumentException("无效的目标节点");

            // 计算从当前节点到目标节点的路径
            var path = CalculatePath(Current, targetNode);

            // 执行路径中的动作
            foreach (var step in path)
            {
                if (step != null)
                {
                    step.Execute();
                }
            }

            Current = targetNode;
        }

        // 计算两个节点之间的路径
        private List<IAction> CalculatePath(VersionNode fromNode, VersionNode toNode)
        {
            var path = new List<IAction>();

            if (fromNode == toNode) return path;

            if (fromNode.IsAncestorOf(toNode))
            {
                // 向前：执行子节点动作
                var current = fromNode;
                while (current != toNode)
                {
                    var child = current.Children.FirstOrDefault(c => c.IsAncestorOf(toNode));
                    if (child == null) break;

                    path.Add(child.Action);
                    current = child;
                }
            }
            else if (toNode.IsAncestorOf(fromNode))
            {
                // 向后：执行逆动作
                var current = fromNode;
                while (current != toNode)
                {
                    path.Add(current.Action.GetInverseAction());
                    current = current.Parent;
                }
            }
            else
            {
                // 需要先回退到公共祖先
                var commonAncestor = FindCommonAncestor(fromNode, toNode);

                // 回退到公共祖先
                var backPath = CalculatePath(fromNode, commonAncestor);
                path.AddRange(backPath);

                // 前进到目标节点
                var forwardPath = CalculatePath(commonAncestor, toNode);
                path.AddRange(forwardPath);
            }

            return path;
        }

        // 查找公共祖先
        private VersionNode FindCommonAncestor(VersionNode node1, VersionNode node2)
        {
            var ancestors1 = new HashSet<VersionNode>();
            var current = node1;
            while (current != null)
            {
                ancestors1.Add(current);
                current = current.Parent;
            }

            current = node2;
            while (current != null)
            {
                if (ancestors1.Contains(current))
                    return current;
                current = current.Parent;
            }

            return Root;
        }

        // 获取所有节点
        private List<VersionNode> GetAllNodes()
        {
            var nodes = new List<VersionNode>();
            CollectNodes(Root, nodes);
            return nodes;
        }

        private void CollectNodes(VersionNode node, List<VersionNode> nodes)
        {
            nodes.Add(node);
            foreach (var child in node.Children)
            {
                CollectNodes(child, nodes);
            }
        }
    }

    // 根动作（虚拟）
    public class RootAction : BaseAction
    {
        public override ActionType Type => ActionType.OtherOperation;
        public override string Description => "初始状态";

        public override void Execute() { }

        public override IAction GetInverseAction() => this;

        public override IAction Clone() => this;

        public override bool CanMergeWith(IAction otherAction) => false;

        public override IAction MergeWith(IAction otherAction) => this;
    }

    // 简化的动作日志器
    public class SimpleActionLogger
    {
        private static SimpleActionLogger _instance;
        private static SimpleActionLogger InstanceGetter()
        {
            if (_instance == null)
                _instance = new SimpleActionLogger();
            return _instance;
        }

        public static SimpleActionLogger Instance { get { return InstanceGetter(); } }

        // CAD数据库实例
        public static CadDatabase Database { get; private set; } = new CadDatabase();

        private ActionDAG _dag = new ActionDAG();
        private readonly object _lock = new object();

        // 记录动作
        public void LogAction(IAction action)
        {
            lock (_lock)
            {
                _dag.AddAction(action);
            }
        }

        // 执行撤销
        public void Undo()
        {
            lock (_lock)
            {
                if (_dag.Current.Parent != null)
                {
                    // 直接切换到父节点，让SwitchTo处理必要的操作
                    _dag.SwitchTo(_dag.Current.Parent);
                }
            }
        }

        // 执行重做
        public void Redo()
        {
            lock (_lock)
            {
                if (_dag.Current.Children.Count > 0)
                {
                    // 默认选择第一个子节点
                    var nextNode = _dag.Current.Children[0];
                    _dag.SwitchTo(nextNode);
                }
            }
        }

        // 获取历史
        public List<IAction> GetHistory()
        {
            return _dag.History.Select(n => n.Action).ToList();
        }

        // 清空历史
        public void Clear()
        {
            lock (_lock)
            {
                _dag = new ActionDAG();
            }
        }
    }
}