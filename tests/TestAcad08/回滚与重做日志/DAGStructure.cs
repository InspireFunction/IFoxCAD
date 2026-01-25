namespace JoinBoxAcad;

using System.Security.Cryptography;

// DAG版本图
public class ActionDAG
{
    /// <summary>
    /// 根节点
    /// </summary>
    public VersionNode Root { get; }
    /// <summary>
    /// 当前版本的节点
    /// </summary>
    public VersionNode Current { get; set; }
    /// <summary>
    /// 分支
    /// </summary>
    public List<VersionNode> Branches { get; } = [];
    /// <summary>
    /// 历史版本
    /// </summary>
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
                Env.Printl($"动作链执行了: ({step.Description})");
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
                var inverseAction = current.Action.GetInverseAction();
                if (inverseAction is not null)
                {
                    // 有逆向命令
                    path.Add(inverseAction);
                }
                else
                {
                    // 没有逆向命令,就用逆向数据
                }
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

    // 序列化
    public string Serialize()
    {
        var data = new
        {
            CurrentNodeId = Current.Id,
            Nodes = SerializeNodes()
        };

        try
        {
            var serializeSettings = new MyJsonSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                Converters = [new ObjectIdConverter()]
            };

            var result = MyJson.SerializeObject(data, serializeSettings);
            Env.Printl($"DAG Serialize Success, Result Length: {result.Length}");
            return result;
        }
        catch (Exception ex)
        {
            Env.Printl($"DAG Serialize Error, Message: {ex.Message}");
            Env.Printl($"DAG Serialize Error Details: {ex}");
            throw;
        }
    }

    private Dictionary<string, object> SerializeNodes()
    {
        var nodes = new Dictionary<string, object>();

        foreach (var node in History)
        {
            try
            {
                Env.Printl($"Serializing node: {node.Id}, Action Type: {node.Action?.GetType().Name}");
                var actionSerialized = node.Action is BaseAction baseAction ? baseAction.Serialize() : "";
                Env.Printl($"Node {node.Id} Action Serialized Length: {actionSerialized.Length}");

                nodes[node.Id] = new
                {
                    Action = actionSerialized,
                    ParentId = node.Parent?.Id,
                    Depth = node.Depth,
                    Hash = node.Hash
                };
            }
            catch (Exception ex)
            {
                Env.Printl($"Error serializing node {node.Id}: {ex.Message}");
                Env.Printl($"Error serializing node details: {ex}");
                throw;
            }
        }

        Env.Printl($"Total nodes serialized: {nodes.Count}");
        return nodes;
    }
}

// 根动作（虚拟）
public class RootAction : BaseAction
{
    public override ActionType Type => ActionType.OtherOperation;
    public override string Description => "系统初始状态 (根节点)";
    public override void Execute() { }
    public override IAction GetInverseAction() => this;
    public override IAction Clone() => this;
    public override bool CanMergeWith(IAction otherAction) => false;
    public override IAction MergeWith(IAction otherAction) => this;
}

// 版本节点
public class VersionNode
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public IAction Action { get; set; }
    public VersionNode Parent { get; set; }
    public List<VersionNode> Children { get; } = new();
    public int Depth { get; set; }
    public string Hash { get; private set; }

    public VersionNode(IAction action, VersionNode parent = null)
    {
        Action = action;
        Parent = parent;
        Depth = parent?.Depth + 1 ?? 0;
        CalculateHash();
    }

    public void RecalculateHash()
    {
        CalculateHash();
    }

    public void CalculateHash()
    {
        var data = $"{Action.GuId}-{Action.Type}-{Action.Timestamp.Ticks}-{Depth}";
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